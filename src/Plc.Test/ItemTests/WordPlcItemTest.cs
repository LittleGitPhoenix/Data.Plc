using System;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestFixture]
	[Category("Item Test")]
	public sealed class WordPlcItemTest
	{
		/// <summary>
		/// The target value for testing.
		/// </summary>
		/// <remarks> The maximum of the next lower numeric data type plus one, so that each byte in the data is filled in a way that the order can be seen. </remarks>
		private static UInt16 TargetValue = (UInt16) (byte.MaxValue + 1);

		[Test]
		public void Check_ItemBuilder()
		{
			WordPlcItem item = new PlcItemBuilder()
				.ConstructWordPlcItem("Word")
				.AtDatablock(0)
				.AtPosition(4)
				.WithoutInitialValue()
				.Build()
				;
			Assert.That(((IPlcItem)item).Value.ByteLength, Is.EqualTo(sizeof(UInt16)));

			System.Diagnostics.Debug.WriteLine(item.ToString());
		}

		[Test]
		public void Check_Clone()
		{
			// Arrange
			var item = new WordPlcItem(0, 0, TargetValue);
			
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
			var targetData = DataConverter.ToBytes(targetValue, WordPlcItem.Endianness);

			// Act
			var item = new WordPlcItem(0, 0, targetValue);

			// Assert: Number → Data
			Assert.That(item.Value, Is.EqualTo(targetValue));
			Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
		}

		[Test]
		public void Check_ToData()
		{
			// Arrange
			var targetValue = TargetValue;
			var targetData = DataConverter.ToBytes(targetValue, WordPlcItem.Endianness);
			var item = new WordPlcItem(0, 0);

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
			var targetData = DataConverter.ToBytes(targetValue, WordPlcItem.Endianness);
			var item = new WordPlcItem(0, 0);
			
			// Act: Data → Number
			((IPlcItem)item).Value.TransferValuesFrom(targetData);

			// Assert
			Assert.That(item.Value, Is.EqualTo(targetValue));
			Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
		}
	}
}