using System.Collections.Concurrent;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.Mock;

namespace Phoenix.Data.Plc.Monitor.Polling.Test;

[TestFixture]
public class PollingPlcMonitorTest
{
	public Data.Plc.Test.Data Data { get; }

	public PollingPlcMonitorTest()
	{
		this.Data = new Data.Plc.Test.Data();
	}

	[Test]
	public async Task MonitorChanges()
	{
		var changes = 100;
		var target = changes * (changes + 1) / 2; // Gaussian sum formula
		var monitoredChanges = 0;
		var collectedValue = 0;

		var monitorItemIdentifier = "MonitoredItem";
		uint monitorItemInterval = 50;

		var monitoredPlc = new MockPlc().MakeMonitorable
		(
			new Dictionary<string, uint>
			{
				{monitorItemIdentifier, monitorItemInterval}
			}
		);
			
		// Create the item that must be monitored.
		var monitorItem = new BytePlcItem(dataBlock: Data.Datablock, position: Data.StartOfModifiableBytes, identifier: monitorItemIdentifier);

		// Create the item that is used to manipulate the value.
		IPlcItem writeItem = monitorItem.Clone("ChangeItem");

		// Set a callback for changes to the items.
		monitorItem.ValueChanged += (sender, args) =>
		{
			monitoredChanges++;
			collectedValue += args.NewValue;
		};

		try
		{
			// Connect to the plc.
			monitoredPlc.Connect();
				
			// Start monitoring those items.
			monitoredPlc.MonitorItem(monitorItem);
			monitoredPlc.Start();

			// Manipulate the monitored value.
			for (byte i = 1; i <= changes; i++)
			{
				writeItem.Value.TransferValuesFrom(new[] { i });
				await monitoredPlc.WriteItemAsync(writeItem);
				await Task.Delay((int)monitorItemInterval * 2); // This must be at least the double amount of the polling interval.
			}

			// Stop monitoring.
			monitoredPlc.Stop();

			// Further manipulate the value to check if this is not monitored.
			writeItem.Value.TransferValuesFrom(new[] { byte.MinValue });
			writeItem.Value.TransferValuesFrom(new[] { byte.MaxValue });

			// Check if all changes where registered.
			Assert.AreEqual(changes, monitoredChanges);
			Assert.AreEqual(target, collectedValue);
			Assert.AreEqual(changes, monitorItem.Value);
		}
		finally
		{
			monitoredPlc.Dispose();
		}
	}

	[Test]
	public async Task MonitorDifferentIntervals()
	{
		var monitoredChangesOfFirstItem = 0;
		var monitoredChangesOfSecondItem = 0;

		var firstMonitorItemIdentifier = "Monitored item #01";
		uint firstMonitorItemInterval = 50;
		var secondMonitorItemIdentifier = "Monitored item #02";
		uint secondMonitorItemInterval = 200;

		var changes = 100;
		var secondChanges = changes / ((secondMonitorItemInterval / firstMonitorItemInterval) / 2);

		var monitoredPlc = new MockPlc().MakeMonitorable
		(
			new Dictionary<string, uint>
			{
				{firstMonitorItemIdentifier, firstMonitorItemInterval}
				,{secondMonitorItemIdentifier, secondMonitorItemInterval}
			}
		);

		// Create items that must be monitored.
		var firstMonitoredItem = new BytePlcItem(dataBlock: Data.Datablock, position: Data.StartOfModifiableBytes, identifier: firstMonitorItemIdentifier);
		var secondMonitoredItem = firstMonitoredItem.Clone(secondMonitorItemIdentifier);

		// Create the item that is used to manipulate the value.
		IPlcItem writeItem = firstMonitoredItem.Clone("ChangeItem");

		// Set a callback for changes to the items.
		firstMonitoredItem.ValueChanged += (sender, args) =>
		{
			monitoredChangesOfFirstItem++;
		};
		secondMonitoredItem.ValueChanged += (sender, args) =>
		{
			monitoredChangesOfSecondItem++;
		};

		try
		{
			// Connect to the plc.
			monitoredPlc.Connect();

			// Start monitoring those items.
			monitoredPlc.MonitorItem(firstMonitoredItem);
			monitoredPlc.MonitorItem(secondMonitoredItem);
			monitoredPlc.Start();

			// Manipulate the monitored value.
			for (byte i = 1; i <= changes; i++)
			{
				writeItem.Value.TransferValuesFrom(new[] { i });
				await monitoredPlc.WriteItemAsync(writeItem);
				await Task.Delay((int)firstMonitorItemInterval * 2); // This must be at least the double amount of the polling interval.
			}

			// Stop monitoring.
			monitoredPlc.Stop();

			// Check if all changes where registered.
			Assert.AreEqual(changes, monitoredChangesOfFirstItem);
			Assert.True(monitoredChangesOfSecondItem >= secondChanges);
		}
		finally
		{
			monitoredPlc.Dispose();
		}
	}

