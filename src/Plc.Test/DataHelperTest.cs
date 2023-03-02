namespace Phoenix.Data.Plc.Test;

[TestFixture]
public class DataHelperTest : DataTest
{
	[Test]
	public void Helper_Is_Bit_In_Byte_Set()
	{
		for (int index = 0; index < base.Bytes.Length; index++)
		{
			var @byte = base.Bytes[index];

			for (byte bitIndex = 0; bitIndex < 8; bitIndex++)
			{
				var targetBoolean = base.Booleans[index * 8 + bitIndex];
				var actualBoolean = DataHelper.IsBitSet(@byte, bitIndex);

				Assert.AreEqual(targetBoolean, actualBoolean);
			}
		}
	}
		
	[Test]
	public void Helper_Is_Bit_In_Byte_Set_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => DataHelper.IsBitSet(0, 9));
	}

	[Test]
	public void Helper_Is_Bit_In_Bytes_Set()
	{
		for (int index = 0; index < base.Bytes.Length; index += 8)
		{
			var bytes = base.Bytes.Skip(index).Take(8).ToArray();

			for (byte bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex++)
			{
				var targetBoolean = base.Booleans[index * 8 + bitIndex];
				var actualBoolean = DataHelper.IsBitSet(bytes, bytePosition: (ushort)(bitIndex / 8), bitPosition: (byte) (bitIndex % 8));

				Assert.AreEqual(targetBoolean, actualBoolean);
			}
		}
	}

	[Test]
	public void Helper_Is_Bit_In_Bytes_Set_Throws()
	{
		Assert.Throws<ArgumentOutOfRangeException>(() => DataHelper.IsBitSet(new byte[0], 1, 0));
	}
}