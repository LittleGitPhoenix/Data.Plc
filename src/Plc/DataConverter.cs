#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// Helper class for conversion of bits and bytes.
	/// </summary>
	public static class DataConverter
	{
		/// <summary> Different byte order types. </summary>
		public enum Endianness : byte
		{
			/// <summary> Endianness of most platforms supported by .NET. </summary>
			LittleEndian,
			/// <summary> Endianness of platforms like Siemens plc. </summary>
			BigEndian
		}

		#region Value ⇒ Byte
		
		/// <summary>
		/// Converts the passed eight <see cref="Boolean"/>s into a single <see cref="Byte"/>.
		/// </summary>
		/// <param name="booleans"> The <see cref="Boolean"/>s that should be converted. </param>
		/// <returns> A <see cref="Byte"/>. </returns>
		/// <exception cref="ArgumentOutOfRangeException"> More than eight <see cref="Boolean"/>s where passed. </exception>
		public static byte ToByte(bool[] booleans)
		{
			if (booleans.Length > 8) throw new ArgumentOutOfRangeException(paramName: nameof(booleans), actualValue: booleans.Length, message: $"The boolean array must not exceed {1 * 8} elements.");
			return DataConverter.ToBytes(booleans)[0];
		}

		#endregion

		#region Value ⇒ Boolean-Array

		/// <summary>
		/// Converts the <paramref name="byte"/> into a <see cref="bool"/> array.
		/// </summary>
		/// <param name="byte"> The <see cref="Byte"/> to convert. </param>
		/// <returns> A new boolean array. </returns>
		public static bool[] ToBooleans(byte @byte)
			=> DataConverter.ToBooleans(new[] { @byte });
		
		/// <summary>
		/// Converts the <paramref name="bytes"/> into a <see cref="bool"/> array.
		/// </summary>
		/// <param name="bytes"> The data to convert. </param>
		/// <returns> A new boolean array. </returns>
		public static bool[] ToBooleans(byte[] bytes)
		{
			if (bytes == null) return null;

			bool[] booleans = new bool[bytes.Length * 8];
			
			// Check each bit.
			for (int i = 0; i < booleans.Length; i++)
			{
				byte @byte = bytes[i/8];
				booleans[i] = (@byte & (1 << i % 8)) != 0;
			}
			
			return booleans;
		}

		#endregion

		#region Value ⇒ Data

		/// <summary>
		/// Converts the passed <paramref name="booleans"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="booleans"> The <see cref="Boolean"/>s that should be converted. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(bool[] booleans)
		{
			// Calculate the length of the bytes array that will be returned.
			uint boolLength = (uint) booleans.Length;
			uint byteLength = DataHelper.GetByteAmountForBits(boolLength);
			byte[] bytes = new byte[byteLength];

			for (var i = 0; i < booleans.Length; i++)
			{
				if (booleans[i]) bytes[i / 8] |= (byte)(1 << (i % 8));
			}
			
			return bytes;
		}

		#region Signed
		
		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="short"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(short value, Endianness endianness)
		{
			var bytes = new byte[sizeof(short)];
			ToBytes(value, endianness, bytes.AsSpan());
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="short"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <param name="destination"> The <see cref="Span{T}"/> where the result will be written to. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static bool ToBytes(short value, Endianness endianness, Span<byte> destination)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteInt16LittleEndian(destination, value);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteInt16BigEndian(destination, value);
			}
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="int"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(int value, Endianness endianness)
		{
			var bytes = new byte[sizeof(int)];
			ToBytes(value, endianness, bytes.AsSpan());
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="int"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <param name="destination"> The <see cref="Span{T}"/> where the result will be written to. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static bool ToBytes(int value, Endianness endianness, Span<byte> destination)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteInt32LittleEndian(destination, value);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteInt32BigEndian(destination, value);
			}
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="long"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(long value, Endianness endianness)
		{
			var bytes = new byte[sizeof(long)];
			ToBytes(value, endianness, bytes.AsSpan());
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="long"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <param name="destination"> The <see cref="Span{T}"/> where the result will be written to. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static bool ToBytes(long value, Endianness endianness, Span<byte> destination)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteInt64LittleEndian(destination, value);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteInt64BigEndian(destination, value);
			}
		}

		#endregion

		#region Unsigned

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="ushort"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(ushort value, Endianness endianness)
		{
			var bytes = new byte[sizeof(ushort)];
			ToBytes(value, endianness, bytes.AsSpan());
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="ushort"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <param name="destination"> The <see cref="Span{T}"/> where the result will be written to. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static bool ToBytes(ushort value, Endianness endianness, Span<byte> destination)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteUInt16LittleEndian(destination, value);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteUInt16BigEndian(destination, value);
			}
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="uint"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(uint value, Endianness endianness)
		{
			var bytes = new byte[sizeof(uint)];
			ToBytes(value, endianness, bytes.AsSpan());
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="uint"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <param name="destination"> The <see cref="Span{T}"/> where the result will be written to. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static bool ToBytes(uint value, Endianness endianness, Span<byte> destination)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteUInt32LittleEndian(destination, value);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteUInt32BigEndian(destination, value);
			}
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="ulong"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(ulong value, Endianness endianness)
		{
			var bytes = new byte[sizeof(ulong)];
			ToBytes(value, endianness, bytes.AsSpan());
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="ulong"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <param name="destination"> The <see cref="Span{T}"/> where the result will be written to. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static bool ToBytes(ulong value, Endianness endianness, Span<byte> destination)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteUInt64LittleEndian(destination, value);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.TryWriteUInt64BigEndian(destination, value);
			}
		}

		#endregion

		#endregion

		#region Data ⇒ Value

		#region Signed

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="short"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="short"/>. </returns>
		public static short ToInt16(byte[] data, Endianness endianness)
			=> ToInt16(data.AsSpan(), endianness);

		/// <summary>
		/// Converts the <paramref name="span"/> into a <see cref="short"/>.
		/// </summary>
		/// <param name="span"> The <see cref="ReadOnlySpan{T}"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="short"/>. </returns>
		public static short ToInt16(ReadOnlySpan<byte> span, Endianness endianness)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(span);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt16BigEndian(span);
			}
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="int"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="int"/>. </returns>
		public static int ToInt32(byte[] data, Endianness endianness)
			=> ToInt32(data.AsSpan(), endianness);

		/// <summary>
		/// Converts the <paramref name="span"/> into a <see cref="int"/>.
		/// </summary>
		/// <param name="span"> The <see cref="ReadOnlySpan{T}"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="int"/>. </returns>
		public static int ToInt32(ReadOnlySpan<byte> span, Endianness endianness)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(span);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(span);
			}
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="long"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="long"/>. </returns>
		public static long ToInt64(byte[] data, Endianness endianness)
			=> ToInt64(data.AsSpan(), endianness);

		/// <summary>
		/// Converts the <paramref name="span"/> into a <see cref="long"/>.
		/// </summary>
		/// <param name="span"> The <see cref="ReadOnlySpan{T}"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="long"/>. </returns>
		public static long ToInt64(ReadOnlySpan<byte> span, Endianness endianness)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(span);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt64BigEndian(span);
			}
		}

		#endregion

		#region Unsigned

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="ushort"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="ushort"/>. </returns>
		public static ushort ToUInt16(byte[] data, Endianness endianness)
			=> ToUInt16(data.AsSpan(), endianness);

		/// <summary>
		/// Converts the <paramref name="span"/> into a <see cref="ushort"/>.
		/// </summary>
		/// <param name="span"> The <see cref="ReadOnlySpan{T}"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="ushort"/>. </returns>
		public static ushort ToUInt16(ReadOnlySpan<byte> span, Endianness endianness)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(span);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(span);
			}
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="uint"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="uint"/>. </returns>
		public static uint ToUInt32(byte[] data, Endianness endianness)
			=> ToUInt32(data.AsSpan(), endianness);

		/// <summary>
		/// Converts the <paramref name="span"/> into a <see cref="uint"/>.
		/// </summary>
		/// <param name="span"> The <see cref="ReadOnlySpan{T}"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="uint"/>. </returns>
		public static uint ToUInt32(ReadOnlySpan<byte> span, Endianness endianness)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(span);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(span);
			}
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="ulong"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="ulong"/>. </returns>
		public static ulong ToUInt64(byte[] data, Endianness endianness)
			=> ToUInt64(data.AsSpan(), endianness);

		/// <summary>
		/// Converts the <paramref name="span"/> into a <see cref="ulong"/>.
		/// </summary>
		/// <param name="span"> The <see cref="ReadOnlySpan{T}"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="ulong"/>. </returns>
		public static ulong ToUInt64(ReadOnlySpan<byte> span, Endianness endianness)
		{
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(span);
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt64BigEndian(span);
			}
		}

		#endregion

		#endregion

		#region Endianness Conversion

		#region Signed

		/// <summary>
		/// Swaps the bytes of <paramref name="value"/> so it can be used in BigEndian systems (Siemens plc).
		/// </summary>
		/// <param name="value"> The<see cref="short"/> to swap. </param>
		/// <returns> The inversed value. </returns>
		public static short SwapBytes(short value)
		{
			return (short) SwapBytes((ushort) value);
		}
		
		/// <summary>
		/// Swaps the bytes of <paramref name="value"/> so it can be used in BigEndian systems (Siemens plc).
		/// </summary>
		/// <param name="value"> The<see cref="int"/> to swap. </param>
		/// <returns> The inversed value. </returns>
		public static int SwapBytes(int value)
		{
			return (int) SwapBytes((uint) value);
		}
		
		/// <summary>
		/// Swaps the bytes of <paramref name="value"/> so it can be used in BigEndian systems (Siemens plc).
		/// </summary>
		/// <param name="value"> The<see cref="long"/> to swap. </param>
		/// <returns> The inversed value. </returns>
		public static long SwapBytes(long value)
		{
			return (long) SwapBytes((ulong) value);
		}

		#endregion

		#region Unsigned

		/// <summary>
		/// Swaps the bytes of <paramref name="value"/> so it can be used in BigEndian systems (Siemens plc).
		/// </summary>
		/// <param name="value"> The<see cref="ushort"/> to swap. </param>
		/// <returns> The inversed value. </returns>
		public static ushort SwapBytes(ushort value)
		{
			// Swap adjacent 8-bit blocks.
			return (ushort)((value >> 8) | (value << 8));
		}

		/// <summary>
		/// Swaps the bytes of <paramref name="value"/> so it can be used in BigEndian systems (Siemens plc).
		/// </summary>
		/// <param name="value"> The<see cref="uint"/> to swap. </param>
		/// <returns> The inversed value. </returns>
		public static uint SwapBytes(uint value)
		{
			// Swap adjacent 16-bit blocks.
			value = (value >> 16) | (value << 16);

			// Swap adjacent 8-bit blocks.
			return ((value & 0xFF00FF00) >> 8) | ((value & 0x00FF00FF) << 8);
		}
		
		/// <summary>
		/// Swaps the bytes of <paramref name="value"/> so it can be used in BigEndian systems (Siemens plc).
		/// </summary>
		/// <param name="value"> The<see cref="ulong"/> to swap. </param>
		/// <returns> The inversed value. </returns>
		public static ulong SwapBytes(ulong value)
		{
			// Swap adjacent 32-bit blocks.
			value = (value >> 32) | (value << 32);

			// Swap adjacent 16-bit blocks.
			value = ((value & 0xFFFF0000FFFF0000) >> 16) | ((value & 0x0000FFFF0000FFFF) << 16);

			// Swap adjacent 8-bit blocks.
			return ((value & 0xFF00FF00FF00FF00) >> 8) | ((value & 0x00FF00FF00FF00FF) << 8);
		}

		[Obsolete("This is the old wy of swapping. It is only used for performance comparision.")]
		internal static ulong SwapBytes_old(ulong value)
		{
			ulong uvalue = value;
			ulong swapped =
				((0x00000000000000FF) & (uvalue >> 56)
				| (0x000000000000FF00) & (uvalue >> 40)
				| (0x0000000000FF0000) & (uvalue >> 24)
				| (0x00000000FF000000) & (uvalue >> 8)
				| (0x000000FF00000000) & (uvalue << 8)
				| (0x0000FF0000000000) & (uvalue << 24)
				| (0x00FF000000000000) & (uvalue << 40)
				| (0xFF00000000000000) & (uvalue << 56));
			return swapped;
		}

		#endregion

		#endregion
	}
}