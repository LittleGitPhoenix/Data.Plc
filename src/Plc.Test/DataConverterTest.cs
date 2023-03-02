using System.Diagnostics;
using MoreLinq;

namespace Phoenix.Data.Plc.Test;

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

	#region Value ⇒ Data conversion

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase((short)((byte.MaxValue + 1) * -1), DataConverter.Endianness.LittleEndian, new byte[] { 0, 255 })]
	[TestCase((short)((byte.MaxValue + 1) * -1), DataConverter.Endianness.BigEndian, new byte[] { 255, 0 })]
	public void Check_Int16_To_Data_Conversion(short value, DataConverter.Endianness endianness, byte[] target)
	{
		// Act
		var actual = DataConverter.ToBytes(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase((int)((ushort.MaxValue + 1) * -1), DataConverter.Endianness.LittleEndian, new byte[] { 0, 0, 255, 255 })]
	[TestCase((int)((ushort.MaxValue + 1) * -1), DataConverter.Endianness.BigEndian, new byte[] { 255, 255, 0, 0 })]
	public void Check_Int32_To_Data_Conversion(int value, DataConverter.Endianness endianness, byte[] target)
	{
		// Act
		var actual = DataConverter.ToBytes(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase(((long)uint.MaxValue + 1) * -1, DataConverter.Endianness.LittleEndian, new byte[] { 0, 0, 0, 0, 255, 255, 255, 255 })]
	[TestCase(((long)uint.MaxValue + 1) * -1, DataConverter.Endianness.BigEndian, new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 })]
	public void Check_Int64_To_Data_Conversion(long value, DataConverter.Endianness endianness, byte[] target)
	{
		// Act
		var actual = DataConverter.ToBytes(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase((short)((byte.MaxValue + 1) * -1), DataConverter.Endianness.LittleEndian, new byte[] { 0, 255 })]
	[TestCase((short)((byte.MaxValue + 1) * -1), DataConverter.Endianness.BigEndian, new byte[] { 255, 0 })]
	public void Check_UInt16_To_Data_Conversion(short value, DataConverter.Endianness endianness, byte[] target)
	{
		// Act
		var actual = DataConverter.ToBytes(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase((uint)ushort.MaxValue, DataConverter.Endianness.LittleEndian, new byte[] { 255, 255, 0, 0 })]
	[TestCase((uint)ushort.MaxValue, DataConverter.Endianness.BigEndian, new byte[] { 0, 0, 255, 255 })]
	public void Check_UInt32_To_Data_Conversion(uint value, DataConverter.Endianness endianness, byte[] target)
	{
		// Act
		var actual = DataConverter.ToBytes(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase((ulong)uint.MaxValue, DataConverter.Endianness.LittleEndian, new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 })]
	[TestCase((ulong)uint.MaxValue, DataConverter.Endianness.BigEndian, new byte[] { 0, 0, 0, 0, 255, 255, 255, 255 })]
	public void Check_UInt64_To_Data_Conversion(ulong value, DataConverter.Endianness endianness, byte[] target)
	{
		// Act
		var actual = DataConverter.ToBytes(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Value ⇒ Data conversion")]
	[TestCase("255,255,255,255,0,0,0,0", new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 })]
	[TestCase("255; 255; 255; 255; 0; 0; 0; 0", new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 })]
	[TestCase("FFFFFFFF00000000", new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 })]
	[TestCase("FF, FF, FF, FF, 00, 00, 00, 00", new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 })]
	public void Check_String_To_Data_Conversion(string value, byte[] target)
	{
		// Act
		var result = DataConverter.TryGetBytesFromString(value, out var data);

		// Assert
		Assert.True(result);
		Assert.AreEqual(data, target);
	}

	#endregion

	#region Data ⇒ Value conversion

	[Test]
	[Category("Data ⇒ Value conversion")]
	[TestCase(new byte[] { 0, 255 }, DataConverter.Endianness.LittleEndian, (short)((byte.MaxValue + 1) * -1))]
	[TestCase(new byte[] { 255, 0 }, DataConverter.Endianness.BigEndian, (short)((byte.MaxValue + 1) * -1))]
	public void Check_Data_To_Int16_Conversion(byte[] value, DataConverter.Endianness endianness, short target)
	{
		// Act
		var actual = DataConverter.ToInt16(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Data ⇒ Value conversion")]
	[TestCase(new byte[] { 0, 0, 255, 255 }, DataConverter.Endianness.LittleEndian, (int)((ushort.MaxValue + 1) * -1))]
	[TestCase(new byte[] { 255, 255, 0, 0 }, DataConverter.Endianness.BigEndian, (int)((ushort.MaxValue + 1) * -1))]
	public void Check_Data_To_Int32_Conversion(byte[] value, DataConverter.Endianness endianness, int target)
	{
		// Act
		var actual = DataConverter.ToInt32(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Data ⇒ Value conversion")]
	[TestCase(new byte[] { 0, 0, 0, 0, 255, 255, 255, 255 }, DataConverter.Endianness.LittleEndian, ((long)uint.MaxValue + 1) * -1)]
	[TestCase(new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 }, DataConverter.Endianness.BigEndian, ((long)uint.MaxValue + 1) * -1)]
	public void Check_Data_To_Int64_Conversion(byte[] value, DataConverter.Endianness endianness, long target)
	{
		// Act
		var actual = DataConverter.ToInt64(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Data ⇒ Value conversion")]
	[TestCase(new byte[] { 255, 0 }, DataConverter.Endianness.LittleEndian, (ushort) byte.MaxValue)]
	[TestCase(new byte[] { 0, 255 }, DataConverter.Endianness.BigEndian, (ushort) byte.MaxValue)]
	public void Check_Data_To_UInt16_Conversion(byte[] value, DataConverter.Endianness endianness, ushort target)
	{
		// Act
		var actual = DataConverter.ToUInt16(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Data ⇒ Value conversion")]
	[TestCase(new byte[] { 255, 255, 0, 0 }, DataConverter.Endianness.LittleEndian, (uint) ushort.MaxValue)]
	[TestCase(new byte[] { 0, 0, 255, 255 }, DataConverter.Endianness.BigEndian, (uint) ushort.MaxValue)]
	public void Check_Data_To_UInt32_Conversion(byte[] value, DataConverter.Endianness endianness, uint target)
	{
		// Act
		var actual = DataConverter.ToUInt32(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	[Test]
	[Category("Data ⇒ Value conversion")]
	[TestCase(new byte[] { 255, 255, 255, 255, 0, 0, 0, 0 }, DataConverter.Endianness.LittleEndian, (ulong) uint.MaxValue)]
	[TestCase(new byte[] { 0, 0, 0, 0, 255, 255, 255, 255 }, DataConverter.Endianness.BigEndian, (ulong) uint.MaxValue)]
	public void Check_Data_To_UInt64_Conversion(byte[] value, DataConverter.Endianness endianness, ulong target)
	{
		// Act
		var actual = DataConverter.ToUInt64(value, endianness);

		// Assert
		Assert.AreEqual(actual, target);
	}

	#endregion

	#region Endianness Conversion

	[Test]
	[Category("Endianness Conversion")]
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

	[Test]
	[Category("Endianness Conversion")]
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

	[Test]
	[Category("Endianness Conversion")]
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

	[Test]
	[Category("Endianness Conversion")]
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

	[Test]
	[Category("Endianness Conversion")]
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

	[Test]
	[Category("Endianness Conversion")]
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

		Assert.IsTrue(newTime < oldTime);
	}

	#endregion

	#region Helper
		
	[Test]
	[Category("Helper")]
	[TestCase("", ' ')]
	[TestCase("#", ' ')]
	[TestCase("0x", ' ')]
	[TestCase("", ',')]
	[TestCase("#", ',')]
	[TestCase("0x", ',')]
	[TestCase("", ';')]
	[TestCase("#", ';')]
	[TestCase("0x", ';')]
	[TestCase("", '-')]
	[TestCase("#", '-')]
	[TestCase("0x", '-')]
	public void Check_Hex_RegEx_Succeeds(string prefix, char separator)
	{
		// Arrange
		var bytes = Enumerable.Range(byte.MinValue, byte.MaxValue).Select(number => (byte)number).ToArray();
		var hexArray = BitConverter.ToString(bytes).Split('-').ToArray();
		var value = prefix + String.Join(separator.ToString(), hexArray);

		// Act
		var match = DataConverter.HexRegEx.Match(value);

		// Assert
		Assert.True(match.Success);
		Assert.True(match.Groups["HexString"].Success);
		var captures = match.Groups["HexString"].Captures;
		Assert.That(captures, Has.Count.EqualTo(hexArray.Length));
		for (int index = 0; index < captures.Count; index++)
		{
			Assert.That(captures[index].Value, Is.EqualTo(hexArray[index]), $"Sequences mismatch at index {index}.");
		}
	}
	[Test]
	[Category("Helper")]
	[TestCase("0x00FFGG")]
	[TestCase("Hex:00FF25")]
	[TestCase("Test")]
	public void Check_Hex_RegEx_Fails(string hexString)
	{
		// Act
		var match = DataConverter.HexRegEx.Match(hexString);
			
		// Assert
		Assert.False(match.Success);
		Assert.False(match.Groups["HexString"].Success);
	}
		
	[Test]
	[Category("Helper")]
	[TestCase(',')]
	[TestCase(';')]
	[TestCase('-')]
	public void Check_Bytes_RegEx_Succeeds(char separator)
	{
		// Arrange
		var target = Enumerable.Range(byte.MinValue, byte.MaxValue).Select(number => (byte)number).ToArray();
		var value = String.Join(separator.ToString(), target);

		// Act
		var match = DataConverter.BytesRegEx.Match(value);

		// Assert
		Assert.True(match.Success);
		Assert.True(match.Groups["ByteString"].Success);
		var captures = match.Groups["ByteString"].Captures;
		Assert.That(captures, Has.Count.EqualTo(target.Length));
		for (int index = 0; index < captures.Count; index++)
		{
			Assert.That(captures[index].Value, Is.EqualTo(target[index].ToString()), $"Sequences mismatch at index {index}.");
		}
	}

	[Test]
	[Category("Helper")]
	public void Check_Bytes_RegEx_Fails_Single_Values()
	{
		void Execute(string value)
		{
			// Act
			var match = DataConverter.BytesRegEx.Match(value);

			// Assert
			Assert.False(match.Success);
			Assert.False(match.Groups["ByteString"].Success);
		}

		// Arrange
		Enumerable
			.Range(-999, 999)
			.ForEach(number => Execute(number.ToString()))
			;

		Enumerable
			.Range(byte.MaxValue + 1, 999)
			.ForEach(number => Execute(number.ToString()))
			;
	}

	[Test]
	[TestCase("0, 255, 256, 255")]
	[TestCase("0#255")]
	[Category("Helper")]
	public void Check_Bytes_RegEx_Fails_Multiple_Values(string byteString)
	{
		// Act
		var match = DataConverter.BytesRegEx.Match(byteString);

		// Assert
		Assert.False(match.Success);
		Assert.False(match.Groups["ByteString"].Success);
	}

	#endregion
}