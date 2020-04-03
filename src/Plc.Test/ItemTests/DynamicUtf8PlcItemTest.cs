using System;
using System.Linq;
using System.Text;
using Phoenix.Data.Plc.Items.Builder;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestFixture]
	[Category("Item Test")]
	public sealed class DynamicUtf8PlcItemTest
	{
		[Test]
		public void Check_ItemBuilder()
		{
			// With predefined length-item.
			DynamicUtf8PlcItem item = new Items.Builder.PlcItemBuilder()
				.ConstructUtf8PlcItem("UTF8")
				.AtDatablock(0)
				.AtPosition(0)
				.WithDynamicItem<UInt32PlcItem>()
				.BuildDynamic()
				;
			System.Diagnostics.Debug.WriteLine(item.ToString());
			Assert.True(item.LengthPlcItem is UInt32PlcItem);
			Assert.AreEqual(default(UInt32), ((UInt32PlcItem) item.LengthPlcItem).Value);
			Assert.IsNotNull(item.Value);
			Assert.AreEqual(String.Empty, item.Value);

			// With initial value.
			var initialValue = Guid.NewGuid().ToString();
			item = new Items.Builder.PlcItemBuilder()
				.ConstructUtf8PlcItem("UTF8")
				.AtDatablock(0)
				.AtPosition(0)
				.WithDynamicItemFromInitialValue(initialValue)
				.BuildDynamic()
				;
			System.Diagnostics.Debug.WriteLine(item.ToString());
			Assert.True(item.LengthPlcItem is BytePlcItem);
			Assert.AreEqual(Encoding.UTF8.GetBytes(initialValue).Length, ((BytePlcItem) item.LengthPlcItem).Value);
			Assert.AreEqual(initialValue, item.Value);
		}

		[Test]
		public void Check_Clone()
		{
			var item = new DynamicUtf8PlcItem(new UInt16PlcItem(dataBlock: 0, position: 0), "DynamicText");
			var clone = item.Clone();

			// Check equality (Equals is overriden).
			Assert.AreEqual(item, clone);

			// Check the value.
			Assert.True(item.Value == clone.Value);

			// Check if both items are different references.
			Assert.False(Object.ReferenceEquals(item, clone));
		}

		[Test]
		public void Check_ToData()
		{
			var target = "Message";
			var targetData = Encoding.UTF8.GetBytes(target);

			var item = new DynamicUtf8PlcItem(new UInt16PlcItem(dataBlock: 0, position: 0), "DynamicText");
			Assert.AreEqual(0, ((UInt16PlcItem)item.LengthPlcItem).Value);
			Assert.AreEqual(String.Empty, item.Value);

			// Check: Bytes → Data
			item.Value = target;
			Assert.AreEqual(target, item.Value);
			Assert.True(targetData.SequenceEqual((byte[])((IPlcItem)item).Value));
			Assert.AreEqual(targetData.Length, ((UInt16PlcItem) item.LengthPlcItem).Value);
		}

		[Test]
		public void Check_FromData()
		{
			var target = "Message";
			var targetData = Encoding.UTF8.GetBytes(target);
			
			var item = new DynamicUtf8PlcItem(new UInt16PlcItem(dataBlock: 0, position: 0), "DynamicText");
			Assert.AreEqual(0, ((UInt16PlcItem)item.LengthPlcItem).Value);
			Assert.AreEqual(String.Empty, item.Value);

			// Check: Data → Bytes
			((IPlcItem) item).Value.TransferValuesFrom(targetData);
			Assert.AreEqual(target, item.Value);
			Assert.True(targetData.SequenceEqual((byte[]) ((IPlcItem) item).Value));
			Assert.AreEqual(targetData.Length, ((UInt16PlcItem)item.LengthPlcItem).Value);
		}
	}
}