using System;
using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IBytesTypePlcItemConstructor ConstructBytesPlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new BytesPlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytesTypePlcItemConstructor
	{
		IBytesDataBlockPlcItemConstructor ForData();
		IBytesPositionPlcItemConstructor ForInput();
		IBytesPositionPlcItemConstructor ForOutput();
		IBytesPositionPlcItemConstructor ForFlags();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytesDataBlockPlcItemConstructor
	{
		IBytesPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytesPositionPlcItemConstructor
	{
		IBytesLengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytesLengthPlcItemConstructor
	{
		IBytesPlcItemCreator ForByteAmount(uint byteAmount);
		IBytesPlcItemCreator WithInitialValue(byte[] initialValue);
		IDynamicBytesPlcItemCreator WithDynamicItem<TNumericPlcItem>() where TNumericPlcItem : INumericPlcItem;
		IDynamicBytesPlcItemCreator WithDynamicItemFromInitialValue(byte[] initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBytesPlcItemCreator
	{
		BytesPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IDynamicBytesPlcItemCreator
	{
		DynamicBytesPlcItem BuildDynamic();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class BytesPlcItemConstructor
		: PlcItemConstructorBase<BytesPlcItem, byte[]>,
			IBytesTypePlcItemConstructor,
			IBytesDataBlockPlcItemConstructor,
			IBytesPositionPlcItemConstructor,
			IBytesLengthPlcItemConstructor,
			IBytesPlcItemCreator,
			IDynamicBytesPlcItemCreator
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private Type _numericPlcItemType;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		public BytesPlcItemConstructor(string identifier)
			: base(identifier) { }

		#endregion

		#region Methods

		public new IBytesDataBlockPlcItemConstructor ForData() => (IBytesDataBlockPlcItemConstructor) base.ForData();

		public new IBytesPositionPlcItemConstructor ForFlags() => (IBytesPositionPlcItemConstructor) base.ForFlags();

		public new IBytesPositionPlcItemConstructor ForInput() => (IBytesPositionPlcItemConstructor) base.ForInput();

		public new IBytesPositionPlcItemConstructor ForOutput() => (IBytesPositionPlcItemConstructor) base.ForOutput();

		public new IBytesPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IBytesPositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IBytesLengthPlcItemConstructor AtPosition(ushort bytePosition) => (IBytesLengthPlcItemConstructor) base.AtPosition(bytePosition);
		
		public new IBytesPlcItemCreator ForByteAmount(uint byteAmount) => (IBytesPlcItemCreator) base.ForByteAmount(byteAmount);

		public new IBytesPlcItemCreator WithInitialValue(byte[] initialValue) => (IBytesPlcItemCreator) base.WithInitialValue(initialValue);

		public IDynamicBytesPlcItemCreator WithDynamicItem<TNumericPlcItem>() where TNumericPlcItem : INumericPlcItem
		{
			base.ForByteAmount(0);
			_numericPlcItemType = typeof(TNumericPlcItem);
			return this;
		}

		public IDynamicBytesPlcItemCreator WithDynamicItemFromInitialValue(byte[] initialValue)
		{
			var length = initialValue.Length;

			base.ForByteAmount((uint)length);
			base.WithInitialValue(initialValue);

			if (length - byte.MaxValue <= 0) _numericPlcItemType = typeof(BytePlcItem);
			else if (length - UInt16.MaxValue <= 0) _numericPlcItemType = typeof(UInt16PlcItem);
			else if (length - UInt32.MaxValue <= 0) _numericPlcItemType = typeof(UInt32PlcItem);
			else throw new NotSupportedException($"The currently maximum length of dynamic plc items is '{UInt32.MaxValue}' which is exceeded by the initial values length of '{length}'.");

			return this;
		}

		public override BytesPlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			if (this.InitialValue is null)
			{
				return new BytesPlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, (ushort) base.ByteAmount.Value, this.Identifier);
			}
			else
			{
				return new BytesPlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.InitialValue, this.Identifier);
			}
			// ReSharper restore PossibleInvalidOperationException
		}

		public DynamicBytesPlcItem BuildDynamic()
		{
			this.Validate();

			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			INumericPlcItem numericPlcItem;
			if (_numericPlcItemType == typeof(BytePlcItem)) numericPlcItem = new BytePlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (byte)base.ByteAmount);
			else if (_numericPlcItemType == typeof(UInt16PlcItem)) numericPlcItem = new UInt16PlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt16)base.ByteAmount);
			else if (_numericPlcItemType == typeof(UInt32PlcItem)) numericPlcItem = new UInt32PlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt32)base.ByteAmount);
			else if (_numericPlcItemType == typeof(WordPlcItem)) numericPlcItem = new WordPlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt16)base.ByteAmount);
			else if (_numericPlcItemType == typeof(DWordPlcItem)) numericPlcItem = new DWordPlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt32)base.ByteAmount);
			else throw new NotSupportedException($"The numeric part of any dynamic plc item must be an {nameof(INumericPlcItem)}. Currently supported are the following concrete items: {nameof(BytePlcItem)}, {nameof(UInt16PlcItem)}, {nameof(UInt32PlcItem)}, {nameof(WordPlcItem)}, {nameof(DWordPlcItem)}");

			return new DynamicBytesPlcItem(numericPlcItem, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member