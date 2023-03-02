#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for <c>S7-LWord</c>.
	/// </summary>
	public sealed class LWordPlcItem : TypedBytesPlcItem<UInt64>, IDeepCloneable<LWordPlcItem>
	{
		#region Delegates / Events
		#endregion

		#region Constants

		internal static DataConverter.Endianness Endianness = DataConverter.Endianness.BigEndian;

		#endregion

		#region Fields
		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		/// <inheritdoc />
		/// <param name="position"> The position where the <c>S7-LWord</c> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public LWordPlcItem(ushort dataBlock, ushort position, ulong initialValue = ulong.MinValue, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				byteAmount: sizeof(ulong),
				false,
				initialValue,
				identifier
			)
		{ }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override ulong ConvertFromData(BitCollection data)
		{
			return DataConverter.ToUInt64(data, Endianness);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(ulong value)
		{
			var bytes = DataConverter.ToBytes(value, Endianness);
			return new BitCollection(false, bytes);
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new LWordPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="LWordPlcItem"/>. </returns>
		public new LWordPlcItem Clone(string identifier)
		{
			return new LWordPlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}