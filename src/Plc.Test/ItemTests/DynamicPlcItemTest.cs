using System;
using System.Linq;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestFixture]
	[Category("Item Test")]
	public sealed class DynamicPlcItemTest
	{
		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthLimit"/> is respected when reading such an item.
		/// </summary>
		[Test]
		[TestCase(3, 2)] // The limit is 2, so a value of 3 should be 2.
		[TestCase(1, 1)] // The limit is 2, so a value of 1 should be 1.
		public void Check_If_Limit_Is_Respected_When_Reading(byte value, byte target)
		{
			// Arrange
			var limit = (uint) 2;
			var numericItem = new BytePlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[4], name);
			// ReSharper disable once ObjectCreationAsStatement → It is necessary to wrap both the numeric and the flexible item to check if the limit supplied to the constructor of the dynamic item is respected.
			new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, limit, 1);

			// Act
			numericItem.Value = value;

			// Assert
			Assert.AreEqual(numericItem.Value, target);
		}

		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthLimit"/> is respected when writing such an item.
		/// </summary>
		[Test]
		[TestCase(new[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue }, 2, new[] { byte.MaxValue, byte.MaxValue })] // This should be limited to only 2 bates.
		[TestCase(new[] { byte.MinValue }, 1, new[] { byte.MinValue })] // This can be written in total.
		public void Check_If_Limit_Is_Respected_When_Writing(byte[] value, byte targetLength, byte[] targetData)
		{
			// Arrange
			var limit = (uint) 2;
			var numericItem = new BytePlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[4], name);
			DynamicPlcItem<byte[]> dynamicPlcItem = new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, limit, 1);

			// Act
			dynamicPlcItem.FlexiblePlcItem.Value = value;
			
			// Assert
			Assert.AreEqual(numericItem.Value, targetLength);
			Assert.True(targetData.SequenceEqual(dynamicPlcItem.FlexiblePlcItem.Value));
		}
	}
}