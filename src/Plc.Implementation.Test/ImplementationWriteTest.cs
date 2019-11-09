using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Data.Plc.Items.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.Test;

namespace Phoenix.Data.Plc.Implementation.Test
{
	public abstract class ImplementationWriteTest<TPlc> : ImplementationTest<TPlc>
		where TPlc : IPlc
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		protected ImplementationWriteTest(TPlc plc) : base(_ => plc) { }

		protected ImplementationWriteTest(Func<Data.Plc.Test.Data, TPlc> plcFactory) : base(plcFactory) { }

		#endregion

		#region Methods

		#region Tests

		[TestMethod]
		public void WriteBit()
		{
			var writeItem = new BitPlcItem(dataBlock: this.Data.Datablock, position: 4, bitPosition: 0, initialValue: false);

			base.ExecuteTest
			(
				async (plc) =>
				{
					var result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);

					// Toggle the bit.
					writeItem.Value = true;
					result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);
				}
			);
		}

		[TestMethod]
		public void WriteBits()
		{
			var allFalse = new BitCollection(false, new[] {false, false, false, false, false, false});
			var allTrue = new BitCollection(false, new[] {true, true, true, true, true, true});
			var partialTrue = new BitCollection(false, new[] {true, true, true, true });
			var mixed = new BitCollection(false, new[] { false, false, true, true, true, true, false, false });

			var fullItem = new BitsPlcItem(dataBlock: this.Data.Datablock, position: this.Data.StartOfModifiableBytes, BitPosition.X0, bitAmount: 8);
			var writeItem = new BitsPlcItem(dataBlock: this.Data.Datablock, position: this.Data.StartOfModifiableBytes, BitPosition.X1, initialValue: allTrue);
			var partialWriteItem = new BitsPlcItem(dataBlock: this.Data.Datablock, position: this.Data.StartOfModifiableBytes, BitPosition.X2, initialValue: partialTrue);

			base.ExecuteTest
			(
				async (plc) =>
				{
					// Clear the whole byte.
					fullItem.Value.SetAllBitsTo(false);
					await plc.WriteItemAsync(fullItem);

					var result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);

					// Set everything to false.
					writeItem.Value.SetAllBitsTo(false);
					result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);

					// Make only a part true.
					result = await plc.WriteItemAsync(partialWriteItem);
					Assert.IsTrue(result);

					// Read everything and compare those two.
					var mirror = await plc.ReadItemAsync(fullItem);
					Assert.IsTrue(mirror.SequenceEqual(mixed));
				}
			);
		}

		[TestMethod]
		public void WriteByte()
		{
			var writeItem = new BytePlcItem(dataBlock: this.Data.Datablock, position: 4, initialValue: this.Data.WriteBytes[0]);

			base.ExecuteTest
			(
				async (plc) =>
				{
					var result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);
				}
			);
		}

		[TestMethod]
		public void WriteBytes()
		{
			var writeItem = new BytesPlcItem(dataBlock: this.Data.Datablock, position: 4, initialValue: this.Data.WriteBytes);
			//var writeItem = new BytesPlcItem(dataBlock: Data.Datablock, position: 0, initialValue: Data.TargetBytes);

			base.ExecuteTest
			(
				async (plc) =>
				{
					var result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);
				}
			);
		}

		[TestMethod]
		public void WriteMultipleSingleBytes()
		{
			var bytePlcItems = this.Data.TargetBytes
				.Select((@byte, index) => new BytePlcItem(dataBlock: this.Data.Datablock, position: (ushort) (index + 4), initialValue: @byte))
				.ToArray()
				;

			base.ExecuteTest
			(
				async (plc) =>
				{
					var result = await plc.WriteItemsWithValidationAsync(bytePlcItems);
					Assert.IsTrue(result);
				}
			);
		}

		[TestMethod]
		public void WriteText()
		{
			var target = System.Text.Encoding.ASCII.GetString(this.Data.WriteBytes);
			var writeItem = new Utf8PlcItem(dataBlock: this.Data.Datablock, position: 4, initialValue: target);

			base.ExecuteTest
			(
				async (plc) =>
				{
					var result = await plc.WriteItemWithValidationAsync(writeItem);
					Assert.IsTrue(result);
				}
			);
		}

		[TestMethod]
		public void WriteDynamic()
		{
			var itemBuilder = new Items.Builder.PlcItemBuilder();

			var target = "ABC";
			//var targetData = Encoding.UTF8.GetBytes(target);

			var bytesItem = itemBuilder
				.ConstructBytesPlcItem()
				.ForData()
				.AtDatablock(Data.Datablock)
				.AtPosition(Data.StartOfModifiableBytes)
				.ForByteAmount((uint) Data.WriteBytes.Length)
				.Build()
				;

			var dynamicTextItem = itemBuilder
				.ConstructUtf8PlcItem()
				.AtDatablock(Data.Datablock)
				.AtPosition(Data.StartOfModifiableBytes)
				.WithDynamicItemFromInitialValue(target)
				.BuildDynamic()
				;

			base.ExecuteTest
			(
				async (plc) =>
				{
					// Clear the bytes.
					var result = await plc.WriteItemWithValidationAsync(bytesItem);
					Assert.IsTrue(result);

					// Write the whole dynamic item.
					result = await plc.WriteItemWithValidationAsync(dynamicTextItem);
					Assert.IsTrue(result);
				}
			);
		}

		#endregion

		#endregion
	}
}