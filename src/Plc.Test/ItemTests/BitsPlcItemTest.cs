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
	public sealed class BitsPlcItemTest
	{
		[Test]
		public void Check_ItemBuilder()
		{
			BitsPlcItem item = new PlcItemBuilder()
				.ConstructBitsPlcItem("Bits")
				.ForData()
				.AtDatablock(0)
				.AtPosition(1, BitPosition.X2)
				.WithInitialValue(new BitCollection(false, true, true, true))
				.Build()
				;
			Assert.AreEqual(new BitCollection(false, true, true, true), item.Value);
			Assert.AreEqual((uint) 3, ((IPlcItem) item).Value.Length);

			System.Diagnostics.Debug.WriteLine(item.ToString());
		}

		[Test]
		public void Check_Clone()
		{
			var item = new BitsPlcItem(0, 0, BitPosition.X3, 13);
			var clone = item.Clone();

			// Check equality (Equals is overriden).
			Assert.AreEqual(item, clone);

			// Check the value.
			Assert.AreEqual(item.Value, clone.Value);

			// Check if both items are different references.
			Assert.False(Object.ReferenceEquals(item, clone));
		}

		[Test]
		public void Check_ToData()
		{
			var item = new BitsPlcItem(0, 0, BitPosition.X3, 13);

			// Check: Boolean → Data
			var target = Enumerable.Range(0, 13).Select(i => true).ToList();
			item.Value.SetAllBitsTo(true);
			Assert.True(((bool[]) item.Value).SequenceEqual(target));

			target = Enumerable.Range(0, 13).Select(i => false).ToList();
			item.Value.SetAllBitsTo(false);
			Assert.True(((bool[])item.Value).SequenceEqual(target));
		}
	}
}