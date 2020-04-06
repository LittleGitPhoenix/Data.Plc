#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IUInt64DataBlockPlcItemConstructor ConstructUInt64PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new UInt64PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt64DataBlockPlcItemConstructor
	{
		IUInt64PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt64PositionPlcItemConstructor
	{
		IUInt64LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt64LengthPlcItemConstructor
	{
		IUInt64PlcItemCreator WithoutInitialValue();
		IUInt64PlcItemCreator WithInitialValue(ulong initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUInt64PlcItemCreator
	{
		UInt64PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class UInt64PlcItemConstructor
		: PlcItemConstructorBase<UInt64PlcItem, ulong>,
			IUInt64DataBlockPlcItemConstructor,
			IUInt64PositionPlcItemConstructor,
			IUInt64LengthPlcItemConstructor,
			IUInt64PlcItemCreator
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

		public UInt64PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			base.ForByteAmount(sizeof(ulong));
		}

		#endregion

		#region Methods

		public new IUInt64PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IUInt64PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IUInt64LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IUInt64LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IUInt64PlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new IUInt64PlcItemCreator WithInitialValue(ulong initialValue) => (IUInt64PlcItemCreator) base.WithInitialValue(initialValue);

		public override UInt64PlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new UInt64PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member