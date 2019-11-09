using System;
using System.ComponentModel;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for an <see cref="UInt64"/>.
	/// </summary>
	public class UInt64PlcItem : TypedBytesPlcItem<UInt64>, IDeepCloneable<UInt64PlcItem>
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
		/// <param name="position"> The position where the <see cref="UInt64"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public UInt64PlcItem(ushort dataBlock, ushort position, ulong initialValue = ulong.MinValue, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				//byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf<ulong>(),
				byteAmount: sizeof(ulong),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override ulong ConvertFromData(BitCollection data)
		{
			return BitConverter.ToUInt64(data, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(ulong value)
		{
			return new BitCollection(false, BitConverter.GetBytes(value));
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new UInt64PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="UInt64PlcItem"/>. </returns>
		public new UInt64PlcItem Clone(string identifier)
		{
			return new UInt64PlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}