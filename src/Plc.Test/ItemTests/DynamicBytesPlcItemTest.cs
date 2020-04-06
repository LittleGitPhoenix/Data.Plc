using System;
using System.Linq;
using Phoenix.Data.Plc.Items.Builder;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestFixture]
	[Category("Item Test")]
	public sealed class DynamicBytesPlcItemTest
	{
		[Test]
		public void Check_ItemBuilder()
		{
			// With predefined length-item.
			DynamicBytesPlcItem item = new Items.Builder.PlcItemBuilder()
				.ConstructBytesPlcItem("Bytes")
				.ForData()
				.AtDatablock(0)
				.AtPosition(0)
				.WithDynamicItem<UInt32PlcItem>()
				.BuildDynamic()
				;
			System.Diagnostics.Debug.WriteLine(item.ToString());
			Assert.True(item.LengthPlcItem is UInt32PlcItem);
			Assert.AreEqual(default(UInt32), ((UInt32PlcItem)item.LengthPlcItem).Value);
			Assert.IsNotNull(item.Value);
			Assert.True(new byte[0].SequenceEqual(item.Value));

			// With initial value.
			var initialValue = new byte[] { Byte.MinValue, Byte.MaxValue, 10, 20, 30, 40, 50 };
			item = new Items.Builder.PlcItemBuilder()
				.ConstructBytesPlcItem("Bytes")
				.ForData()
				.AtDatablock(0)
				.AtPosition(0)
				.WithDynamicItemFromInitialValue(initialValue)
				.BuildDynamic()
				;
			System.Diagnostics.Debug.WriteLine(item.ToString());
			Assert.True(item.LengthPlcItem is BytePlcItem);
			Assert.AreEqual(initialValue.Length, ((BytePlcItem)item.LengthPlcItem).Value);
			Assert.True(initialValue.SequenceEqual(item.Value));
		}

		[Test]
		public void Check_Clone()
		{
			var item = new DynamicBytesPlcItem(new UInt16PlcItem(dataBlock: 0, position: 0), "DynamicBytes");
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
			var target = new byte[] {Byte.MinValue, Byte.MaxValue, 10, 20, 30, 40, 50};

			var item = new DynamicBytesPlcItem(new UInt16PlcItem(dataBlock: 0, position: 0), "DynamicBytes");
			Assert.AreEqual(0, ((UInt16PlcItem)item.LengthPlcItem).Value);
			Assert.True(new byte[0].SequenceEqual(item.Value));

			// Check: Bytes → Data
			item.Value = target;
			Assert.True(target.SequenceEqual(item.Value));
			Assert.True(target.SequenceEqual((byte[])((IPlcItem)item).Value));
			Assert.AreEqual((ushort) target.Length, ((UInt16PlcItem) item.LengthPlcItem).Value);
		}

		[Test]
		public void Check_FromData()
		{
			var target = new byte[] {50, 40, 30, 20, 10, Byte.MaxValue, Byte.MinValue};

			var item = new DynamicBytesPlcItem(new UInt16PlcItem(dataBlock: 0, position: 0), "DynamicBytes");

			// Check: Data → Bytes
			((IPlcItem) item).Value.TransferValuesFrom(target);
			Assert.True(target.SequenceEqual(item.Value));
			Assert.True(target.SequenceEqual((byte[])((IPlcItem)item).Value));
			Assert.AreEqual((ushort)target.Length, ((UInt16PlcItem)item.LengthPlcItem).Value);
		}
	}
}