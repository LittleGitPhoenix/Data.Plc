#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IInt16DataBlockPlcItemConstructor ConstructInt16PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new Int16PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt16DataBlockPlcItemConstructor
	{
		IInt16PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt16PositionPlcItemConstructor
	{
		IInt16LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt16LengthPlcItemConstructor
	{
		IInt16PlcItemCreator WithoutInitialValue();
		IInt16PlcItemCreator WithInitialValue(short initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt16PlcItemCreator
	{
		Int16PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class Int16PlcItemConstructor
		: PlcItemConstructorBase<Int16PlcItem, short>,
			IInt16DataBlockPlcItemConstructor,
			IInt16PositionPlcItemConstructor,
			IInt16LengthPlcItemConstructor,
			IInt16PlcItemCreator
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

		public Int16PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			base.ForByteAmount(sizeof(short));
		}

		#endregion

		#region Methods

		public new IInt16PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IInt16PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IInt16LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IInt16LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IInt16PlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new IInt16PlcItemCreator WithInitialValue(short initialValue) => (IInt16PlcItemCreator) base.WithInitialValue(initialValue);

		public override Int16PlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new Int16PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member