using System.Diagnostics;
using System.Net.NetworkInformation;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.SiAccess;

namespace Phoenix.Data.Plc.Monitor.Polling.Test;

[TestFixture]
public class PollingPlcMonitorIntegrationTest
{
	#region Setup

	private const string HostEnvironmentVariableName = "PLC_HOST";

	private string _host;

	[SetUp]
	public void BeforeEachTest()
	{
		_host = System.Environment.GetEnvironmentVariable(HostEnvironmentVariableName);
		if (String.IsNullOrWhiteSpace(_host)) Assert.Ignore($"The environment variable '{HostEnvironmentVariableName}' that points to the host/ip of the actual plc is not defined. This integration test will not be executed.");

		var ping = new Ping();
		PingReply reply = null;
		try
		{
			reply = ping.Send(_host, 1000);
		}
		catch { /* ignore */ }
		if (reply is null || reply.Status != IPStatus.Success) Assert.Ignore($"Establishing a connection to '{_host}' failed. This integration test will not be executed, because it needs special hardware which seems to be unavailable.");
	}

	#endregion

	#region Data

	public Data.Plc.Test.Data Data { get; } = new Data.Plc.Test.Data();

	#endregion

	#region Tests

	#region Monitoring

	/// <summary>
	/// Checks against a real plc that changes of multiple monitored items (bytes and bits) are detected and that all of them are read with a single call per polling cycle.
	/// </summary>
	[Test]
	[Category("Monitoring")]
	public async Task MonitorChanges_WithBatchedReads()
	{
		// Arrange
		var interval = 100;
		using var plc = new DemoSiAccessPlc(new SiAccessPlcConnectionData(0, _host, 0, 0));
		Assert.That(plc.Connect(), Is.True, "Connecting to the plc failed.");

		// Remember the original values, so that they can be restored afterwards.
		var originalItem = new BytesPlcItem(Data.Datablock, Data.StartOfModifiableBytes, byteAmount: 4);
		await plc.ReadItemAsync(originalItem);
		var originalBytes = (byte[]) originalItem.Value.Clone();

		var items = new List<IPlcItem>();
		for (var offset = 0; offset < 4; offset++)
		{
			items.Add(new BytePlcItem(Data.Datablock, (ushort) (Data.StartOfModifiableBytes + offset)));
			items.Add(new BitPlcItem(Data.Datablock, (ushort) (Data.StartOfModifiableBytes + offset), BitPosition.X0));
		}

		// Initialize the items with the current plc values before subscribing, so that only the intended change is counted and not the initial read.
		await plc.ReadItemsAsync(items);

		var counters = items.ToDictionary(item => item, item => new ChangeCounter());
		foreach (var item in items) item.ValueChanged += counters[item].OnChanged;

		// Record all reads, but forward them to the actual plc.
		var readItemCounts = new System.Collections.Concurrent.ConcurrentQueue<int>();
		var plcMock = new Mock<IPlc>();
		plcMock
			.Setup(mock => mock.ReadItemsAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<CancellationToken>()))
			.Returns<ICollection<IPlcItem>, CancellationToken>
			(
				(plcItems, cancellationToken) =>
				{
					readItemCounts.Enqueue(plcItems.Count);
					return plc.ReadItemsAsync(plcItems, cancellationToken);
				}
			);
		using var monitor = new PollingPlcMonitor(plcMock.Object, TimeSpan.FromMilliseconds(interval));
		foreach (var item in items) monitor.MonitorItem(item);

		try
		{
			// Act
			var stopwatch = Stopwatch.StartNew();
			monitor.Start();
			await Task.Delay(interval * 3);
			var newBytes = originalBytes.Select(value => (byte) (value ^ byte.MaxValue)).ToArray();
			await plc.WriteItemAsync(new BytesPlcItem(Data.Datablock, Data.StartOfModifiableBytes, newBytes));
			var detected = await WaitUntilAsync(() => items.All(item => counters[item].Count == 1));
			monitor.Stop();
			stopwatch.Stop();

			// Assert
			Assert.That(detected, Is.True, "Not all changes were detected.");
			Assert.That(items.OfType<BitPlcItem>().Select(item => counters[item].Count), Has.All.EqualTo(1), "Not all bit changes were detected.");
			Assert.That(readItemCounts, Has.All.EqualTo(items.Count), "Every read must contain all distinct items.");
			Assert.That(readItemCounts.Count, Is.LessThanOrEqualTo(stopwatch.ElapsedMilliseconds / interval + 3), "The number of reads must correspond to the interval and not to interval x item count.");
		}
		finally
		{
			// Restore the original values.
			monitor.Stop();
			await plc.WriteItemAsync(new BytesPlcItem(Data.Datablock, Data.StartOfModifiableBytes, originalBytes));
		}
	}

	#endregion

	#endregion

	#region Nested Types

	private sealed class ChangeCounter
	{
		private int _count;

		/// <summary> The number of raised events. </summary>
		public int Count => Volatile.Read(ref _count);

		/// <summary> Event handler that can be subscribed to any <see cref="IPlcItem.ValueChanged"/> event. </summary>
		public void OnChanged(object sender, EventArgs args) => Interlocked.Increment(ref _count);
	}

	private static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeout = 10000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeout);
		while (DateTime.UtcNow < deadline)
		{
			if (condition()) return true;
			await Task.Delay(10);
		}
		return condition();
	}

	#endregion
}