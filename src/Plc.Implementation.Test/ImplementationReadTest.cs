using System.Text;
using Phoenix.Data.Plc.Items.Builder;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Implementation.Test;

public abstract class ImplementationReadTest<TPlc> : ImplementationTest<TPlc>
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

	protected ImplementationReadTest(TPlc plc) : base(_ => plc) { }

	protected ImplementationReadTest(Func<Data.Plc.Test.Data, TPlc> plcFactory) : base(plcFactory) { }

	#endregion

	#region Methods

	#region Tests

	[Test]
	public void ReadBit()
	{
		var bitItem = new BitPlcItem(dataBlock: this.Data.Datablock, position: this.Data.StartOfFixedBytes, bitPosition: BitPosition.X4);

		bool? oldValue = null;
		bool? newValue = null;
		bitItem.ValueChanged += (sender, args) =>
		{
			oldValue = args.OldValue;
			newValue = args.NewValue;
		};

		base.ExecuteTest
		(
			async (plc) =>
			{
				var result = await plc.ReadItemAsync(bitItem);
				Assert.True(bitItem.Value == result);
				Assert.True(bitItem.Value == true);

				Assert.IsNotNull(oldValue);
				Assert.True(oldValue.Value == false);

				Assert.IsNotNull(newValue);
				Assert.True(newValue.Value == true);
			}
		);
	}

	[Test]
	public void ReadBits()
	{
		var bitsItem = new BitsPlcItem(dataBlock: this.Data.Datablock, position: 1, bitPosition: BitPosition.X1, bitAmount: 5);

		base.ExecuteTest
		(
			async (plc) =>
			{
				var result = await plc.ReadItemAsync(bitsItem);
				Assert.True(bitsItem.Value == result);

				// Get the target value.
				var booleans = DataConverter.ToBooleans(this.Data.TargetBytes);
				var relevantBooleans = booleans.Skip(bitsItem.Position * 8 + ((byte)bitsItem.BitPosition)).Take((int)bitsItem.Value.Length).ToArray();
				var target = new BitCollection(false, relevantBooleans);

				Assert.True(result == target);
			}
		);
	}

	[Test]
	public void ReadByte()
	{
		var byteItem = new BytePlcItem(dataBlock: this.Data.Datablock, position: 0);
			
		base.ExecuteTest
		(
			async (plc) =>
			{
				var result = await plc.ReadItemAsync(byteItem);
				//var result = await PlcExtensions.ReadItemAsync(plc, byteItem);
				Assert.True(byteItem.Value == result);
				Assert.True(byteItem.Value == this.Data.TargetBytes[0]);
			}
		);
	}

	[Test]
	public void ReadBytes()
	{
		var bytesItem = new BytesPlcItem(dataBlock: this.Data.Datablock, position: 0, byteAmount: (ushort) this.Data.TargetBytes.Length);
			
		base.ExecuteTest
		(
			async (plc) =>
			{
				var result = await plc.ReadItemAsync(bytesItem);
				Assert.True(bytesItem.Value == result); //! Because of the conversion, this is sequentially equal, but not referential.
				Assert.True(result.SequenceEqual(this.Data.TargetBytes));
			}
		);
	}

	[Test]
	public void ReadMultipleSingleBytes()
	{
		var bytePlcItems = this.Data.TargetBytes
				.Select((@byte, index) => new BytePlcItem(dataBlock: this.Data.Datablock, position: (ushort) index))
				.ToArray()
			;
			
		base.ExecuteTest
		(
			async (plc) =>
			{
				await plc.ReadItemsAsync(bytePlcItems);
				var actualData = bytePlcItems.Select(plcItem => plcItem.Value).ToArray();
				Assert.True(actualData.SequenceEqual(this.Data.TargetBytes));
			}
		);
	}

	[Test]
	public void ReadText()
	{
		var asciiItem = new Utf8PlcItem(dataBlock: this.Data.Datablock, position: 0, length: (ushort) this.Data.TargetBytes.Length);
			
		base.ExecuteTest
		(
			async (plc) =>
			{
				var result = await plc.ReadItemAsync(asciiItem);
				Assert.AreEqual((int) ((IPlcItem) asciiItem).Value.ByteLength, result.Length);
				Assert.True(asciiItem.Value.Equals(result, StringComparison.Ordinal));
				Assert.AreEqual("ROCK", asciiItem.Value);
			}
		);
	}

	[Test]
	public void ReadDynamicText()
	{
		var itemBuilder = new Items.Builder.PlcItemBuilder();

		var target = "ABC";
		var targetData = Encoding.UTF8.GetBytes(target);

		var bytesItem = itemBuilder
				.ConstructBytesPlcItem()
				.ForData()
				.AtDatablock(Data.Datablock)
				.AtPosition(Data.StartOfModifiableBytes)
				.WithInitialValue(new[] {(byte) targetData.Length}.Concat(targetData).ToArray())
				.Build()
			;

		var dynamicTextItem = itemBuilder
				.ConstructUtf8PlcItem()
				.AtDatablock(Data.Datablock)
				.AtPosition(Data.StartOfModifiableBytes)
				.WithDynamicItem<BytePlcItem>()
				.BuildDynamic()
			;

		base.ExecuteTest
		(
			async (plc) =>
			{
				var success = await plc.WriteItemAsync(bytesItem);
				Assert.True(success);
					
				// Read the dynamic item.
				var result = await plc.ReadItemAsync(dynamicTextItem);

				//Check value and length.
				Assert.AreEqual(target, result);
				Assert.AreEqual(target, dynamicTextItem.Value);
				Assert.AreEqual(result.Length, ((BytePlcItem) dynamicTextItem.LengthPlcItem).Value);
			}
		);
	}

	[Test]
	public virtual void ReadUndefinedDatablock()
	{
		// Arrange
		var validItem = new BytesPlcItem(dataBlock: this.Data.Datablock, position: 0, byteAmount: (ushort)this.Data.TargetBytes.Length);
		var invalidItem = new BytesPlcItem(dataBlock: ushort.MaxValue, position: 1, byteAmount: 100); //! Hopefully that datablock doesn't exists in the test system...

		base.ExecuteTest
		(
			(plc) =>
			{
				// Act + Assert
				var ex = Assert.ThrowsAsync<ReadPlcException>(() => plc.ReadItemsAsync(new[] {validItem, invalidItem}));
				Assert.That(ex.ValidItems.FirstOrDefault(), Is.EqualTo(validItem));
				Assert.That(ex.FailedItems.FirstOrDefault().FailedItem, Is.EqualTo(invalidItem));

#if NET45
					return CompletedTask;
#else
				return System.Threading.Tasks.Task.CompletedTask;
#endif
			}
		);
	}

	#endregion

	#endregion
}