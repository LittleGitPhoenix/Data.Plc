#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static ILWordDataBlockPlcItemConstructor ConstructLWordPlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new LWordPlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILWordDataBlockPlcItemConstructor
	{
		ILWordPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILWordPositionPlcItemConstructor
	{
		ILWordLengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILWordLengthPlcItemConstructor
	{
		ILWordPlcItemCreator WithoutInitialValue();
		ILWordPlcItemCreator WithInitialValue(ulong initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILWordPlcItemCreator
	{
		LWordPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class LWordPlcItemConstructor
		: PlcItemConstructorBase<LWordPlcItem, ulong>,
			ILWordDataBlockPlcItemConstructor,
			ILWordPositionPlcItemConstructor,
			ILWordLengthPlcItemConstructor,
			ILWordPlcItemCreator
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

		public LWordPlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			//base.ForByteAmount((ushort)System.Runtime.InteropServices.Marshal.SizeOf<ulong>());
			base.ForByteAmount(sizeof(ulong));
		}

		#endregion

		#region Methods

		public new ILWordPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (ILWordPositionPlcItemConstructor)base.AtDatablock(dataBlock);

		public new ILWordLengthPlcItemConstructor AtPosition(ushort bytePosition) => (ILWordLengthPlcItemConstructor)base.AtPosition(bytePosition);

		public ILWordPlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new ILWordPlcItemCreator WithInitialValue(ulong initialValue) => (ILWordPlcItemCreator)base.WithInitialValue(initialValue);

		public override LWordPlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new LWordPlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member