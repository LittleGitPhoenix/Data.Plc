using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Phoenix.Data.Plc.Test
{
	[TestClass]
	public class DataConverterTest : DataTest
	{
		[TestMethod]
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

		[TestMethod]
		public void Converter_Booleans_To_Byte_Throws()
		{
			var booleans = base.Booleans.Take(9).ToArray();
			var targetByte = base.Bytes[0];

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => DataConverter.ToByte(booleans));
		}
		
		[TestMethod]
		public void Converter_Booleans_To_Bytes()
		{
			var actualBytes = DataConverter.ToBytes(base.Booleans);

			Assert.IsTrue(base.Bytes.Length * 8 == base.Booleans.Length);
			Assert.IsTrue(base.Bytes.SequenceEqual(actualBytes));
		}

		[TestMethod]
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
			Assert.IsTrue(actualTimePerConversion <= targetTimePerConversion, $"The target conversion time should be {targetTimePerConversion}ms but actually was {actualTimePerConversion}ms.");
		}

		[TestMethod]
		public void Converter_Byte_To_Booleans()
		{
			for (int index = 0; index < base.Bytes.Length; index++)
			{
				var @byte = base.Bytes[index];
				var targetBooleans = base.Booleans.Skip(index*8).Take(8).ToArray();
				var actualBooleans = DataConverter.ToBooleans(@byte);

				Assert.IsTrue(actualBooleans.Length == 8);
				Assert.IsTrue(targetBooleans.SequenceEqual(actualBooleans));
			}
		}

		[TestMethod]
		public void Converter_Bytes_To_Booleans()
		{
			var actualBooleans = DataConverter.ToBooleans(base.Bytes);
			
			Assert.IsTrue(actualBooleans.Length == base.Bytes.Length * 8);
			Assert.IsTrue(base.Booleans.SequenceEqual(actualBooleans));
		}

		[TestMethod]
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
			Assert.IsTrue(actualTimePerConversion <= targetTimePerConversion, $"The target conversion time should be {targetTimePerConversion}ms but actually was {actualTimePerConversion}ms.");
		}
	}
}