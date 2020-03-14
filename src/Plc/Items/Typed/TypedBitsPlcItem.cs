#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="BitsPlcItem"/> with concrete data as <typeparamref name="TValue"/>.
	/// </summary>
	/// <typeparam name="TValue"> The concrete type of the value. </typeparam>
	public abstract class TypedBitsPlcItem<TValue> : TypedPlcItem<TValue>
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
		protected TypedBitsPlcItem
		(
			PlcItemType type,
			ushort dataBlock,
			ushort position,
			BitPosition bitPosition,
			byte bitAmount,
			bool isFlexible,
			TValue initialValue,
			string identifier = default
		)
			: base(type, dataBlock, position, bitPosition, bitAmount, isFlexible, initialValue, identifier) { }

		#endregion

		#region Methods
		#endregion
	}
}