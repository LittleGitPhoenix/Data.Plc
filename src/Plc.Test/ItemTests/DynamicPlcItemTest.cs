using System;
using System.Linq;
using System.Text;
using Phoenix.Data.Plc.Items.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestClass]
	[TestCategory("Item Test")]
	public sealed class DynamicPlcItemTest
	{
		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthLimit"/> is respected when reading such an item.
		/// </summary>
		[TestMethod]
		public void Check_If_Limit_Is_Respected_When_Reading()
		{
			// Arrange
			var limit = (uint) 2;
			var numericItem = new BytePlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[4], name);
			// ReSharper disable once ObjectCreationAsStatement → It is necessary to wrap both the numeric and the flexible item to check if the limit supplied to the constructor of the dynamic item is respected.
			new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, limit);

			// Act
			numericItem.Value = 3;

			// Assert
			Assert.AreEqual(numericItem.Value, limit);
			
			// Act
			numericItem.Value = 1;

			// Assert
			Assert.AreEqual(numericItem.Value, 1);
		}

		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthLimit"/> is respected when writing such an item.
		/// </summary>
		[TestMethod]
		public void Check_If_Limit_Is_Respected_When_Writing()
		{
			// Arrange
			var limit = (uint) 2;
			var numericItem = new BytePlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[4], name);
			DynamicPlcItem<byte[]> dynamicPlcItem = new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, limit);

			// Act
			dynamicPlcItem.FlexiblePlcItem.Value = new [] {byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue};
			
			// Assert
			Assert.AreEqual(numericItem.Value, limit);
			Assert.IsTrue(new [] {byte.MaxValue, byte.MaxValue}.SequenceEqual(dynamicPlcItem.FlexiblePlcItem.Value));

			// Act
			dynamicPlcItem.FlexiblePlcItem.Value = new[] { byte.MinValue };

			// Assert
			Assert.AreEqual(numericItem.Value, 1);
			Assert.IsTrue(new[] { byte.MinValue }.SequenceEqual(dynamicPlcItem.FlexiblePlcItem.Value));
		}
	}
}