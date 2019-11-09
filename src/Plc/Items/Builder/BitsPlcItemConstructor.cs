using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public static partial class PlcItemBuilderExtension
	{
		public static IBitsTypePlcItemConstructor ConstructBitsPlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new BitsPlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitsTypePlcItemConstructor
	{
		IBitsDataBlockPlcItemConstructor ForData();
		IBitsPositionPlcItemConstructor ForInput();
		IBitsPositionPlcItemConstructor ForOutput();
		IBitsPositionPlcItemConstructor ForFlags();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitsDataBlockPlcItemConstructor
	{
		IBitsPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitsPositionPlcItemConstructor
	{
		IBitsLengthPlcItemConstructor AtPosition(ushort bytePosition);
		IBitsLengthPlcItemConstructor AtPosition(ushort bytePosition, BitPosition bitPosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitsLengthPlcItemConstructor
	{
		IBitsPlcItemCreator ForBitAmount(byte bitAmount);
		IBitsPlcItemCreator WithInitialValue(BitCollection initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitsPlcItemCreator
	{
		BitsPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class BitsPlcItemConstructor
		: PlcItemConstructorBase<BitsPlcItem, BitCollection>,
			IBitsTypePlcItemConstructor,
			IBitsDataBlockPlcItemConstructor,
			IBitsPositionPlcItemConstructor,
			IBitsLengthPlcItemConstructor,
			IBitsPlcItemCreator
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

		public BitsPlcItemConstructor(string identifier)
			: base(identifier) { }

		#endregion

		#region Methods

		public new IBitsDataBlockPlcItemConstructor ForData() => (IBitsDataBlockPlcItemConstructor)base.ForData();

		public new IBitsPositionPlcItemConstructor ForFlags() => (IBitsPositionPlcItemConstructor)base.ForFlags();

		public new IBitsPositionPlcItemConstructor ForInput() => (IBitsPositionPlcItemConstructor)base.ForInput();

		public new IBitsPositionPlcItemConstructor ForOutput() => (IBitsPositionPlcItemConstructor)base.ForOutput();

		public new IBitsPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IBitsPositionPlcItemConstructor)base.AtDatablock(dataBlock);

		public new IBitsLengthPlcItemConstructor AtPosition(ushort bytePosition) => this.AtPosition(bytePosition, Items.BitPosition.X0);

		public new IBitsLengthPlcItemConstructor AtPosition(ushort bytePosition, BitPosition bitPosition) => (IBitsLengthPlcItemConstructor)base.AtPosition(bytePosition, bitPosition);

		public IBitsPlcItemCreator ForBitAmount(byte bitAmount) => (IBitsPlcItemCreator) base.ForBitAmount(bitAmount);

		public new IBitsPlcItemCreator WithInitialValue(BitCollection initialValue) => (IBitsPlcItemCreator) base.WithInitialValue(initialValue);

		public override BitsPlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			if (this.InitialValue is null)
			{
				return new BitsPlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.BitPosition.Value, (byte) base.BitAmount.Value, base.Identifier);
			}
			else
			{
				return new BitsPlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.BitPosition.Value, base.InitialValue, base.Identifier);
			}
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member