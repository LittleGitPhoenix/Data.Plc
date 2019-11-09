using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IInt32DataBlockPlcItemConstructor ConstructInt32PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new Int32PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt32DataBlockPlcItemConstructor
	{
		IInt32PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt32PositionPlcItemConstructor
	{
		IInt32LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt32LengthPlcItemConstructor
	{
		IInt32PlcItemCreator WithoutInitialValue();
		IInt32PlcItemCreator WithInitialValue(int initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IInt32PlcItemCreator
	{
		Int32PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class Int32PlcItemConstructor
		: PlcItemConstructorBase<Int32PlcItem, int>,
			IInt32DataBlockPlcItemConstructor,
			IInt32PositionPlcItemConstructor,
			IInt32LengthPlcItemConstructor,
			IInt32PlcItemCreator
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

		public Int32PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			//base.ForByteAmount((ushort) System.Runtime.InteropServices.Marshal.SizeOf<int>());
			base.ForByteAmount((ushort) sizeof(int));
		}

		#endregion

		#region Methods

		public new IInt32PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IInt32PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IInt32LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IInt32LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IInt32PlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

		public new IInt32PlcItemCreator WithInitialValue(int initialValue) => (IInt32PlcItemCreator)base.WithInitialValue(initialValue);

		public override Int32PlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new Int32PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member