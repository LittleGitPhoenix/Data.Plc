#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IInt64DataBlockPlcItemConstructor ConstructInt64PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new Int64PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt64DataBlockPlcItemConstructor
	{
		IInt64PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt64PositionPlcItemConstructor
	{
		IInt64LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt64LengthPlcItemConstructor
	{
		IInt64PlcItemCreator WithoutInitialValue();
		IInt64PlcItemCreator WithInitialValue(long initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt64PlcItemCreator
	{
		Int64PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class Int64PlcItemConstructor
		: PlcItemConstructorBase<Int64PlcItem, long>,
			IInt64DataBlockPlcItemConstructor,
			IInt64PositionPlcItemConstructor,
			IInt64LengthPlcItemConstructor,
			IInt64PlcItemCreator
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

		public Int64PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			base.ForByteAmount(sizeof(long));
		}

		#endregion

		#region Methods

		public new IInt64PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IInt64PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IInt64LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IInt64LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IInt64PlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new IInt64PlcItemCreator WithInitialValue(long initialValue) => (IInt64PlcItemCreator) base.WithInitialValue(initialValue);

		public override Int64PlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new Int64PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member