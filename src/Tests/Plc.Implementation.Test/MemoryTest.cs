using System.Diagnostics;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Implementation.Test;

public abstract class MemoryTest<TPlc> : PlcTest<TPlc>
	where TPlc : IPlc
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	protected MemoryTest(TPlc plc) : base(_ => plc) { }

	protected MemoryTest(Func<Data.Plc.Test.Data, TPlc> plcFactory) : base(plcFactory) { }

	#endregion

	#region Methods

	#region Tests

	/// <summary>
	/// Checks that the <see cref="CancellationTokenSource"/> that is created while reading/writing is properly disposed.
	/// </summary>
	protected async Task CheckMemoryUsage(int iterations, int warmupIterations)
	{
		// Prevent log overflow by temporarily disabling logging of all read and write operations.
		var previosuState = Phoenix.Data.Plc.Logging.LogManager.LogAllReadAndWriteOperations;
		Phoenix.Data.Plc.Logging.LogManager.LogAllReadAndWriteOperations = false;

		// Arrange
		var byteItem = new BytePlcItem(dataBlock: 1, position: 0, initialValue: byte.MaxValue);
		var plc = this.Plc;
		plc.Connect();

		// Perform some warm-up
		for (var iteration = 1; iteration <= warmupIterations; iteration++)
		{
			await plc.ReadItemAsync(byteItem, CancellationToken.None);
			if (iteration % 100 == 0) await Task.Delay(250, CancellationToken.None);
		}

		var initialManagedMemoryUsage = GC.GetTotalMemory(forceFullCollection: true) / (double) (1024 * 1024);
		var initialPrivateMemoryUsage = 0d;

		using (var process = Process.GetCurrentProcess())
		{
			initialPrivateMemoryUsage = process.PrivateMemorySize64 / (double) (1024 * 1024);
		}
		Console.WriteLine($"Initial live managed memory: {initialManagedMemoryUsage} MByte");
		Console.WriteLine($"Initial process private memory: {initialPrivateMemoryUsage} MByte");

		// Act
		for (var iteration = 1; iteration <= iterations; iteration++)
		{
			await plc.ReadItemAsync(byteItem, CancellationToken.None);
			if (iteration % 500 == 0) await Task.Delay(250, CancellationToken.None);
		}

		var finalManagedMemoryUsage = GC.GetTotalMemory(forceFullCollection: true) / (double) (1024 * 1024);
		var finalPrivateMemoryUsage = 0d;

		using (var process = Process.GetCurrentProcess())
		{
			finalPrivateMemoryUsage = process.PrivateMemorySize64 / (double) (1024 * 1024);
		}
		Console.WriteLine($"Final live managed memory: {finalManagedMemoryUsage} MByte");
		Console.WriteLine($"Final process private memory: {finalPrivateMemoryUsage} MByte");

		// Assert
		var managedMemoryDifference = finalManagedMemoryUsage - initialManagedMemoryUsage;
		var privateMemoryDifference = finalPrivateMemoryUsage - initialPrivateMemoryUsage;
		Console.WriteLine($"Live managed memory increased by: {managedMemoryDifference} MByte");
		Console.WriteLine($"Process private memory increased by: {privateMemoryDifference} MByte");

//#if NETFRAMEWORK
//			if (difference <= 15)
//			{
//				Assert.Pass();
//			}
//			else
//			{
//				Assert.Inconclusive("When running in .NET Framework 4.5 environment, the memory usage seems not predictable. Tests with 100.000 iterations repetitively showed, that the memory consumption spikes from 180 MByte after warmup to 620 MByte in a linear fashion but then immediately slows down. The final value is then around 635 MByte. Manual memory snapshots showed no significant increase in allocated objects however.");
//			}
//#else
		Assert.That(managedMemoryDifference, Is.Not.GreaterThan(15)); //! Both 100.000 and 1.000.000 iterations proofed to not allocate more than 13 MByte.
//#endif

		Phoenix.Data.Plc.Logging.LogManager.LogAllReadAndWriteOperations = previosuState;
	}

	#endregion

	#endregion
}