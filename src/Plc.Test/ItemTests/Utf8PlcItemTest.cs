using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestFixture]
	[Category("Item Test")]
	public sealed class Utf8PlcItemTest
	{
		[Test]
		public void Check_ItemBuilder()
		{
			Utf8PlcItem item = new PlcItemBuilder()
				.ConstructUtf8PlcItem("UTF-8")
				.AtDatablock(0)
				.AtPosition(4)
				.WithLength(10)
				.Build()
				;
			Assert.AreEqual((uint) 10, ((IPlcItem) item).Value.ByteLength);

			System.Diagnostics.Debug.WriteLine(item.ToString());
		}

		[Test]
		public void Check_Clone()
		{
			var item = new Utf8PlcItem(0, 0, "Some test message.");
			var clone = item.Clone();
			
			// Check equality (Equals is overriden).
			Assert.AreEqual(item, clone);

			// Check the value.
			Assert.AreEqual(item.Value,  clone.Value);

			// Check if both items are different references.
			Assert.False(Object.ReferenceEquals(item, clone));
		}

		[Test]
		public void Check_Initial_Data()
		{
			// Arrange
			var targetValue = "Some test message.";
			var targetData = Encoding.UTF8.GetBytes(targetValue);

			// Act
			var item = new Utf8PlcItem(0, 0, targetValue);

			// Assert: Number → Data
			Assert.That(item.Value, Is.EqualTo(targetValue));
			Assert.That((byte[])((IPlcItem)item).Value, Is.EqualTo(targetData));
		}

		[Test]
		public void Check_ToData()
		{
			var target = Guid.NewGuid().ToString();
			var targetData = Encoding.UTF8.GetBytes(target);
			var targetLength = (ushort) targetData.Length;

			var item = new Utf8PlcItem(0, 0, targetLength);

			// Check: String → Data
			item.Value = target;
			Assert.AreEqual(target, item.Value);
			Assert.True(targetData.SequenceEqual((byte[]) ((IPlcItem) item).Value));
		}

		[Test]
		public void Check_FromData()
		{
			var target = Guid.NewGuid().ToString();
			var targetData = Encoding.UTF8.GetBytes(target);
			var targetLength = (ushort)targetData.Length;

			var item = new Utf8PlcItem(0, 0, targetLength);

			// Check: Data → String
			
			((IPlcItem)item).Value.TransferValuesFrom(targetData);
			Assert.AreEqual(target, item.Value);
			Assert.True(targetData.SequenceEqual((byte[])((IPlcItem)item).Value));
		}

		[Test]
		public void Check_Null_Assignment()
		{
			var item = new Utf8PlcItem(0, 0, 0);
			Assert.AreEqual(String.Empty, item.Value);

			item = new Utf8PlcItem(0, 0, null);
			Assert.AreEqual(String.Empty, item.Value);

			var customString = Guid.NewGuid().ToString();
			item = new Utf8PlcItem(0, 0, customString);
			Assert.AreEqual(customString, item.Value);
			var byteLength = ((IPlcItem) item).Value.ByteLength;

			// Set the value to null.
			item.Value = null;
			Assert.AreEqual(String.Empty, item.Value); // The value should now be an empty string.
			Assert.True(((IPlcItem)item).Value.ContainsOnlyZeros); // The underlying BitCollection should contain a zero filled boolean array.
			Assert.AreEqual(byteLength, ((IPlcItem)item).Value.ByteLength); // The size of the underlying BitCollection should not have changed.
		}
	}
}