using System;
using System.Linq;
using Phoenix.Data.Plc.Items.Builder;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> representing an <see cref="Enum"/>.
	/// </summary>
	/// <remarks> Casting the values back from plc data into <typeparamref name="TEnum"/> may fail if the new value is not defined on the enumeration. In this case if the enumeration has <c>unknown</c> or <c>undefined</c> defined as values, those will be used as fallback. </remarks>
	public class EnumPlcItem<TEnum> : TypedBytesPlcItem<TEnum>, IDeepCloneable<EnumPlcItem<TEnum>>
		where TEnum : Enum
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
		/// <remarks> The length will be inferred by the enumerations underlying type. </remarks>
		public EnumPlcItem(ushort dataBlock, ushort position, TEnum initialValue = default, string identifier = default)
			: base
			(
				PlcItemType.Data,
				dataBlock,
				position,
				byteAmount: (ushort) System.Runtime.InteropServices.Marshal.SizeOf(typeof(TEnum).GetEnumUnderlyingType()),
				false,
				initialValue,
				identifier
			) { }

		#endregion

		#region Methods

		#region Convert

		/// <inheritdoc />
		/// <exception cref="ArgumentOutOfRangeException"> Is thrown if the <paramref name="data"/> could not be converted back into the enumeration. </exception>
		public override TEnum ConvertFromData(BitCollection data)
		{
			// Check if the underlying enumeration type has such a numerical value.
			if (TryConvertToEnum(data, out object numerical, out TEnum enumeration))
			{
				// YES: Return the enumeration value.
				return enumeration;
			}
			else
			{
				// NO: Check if the enumeration has either a value that is named 'unknown' or 'undefined' and use this as return value.
				var undefinedValues = new[] { "unknown", "undefined"};
				foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
				{
					// Check the name.
					var description = value.ToString().ToLower();
					if (undefinedValues.Contains(description))
					{
						return value;
					}
				}
			}

			throw new ArgumentOutOfRangeException($"The numerical data from the plc has a value of '{numerical}' that is not defined on {typeof(TEnum)}.");
		}

		/// <inheritdoc />
		public override BitCollection ConvertToData(TEnum value)
		{
			var byteAmount = (int) ((IPlcItem) this).Value.ByteLength;
			var numerical = Convert.ToInt64(value);
			var data = BitConverter.GetBytes(numerical);
			return new BitCollection(false, data.Take(byteAmount).ToArray());
		}

		/// <summary>
		/// Tries to convert the passed <paramref name="data"/> into <typeparamref name="TEnum"/>.
		/// </summary>
		/// <param name="data"> The data to convert. </param>
		/// <param name="numerical"> The numerical representation of <paramref name="data"/>. </param>
		/// <param name="enumeration"> The converted <typeparamref name="TEnum"/>. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		private static bool TryConvertToEnum(byte[] data, out object numerical, out TEnum enumeration)
		{
			numerical = default;
			enumeration = default;

			var underlyingType = typeof(TEnum).GetEnumUnderlyingType();
			if (underlyingType == typeof(SByte))
			{
				unchecked { numerical = (sbyte) data[0]; }
			}
			else if (underlyingType == typeof(Int16))
			{
				numerical = BitConverter.ToInt16(data, 0);
			}
			else if (underlyingType == typeof(Int32))
			{
				numerical = BitConverter.ToInt32(data, 0);
			}
			else if (underlyingType == typeof(Int64))
			{
				numerical = BitConverter.ToInt64(data, 0);
			}
			else if (underlyingType == typeof(Byte))
			{
				numerical = data[0];
			}
			else if (underlyingType == typeof(UInt16))
			{
				numerical = BitConverter.ToUInt16(data, 0);
			}
			else if (underlyingType == typeof(UInt32))
			{
				numerical = BitConverter.ToUInt32(data, 0);
			}
			else if (underlyingType == typeof(UInt64))
			{
				numerical = BitConverter.ToUInt64(data, 0);
			}

			if (numerical is null) return false;
			if (Enum.IsDefined(typeof(TEnum), numerical))
			{
				// YES: Return the enumeration value.
				enumeration = (TEnum) Enum.ToObject(typeof(TEnum), numerical);
				return true;
			}
			return false;
		}

		#endregion

		#region Clone

		/// <inheritdoc />
		public new EnumPlcItem<TEnum> Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="BitsPlcItem"/>. </returns>
		public new EnumPlcItem<TEnum> Clone(string identifier)
		{
			return new EnumPlcItem<TEnum>(base.DataBlock, base.Position, this.Value, identifier);
		}

		#endregion

		#endregion
	}
}