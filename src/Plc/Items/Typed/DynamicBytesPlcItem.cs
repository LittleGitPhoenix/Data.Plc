﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Linq;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IDynamicPlcItem"/> for pure byte data.
	/// </summary>
	public sealed class DynamicBytesPlcItem : DynamicPlcItem<byte[]>, IDeepCloneable<DynamicBytesPlcItem>
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
		public DynamicBytesPlcItem(INumericPlcItem numericPlcItem, string identifier = default)
			: this(numericPlcItem, new byte[0], null, 1, identifier) { }

		/// <inheritdoc />
		public DynamicBytesPlcItem(INumericPlcItem numericPlcItem, uint? lengthLimit, byte lengthFactor, string identifier = default)
			: this(numericPlcItem, new byte[0], lengthLimit, lengthFactor, identifier) { }

		/// <inheritdoc />
		public DynamicBytesPlcItem(INumericPlcItem numericPlcItem, byte[] initialValue, string identifier = default)
			: this(numericPlcItem, initialValue, null, 1, identifier) { }

		/// <inheritdoc />
		public DynamicBytesPlcItem(INumericPlcItem numericPlcItem, byte[] initialValue, uint? lengthLimit, byte lengthFactor, string identifier = default)
			: base
			(
				numericPlcItem,
				name =>
					new BytesPlcItem
					(
						dataBlock: numericPlcItem.DataBlock,
						position: (ushort) (numericPlcItem.Position + numericPlcItem.Value.ByteLength),
						isFlexible: true,
						initialValue: initialValue,
						identifier: name
					),
				lengthLimit,
				lengthFactor,
				identifier
			)
		{ }

		#endregion

		#region Methods

		#region Clone

		/// <inheritdoc />
		public new DynamicBytesPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="DynamicBytesPlcItem"/>. </returns>
		public new DynamicBytesPlcItem Clone(string identifier)
		{
			return new DynamicBytesPlcItem(base.LengthPlcItem, this.Value, ((IDynamicPlcItem)this).LengthLimit, base.LengthFactor, identifier);
		}

		#endregion

		#endregion
	}
}