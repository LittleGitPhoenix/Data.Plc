using System.Diagnostics;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Implementation.Test;

public abstract class ImplementationMemoryTest<TPlc> : ImplementationTest<TPlc>
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

	protected ImplementationMemoryTest(TPlc plc) : base(_ => plc) { }

	protected ImplementationMemoryTest(Func<Data.Plc.Test.Data, TPlc> plcFactory) : base(plcFactory) { }

	#endregion

	#region Methods

	#region Tests

	/// <summary>
	/// Checks that the <see cref="CancellationTokenSource"/> that is created while reading/writing is properly disposed.
	/// </summary>
	protected async Task CheckMemoryUsage(int iterations, int warmupIterations)
	{
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

		GC.Collect();
		GC.WaitForPendingFinalizers();

		var initialMemoryUsage = 0d;
		var finalMemoryUsage = 0d;

		using (var process = Process.GetCurrentProcess())
		{
			initialMemoryUsage = process.PrivateMemorySize64 / (double)(1024 * 1024);
			Console.WriteLine($"Initial memory usage: {initialMemoryUsage} MByte");
		}

		// Act
		for (var iteration = 1; iteration <= iterations; iteration++)
		{
			await plc.ReadItemAsync(byteItem, CancellationToken.None);
			if (iteration % 500 == 0) await Task.Delay(250, CancellationToken.None);
		}

		GC.Collect();
		GC.WaitForPendingFinalizers();

		using (var process = Process.GetCurrentProcess())
		{
			finalMemoryUsage = process.PrivateMemorySize64 / (double)(1024 * 1024);
			Console.WriteLine($"Final memory usage: {finalMemoryUsage} MByte");
		}

		// Assert
		var difference = finalMemoryUsage - initialMemoryUsage;
		Console.WriteLine($"Memory increased by: {difference} MByte");

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
		Assert.That(difference, Is.Not.GreaterThan(15)); //! Both 100.000 and 1.000.000 iterations proofed to not allocate more than 13 MByte.
//#endif
	}

	#endregion

	#endregion
}