#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for <c>S7-Word</c>.
	/// </summary>
	//public sealed class WordPlcItem : UInt16PlcItem, INumericPlcItem, IDeepCloneable<WordPlcItem>
	public sealed class WordPlcItem : TypedBytesPlcItem<UInt16>, INumericPlcItem, IDeepCloneable<WordPlcItem>
	{
		#region Delegates / Events
		#endregion

		#region Constants

		internal static DataConverter.Endianness Endianness = DataConverter.Endianness.BigEndian;

		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <inheritdoc />
		uint INumericPlcItem.Value
		{
			get => this.Value;
			set => this.Value = value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
		}

		#endregion

		#region (De)Constructors

		/// <inheritdoc />
		/// <param name="position"> The position where the <c>S7-Word</c> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public WordPlcItem(ushort dataBlock, ushort position, ushort initialValue = ushort.MinValue, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				byteAmount: sizeof(ushort),
				false,
				initialValue,
				identifier
			)
		{ }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override ushort ConvertFromData(BitCollection data)
		{
			return DataConverter.ToUInt16(data, Endianness);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(ushort value)
		{
			var bytes = DataConverter.ToBytes(value, Endianness);
			return new BitCollection(false, bytes);
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		INumericPlcItem INumericPlcItem.Clone(string identifier) => this.Clone(identifier);

		/// <inheritdoc />
		public new WordPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="WordPlcItem"/>. </returns>
		public new WordPlcItem Clone(string identifier)
		{
			return new WordPlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}