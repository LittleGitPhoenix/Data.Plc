using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Phoenix.Data.Plc.Test
{
	[TestFixture]
	public class DataConverterTest : DataTest
	{
		[Test]
		public void Converter_Booleans_To_Byte()
		{
			for (int index = 0; index < base.Booleans.Length; index += 8)
			{
				var booleans = base.Booleans.Skip(index).Take(8).ToArray();
				var targetByte = index / 8;
				var actualByte = DataConverter.ToByte(booleans);
								
				Assert.AreEqual(targetByte, actualByte);
			}
		}

		[Test]
		public void Converter_Booleans_To_Byte_Throws()
		{
			var booleans = base.Booleans.Take(9).ToArray();
			var targetByte = base.Bytes[0];

			Assert.Throws<ArgumentOutOfRangeException>(() => DataConverter.ToByte(booleans));
		}
		
		[Test]
		public void Converter_Booleans_To_Bytes()
		{
			var actualBytes = DataConverter.ToBytes(base.Booleans);

			Assert.True(base.Bytes.Length * 8 == base.Booleans.Length);
			Assert.True(base.Bytes.SequenceEqual(actualBytes));
		}

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

		[Test]
		public void Converter_Byte_To_Booleans()
		{
			for (int index = 0; index < base.Bytes.Length; index++)
			{
				var @byte = base.Bytes[index];
				var targetBooleans = base.Booleans.Skip(index*8).Take(8).ToArray();
				var actualBooleans = DataConverter.ToBooleans(@byte);

				Assert.True(actualBooleans.Length == 8);
				Assert.True(targetBooleans.SequenceEqual(actualBooleans));
			}
		}

		[Test]
		public void Converter_Bytes_To_Booleans()
		{
			var actualBooleans = DataConverter.ToBooleans(base.Bytes);
			
			Assert.True(actualBooleans.Length == base.Bytes.Length * 8);
			Assert.True(base.Booleans.SequenceEqual(actualBooleans));
		}

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

		[TestMethod]
		public void Check_Data_To_UInt16_Conversion()
		{
			// Arrange
			var value = new byte[] {255, 0};
			var target = (ushort) byte.MaxValue;

			// Act
			var actual = DataConverter.ToUInt16(value, DataConverter.Endianness.LittleEndian);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Swap_Short()
		{
			// Arrange
			var value = (short) byte.MaxValue;
			var target = (short) ((byte.MaxValue + 1) * -1);

			// Act
			var actual = DataConverter.SwapBytes(value);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Swap_Int()
		{
			// Arrange
			var value = (int) ushort.MaxValue;
			var target = (int) ((ushort.MaxValue + 1) * -1);

			// Act
			var actual = DataConverter.SwapBytes(value);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Swap_Long()
		{
			// Arrange
			var value = (long) uint.MaxValue;
			var target = ((long) uint.MaxValue + 1) * -1;

			// Act
			var actual = DataConverter.SwapBytes(value);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Swap_UShort()
		{
			// Arrange
			var value = (ushort) byte.MaxValue;
			var target = ushort.MaxValue - byte.MaxValue;

			// Act
			var actual = DataConverter.SwapBytes(value);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Swap_UInt()
		{
			// Arrange
			var value = (uint) ushort.MaxValue;
			var target = uint.MaxValue - ushort.MaxValue;

			// Act
			var actual = DataConverter.SwapBytes(value);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Swap_ULong()
		{
			// Arrange
			var value = (ulong) uint.MaxValue;
			var target = ulong.MaxValue - uint.MaxValue;

			// Act
			var actual = DataConverter.SwapBytes(value);

			// Assert
			Assert.AreEqual(actual, target);
		}

		[TestMethod]
		public void Measure_Swap_Performance()
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			for (ulong i = 0; i < 30000000; i++)
			{
#pragma warning disable CS0612 // Type or member is obsolete
				DataConverter.SwapBytes_old(i);
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

			Assert.IsTrue(newTime < oldTime);
		}
	}
}