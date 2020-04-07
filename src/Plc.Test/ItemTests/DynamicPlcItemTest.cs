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
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthFactor"/> is applied when reading an <see cref="IDynamicPlcItem"/>.
		/// </summary>
		[Test]
		[TestCase((ushort) 65535)]
		public void Check_If_Factor_Is_Applied_When_Reading(ushort value)
		{
			// Arrange
			var factor = (byte) 2;
			var targetDataLength = (uint) value * factor;
			var numericItem = new UInt16PlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[0], name);
			var dynamicPlcItem = new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, factor, null);

			// Act
			numericItem.Value = value;

			// Assert
			Assert.That(numericItem.Value, Is.EqualTo(value));
			Assert.That(dynamicPlcItem.FlexiblePlcItem.Value, Has.Length.EqualTo(targetDataLength));
		}

		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthFactor"/> is applied when writing an <see cref="IDynamicPlcItem"/>.
		/// </summary>
		[Test]
		[TestCase(new byte[]{0,0,0,0,0,0,0,0,0,0})]
		public void Check_If_Factor_Is_Applied_When_Writing(byte[] value)
		{
			// Arrange
			var factor = (byte) 2;
			var targetLength = value.Length / factor;
			var numericItem = new UInt16PlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[0], name);
			var dynamicPlcItem = new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, factor, null);

			// Act
			dynamicPlcItem.FlexiblePlcItem.Value = value;

			// Assert
			Assert.That(numericItem.Value, Is.EqualTo(targetLength));
			Assert.That(dynamicPlcItem.FlexiblePlcItem.Value, Has.Length.EqualTo(value.Length));
		}

		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthLimit"/> is respected when reading an <see cref="IDynamicPlcItem"/>.
		/// </summary>
		[Test]
		[TestCase(3, 2)] // The limit is 2, so a value of 3 should be 2.
		[TestCase(1, 1)] // The limit is 2, so a value of 1 should be 1.
		public void Check_If_Limit_Is_Respected_When_Reading(byte value, byte target)
		{
			// Arrange
			var limit = (uint) 2;
			var numericItem = new BytePlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[0], name);
			var dynamicPlcItem = new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, 1, limit);

			// Act
			numericItem.Value = value;

			// Assert
			Assert.That(numericItem.Value, Is.EqualTo(target));
			Assert.That(dynamicPlcItem.FlexiblePlcItem.Value, Has.Length.EqualTo(target));
		}

		/// <summary>
		/// Checks if a custom <see cref="IDynamicPlcItem.LengthLimit"/> is respected when writing an <see cref="IDynamicPlcItem"/>.
		/// </summary>
		[Test]
		[TestCase(new byte[] { 255, 255, 255, 255 }, 2, new byte[] { 255, 255 })] // This should be limited to only 2 bytes.
		[TestCase(new byte[] { 0 }, 1, new byte[] { 0 })] // This can be written in total.
		public void Check_If_Limit_Is_Respected_When_Writing(byte[] value, byte targetLength, byte[] targetData)
		{
			// Arrange
			var limit = (uint) 2;
			var numericItem = new BytePlcItem(0, 0);
			Func<string, IPlcItem<byte[]>> flexiblePlcItemFactory = name => new BytesPlcItem(0, 1, true, new byte[0], name);
			var dynamicPlcItem = new DynamicPlcItem<byte[]>(numericItem, flexiblePlcItemFactory, 1, limit);

			// Act
			dynamicPlcItem.FlexiblePlcItem.Value = value;
			
			// Assert
			Assert.That(numericItem.Value, Is.EqualTo(targetLength));
			Assert.True(targetData.SequenceEqual(dynamicPlcItem.FlexiblePlcItem.Value));
		}
	}
}