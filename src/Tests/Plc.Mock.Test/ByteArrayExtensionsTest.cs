using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
class ByteArrayExtensionsTest
{
	[Test]
	public void Check_Applying_Bool_Array()
	{
		// Arrange
		var targetData = new byte[4] { 0, 0, 22, 1 };

		// Act
		var actualData = new byte[4].ApplyValue(2, BitPosition.X0, new bool[9] { false, true, true, false, true, false, false, false, true });

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_Bool_Array_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new byte[1].ApplyValue(2, BitPosition.X0, new bool[0] { }));
	}

	[Test]
	public void Check_Applying_Byte()
	{
		// Arrange
		var targetData = new byte[4] { 0, 0, 22, 0 };

		// Act
		var actualData = new byte[4].ApplyValue(2, (byte)22);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_Byte_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new byte[1].ApplyValue(2, byte.MinValue));
	}

	[Test]
	public void Check_Applying_Bytes()
	{
		// Arrange
		var targetData = new byte[4] { 0, 0, 22, 1 };

		// Act
		var actualData = new byte[4].ApplyValue(2, new byte[2] { 22, 1 });

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_Bytes_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => new byte[1].ApplyValue(2, new byte[0] { }));
	}

	[Test]
	public void Check_Applying_Int16()
	{
		// Arrange
		var targetData = new byte[4] { 0, 0, 0, 128 };

		// Act
		var actualData = new byte[4].ApplyValue(2, Int16.MinValue, DataConverter.Endianness.LittleEndian);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_UInt16()
	{
		// Arrange
		var targetData = new byte[4] { 0, 0, 255, 255 };

		// Act
		var actualData = new byte[4].ApplyValue(2, UInt16.MaxValue, DataConverter.Endianness.LittleEndian);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_Int32()
	{
		// Arrange
		var targetData = new byte[4] { 0, 0, 0, 128 };

		// Act
		var actualData = new byte[4].ApplyValue(0, Int32.MinValue, DataConverter.Endianness.LittleEndian);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_UInt32()
	{
		// Arrange
		var targetData = new byte[4] { 255, 255, 255, 255 };

		// Act
		var actualData = new byte[4].ApplyValue(0, UInt32.MaxValue, DataConverter.Endianness.LittleEndian);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_Int64()
	{
		// Arrange
		var tt = DataConverter.ToBytes(Int64.MinValue, DataConverter.Endianness.LittleEndian);
		var targetData = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 128 };

		// Act
		var actualData = new byte[8].ApplyValue(0, Int64.MinValue, DataConverter.Endianness.LittleEndian);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_UInt64()
	{
		// Arrange
		var targetData = new byte[8] { 255, 255, 255, 255, 255, 255, 255, 255 };

		// Act
		var actualData = new byte[8].ApplyValue(0, UInt64.MaxValue, DataConverter.Endianness.LittleEndian);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}

	[Test]
	public void Check_Applying_String()
	{
		// Arrange
		var targetData = new byte[8] { 0, 0, 84, 101, 115, 116, 0, 0 };

		// Act
		var actualData = new byte[8].ApplyValue(2, "Test", System.Text.Encoding.ASCII);

		// Assert
		Assert.AreEqual(targetData, actualData);
	}
}