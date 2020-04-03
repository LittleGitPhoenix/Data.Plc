#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for <c>S7-DWord</c>.
	/// </summary>
	public sealed class DWordPlcItem : UInt32PlcItem, INumericPlcItem, IDeepCloneable<DWordPlcItem>
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
		/// <param name="position"> The position where the <c>S7-DWord</c> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public DWordPlcItem(ushort dataBlock, ushort position, uint initialValue = uint.MinValue, string identifier = default)
			: base(dataBlock, position, initialValue, identifier) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override uint ConvertFromData(BitCollection data)
		{
			// PLC uses BigEndian, so toggle the bytes.
			byte[] bytes = data;
			Array.Reverse(bytes);

			return BitConverter.ToUInt32(bytes, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(uint value)
		{
			// PLC uses BigEndian, so toggle the bytes.
			value = DataConverter.SwapBytes(value);
			byte[] bytes = BitConverter.GetBytes(value);
			//Array.Reverse(bytes);

			return new BitCollection(false, bytes);
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		INumericPlcItem INumericPlcItem.Clone(string identifier) => this.Clone(identifier);

		/// <inheritdoc />
		public new DWordPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="DWordPlcItem"/>. </returns>
		public new DWordPlcItem Clone(string identifier)
		{
			return new DWordPlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}