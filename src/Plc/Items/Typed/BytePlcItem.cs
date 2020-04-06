#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> representing a single <see cref="byte"/>.
	/// </summary>
	public sealed class BytePlcItem : TypedBytesPlcItem<byte>, INumericPlcItem, IDeepCloneable<BytePlcItem>
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

		/// <inheridoc />
		/// <summary>
		/// Constructor for <see cref="PlcItemType.Data"/> <see cref="BytePlcItem"/>s.
		/// </summary>
		public BytePlcItem(ushort dataBlock, ushort position, byte initialValue = default, string identifier = default)
			: this(PlcItemType.Data, dataBlock, position, initialValue: initialValue, identifier) { }

		/// <inheridoc />
		/// <summary>
		/// Constructor
		/// </summary>
		public BytePlcItem(PlcItemType type, ushort dataBlock, ushort position, byte initialValue = default, string identifier = default)
			: base(type, dataBlock, position, byteAmount: 1, false, initialValue, identifier) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		public override byte ConvertFromData(BitCollection data)
		{
			return ((byte[]) data)[0];
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(byte value)
		{
			return new BitCollection(false, new[] { value });
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		INumericPlcItem INumericPlcItem.Clone(string identifier) => this.Clone(identifier);

		/// <inheritdoc />
		public new BytePlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="BytePlcItem"/>. </returns>
		public new BytePlcItem Clone(string identifier)
		{
			return new BytePlcItem(base.Type, base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion
		
		#endregion
	}
}