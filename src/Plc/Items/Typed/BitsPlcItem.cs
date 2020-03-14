#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// Direct descendant of <see cref="PlcItem"/> with easier to use constructors.
	/// </summary>
	public class BitsPlcItem : PlcItem, IDeepCloneable<BitsPlcItem>
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
		/// Constructor for <see cref="PlcItemType.Data"/> <see cref="BitsPlcItem"/>s with bit amount.
		/// </summary>
		public BitsPlcItem(ushort dataBlock, ushort position, BitPosition bitPosition, byte bitAmount, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, bitPosition, bitAmount, identifier)
		{ }

		/// <inheritdoc />
		/// <summary>
		/// Constructor with bit amount.
		/// </summary>
		public BitsPlcItem(PlcItemType type, ushort dataBlock, ushort position, BitPosition bitPosition, byte bitAmount, string identifier = default)
			: base(type, dataBlock, position, bitPosition, bitAmount, isFlexible: false, identifier: identifier)
		{ }

		/// <inheritdoc />
		/// <summary>
		/// Constructor for <see cref="PlcItemType.Data"/> <see cref="BitsPlcItem"/>s with initial value.
		/// </summary>
		public BitsPlcItem(ushort dataBlock, ushort position, BitPosition bitPosition, BitCollection initialValue, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, bitPosition, initialValue, identifier)
		{ }

		/// <inheritdoc />
		/// <summary>
		/// Constructor with initial value.
		/// </summary>
		public BitsPlcItem(PlcItemType type, ushort dataBlock, ushort position, BitPosition bitPosition, BitCollection initialValue, string identifier = default)
			: base(type, dataBlock, position, bitPosition, initialValue, identifier)
		{ }

		#endregion

		#region Methods

		#region Clone

		/// <inheritdoc />
		public new BitsPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="BitsPlcItem"/>. </returns>
		public new BitsPlcItem Clone(string identifier)
		{
			return new BitsPlcItem(base.Type, base.DataBlock, base.Position, base.BitPosition, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}