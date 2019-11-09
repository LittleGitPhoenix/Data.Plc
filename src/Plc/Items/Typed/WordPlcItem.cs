using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for <c>S7-Word</c>.
	/// </summary>
	public sealed class WordPlcItem : UInt16PlcItem, INumericPlcItem, IDeepCloneable<WordPlcItem>
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
		/// <param name="position"> The position where the <c>S7-Word</c> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
		public WordPlcItem(ushort dataBlock, ushort position, ushort initialValue = ushort.MinValue, string identifier = default)
			: base(dataBlock, position, initialValue, identifier) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override ushort ConvertFromData(BitCollection data)
		{
			// PLC uses littleENDIAN, so toggle the bytes.
			byte[] bytes = data;
			Array.Reverse(bytes);

			return BitConverter.ToUInt16(bytes, 0);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			Array.Reverse(bytes);

			// PLC uses littleENDIAN, so toggle the bytes.
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