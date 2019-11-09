using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for an <see cref="Int32"/>.
	/// </summary>
	public sealed class Int32PlcItem : TypedBytesPlcItem<Int32>, IDeepCloneable<Int32PlcItem>
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
		/// <param name="position"> The position where the <see cref="Int32"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public Int32PlcItem(ushort dataBlock, ushort position, int initialValue = default, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				//byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf<int>(),
				byteAmount: sizeof(int),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override int ConvertFromData(BitCollection data)
		{
			return BitConverter.ToInt32(data, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(int value)
		{
			return new BitCollection(false, BitConverter.GetBytes(value));
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new Int32PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="Int32PlcItem"/>. </returns>
		public new Int32PlcItem Clone(string identifier)
		{
			return new Int32PlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}