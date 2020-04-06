#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IUInt32DataBlockPlcItemConstructor ConstructUInt32PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new UInt32PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt32DataBlockPlcItemConstructor
	{
		IUInt32PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt32PositionPlcItemConstructor
	{
		IUInt32LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt32LengthPlcItemConstructor
	{
		IUInt32PlcItemCreator WithoutInitialValue();
		IUInt32PlcItemCreator WithInitialValue(uint initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt32PlcItemCreator
	{
		UInt32PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class UInt32PlcItemConstructor
		: PlcItemConstructorBase<UInt32PlcItem, uint>,
			IUInt32DataBlockPlcItemConstructor,
			IUInt32PositionPlcItemConstructor,
			IUInt32LengthPlcItemConstructor,
			IUInt32PlcItemCreator
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

		public UInt32PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			base.ForByteAmount(sizeof(uint));
		}

		#endregion

		#region Methods

		public new IUInt32PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IUInt32PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IUInt32LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IUInt32LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IUInt32PlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new IUInt32PlcItemCreator WithInitialValue(uint initialValue) => (IUInt32PlcItemCreator) base.WithInitialValue(initialValue);

		public override UInt32PlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new UInt32PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member