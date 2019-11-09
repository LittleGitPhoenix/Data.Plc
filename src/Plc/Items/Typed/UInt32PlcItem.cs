﻿using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for an <see cref="UInt32"/>.
	/// </summary>
	public class UInt32PlcItem : TypedBytesPlcItem<UInt32>, INumericPlcItem, IDeepCloneable<UInt32PlcItem>
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

		/// <inheritdoc />
		/// <param name="position"> The position where the <see cref="UInt32"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public UInt32PlcItem(ushort dataBlock, ushort position, uint initialValue = uint.MinValue, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				//byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf<uint>(),
				byteAmount: sizeof(uint),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override uint ConvertFromData(BitCollection data)
		{
			return BitConverter.ToUInt32(data, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(uint value)
		{
			return new BitCollection(false, BitConverter.GetBytes(value));
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		INumericPlcItem INumericPlcItem.Clone(string identifier) => this.Clone(identifier);

		/// <inheritdoc />
		public new UInt32PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="UInt32PlcItem"/>. </returns>
		public new UInt32PlcItem Clone(string identifier)
		{
			return new UInt32PlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}