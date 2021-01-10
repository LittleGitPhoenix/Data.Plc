using System;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestFixture]
	[Category("Item Test")]
	public sealed class Int16PlcItemTest
	{
		/// <summary>
		/// The target value for testing.
		/// </summary>
		private static Int16 TargetValue = Int16.MinValue + 1;

		[Test]
		public void Check_ItemBuilder()
		{
			Int16PlcItem item = new PlcItemBuilder()
				.ConstructInt16PlcItem("Int16")
				.AtDatablock(0)
				.AtPosition(4)
				.WithoutInitialValue()
				.Build()
				;
			Assert.That(((IPlcItem)item).Value.ByteLength, Is.EqualTo(sizeof(Int16)));

			System.Diagnostics.Debug.WriteLine(item.ToString());
		}

		[Test]
		public void Check_Clone()
		{
			// Arrange
			var item = new Int16PlcItem(0, 0, TargetValue);

			// Act
			var clone = item.Clone();

			// Check if both items are different references.
			Assert.False(Object.ReferenceEquals(item, clone));

			// Check equality (Equals is overriden).
			Assert.AreEqual(item, clone);

			// Check the value.
			Assert.AreEqual(item.Value, clone.Value);
		}

		[Test]
		public void Check_Initial_Data()
		{
			// Arrange
			var targetValue = TargetValue;
			var targetData = DataConverter.ToBytes(targetValue, Int16PlcItem.Endianness);

			// Act
			var item = new Int16PlcItem(0, 0, targetValue);

			// Assert: Number → Data
			Assert.That(item.Value, Is.EqualTo(targetValue));
			Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
		}

		[Test]
		public void Check_ToData()
		{
			// Arrange
			var targetValue = TargetValue;
			var targetData = DataConverter.ToBytes(targetValue, Int16PlcItem.Endianness);
			var item = new Int16PlcItem(0, 0);

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
			var targetData = DataConverter.ToBytes(targetValue, Int16PlcItem.Endianness);
			var item = new Int16PlcItem(0, 0);

			// Act: Data → Number
			((IPlcItem)item).Value.TransferValuesFrom(targetData);

			// Assert
			Assert.That(item.Value, Is.EqualTo(targetValue));
			Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
		}
	}
}