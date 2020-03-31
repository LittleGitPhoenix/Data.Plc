﻿using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestClass]
	[TestCategory("Item Test")]
	public sealed class BitPlcItemTest
	{
		[TestMethod]
		public void Check_ItemBuilder()
		{
			BitPlcItem item = new PlcItemBuilder()
				.ConstructBitPlcItem("Bit")
				.ForData()
				.AtDatablock(0)
				.AtPosition(4)
				.AsSet()
				.Build()
				;
			Assert.AreEqual(true, item.Value);
			Assert.AreEqual((uint) 1, ((IPlcItem) item).Value.Length);

			System.Diagnostics.Debug.WriteLine(item.ToString());
		}

		[TestMethod]
		public void Check_Clone()
		{
			var item = new BitPlcItem(0, 0, BitPosition.X3, true);
			var clone = item.Clone();

			// Check equality (Equals is overriden).
			Assert.AreEqual(item, clone);

			// Check the value.
			Assert.AreEqual(item.Value, clone.Value);

			// Check if both items are different references.
			Assert.IsFalse(Object.ReferenceEquals(item, clone));
		}

		[TestMethod]
		public void Check_ToData()
		{
			var item = new BitPlcItem(0, 0, BitPosition.X4);

			// Check: Boolean → Data
			item.Value = true;
			Assert.AreEqual(true, item.Value);

			item.Value = false;
			Assert.AreEqual(false, item.Value);
		}
	}
}