#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ITypePlcItemConstructor
	{
		IDataBlockPlcItemConstructor ForData();
		IPositionPlcItemConstructor ForInput();
		IPositionPlcItemConstructor ForOutput();
		IPositionPlcItemConstructor ForFlags();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IDataBlockPlcItemConstructor
	{
		IPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IPositionPlcItemConstructor
	{
		ILengthPlcItemConstructor AtPosition(ushort bytePosition);
		ILengthPlcItemConstructor AtPosition(ushort bytePosition, BitPosition bitPosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILengthPlcItemConstructor
	{
		IPlcItemCreator ForBitAmount(uint bitAmount);
		IPlcItemCreator ForByteAmount(uint byteAmount);
		IPlcItemCreator WithInitialValue(BitCollection initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IPlcItemCreator
	{
		IPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class PlcItemConstructor
		: PlcItemConstructorBase<IPlcItem, BitCollection>,
			ITypePlcItemConstructor,
			IDataBlockPlcItemConstructor,
			IPositionPlcItemConstructor,
			ILengthPlcItemConstructor,
			IPlcItemCreator
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
		
		public PlcItemConstructor(string identifier)
			: base(identifier) { }

		#endregion

		#region Methods

		public new IDataBlockPlcItemConstructor ForData() => (IDataBlockPlcItemConstructor) base.ForData();

		public new IPositionPlcItemConstructor ForFlags() => (IPositionPlcItemConstructor) base.ForFlags();

		public new IPositionPlcItemConstructor ForInput() => (IPositionPlcItemConstructor) base.ForInput();

		public new IPositionPlcItemConstructor ForOutput() => (IPositionPlcItemConstructor) base.ForOutput();

		public new IPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IPositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new ILengthPlcItemConstructor AtPosition(ushort bytePosition) => this.AtPosition(bytePosition, Items.BitPosition.X0);

		public new ILengthPlcItemConstructor AtPosition(ushort bytePosition, BitPosition bitPosition) => (ILengthPlcItemConstructor) base.AtPosition(bytePosition, bitPosition);

		public new IPlcItemCreator ForBitAmount(uint bitAmount) => (IPlcItemCreator) base.ForBitAmount(bitAmount);

		public new IPlcItemCreator ForByteAmount(uint byteAmount) => (IPlcItemCreator) base.ForByteAmount(byteAmount);

		public new IPlcItemCreator WithInitialValue(BitCollection initialValue) => (IPlcItemCreator) base.WithInitialValue(initialValue);
		
		public override IPlcItem Build()
		{
			this.Validate();

			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			if (base.InitialValue is null)
			{
				return new PlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.BitPosition.Value, base.BitAmount.Value, false, base.Identifier);
			}
			else
			{
				return new PlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.BitPosition.Value, base.InitialValue, base.Identifier);
			}
			// ReSharper restore PossibleInvalidOperationException

		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member