	[Test]
	public async Task MonitorDifferentIdenticalItems()
	{
		var firstMonitoredItem = new BytesPlcItem(dataBlock: Data.Datablock, position: Data.StartOfFixedBytes, byteAmount: 2);
		var secondMonitoredItem = new UInt16PlcItem(dataBlock: Data.Datablock, position: Data.StartOfFixedBytes);
		var thirdMonitoredItem = new BitsPlcItem(dataBlock: Data.Datablock, position: Data.StartOfFixedBytes, bitPosition: BitPosition.X0, bitAmount: 16);

		var differentReadItems = new HashSet<IPlcItem>(new ReferenceComparer());
		var mockPlc = new MockPlc();
		Func<ICollection<IPlcItem>, CancellationToken, Task> performReadItemsAsyncMock = (plcItems, cancellationToken) =>
		{
			foreach (var plcItem in plcItems)
			{
				differentReadItems.Add(plcItem);
			}
			return mockPlc.ReadItemsAsync(plcItems, cancellationToken);
		};

		// Create a mock of MockPlc that signals which items are read.
		var plcMock = new Mock<IPlc>(behavior: MockBehavior.Strict)
		{
			CallBase = true,
		};
		plcMock
			.Setup(x => x.ReadItemsAsync(It.IsAny<IPlcItem[]>(), It.IsAny<CancellationToken>()))
			.Returns(performReadItemsAsyncMock)
			.Verifiable()
			;

		// Create the monitor with the mocked instance.
		var monitoredPlc = new PollingMonitorablePlc(plcMock.Object);

		// Start monitoring those items.
		monitoredPlc.MonitorItem(firstMonitoredItem);
		monitoredPlc.MonitorItem(secondMonitoredItem);
		monitoredPlc.MonitorItem(thirdMonitoredItem);
		monitoredPlc.Start();
		await Task.Delay(1000);
		monitoredPlc.Stop();
		Assert.AreEqual(1, differentReadItems.Count);
	}

	#region Data

	/// <summary>
	/// Wraps a <see cref="MockPlc"/> in a mocked <see cref="IPlc"/> that records every call to <see cref="IPlc.ReadItemsAsync"/>.
	/// </summary>
	private sealed class ReadRecorder
	{
		private readonly ConcurrentQueue<(DateTime Time, IPlcItem[] Items)> _calls = new();
		private int _readCount;

		/// <summary> The <see cref="MockPlc"/> holding the actual data. </summary>
		public MockPlc MockPlc { get; } = new MockPlc();

		/// <summary> The recording <see cref="IPlc"/> that should be given to the monitor. </summary>
		public IPlc Plc { get; }

		/// <summary> The number of calls to <see cref="IPlc.ReadItemsAsync"/>. </summary>
		public int ReadCount => Volatile.Read(ref _readCount);

		/// <summary> Snapshot of all recorded calls. </summary>
		public IReadOnlyList<(DateTime Time, IPlcItem[] Items)> Calls => _calls.ToArray();

