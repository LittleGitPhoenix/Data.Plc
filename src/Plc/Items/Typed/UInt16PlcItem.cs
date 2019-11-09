using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for an <see cref="UInt16"/>.
	/// </summary>
	public class UInt16PlcItem : TypedBytesPlcItem<UInt16>, INumericPlcItem, IDeepCloneable<UInt16PlcItem>
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
		/// <param name="position"> The position where the <see cref="UInt16"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public UInt16PlcItem(ushort dataBlock, ushort position, ushort initialValue = ushort.MinValue, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				//byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf<ushort>(),
				byteAmount: sizeof(ushort),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override ushort ConvertFromData(BitCollection data)
		{
			return BitConverter.ToUInt16(data, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(ushort value)
		{
			return new BitCollection(false, BitConverter.GetBytes(value));
		}

		#endregion

		#region Clone
		
		/// <inheritdoc />
		INumericPlcItem INumericPlcItem.Clone(string identifier) => (INumericPlcItem) this.Clone(identifier);

		/// <inheritdoc />
		public new UInt16PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="UInt16PlcItem"/>. </returns>
		public new UInt16PlcItem Clone(string identifier)
		{
			return new UInt16PlcItem( base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}