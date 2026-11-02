using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests;

[TestFixture]
[Category("Item Test")]
public sealed class BytePlcItemTest
{
	/// <summary>
	/// The target value for testing.
	/// </summary>
	/// <remarks> The maximum of the next lower numeric data type plus one, so that each byte in the data is filled in a way that the order can be seen. </remarks>
	private static byte TargetValue = byte.MaxValue;

	[Test]
	public void Check_ItemBuilder()
	{
		BytePlcItem item = new PlcItemBuilder()
				.ConstructBytePlcItem("Byte")
				.ForData()
				.AtDatablock(0)
				.AtPosition(4)
				.WithoutInitialValue()
				.Build()
			;
		Assert.AreEqual((uint)1, ((IPlcItem)item).Value.ByteLength);

		System.Diagnostics.Debug.WriteLine(item.ToString());
	}

	[Test]
	public void Check_Clone()
	{
		var item = new BytePlcItem(0, 0, TargetValue);
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
		var targetData = new byte[] {TargetValue};

		// Act
		var item = new BytePlcItem(0, 0, targetValue);

		// Assert: Number → Data
		Assert.That(item.Value, Is.EqualTo(targetValue));
		Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
	}

	[Test]
	public void Check_ToData()
	{
		// Arrange
		var targetValue = TargetValue;
		var targetData = new byte[] { TargetValue };
		var item = new BytePlcItem(0, 0, (ushort) targetData.Length);

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
		var targetData = new byte[] { TargetValue };
		var item = new BytePlcItem(0, 0, (ushort) targetData.Length);

		// Act: Data → Number
		((IPlcItem) item).Value.TransferValuesFrom(targetData);

		// Assert
		Assert.That(item.Value, Is.EqualTo(targetValue));
		Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
	}
}