		/// <param name="beforeRead"> Optional callback that is invoked with the one-based call number before the data is read. It may throw. </param>
		/// <param name="selectFailingItems"> Optional callback that selects the items of a call that fail to be read. All other items are read successfully and are reported as valid items of the thrown <see cref="ReadPlcException"/>. </param>
		public ReadRecorder(Action<int> beforeRead = null, Func<IPlcItem[], IPlcItem[]> selectFailingItems = null)
		{
			this.MockPlc.Connect();
			var plcMock = new Mock<IPlc>();
			plcMock
				.Setup(mock => mock.ReadItemsAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<CancellationToken>()))
				.Returns<ICollection<IPlcItem>, CancellationToken>
				(
					async (plcItems, cancellationToken) =>
					{
						var callNumber = Interlocked.Increment(ref _readCount);
						_calls.Enqueue((DateTime.UtcNow, plcItems.ToArray()));
						beforeRead?.Invoke(callNumber);
						var failedItems = selectFailingItems?.Invoke(plcItems.ToArray()) ?? Array.Empty<IPlcItem>();
						var validItems = plcItems.Where(plcItem => !failedItems.Any(failedItem => Object.ReferenceEquals(failedItem, plcItem))).ToList();
						await this.MockPlc.ReadItemsAsync(validItems, cancellationToken);
						if (failedItems.Any()) throw new ReadPlcException(validItems, failedItems.Select(failedItem => (failedItem, "Failed on purpose.")).ToList());
					}
				);
			this.Plc = plcMock.Object;
		}

