using System;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Mock
{
	/// <summary>
	/// Extension for directly changing the data of <see cref="byte"/> array according to different types of values.
	/// </summary>
	public static class ByteArrayExtensions
	{
		/// <summary>
		/// Applies the <see cref="bool"/> array to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, bool[] value)
			=> data.ApplyValue(bytePosition, BitPosition.X0, value);

		/// <summary>
		/// Applies the <see cref="bool"/> array to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="bitPosition"> A specific <see cref="BitPosition"/> at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, BitPosition bitPosition, bool[] value)
		{
			var offset = bytePosition * 8 + (int)bitPosition;
			if (offset + value.Length > data.Length * 8) throw new ArgumentOutOfRangeException($"The combined length of byte position, bit position and the new values length with a total of {offset + value.Length} bits is outside of the data length of {data.Length * 8} bits.");

			var currentBooleans = DataConverter.ToBooleans(data);
			for (var index = 0; index < value.Length; index++)
			{
				currentBooleans[index + offset] = value[index];
			}

			return data.ApplyValue(0, DataConverter.ToBytes(currentBooleans));
		}

		/// <summary>
		/// Applies the <see cref="byte"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, byte value)
		{
			if (data.Length < bytePosition) throw new ArgumentOutOfRangeException(nameof(bytePosition), $"The byte position {bytePosition} is outside of the data length of {data.Length}.");
			data[bytePosition] = value;
			return data;
		}

		/// <summary>
		/// Applies the <see cref="byte"/> array to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, byte[] value)
		{
			var totalLength = bytePosition + value.Length;
			if (totalLength > data.Length) throw new ArgumentOutOfRangeException($"The combined length of byte position and the new values length with a total of {totalLength} bytes is outside of the data length of {data.Length} bytes.");

			for (int index = 0; index < value.Length; index++)
			{
				data[bytePosition + index] = value[index];
			}

			return data;
		}

		/// <summary>
		/// Applies the <see cref="Int16"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="endianness"> The <see cref="DataConverter.Endianness"/> used when converting the value. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, Int16 value, DataConverter.Endianness endianness)
			=> data.ApplyValue(bytePosition, DataConverter.ToBytes(value, endianness));

		/// <summary>
		/// Applies the <see cref="UInt16"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="endianness"> The <see cref="DataConverter.Endianness"/> used when converting the value. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, UInt16 value, DataConverter.Endianness endianness)
			=> data.ApplyValue(bytePosition, DataConverter.ToBytes(value, endianness));

		/// <summary>
		/// Applies the <see cref="Int32"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="endianness"> The <see cref="DataConverter.Endianness"/> used when converting the value. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, Int32 value, DataConverter.Endianness endianness)
			=> data.ApplyValue(bytePosition, DataConverter.ToBytes(value, endianness));

		/// <summary>
		/// Applies the <see cref="UInt32"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="endianness"> The <see cref="DataConverter.Endianness"/> used when converting the value. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, UInt32 value, DataConverter.Endianness endianness)
			=> data.ApplyValue(bytePosition, DataConverter.ToBytes(value, endianness));

		/// <summary>
		/// Applies the <see cref="Int64"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="endianness"> The <see cref="DataConverter.Endianness"/> used when converting the value. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, Int64 value, DataConverter.Endianness endianness)
			=> data.ApplyValue(bytePosition, DataConverter.ToBytes(value, endianness));

		/// <summary>
		/// Applies the <see cref="UInt64"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="endianness"> The <see cref="DataConverter.Endianness"/> used when converting the value. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, UInt64 value, DataConverter.Endianness endianness)
			=> data.ApplyValue(bytePosition, DataConverter.ToBytes(value, endianness));

		/// <summary>
		/// Applies the <see cref="string"/> value to the <paramref name="data"/>.
		/// </summary>
		/// <param name="data"> The extended <see cref="byte"/> array. </param>
		/// <param name="bytePosition"> The starting position at where to apply <paramref name="value"/>. </param>
		/// <param name="value"> The value to apply. </param>
		/// <param name="encoding"> The <see cref="System.Text.Encoding"/> used to encode the string. </param>
		/// <returns> The same <paramref name="data"/> instance. </returns>
		public static byte[] ApplyValue(this byte[] data, ushort bytePosition, string value, System.Text.Encoding encoding)
			=> data.ApplyValue(bytePosition, encoding.GetBytes(value));
	}
}