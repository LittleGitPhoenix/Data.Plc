using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests;

[TestFixture]
[Category("Item Test")]
public sealed class BytesPlcItemTest
{
	/// <summary>
	/// The target value for testing.
	/// </summary>
	/// <remarks> The maximum of the next lower numeric data type plus one, so that each byte in the data is filled in a way that the order can be seen. </remarks>
	private static byte[] TargetValue = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0};

	[Test]
	public void Check_ItemBuilder()
	{
		BytesPlcItem item = new PlcItemBuilder()
				.ConstructBytesPlcItem("Bytes")
				.ForData()
				.AtDatablock(0)
				.AtPosition(4)
				.ForByteAmount(10)	 
				.Build()
			;
		Assert.AreEqual((uint)10, ((IPlcItem)item).Value.ByteLength);

		System.Diagnostics.Debug.WriteLine(item.ToString());
	}

	[Test]
	public void Check_Clone()
	{
		var item = new BytesPlcItem(0, 0, TargetValue);
		var clone = item.Clone();

		// Check equality (Equals is overriden).
		Assert.AreEqual(item, clone);

		// Check the value.
		Assert.AreEqual(item.Value, clone.Value);

		// Check if both items are different references.
		Assert.False(Object.ReferenceEquals(item, clone));
	}

	[Test]
	public void Check_Initial_Data()
	{
		// Arrange
		var targetValue = TargetValue;
		var targetData = TargetValue;

		// Act
		var item = new BytesPlcItem(0, 0, targetValue);

		// Assert: Number → Data
		Assert.That(item.Value, Is.EqualTo(targetValue));
		Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
	}

	[Test]
	public void Check_ToData()
	{
		// Arrange
		var targetValue = TargetValue;
		var targetData = TargetValue;
		var item = new BytesPlcItem(0, 0, (ushort) targetData.Length);

		// Act: Number → Data
		item.Value = targetValue;

		// Assert
		Assert.That(item.Value, Is.EqualTo(targetValue));
		Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
	}

	[Test]
	public void Check_FromData()
	{
		// Arrange
		var targetValue = TargetValue;
		var targetData = TargetValue;
		var item = new BytesPlcItem(0, 0, (ushort) targetData.Length);

		// Act: Data → Number
		((IPlcItem)item).Value.TransferValuesFrom(targetData);

		// Assert
		Assert.That(item.Value, Is.EqualTo(targetValue));
		Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
	}
}