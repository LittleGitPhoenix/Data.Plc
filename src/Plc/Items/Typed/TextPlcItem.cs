#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Linq;
using System.Text;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for <see cref="string"/>s with a definable <see cref="Encoding"/>.
	/// </summary>
	public class TextPlcItem : TypedBytesPlcItem<string>, IDeepCloneable<TextPlcItem>
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> The <see cref="Encoding"/> used to convert data into text. </summary>
		protected Encoding Encoding { get; }

		/// <summary> Does this item support changing the size of its <see cref="IPlcItem.Value"/> whenever a new value is assigned. </summary>
		protected bool AutomaticallyAdaptSize => ((IPlcItem) this).Value.AutomaticallyAdaptSize;

		#endregion

		#region (De)Constructors

		/// <inheritdoc />
		/// <param name="length"> The length of the ASCII-<see cref="String"/> within the <see cref="IPlcItem.DataBlock"/>. </param>
		/// <param name="encoding"> The <see cref="Encoding"/> used for text conversion. </param>
		public TextPlcItem(ushort dataBlock, ushort position, ushort length, Encoding encoding, string identifier = default)
			: this
			(
				dataBlock,
				position,
				new String('\0', length),
				encoding,
				false,
				identifier
			) { }

		/// <inheritdoc />
		/// <param name="encoding"> The <see cref="Encoding"/> used for text conversion. </param>
		public TextPlcItem(ushort dataBlock, ushort position, string initialValue, Encoding encoding, string identifier = default)
			: this
			(
				dataBlock,
				position,
				initialValue,
				encoding,
				false,
				identifier
			)
		{
			this.Encoding = encoding;
			this.Value = initialValue;
		}
		
		/// <inheritdoc />
		/// <param name="encoding"> The <see cref="Encoding"/> used for text conversion. </param>
		/// <param name="canChangeSize"> Should the size of this <see cref="IPlcItem"/>s underlying <see cref="BitCollection"/> be changed together with a new string value. </param>
		internal TextPlcItem(ushort dataBlock, ushort position, string initialValue, Encoding encoding, bool canChangeSize, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				byteAmount: (ushort) Math.Min(ushort.MaxValue, (initialValue is null ? 0 : encoding.GetBytes(initialValue).Length)),
				isFlexible: canChangeSize,
				initialValue: String.Empty,
				identifier
			)
		{
			this.Encoding = encoding;
			this.Value = initialValue;
		}

		#endregion

		#region Methods

		#region Validation

		/// <inheritdoc />
		public override string ValidateNewValue(string newValue)
		{
			return newValue ?? String.Empty;
		}

		#endregion

		#region Convert

		/// <inheritdoc />
		public override string ConvertFromData(BitCollection data)
		{
			return (data.ContainsOnlyZeros) ? String.Empty : this.Encoding.GetString(data);
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(string value)
		{
			var currentByteAmount = ((IPlcItem)this).Value.ByteLength;
			var automaticallyAdaptSize = this.AutomaticallyAdaptSize;

			// Empty or even NULL-Strings can be converted without encoding. That is the main reason, why this overridden abstract method can be called from the base class constructor without failing because the Encoding property is not set by then.
			if (String.IsNullOrEmpty(value))
			{
				if (automaticallyAdaptSize)
				{
					return new BitCollection(automaticallyAdaptSize, new byte[0]);
				}
				else
				{
					return new BitCollection(automaticallyAdaptSize, new byte[currentByteAmount]);
				}
			}
			
			var newData = this.Encoding.GetBytes(value);
			var newByteAmount = newData.Length;

			// Check if the new data has the same length like before.
			if (newByteAmount != currentByteAmount)
			{
				// Check if this item is flexible and must adapt to its new value.
				if (automaticallyAdaptSize)
				{
					// YES: Resize the underlying BitCollection to match the new value.
					((IPlcItem) this).Value.Resize(((uint) newByteAmount * 8));

					/*!
					Since it is possible for the BitCollection to be smaller then specified (when using this item as part of an IDynamicPlcItem with a limited maximum size),
					check if value needs to be truncated too.
					*/
					var newLength = ((IPlcItem) this).Value.ByteLength;
					if (value.Length > newLength) newData = newData.Take((int) newLength).ToArray();
				}
				else
				{
					// NO: Copy the encoded data into a fixed sized array. This way longer strings are truncated and shorter ones are padded.
					var data = new byte[currentByteAmount];
					Buffer.BlockCopy(newData, 0, data, 0, Math.Min(newData.Length, data.Length));
					newData = data;
				}
			}

			return new BitCollection(automaticallyAdaptSize, newData);
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new TextPlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="TextPlcItem"/>. </returns>
		public new TextPlcItem Clone(string identifier)
		{
			return new TextPlcItem(base.DataBlock, base.Position, this.Value, this.Encoding, this.AutomaticallyAdaptSize, identifier);
		}

		#endregion

		#endregion
	}
}