		/// <summary>
		/// Writes a new value into one of the modifiable bytes of the data block.
		/// </summary>
		public async Task WriteAsync(Data.Plc.Test.Data data, byte value, int byteOffset = 0)
		{
			IPlcItem writeItem = new BytePlcItem(dataBlock: data.Datablock, position: (ushort) (data.StartOfModifiableBytes + byteOffset), identifier: $"Writer_{byteOffset}");
			writeItem.Value.TransferValuesFrom(new[] { value });
			await this.MockPlc.WriteItemAsync(writeItem);
		}
	}

	/// <summary>
	/// Thread safe counter for <see cref="IPlcItem.ValueChanged"/> events.
	/// </summary>
	private sealed class ChangeCounter
	{
		private int _count;

		/// <summary> The number of raised events. </summary>
		public int Count => Volatile.Read(ref _count);

		/// <summary> Event handler that can be subscribed to any <see cref="IPlcItem.ValueChanged"/> event. </summary>
		public void OnChanged(object sender, EventArgs args) => Interlocked.Increment(ref _count);
	}

	/// <summary>
	/// Waits until the <paramref name="condition"/> is met or the <paramref name="timeout"/> elapsed.
	/// </summary>
	/// <returns> <see langword="true"/> if the <paramref name="condition"/> was met. </returns>
	private static async Task<bool> WaitUntilAsync(Func<bool> condition, int timeout = 5000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeout);
		while (DateTime.UtcNow < deadline)
		{
			if (condition()) return true;
			await Task.Delay(10);
		}
		return condition();
	}

	private BytePlcItem CreateByteItem(int byteOffset = 0, string identifier = null)
		=> new BytePlcItem(dataBlock: Data.Datablock, position: (ushort) (Data.StartOfModifiableBytes + byteOffset), identifier: identifier);

	#endregion

	#region Tests

	#region UnMonitor

	/// <summary>
	/// Checks that no further <see cref="IPlcItem.ValueChanged"/> events are raised after an item has been unmonitored.
	/// </summary>
	[Test]
	[Category("UnMonitor")]
	public async Task UnMonitorItem_StopsChangeNotifications()
	{
		// Arrange
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc);
		var counter = new ChangeCounter();
		var item = this.CreateByteItem();
		item.ValueChanged += counter.OnChanged;
		monitor.MonitorItem(item);
		monitor.Start();
		await recorder.WriteAsync(Data, 1);
		Assert.That(await WaitUntilAsync(() => counter.Count == 1), Is.True, "The first change was not detected.");

		// Act
		monitor.UnMonitorItem(item);
		await recorder.WriteAsync(Data, 2);
		await Task.Delay((int) PlcItemMonitorConfigurations.DefaultPollingFrequency.TotalMilliseconds * 4);

		// Assert
		Assert.That(counter.Count, Is.EqualTo(1));
	}

	/// <summary>
	/// Checks that no further <see cref="IPlcItem.ValueChanged"/> events are raised after an item with a custom polling frequency has been unmonitored.
	/// </summary>
	[Test]
	[Category("UnMonitor")]
	public async Task UnMonitorItem_WithConfiguredPollingFrequency_StopsChangeNotifications()
	{
		// Arrange
		var identifier = "ConfiguredItem";
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { identifier, 50 } });
		var counter = new ChangeCounter();
		var item = this.CreateByteItem(identifier: identifier);
		item.ValueChanged += counter.OnChanged;
		monitor.MonitorItem(item);
		monitor.Start();
		await recorder.WriteAsync(Data, 1);
		Assert.That(await WaitUntilAsync(() => counter.Count == 1), Is.True, "The first change was not detected.");

		// Act
		monitor.UnMonitorItem(item);
		await recorder.WriteAsync(Data, 2);
		await Task.Delay(400);

		// Assert
		Assert.That(counter.Count, Is.EqualTo(1));
	}

	/// <summary>
	/// Checks that unmonitoring one of two identical (equal but not same) items only stops the notifications of that very item.
	/// </summary>
	/// <param name="unMonitorFirst"> Whether the first or the second item is unmonitored. </param>
	[Test]
	[Category("UnMonitor")]
	[TestCase(true)]
	[TestCase(false)]
	public async Task UnMonitorItem_OfOneOfTwoIdenticalItems_KeepsTheOther(bool unMonitorFirst)
	{
		// Arrange
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { "First", 50 }, { "Second", 50 } });
		var firstCounter = new ChangeCounter();
		var secondCounter = new ChangeCounter();
		var first = this.CreateByteItem(identifier: "First");
		var second = this.CreateByteItem(identifier: "Second");
		first.ValueChanged += firstCounter.OnChanged;
		second.ValueChanged += secondCounter.OnChanged;
		monitor.MonitorItem(first);
		monitor.MonitorItem(second);
		monitor.Start();
		await recorder.WriteAsync(Data, 1);
		Assert.That(await WaitUntilAsync(() => firstCounter.Count == 1 && secondCounter.Count == 1), Is.True, "The first change was not detected by both items.");

		// Act
		monitor.UnMonitorItem(unMonitorFirst ? first : second);
		await recorder.WriteAsync(Data, 2);
		var (removedCounter, keptCounter) = unMonitorFirst ? (firstCounter, secondCounter) : (secondCounter, firstCounter);
		var keptDetected = await WaitUntilAsync(() => keptCounter.Count == 2);
		await Task.Delay(400);

		// Assert
		Assert.That(keptDetected, Is.True, "The kept item did not receive the change.");
		Assert.That(removedCounter.Count, Is.EqualTo(1));
		Assert.That(keptCounter.Count, Is.EqualTo(2));
	}

	#region Exceptions

	/// <summary>
	/// Checks that polling continues after a read failed.
	/// </summary>
	[Test]
	[Category("Exceptions")]
	public async Task Polling_ContinuesAfterReadException()
	{
		// Arrange
		var identifier = "Item";
		var recorder = new ReadRecorder
		(
			callNumber =>
			{
				if (callNumber == 1) throw new ReadPlcException(new List<IPlcItem>(), new List<(IPlcItem, string)>());
			}
		);
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { identifier, 50 } });
		var counter = new ChangeCounter();
		var item = this.CreateByteItem(identifier: identifier);
		item.ValueChanged += counter.OnChanged;
		monitor.MonitorItem(item);

		// Act
		monitor.Start();
		await recorder.WriteAsync(Data, 1);
		var firstDetected = await WaitUntilAsync(() => counter.Count == 1);
		await recorder.WriteAsync(Data, 2);
		var secondDetected = await WaitUntilAsync(() => counter.Count == 2);

		// Assert
		Assert.That(firstDetected, Is.True, "The change after the failed read was not detected.");
		Assert.That(secondDetected, Is.True, "The change after the first successful read was not detected.");
		Assert.That(recorder.ReadCount, Is.GreaterThan(2));
	}

	/// <summary>
	/// Checks that polling continues after a subscriber of <see cref="IPlcItem.ValueChanged"/> threw an exception.
	/// </summary>
	[Test]
	[Category("Exceptions")]
	public async Task Polling_ContinuesAfterSubscriberException()
	{
		// Arrange
		var identifier = "Item";
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { identifier, 50 } });
		var counter = new ChangeCounter();
		var item = this.CreateByteItem(identifier: identifier);
		item.ValueChanged += (sender, args) =>
		{
			// Only the first invocation throws.
			if (counter.Count == 0)
			{
				counter.OnChanged(sender, args);
				throw new InvalidOperationException("Subscriber failure.");
			}
			counter.OnChanged(sender, args);
		};
		monitor.MonitorItem(item);

		// Act
		monitor.Start();
		await recorder.WriteAsync(Data, 1);
		var firstDetected = await WaitUntilAsync(() => counter.Count == 1);
		await recorder.WriteAsync(Data, 2);
		var secondDetected = await WaitUntilAsync(() => counter.Count == 2);

		// Assert
		Assert.That(firstDetected, Is.True, "The change that made the subscriber throw was not detected.");
		Assert.That(secondDetected, Is.True, "The change after the subscriber exception was not detected.");
	}

	#endregion

	#region StartStop

	/// <summary>
	/// Checks that an item that is monitored before the monitor has been started is not polled.
	/// </summary>
	[Test]
	[Category("StartStop")]
	public async Task MonitorItem_BeforeStart_DoesNotPoll()
	{
		// Arrange
		var identifier = "Item";
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { identifier, 50 } });
		var counter = new ChangeCounter();
		var item = this.CreateByteItem(identifier: identifier);
		item.ValueChanged += counter.OnChanged;

		// Act
		monitor.MonitorItem(item);
		await recorder.WriteAsync(Data, 1);
		await Task.Delay(500);

		// Assert
		Assert.That(counter.Count, Is.EqualTo(0));
		Assert.That(recorder.ReadCount, Is.EqualTo(0));
	}

	/// <summary>
	/// Checks that an item that is monitored after the monitor has been stopped is not polled until the monitor is started again.
	/// </summary>
	[Test]
	[Category("StartStop")]
	public async Task MonitorItem_AfterStop_DoesNotPoll()
	{
		// Arrange
		var identifier = "Item";
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { identifier, 50 } });
		var counter = new ChangeCounter();
		var item = this.CreateByteItem(identifier: identifier);
		item.ValueChanged += counter.OnChanged;
		monitor.Start();
		monitor.Stop();

		// Act
		monitor.MonitorItem(item);
		await recorder.WriteAsync(Data, 1);
		await Task.Delay(500);
		var countWhileStopped = counter.Count;
		var readsWhileStopped = recorder.ReadCount;
		monitor.Start();
		var detectedAfterStart = await WaitUntilAsync(() => counter.Count == 1);

		// Assert
		Assert.That(countWhileStopped, Is.EqualTo(0));
		Assert.That(readsWhileStopped, Is.EqualTo(0));
		Assert.That(detectedAfterStart, Is.True, "The change was not detected after the monitor has been started again.");
	}

	#endregion

	#region Handlers

	/// <summary>
	/// Checks that polling stops completely once the last monitored item has been unmonitored.
	/// </summary>
	[Test]
	[Category("Handlers")]
	public async Task UnMonitorItem_OfLastItem_StopsReading()
	{
		// Arrange
		var identifier = "Item";
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { identifier, 50 } });
		var item = this.CreateByteItem(identifier: identifier);
		monitor.MonitorItem(item);
		monitor.Start();
		Assert.That(await WaitUntilAsync(() => recorder.ReadCount >= 3), Is.True, "The item was not polled.");

		// Act
		monitor.UnMonitorItem(item);
		await Task.Delay(200); // Grace period for a read that is still in progress.
		var readsAfterGracePeriod = recorder.ReadCount;
		await Task.Delay(500);

		// Assert
		Assert.That(recorder.ReadCount, Is.EqualTo(readsAfterGracePeriod));
	}

	#endregion

	#region Batching

	/// <summary>
	/// Checks that all distinct items sharing the same polling frequency are read within a single call per polling cycle and that all monitored instances receive their values.
	/// </summary>
	[Test]
	[Category("Batching")]
	public async Task DistinctItems_WithSameFrequency_AreReadInOneCall()
	{
		// Arrange
		var interval = 100;
		var window = 1000;
		var items = new List<IPlcItem>();
		var identifiers = new Dictionary<string, uint>();
		for (var offset = 0; offset < 4; offset++)
		{
			items.Add(this.CreateByteItem(offset, $"Byte{offset}"));
			items.Add(new BitPlcItem(Data.Datablock, (ushort) (Data.StartOfModifiableBytes + offset), BitPosition.X0, identifier: $"Bit{offset}_0"));
			items.Add(new BitPlcItem(Data.Datablock, (ushort) (Data.StartOfModifiableBytes + offset), BitPosition.X1, identifier: $"Bit{offset}_1"));
		}
		foreach (var item in items) identifiers.Add(item.Identifier, (uint) interval);
		var counters = items.ToDictionary(item => item, item => new ChangeCounter());
		foreach (var item in items) item.ValueChanged += counters[item].OnChanged;
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, identifiers);
		foreach (var item in items) monitor.MonitorItem(item);

		// Act
		await recorder.WriteAsync(Data, byte.MaxValue, byteOffset: 0);
		monitor.Start();
		await Task.Delay(window);
		monitor.Stop();

		// Assert
		var calls = recorder.Calls;
		Assert.That(calls, Is.Not.Empty);
		Assert.That(calls.Select(call => call.Items.Length), Has.All.EqualTo(items.Count));
		Assert.That(recorder.ReadCount, Is.InRange(window / interval / 2, window / interval + 5), "The number of reads must correspond to the interval and not to interval x item count.");
		Assert.That(counters[items[0]].Count, Is.EqualTo(1), "The byte item did not receive its change.");
		Assert.That(counters[items[1]].Count, Is.EqualTo(1), "The first bit item did not receive its change.");
		Assert.That(counters[items[2]].Count, Is.EqualTo(1), "The second bit item did not receive its change.");
		Assert.That(counters[items[3]].Count, Is.EqualTo(0), "An item of an unchanged byte was notified.");
	}

	/// <summary>
	/// Checks that items with different polling frequencies are read in one call per frequency group and cycle.
	/// </summary>
	[Test]
	[Category("Batching")]
	public async Task DistinctItems_WithDifferentFrequencies_AreReadInOneCallPerFrequency()
	{
		// Arrange
		var window = 1200;
		var fastInterval = 100;
		var slowInterval = 400;
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor
		(
			recorder.Plc,
			new Dictionary<string, uint> { { "Fast0", (uint) fastInterval }, { "Fast1", (uint) fastInterval }, { "Slow0", (uint) slowInterval }, { "Slow1", (uint) slowInterval } }
		);
		monitor.MonitorItem(this.CreateByteItem(0, "Fast0"));
		monitor.MonitorItem(this.CreateByteItem(1, "Fast1"));
		monitor.MonitorItem(this.CreateByteItem(2, "Slow0"));
		monitor.MonitorItem(this.CreateByteItem(3, "Slow1"));

		// Act
		monitor.Start();
		await Task.Delay(window);
		monitor.Stop();

		// Assert
		var calls = recorder.Calls;
		var fastCalls = calls.Count(call => call.Items.Any(item => item.Position == Data.StartOfModifiableBytes));
		var slowCalls = calls.Count(call => call.Items.Any(item => item.Position == Data.StartOfModifiableBytes + 2));
		Assert.That(calls.Select(call => call.Items.Length), Has.All.EqualTo(2), "Every call must contain exactly the two items of one frequency group.");
		Assert.That(fastCalls + slowCalls, Is.EqualTo(calls.Count));
		Assert.That(fastCalls, Is.InRange(window / fastInterval / 2, window / fastInterval + 5));
		Assert.That(slowCalls, Is.InRange(1, window / slowInterval + 2));
	}

	/// <summary>
	/// Checks that a failing subscriber of one item neither affects other items of the same frequency group nor the shared polling.
	/// </summary>
	[Test]
	[Category("Batching")]
	public async Task Polling_SubscriberException_DoesNotAffectOtherItems()
	{
		// Arrange
		var recorder = new ReadRecorder();
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { "Failing", 50 }, { "Healthy", 50 } });
		var failingCounter = new ChangeCounter();
		var healthyCounter = new ChangeCounter();
		var failingItem = this.CreateByteItem(byteOffset: 0, identifier: "Failing");
		var healthyItem = this.CreateByteItem(byteOffset: 1, identifier: "Healthy");
		failingItem.ValueChanged += (sender, args) =>
		{
			failingCounter.OnChanged(sender, args);
			throw new InvalidOperationException("Subscriber failure.");
		};
		healthyItem.ValueChanged += healthyCounter.OnChanged;
		monitor.MonitorItem(failingItem);
		monitor.MonitorItem(healthyItem);
		monitor.Start();

		// Act
		await recorder.WriteAsync(Data, 1, byteOffset: 0);
		var failingDetected = await WaitUntilAsync(() => failingCounter.Count == 1);
		var healthyChanges = 5;
		for (byte i = 1; i <= healthyChanges; i++)
		{
			await recorder.WriteAsync(Data, i, byteOffset: 1);
			await WaitUntilAsync(() => healthyCounter.Count == i);
		}

		// Assert
		Assert.That(failingDetected, Is.True, "The change of the failing item was not detected.");
		Assert.That(healthyCounter.Count, Is.EqualTo(healthyChanges));
	}

	/// <summary>
	/// Checks that items that have been read successfully are still updated if another item of the same read call failed.
	/// </summary>
	[Test]
	[Category("Batching")]
	public async Task Polling_WithPartiallyFailingRead_UpdatesValidItems()
	{
		// Arrange
		var failingPosition = Data.StartOfModifiableBytes;
		var recorder = new ReadRecorder(selectFailingItems: plcItems => plcItems.Where(item => item.Position == failingPosition).ToArray());
		using var monitor = new PollingPlcMonitor(recorder.Plc, new Dictionary<string, uint> { { "Failing", 50 }, { "Healthy", 50 } });
		var failingCounter = new ChangeCounter();
		var healthyCounter = new ChangeCounter();
		var failingItem = this.CreateByteItem(byteOffset: 0, identifier: "Failing");
		var healthyItem = this.CreateByteItem(byteOffset: 1, identifier: "Healthy");
		failingItem.ValueChanged += failingCounter.OnChanged;
		healthyItem.ValueChanged += healthyCounter.OnChanged;
		monitor.MonitorItem(failingItem);
		monitor.MonitorItem(healthyItem);
		await recorder.WriteAsync(Data, 1, byteOffset: 0);
		await recorder.WriteAsync(Data, 1, byteOffset: 1);

		// Act
		monitor.Start();
		var healthyDetected = await WaitUntilAsync(() => healthyCounter.Count == 1);
		await Task.Delay(300);

		// Assert
		Assert.That(healthyDetected, Is.True, "The healthy item was not updated although it has been read successfully.");
		Assert.That(failingCounter.Count, Is.EqualTo(0), "The failing item must not be updated.");
		Assert.That(recorder.ReadCount, Is.GreaterThan(1));
	}

	#endregion

	#endregion

	#endregion

	private class ReferenceComparer : IEqualityComparer<IPlcItem>
	{
		#region Implementation of IEqualityComparer<in object>

		/// <inheritdoc />
		public bool Equals(IPlcItem x, IPlcItem y)
		{
			return Object.ReferenceEquals(x, y);
		}

		/// <inheritdoc />
		public int GetHashCode(IPlcItem obj)
		{
			return obj.GetHashCode();
		}

		#endregion
	}
}