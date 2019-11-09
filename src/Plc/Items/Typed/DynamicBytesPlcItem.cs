﻿namespace Phoenix.Data.Plc.Items.Typed
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
			: this(numericPlcItem, new byte[0], identifier) { }

		/// <inheritdoc />
		public DynamicBytesPlcItem(INumericPlcItem numericPlcItem, byte[] initialValue, string identifier = default)
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
			return new DynamicBytesPlcItem(base.LengthPlcItem, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}