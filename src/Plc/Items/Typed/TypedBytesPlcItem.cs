using System;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="BytesPlcItem"/> with concrete data as <typeparamref name="TValue"/>.
	/// </summary>
	/// <typeparam name="TValue"> The concrete type of the value. </typeparam>
	public abstract class TypedBytesPlcItem<TValue> : TypedPlcItem<TValue>
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
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="byteAmount"> The amount of <see cref="Byte"/>s of this item. </param>
		protected TypedBytesPlcItem
		(
			PlcItemType type,
			ushort dataBlock,
			ushort position,
			ushort byteAmount,
			bool isFlexible,
			TValue initialValue,
			string identifier = default
		)
			: base(type, dataBlock, position, Items.BitPosition.X0, bitAmount: (uint) byteAmount * 8, isFlexible, initialValue, identifier) { }

		#endregion

		#region Methods
		#endregion
	}
}