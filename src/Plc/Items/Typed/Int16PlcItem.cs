using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for an <see cref="Int16"/>.
	/// </summary>
	public sealed class Int16PlcItem : TypedBytesPlcItem<Int16>, IDeepCloneable<Int16PlcItem>
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
		/// <param name="position"> The position where the <see cref="Int16"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public Int16PlcItem(ushort dataBlock, ushort position, short initialValue = default, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				//byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf<short>(),
				byteAmount: sizeof(short),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override short ConvertFromData(BitCollection data)
		{
			return BitConverter.ToInt16(data, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(short value)
		{
			return new BitCollection(false, BitConverter.GetBytes(value));
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new Int16PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="Int16PlcItem"/>. </returns>
		public new Int16PlcItem Clone(string identifier)
		{
			return new Int16PlcItem(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}