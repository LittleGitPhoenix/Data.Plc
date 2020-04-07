using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test
{
	[TestFixture]
	public class ItemTest
	{
		public Data Data { get; }

		public ItemTest()
		{
			this.Data = new Data();
		}

		[Test]
		public void CheckItemIdentifierAndPlcString()
		{
			IPlcItem plcItem;

			plcItem = new BitPlcItem(dataBlock: 0, position: 0, bitPosition: BitPosition.X1, identifier: "Bit");
			Assert.AreEqual("Bit", plcItem.Identifier);
			Assert.AreEqual("DB0,X0.1,1", plcItem.PlcString);

			plcItem = new BytePlcItem(dataBlock: 1234, position: 0, identifier: "Byte");
			Assert.AreEqual("Byte", plcItem.Identifier);
			Assert.AreEqual("DB1234,B0,1", plcItem.PlcString);

			plcItem = new BitsPlcItem(type: PlcItemType.Input, dataBlock: 0, position: 0, bitPosition: BitPosition.X1, bitAmount: 2, identifier: "Bits");
			Assert.AreEqual("Bits", plcItem.Identifier);
			Assert.AreEqual("Input,X0.1,2", plcItem.PlcString);

			plcItem = new BytePlcItem(type: PlcItemType.Output, dataBlock: 0, position: 0);
			Assert.AreEqual("Output,B0,1", plcItem.Identifier);
			Assert.AreEqual(plcItem.Identifier, plcItem.PlcString);
		}

		[Test]
		public void CheckClone()
		{
			IPlcItem plcItem = new BytesPlcItem(dataBlock: 1234, position: 10, initialValue: Data.TargetBytes); // Explicitly cast this to 'IPlcItem' so that its 'Value' is a 'BitCollection'.
			IPlcItem clonedPlcItem = plcItem.Clone();
			
			Assert.True(plcItem.Equals(clonedPlcItem));
			Assert.False( Object.ReferenceEquals(plcItem, clonedPlcItem));
			Assert.True(plcItem.Value.Equals(clonedPlcItem.Value));
		}
		
		[Test]
		public void CheckTypedValueEquality()
		{
			var plcItem = new BytesPlcItem(dataBlock: 1234, position: 10, initialValue: Data.TargetBytes);
			var typedValue = plcItem.Value;
			var underlyingValue = (byte[]) ((IPlcItem) plcItem).Value;
			
			Assert.True(typedValue.SequenceEqual(underlyingValue));
		}

		[Test]
		public void CheckItemBuilder()
		{
			var itemBuilder = new PlcItemBuilder();

			var plcItem = itemBuilder
				.Construct("Generic")
				.ForData()
				.AtDatablock(0)
				.AtPosition(0, BitPosition.X2)
				.ForBitAmount(3)
				.Build()
				;
			Assert.AreEqual((uint) 3, plcItem.Value.Length);

			BitsPlcItem bitsItem = itemBuilder
				.ConstructBitsPlcItem()
				.ForFlags()
				.AtPosition(20, BitPosition.X5)
				.ForBitAmount(5)
				.Build()
				;
			Assert.AreEqual((uint) 5, ((IPlcItem) bitsItem).Value.Length);

			BitPlcItem bitItem = itemBuilder
				.ConstructBitPlcItem("Bit")
				.ForData()
				.AtDatablock(0)
				.AtPosition(5)
				.AsSet()
				.Build()
				;
			Assert.AreEqual((uint) 1, ((IPlcItem) bitItem).Value.Length);

			BytesPlcItem bytesItem = itemBuilder
				.ConstructBytesPlcItem(identifier: "Bytes")
				.ForOutput()
				.AtPosition(0)
				.WithInitialValue(new[] {byte.MinValue, byte.MaxValue})
				.Build()
				;
			Assert.AreEqual((uint) 2, ((IPlcItem) bytesItem).Value.ByteLength);

			BytePlcItem byteItem = itemBuilder
				.ConstructBytePlcItem("Byte")
				.ForInput()
				.AtPosition(10)
				.WithInitialValue(Byte.MaxValue)
				.Build()
				;
			Assert.AreEqual((uint) sizeof(Byte), ((IPlcItem) byteItem).Value.ByteLength);

			Int16PlcItem int16Item = itemBuilder
				.ConstructInt16PlcItem("Int16")
				.AtDatablock(0)
				.AtPosition(1)
				.WithoutInitialValue()
				.Build()
				;
			Assert.AreEqual((uint) sizeof(Int16), ((IPlcItem) int16Item).Value.ByteLength);
			
			Int32PlcItem int32Item = itemBuilder
				.ConstructInt32PlcItem("Int32")
				.AtDatablock(0)
				.AtPosition(1)
				.WithInitialValue(int.MinValue)
				.Build()
				;
			Assert.AreEqual((uint) sizeof(Int32), ((IPlcItem) int32Item).Value.ByteLength);
			
			Int64PlcItem int64Item = itemBuilder
				.ConstructInt64PlcItem("Int64")
				.AtDatablock(0)
				.AtPosition(1)
				.WithInitialValue(long.MinValue)
				.Build()
				;
			Assert.AreEqual((uint) sizeof(Int64), ((IPlcItem) int64Item).Value.ByteLength);
			
			UInt16PlcItem uInt16Item = itemBuilder
				.ConstructUInt16PlcItem("UInt16")
				.AtDatablock(0)
				.AtPosition(1)
				.WithoutInitialValue()
				.Build()
				;
			Assert.AreEqual((uint) sizeof(UInt16), ((IPlcItem) uInt16Item).Value.ByteLength);

			UInt32PlcItem uInt32PlcItem = itemBuilder
				.ConstructUInt32PlcItem("UInt32")
				.AtDatablock(0)
				.AtPosition(1)
				.WithInitialValue(uint.MaxValue)
				.Build()
				;
			Assert.AreEqual((uint) sizeof(UInt32), ((IPlcItem) uInt32PlcItem).Value.ByteLength);

			UInt64PlcItem uInt64PlcItem = itemBuilder
				.ConstructUInt64PlcItem("UInt64")
				.AtDatablock(0)
				.AtPosition(1)
				.WithInitialValue(ulong.MaxValue)
				.Build()
				;
			Assert.AreEqual((uint) sizeof(UInt64), ((IPlcItem) uInt64PlcItem).Value.ByteLength);

			WordPlcItem wordItem = itemBuilder
				.ConstructWordPlcItem("Word")
				.AtDatablock(0)
				.AtPosition(2)
				.WithInitialValue(32458)
				.Build()
				;
			Assert.AreEqual((uint) 2, ((IPlcItem) wordItem).Value.ByteLength);

			DWordPlcItem dwordItem = itemBuilder
				.ConstructDWordPlcItem("DWord")
				.AtDatablock(0)
				.AtPosition(2)
				.WithInitialValue(uint.MaxValue)
				.Build()
				;
			Assert.AreEqual((uint) 4, ((IPlcItem) dwordItem).Value.ByteLength);

			LWordPlcItem lwordItem = itemBuilder
				.ConstructLWordPlcItem("LWord")
				.AtDatablock(0)
				.AtPosition(2)
				.WithInitialValue(ulong.MaxValue)
				.Build()
				;
			Assert.AreEqual((uint) 8, ((IPlcItem) lwordItem).Value.ByteLength);

			TextPlcItem textItem = itemBuilder
				.ConstructTextPlcItem("Text")
				.WithEncoding(Encoding.UTF7)
				.AtDatablock(0)
				.AtPosition(3)
				.WithInitialValue("Some String")
				.Build()
				;
			Assert.AreEqual((uint) Encoding.UTF7.GetBytes("Some String").Length, ((IPlcItem) textItem).Value.ByteLength);

			Utf8PlcItem utf8Item = itemBuilder
				.ConstructUtf8PlcItem("UTF-8")
				.AtDatablock(0)
				.AtPosition(4)
				.WithLength(10)
				.Build()
				;
			Assert.AreEqual((uint) 10, ((IPlcItem) utf8Item).Value.ByteLength);

			var initialText = "String whose length fits into a single byte.";
			DynamicUtf8PlcItem secondDynamicUtf8Item = itemBuilder
				.ConstructUtf8PlcItem("UTF-8")
				.AtDatablock(0)
				.AtPosition(4)
				.WithDynamicItemFromInitialValue(initialText)
				.BuildDynamic()
				;
			Assert.That(secondDynamicUtf8Item.LengthPlcItem.Value, Is.EqualTo((uint) Encoding.UTF8.GetBytes(initialText).Length));
			Assert.AreEqual(initialText, secondDynamicUtf8Item.Value);

			var items = new []
			{
				plcItem,
				bitsItem,
				bitItem,
				bytesItem,
				byteItem,
				int16Item,
				int32Item,
				int64Item,
				uInt16Item,
				uInt32PlcItem,
				uInt64PlcItem,
				wordItem,
				dwordItem,
				lwordItem,
				textItem,
				utf8Item,
				secondDynamicUtf8Item,
			};
			
			foreach (var item in items)
			{
				Debug.WriteLine($" -> {item}");
			}
		}
	}
}