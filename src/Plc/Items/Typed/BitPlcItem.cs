#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> representing a <see cref="bool"/>.
	/// </summary>
	public sealed class BitPlcItem : TypedBitsPlcItem<bool>, IDeepCloneable<BitPlcItem>
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
		/// Constructor for <see cref="PlcItemType.Data"/> <see cref="BitPlcItem"/>s with bit amount.
		/// </summary>
		public BitPlcItem(ushort dataBlock, ushort position, BitPosition bitPosition, bool initialValue = default, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, bitPosition, initialValue, identifier) { }

		/// <summary>
		/// Constructor
		/// </summary>
		public BitPlcItem(PlcItemType type, ushort dataBlock, ushort position, BitPosition bitPosition, bool initialValue = default, string identifier = default)
			: base(type, dataBlock, position, bitPosition, bitAmount: 1, false, initialValue, identifier) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override bool ConvertFromData(BitCollection data)
		{
			return data[0];
		}		

		/// <inheritdoc />
		public override BitCollection ConvertToData(bool value)
		{
			return new BitCollection(false, value);
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new BitPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="BitPlcItem"/>. </returns>
		public new BitPlcItem Clone(string identifier)
		{
			return new BitPlcItem(base.Type, base.DataBlock, base.Position, base.BitPosition, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}