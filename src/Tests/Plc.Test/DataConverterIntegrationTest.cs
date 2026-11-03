using System.Diagnostics;

namespace Phoenix.Data.Plc.Test;

[TestFixture]
public class DataConverterIntegrationTest
{
	#region Tests

	/// <summary> Measures the conversion time from booleans to bytes. </summary>
	[Test]
	public void Converter_Booleans_To_Bytes_Performance()
	{
		var random = new Random();
		var iteration = 1;
		var iterations = 100;
		var targetTimePerConversion = 10;
		var stopWatch = new Stopwatch();

		do
		{
			// Create a very large boolean array filled with random data.
			var booleans = new bool[1000000].Select(b => random.NextDouble() >= 0.5).ToArray();

			// Convert it into bytes while measuring the time.
			stopWatch.Start();
			DataConverter.ToBytes(booleans);
			stopWatch.Stop();

		} while (++iteration <= iterations);

		var actualTimePerConversion = (stopWatch.ElapsedMilliseconds / iterations);
		Assert.True(actualTimePerConversion <= targetTimePerConversion, $"The target conversion time should be {targetTimePerConversion}ms but actually was {actualTimePerConversion}ms.");
	}

	/// <summary> Measures the conversion time from bytes to booleans. </summary>
	[Test]
	public void Converter_Bytes_To_Booleans_Performance()
	{
		var random = new Random();
		var iteration = 1;
		var iterations = 200;
		var targetTimePerConversion = 5;
		var stopWatch = new Stopwatch();

		do
		{
			// Create a very large byte array filled with random data.
			var bytes = new byte[100000];
			random.NextBytes(bytes);

			// Convert it into booleans while measuring the time.
			stopWatch.Start();
			DataConverter.ToBooleans(bytes);
			stopWatch.Stop();

		} while (++iteration <= iterations);

		var actualTimePerConversion = (stopWatch.ElapsedMilliseconds / iterations);
		Assert.True(actualTimePerConversion <= targetTimePerConversion, $"The target conversion time should be {targetTimePerConversion}ms but actually was {actualTimePerConversion}ms.");
	}

	/// <summary> Compares the execution time of the current and obsolete byte-swapping implementations. </summary>
	[Test]
	[Category("Endianness Conversion")]
	public void Measure_Swap_Performance()
	{
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		for (ulong i = 0; i < 30000000; i++)
		{
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			DataConverter.SwapBytes_old(i);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
		stopWatch.Stop();
		var oldTime = stopWatch.ElapsedTicks;

		stopWatch.Restart();
		for (ulong i = 0; i < 30000000; i++)
		{
			DataConverter.SwapBytes(i);
		}
		stopWatch.Stop();
		var newTime = stopWatch.ElapsedTicks;

		Assert.IsTrue(newTime < oldTime, $"New implementation should be faster, but actually took {newTime} ticks while the old implementation took {oldTime} ticks.");
	}

	#endregion
}