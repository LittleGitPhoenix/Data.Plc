#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> representing <see cref="Byte"/>s.
	/// </summary>
	public sealed class BytesPlcItem : TypedBytesPlcItem<byte[]>, IDeepCloneable<BytesPlcItem>
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> Does this item support changing the size of its <see cref="IPlcItem.Value"/> whenever a new value is assigned. </summary>
		private bool AutomaticallyAdaptSize => ((IPlcItem)this).Value.AutomaticallyAdaptSize;

		#endregion

		#region (De)Constructors

		/// <inheridoc />
		/// <summary>
		/// Constructor for <see cref="PlcItemType.Data"/> <see cref="BytesPlcItem"/>s with amount.
		/// </summary>
		public BytesPlcItem(ushort dataBlock, ushort position, ushort byteAmount, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, byteAmount, identifier)
		{ }
		
		/// <summary>
		/// Constructor with amount.
		/// </summary>
		/// <param name="identifier"> <see cref="IPlcItem.Identifier"/> </param>
		/// <param name="type"> <see cref="IPlcItem.Type"/> </param>
		/// <param name="dataBlock"> <see cref="IPlcItem.DataBlock"/> </param>
		/// <param name="position"> <see cref="IPlcItem.Position"/> </param>
		/// <param name="byteAmount"> The total amount of <see cref="Byte"/>s for this item. </param>
		public BytesPlcItem(PlcItemType type, ushort dataBlock, ushort position, ushort byteAmount, string identifier = default)
			: this(type, dataBlock, position, false, new byte[byteAmount], identifier)
		{ }

		/// <inheridoc />
		/// <summary>
		/// Constructor for <see cref="PlcItemType.Data"/> <see cref="BytesPlcItem"/>s with initial value.
		/// </summary>
		public BytesPlcItem(ushort dataBlock, ushort position, byte[] initialValue, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, false, initialValue, identifier)
		{ }
		
		/// <summary>
		/// Constructor with initial value.
		/// </summary>
		/// <param name="identifier"> <see cref="IPlcItem.Identifier"/> </param>
		/// <param name="type"> <see cref="IPlcItem.Type"/> </param>
		/// <param name="dataBlock"> <see cref="IPlcItem.DataBlock"/> </param>
		/// <param name="position"> <see cref="IPlcItem.Position"/> </param>
		/// <param name="initialValue"> The initial value. </param>
		public BytesPlcItem(PlcItemType type, ushort dataBlock, ushort position, byte[] initialValue, string identifier = default)
			: this(type, dataBlock, position, false, initialValue, identifier)
		{ }
		
		/// <inheridoc />
		/// <summary>
		/// Constructor used by <see cref="DynamicBytesPlcItem"/>.
		/// </summary>
		internal BytesPlcItem(ushort dataBlock, ushort position, bool isFlexible, byte[] initialValue, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, isFlexible, initialValue, identifier)
		{ }

		/// <summary>
		/// Constructor with initial value.
		/// </summary>
		/// <param name="identifier"> <see cref="IPlcItem.Identifier"/> </param>
		/// <param name="type"> <see cref="IPlcItem.Type"/> </param>
		/// <param name="dataBlock"> <see cref="IPlcItem.DataBlock"/> </param>
		/// <param name="position"> <see cref="IPlcItem.Position"/> </param>
		/// <param name="isFlexible"> Is the <see cref="IPlcItem.Value"/>s <see cref="BitCollection"/> fixed or will it change size according to its data. </param>
		/// <param name="initialValue"> The initial value. </param>
		private BytesPlcItem(PlcItemType type, ushort dataBlock, ushort position, bool isFlexible, byte[] initialValue, string identifier = default)
			: base
			(
				type,
				dataBlock,
				position,
				byteAmount: (ushort) (initialValue?.Length ?? 0),
				isFlexible,
				initialValue ?? new byte[0],
				identifier
			) { }

		#endregion

		#region Methods

		#region Validation

		/// <inheritdoc />
		public override byte[] ValidateNewValue(byte[] newValue)
		{
			return newValue ?? new byte[0];
		}

		#endregion

		#region Convert

		/// <inheritdoc />
		public override byte[] ConvertFromData(BitCollection data)
		{
			return data;
		}
		
		/// <inheritdoc />
		public override BitCollection ConvertToData(byte[] value)
		{
			var automaticallyAdaptSize = this.AutomaticallyAdaptSize;
			if (automaticallyAdaptSize)
			{
				// YES: Resize the underlying BitCollection to match the new value.
				((IPlcItem) this).Value.Resize(((uint) value.Length * 8));
			}
			return new BitCollection(automaticallyAdaptSize, value);
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new BytesPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="BytesPlcItem"/>. </returns>
		public new BytesPlcItem Clone(string identifier)
		{
			return new BytesPlcItem(base.Type, base.DataBlock, base.Position, this.AutomaticallyAdaptSize, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}