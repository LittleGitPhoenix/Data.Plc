#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IUInt16DataBlockPlcItemConstructor ConstructUInt16PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new UInt16PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt16DataBlockPlcItemConstructor
	{
		IUInt16PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt16PositionPlcItemConstructor
	{
		IUInt16LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt16LengthPlcItemConstructor
	{
		IUInt16PlcItemCreator WithoutInitialValue();
		IUInt16PlcItemCreator WithInitialValue(ushort initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt16PlcItemCreator
	{
		UInt16PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class UInt16PlcItemConstructor
		: PlcItemConstructorBase<UInt16PlcItem, ushort>,
			IUInt16DataBlockPlcItemConstructor,
			IUInt16PositionPlcItemConstructor,
			IUInt16LengthPlcItemConstructor,
			IUInt16PlcItemCreator
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

		public UInt16PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			//base.ForByteAmount((ushort) System.Runtime.InteropServices.Marshal.SizeOf<ushort>());
			base.ForByteAmount(sizeof(ushort));
		}

		#endregion

		#region Methods

		public new IUInt16PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IUInt16PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IUInt16LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IUInt16LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IUInt16PlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new IUInt16PlcItemCreator WithInitialValue(ushort initialValue) => (IUInt16PlcItemCreator) base.WithInitialValue(initialValue);
		
		public override UInt16PlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new UInt16PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member