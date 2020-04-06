#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Text;
using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IUtf8DataBlockPlcItemConstructor ConstructUtf8PlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new Utf8PlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUtf8DataBlockPlcItemConstructor
	{
		IUtf8PositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUtf8PositionPlcItemConstructor
	{
		IUtf8LengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUtf8LengthPlcItemConstructor
	{
		IUtf8PlcItemCreator WithLength(ushort length);
		IUtf8PlcItemCreator WithInitialValue(string initialValue);
		ILengthFactorUtf8PlcItemConstructor WithDynamicItem<TNumericPlcItem>() where TNumericPlcItem : INumericPlcItem;
		ILengthFactorUtf8PlcItemConstructor WithDynamicItemFromInitialValue(string initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IUtf8PlcItemCreator
	{
		Utf8PlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILengthFactorUtf8PlcItemConstructor : IDynamicUtf8PlcItemCreator
	{
		ILengthLimiterUtf8PlcItemConstructor WithLengthFactor(byte lengthFactor);
		ILengthLimiterUtf8PlcItemConstructor WithoutLengthFactor();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ILengthLimiterUtf8PlcItemConstructor : IDynamicUtf8PlcItemCreator
	{
		IDynamicUtf8PlcItemCreator WithLengthLimit(uint lengthLimit);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IDynamicUtf8PlcItemCreator
	{
		DynamicUtf8PlcItem BuildDynamic();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class Utf8PlcItemConstructor
		: PlcItemConstructorBase<Utf8PlcItem, string>,
			IUtf8DataBlockPlcItemConstructor,
			IUtf8PositionPlcItemConstructor,
			IUtf8LengthPlcItemConstructor,
			ILengthFactorUtf8PlcItemConstructor,
			ILengthLimiterUtf8PlcItemConstructor,
			IUtf8PlcItemCreator,
			IDynamicUtf8PlcItemCreator
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		
		private Type _numericPlcItemType;

		private byte _lengthFactor;

		private uint? _lengthLimit;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		public Utf8PlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Save parameters.
			_lengthLimit = null;

			// Set default values.
			base.ForData();
		}

		#endregion

		#region Methods

		public new IUtf8PositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IUtf8PositionPlcItemConstructor) base.AtDatablock(dataBlock);

		public new IUtf8LengthPlcItemConstructor AtPosition(ushort bytePosition) => (IUtf8LengthPlcItemConstructor) base.AtPosition(bytePosition);

		public IUtf8PlcItemCreator WithLength(ushort length) => (IUtf8PlcItemCreator) base.ForByteAmount(length);

		public new IUtf8PlcItemCreator WithInitialValue(string initialValue) => (IUtf8PlcItemCreator) base.WithInitialValue(initialValue);
		
		public ILengthFactorUtf8PlcItemConstructor WithDynamicItem<TNumericPlcItem>() where TNumericPlcItem : INumericPlcItem
		{
			base.ForByteAmount(0);
			_numericPlcItemType = typeof(TNumericPlcItem);
			return this;
		}

		public ILengthFactorUtf8PlcItemConstructor WithDynamicItemFromInitialValue(string initialValue)
		{
			var length = Encoding.UTF8.GetBytes(initialValue).Length;

			base.ForByteAmount((uint) length);
			base.WithInitialValue(initialValue);

			if (length - byte.MaxValue <= 0) _numericPlcItemType = typeof(BytePlcItem);
			else if (length - UInt16.MaxValue <= 0) _numericPlcItemType = typeof(UInt16PlcItem);
			else if (length - UInt32.MaxValue <= 0) _numericPlcItemType = typeof(UInt32PlcItem);
			else throw new NotSupportedException($"The currently maximum length of dynamic plc items is '{UInt32.MaxValue}' which is exceeded by the initial values length of '{length}'.");
			
			return this;
		}


		/// <inheritdoc />
		public ILengthLimiterUtf8PlcItemConstructor WithLengthFactor(byte lengthFactor)
		{
			_lengthFactor = lengthFactor;
			return this;
		}

		/// <inheritdoc />
		public ILengthLimiterUtf8PlcItemConstructor WithoutLengthFactor()
		{
			return this;
		}

		/// <inheritdoc />
		public IDynamicUtf8PlcItemCreator WithLengthLimit(uint lengthLimit)
		{
			_lengthLimit = lengthLimit;
			return this;
		}

		public override Utf8PlcItem Build()
		{
			this.Validate();

			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			if (this.InitialValue is null)
			{
				return new Utf8PlcItem(base.DataBlock.Value, base.Position.Value, (ushort) base.ByteAmount.Value, base.Identifier);
			}
			else
			{
				return new Utf8PlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			}
			// ReSharper restore PossibleInvalidOperationException
		}

		public DynamicUtf8PlcItem BuildDynamic()
		{
			this.Validate();

			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			INumericPlcItem numericPlcItem;
			if (_numericPlcItemType == typeof(BytePlcItem)) numericPlcItem = new BytePlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (byte) base.ByteAmount);
			else if (_numericPlcItemType == typeof(UInt16PlcItem)) numericPlcItem = new UInt16PlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt16) base.ByteAmount);
			else if (_numericPlcItemType == typeof(UInt32PlcItem)) numericPlcItem = new UInt32PlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt32) base.ByteAmount);
			else if (_numericPlcItemType == typeof(WordPlcItem)) numericPlcItem = new WordPlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt16) base.ByteAmount);
			else if (_numericPlcItemType == typeof(DWordPlcItem)) numericPlcItem = new DWordPlcItem(base.DataBlock.Value, base.Position.Value, initialValue: (UInt32) base.ByteAmount);
			else throw new NotSupportedException($"The numeric part of any dynamic plc item must be an {nameof(INumericPlcItem)}. Currently supported are the following concrete items: {nameof(BytePlcItem)}, {nameof(UInt16PlcItem)}, {nameof(UInt32PlcItem)}, {nameof(WordPlcItem)}, {nameof(DWordPlcItem)}");

			return new DynamicUtf8PlcItem(numericPlcItem, _lengthLimit, _lengthFactor, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member