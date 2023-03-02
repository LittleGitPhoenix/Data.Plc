#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Text;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IDynamicPlcItem"/> for <see cref="Encoding.UTF8"/> <see cref="string"/>s.
	/// </summary>
	public sealed class DynamicUtf8PlcItem : DynamicPlcItem<string>, IDeepCloneable<DynamicUtf8PlcItem>
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
		public DynamicUtf8PlcItem(INumericPlcItem numericPlcItem, string identifier = default)
			: this(numericPlcItem, 1, null, String.Empty, identifier)
		{ }

		/// <inheritdoc />
		public DynamicUtf8PlcItem(INumericPlcItem numericPlcItem, byte lengthFactor, uint? lengthLimit, string identifier = default)
			: this(numericPlcItem, lengthFactor, lengthLimit, String.Empty, identifier)
		{ }

		/// <inheritdoc />
		public DynamicUtf8PlcItem(INumericPlcItem numericPlcItem, string initialValue = default, string identifier = default)
			: this(numericPlcItem, 1, null, initialValue, identifier) { }

		/// <inheritdoc />
		public DynamicUtf8PlcItem(INumericPlcItem numericPlcItem, byte lengthFactor, uint? lengthLimit, string initialValue = default, string identifier = default)
			: base
			(
				numericPlcItem,
				name =>
					new Utf8PlcItem
					(
						dataBlock: numericPlcItem.DataBlock,
						position: (ushort) (numericPlcItem.Position + ((IPlcItem) numericPlcItem).Value.ByteLength),
						initialValue: initialValue,
						canChangeSize: true,
						identifier: name
					),
				lengthFactor,
				lengthLimit, identifier
			)
		{ }

		#endregion

		#region Methods

		#region Clone

		/// <inheritdoc />
		public new DynamicUtf8PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="DynamicUtf8PlcItem"/>. </returns>
		public new DynamicUtf8PlcItem Clone(string identifier)
		{
			return new DynamicUtf8PlcItem(base.LengthPlcItem, base.LengthFactor, base.LengthLimit, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}