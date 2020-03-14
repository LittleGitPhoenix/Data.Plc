#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public interface IPlcItemBuilder<out TPlcItem, in TValue>
		where TPlcItem : IPlcItem
	{
		ITypePlcItemBuilder<TPlcItem, TValue> Construct(string identifier = null);
	}

	public interface ITypePlcItemBuilder<out TPlcItem, in TValue>
		where TPlcItem : IPlcItem
	{
		IDataBlockPlcItemBuilder<TPlcItem, TValue> ForData();
		IPositionPlcItemBuilder<TPlcItem, TValue> ForInput();
		IPositionPlcItemBuilder<TPlcItem, TValue> ForOutput();
		IPositionPlcItemBuilder<TPlcItem, TValue> ForFlags();
	}

	public interface IDataBlockPlcItemBuilder<out TPlcItem, in TValue>
		where TPlcItem : IPlcItem
	{
		IPositionPlcItemBuilder<TPlcItem, TValue> AtDatablock(ushort dataBlock);
	}

	public interface IPositionPlcItemBuilder<out TPlcItem, in TValue>
		where TPlcItem : IPlcItem
	{
		ILengthPlcItemBuilder<TPlcItem, TValue> AtPosition(ushort bytePosition);
		ILengthPlcItemBuilder<TPlcItem, TValue> AtPosition(ushort bytePosition, BitPosition bitPosition);
	}

	public interface ILengthPlcItemBuilder<out TPlcItem, in TValue>
		where TPlcItem : IPlcItem
	{
		IPlcItemCreator<TPlcItem> ForBitAmount(uint bitAmount);
		IPlcItemCreator<TPlcItem> ForByteAmount(uint byteAmount);
		IPlcItemCreator<TPlcItem> WithInitialValue(TValue initialValue);
	}

	public interface IPlcItemCreator<out TPlcItem>
		where TPlcItem : IPlcItem
	{
		TPlcItem Build();
	}
	
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public abstract class PlcItemConstructorBase<TPlcItem, TValue>
		: ITypePlcItemBuilder<TPlcItem, TValue>,
			IDataBlockPlcItemBuilder<TPlcItem, TValue>,
			IPositionPlcItemBuilder<TPlcItem, TValue>,
			ILengthPlcItemBuilder<TPlcItem, TValue>,
			IPlcItemCreator<TPlcItem>
		where TPlcItem : IPlcItem
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		protected string Identifier { get; private set; }

		protected PlcItemType? Type { get; private set; }

		protected ushort? DataBlock { get; private set; }

		protected ushort? Position { get; private set; }

		protected BitPosition? BitPosition { get; private set; }

		protected uint? BitAmount { get; private set; }

		protected uint? ByteAmount => this.BitAmount / 8;

		protected TValue InitialValue { get; private set; }

		#endregion

		#region (De)Constructors
		
		protected PlcItemConstructorBase(string identifier)
		{
			this.Identifier = identifier;
		}

		#endregion

		#region Methods

		public IDataBlockPlcItemBuilder<TPlcItem, TValue> ForData()
		{
			this.Type = PlcItemType.Data;
			return this;
		}

		public IPositionPlcItemBuilder<TPlcItem, TValue> ForFlags()
		{
			this.Type = PlcItemType.Flags;
			this.DataBlock = 0;
			return this;
		}

		public IPositionPlcItemBuilder<TPlcItem, TValue> ForInput()
		{
			this.Type = PlcItemType.Input;
			this.DataBlock = 0;
			return this;
		}

		public IPositionPlcItemBuilder<TPlcItem, TValue> ForOutput()
		{
			this.Type = PlcItemType.Output;
			this.DataBlock = 0;
			return this;
		}

		public IPositionPlcItemBuilder<TPlcItem, TValue> AtDatablock(ushort dataBlock)
		{
			this.DataBlock = dataBlock;
			return this;
		}

		public ILengthPlcItemBuilder<TPlcItem, TValue> AtPosition(ushort bytePosition) => this.AtPosition(bytePosition, Items.BitPosition.X0);

		public ILengthPlcItemBuilder<TPlcItem, TValue> AtPosition(ushort bytePosition, BitPosition bitPosition)
		{
			this.Position = bytePosition;
			this.BitPosition = bitPosition;
			return this;
		}

		public IPlcItemCreator<TPlcItem> ForBitAmount(uint bitAmount)
		{
			this.BitAmount = bitAmount;
			return this;
		}

		public IPlcItemCreator<TPlcItem> ForByteAmount(uint byteAmount)
		{
			this.BitAmount = byteAmount * 8;
			return this;
		}

		public IPlcItemCreator<TPlcItem> WithInitialValue(TValue initialValue)
		{
			this.InitialValue = initialValue;
			return this;
		}
		
		protected virtual void Validate()
		{
			if (this.Type is null) throw new PlcItemBuilderException(nameof(Type));
			if (this.DataBlock is null) throw new PlcItemBuilderException(nameof(DataBlock));
			if (this.Position is null) throw new PlcItemBuilderException(nameof(Position));
			if (this.BitPosition is null) throw new PlcItemBuilderException(nameof(BitPosition));
			if (this.BitAmount is null && this.InitialValue == null) throw new PlcItemBuilderException($"{nameof(BitAmount)} + {nameof(InitialValue)}", "One of those must not be null.");
		}

		public abstract TPlcItem Build();

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member