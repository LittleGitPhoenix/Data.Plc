#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IByteTypePlcItemConstructor ConstructBytePlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new BytePlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IByteTypePlcItemConstructor
	{
		IByteDataBlockPlcItemConstructor ForData();
		IBytePositionPlcItemConstructor ForInput();
		IBytePositionPlcItemConstructor ForOutput();
		IBytePositionPlcItemConstructor ForFlags();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IByteDataBlockPlcItemConstructor
	{
		IBytePositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytePositionPlcItemConstructor
	{
		IByteLengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IByteLengthPlcItemConstructor
	{
		IBytePlcItemCreator WithoutInitialValue();
		IBytePlcItemCreator WithInitialValue(byte initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytePlcItemCreator
	{
		BytePlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class BytePlcItemConstructor
		: PlcItemConstructorBase<BytePlcItem, byte>,
			IByteTypePlcItemConstructor,
			IByteDataBlockPlcItemConstructor,
			IBytePositionPlcItemConstructor,
			IByteLengthPlcItemConstructor,
			IBytePlcItemCreator
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

		public BytePlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			//base.ForByteAmount((ushort) System.Runtime.InteropServices.Marshal.SizeOf<byte>());
			base.ForByteAmount(sizeof(byte));
		}

		#endregion

		#region Methods

		public new IByteDataBlockPlcItemConstructor ForData() => (IByteDataBlockPlcItemConstructor)base.ForData();

		public new IBytePositionPlcItemConstructor ForFlags() => (IBytePositionPlcItemConstructor)base.ForFlags();

		public new IBytePositionPlcItemConstructor ForInput() => (IBytePositionPlcItemConstructor)base.ForInput();

		public new IBytePositionPlcItemConstructor ForOutput() => (IBytePositionPlcItemConstructor)base.ForOutput();

		public new IBytePositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IBytePositionPlcItemConstructor)base.AtDatablock(dataBlock);

		public new IByteLengthPlcItemConstructor AtPosition(ushort bytePosition) => (IByteLengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IBytePlcItemCreator WithoutInitialValue() => this.WithInitialValue(default);

		public new IBytePlcItemCreator WithInitialValue(byte initialValue) => (IBytePlcItemCreator) base.WithInitialValue(initialValue);

		public override BytePlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new BytePlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.InitialValue, this.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member