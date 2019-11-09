using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for an <see cref="Int64"/>.
	/// </summary>
	public class Int64PlcItem : TypedBytesPlcItem<Int64>, IDeepCloneable<Int64PlcItem>
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
		/// <param name="position"> The position where the <see cref="Int64"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public Int64PlcItem(ushort dataBlock, ushort position, long initialValue = default, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				//byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf<long>(),
				byteAmount: sizeof(long),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override long ConvertFromData(BitCollection data)
		{
			return BitConverter.ToInt64(data, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(long value)
		{
			return new BitCollection(false, BitConverter.GetBytes(value));
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new Int64PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="Int64PlcItem"/>. </returns>
		public new Int64PlcItem Clone(string identifier)
		{
			return new Int64PlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}