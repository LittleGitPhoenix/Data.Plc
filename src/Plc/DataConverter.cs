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

		#region Value ⇒ Byte-Array

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
#if NETSTANDARD2_1
			var bytes = new byte[sizeof(short)];
			if (endianness == Endianness.LittleEndian)
			{
				System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(), value);
			}
			else
			{
				System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(bytes.AsSpan(), value);
			}
#else
			if (DataConverter.MustSwap(endianness)) value = DataConverter.SwapBytes(value);
			var bytes = BitConverter.GetBytes(value);
#endif
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="int"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(int value, Endianness endianness)
		{
#if NETSTANDARD2_1
			var bytes = new byte[sizeof(int)];
			if (endianness == Endianness.LittleEndian)
			{
				System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(), value);
			}
			else
			{
				System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(), value);
			}
#else
			if (DataConverter.MustSwap(endianness)) value = DataConverter.SwapBytes(value);
			var bytes = BitConverter.GetBytes(value);
#endif
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="long"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(long value, Endianness endianness)
		{
#if NETSTANDARD2_1
			var bytes = new byte[sizeof(long)];
			if (endianness == Endianness.LittleEndian)
			{
				System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(), value);
			}
			else
			{
				System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(bytes.AsSpan(), value);
			}
#else
			if (DataConverter.MustSwap(endianness)) value = DataConverter.SwapBytes(value);
			var bytes = BitConverter.GetBytes(value);
#endif
			return bytes;
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
#if NETSTANDARD2_1
			var bytes = new byte[sizeof(ushort)];
			if (endianness == Endianness.LittleEndian)
			{
				System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(), value);
			}
			else
			{
				System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(), value);
			}
#else
			if (DataConverter.MustSwap(endianness)) value = DataConverter.SwapBytes(value);
			var bytes = BitConverter.GetBytes(value);
#endif
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="uint"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(uint value, Endianness endianness)
		{
#if NETSTANDARD2_1
			var bytes = new byte[sizeof(uint)];
			if (endianness == Endianness.LittleEndian)
			{
				System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(), value);
			}
			else
			{
				System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(), value);
			}
#else
			if (DataConverter.MustSwap(endianness)) value = DataConverter.SwapBytes(value);
			var bytes = BitConverter.GetBytes(value);
#endif
			return bytes;
		}

		/// <summary>
		/// Converts the <paramref name="value"/> into a <see cref="Byte"/> array.
		/// </summary>
		/// <param name="value"> The <see cref="ulong"/> to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="Byte"/> array. </returns>
		public static byte[] ToBytes(ulong value, Endianness endianness)
		{
#if NETSTANDARD2_1
			var bytes = new byte[sizeof(ulong)];
			if (endianness == Endianness.LittleEndian)
			{
				System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(), value);
			}
			else
			{
				System.Buffers.Binary.BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(), value);
			}
#else
			if (DataConverter.MustSwap(endianness)) value = DataConverter.SwapBytes(value);
			var bytes = BitConverter.GetBytes(value);
#endif
			return bytes;
		}

		#endregion

		#endregion

		#region Byte-Array ⇒ Value

		#region Signed

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="short"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="short"/>. </returns>
		public static short ToInt16(byte[] data, Endianness endianness)
		{
#if NETSTANDARD2_1
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan());
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt16BigEndian(data.AsSpan());
			}
#else
			if (DataConverter.MustSwap(endianness)) Array.Reverse(data);
			return BitConverter.ToInt16(data, 0);
#endif
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="int"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="int"/>. </returns>
		public static int ToInt32(byte[] data, Endianness endianness)
		{
#if NETSTANDARD2_1
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan());
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(data.AsSpan());
			}
#else
			if (DataConverter.MustSwap(endianness)) Array.Reverse(data);
			return BitConverter.ToInt32(data, 0);
#endif
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="long"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="long"/>. </returns>
		public static long ToInt64(byte[] data, Endianness endianness)
		{
#if NETSTANDARD2_1
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan());
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadInt64BigEndian(data.AsSpan());
			}
#else
			if (DataConverter.MustSwap(endianness)) Array.Reverse(data);
			return BitConverter.ToInt64(data, 0);
#endif
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
		{
#if NETSTANDARD2_1
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan());
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan());
			}
#else
			if (DataConverter.MustSwap(endianness)) Array.Reverse(data);
			return BitConverter.ToUInt16(data, 0);
#endif
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="uint"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="uint"/>. </returns>
		public static uint ToUInt32(byte[] data, Endianness endianness)
		{
#if NETSTANDARD2_1
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan());
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan());
			}
#else
			if (DataConverter.MustSwap(endianness)) Array.Reverse(data);
			return BitConverter.ToUInt32(data, 0);
#endif
		}

		/// <summary>
		/// Converts the <paramref name="data"/> into a <see cref="ulong"/>.
		/// </summary>
		/// <param name="data"> The <see cref="Byte"/> array to convert. </param>
		/// <param name="endianness"> The <see cref="Endianness"/> to use when converting. </param>
		/// <returns> A <see cref="ulong"/>. </returns>
		public static ulong ToUInt64(byte[] data, Endianness endianness)
		{
#if NETSTANDARD2_1
			if (endianness == Endianness.LittleEndian)
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan());
			}
			else
			{
				return System.Buffers.Binary.BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan());
			}
#else
			if (DataConverter.MustSwap(endianness)) Array.Reverse(data);
			return BitConverter.ToUInt64(data, 0);
#endif
		}

		#endregion

		#endregion

		#region Endianess Conversion

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

		#region Helper
		
#if !NETSTANDARD2_1
		private static bool MustSwap(Endianness endianness)
		{
			return (BitConverter.IsLittleEndian && endianness == Endianness.BigEndian) || (!BitConverter.IsLittleEndian && endianness == Endianness.LittleEndian);
		}
#endif

		#endregion
	}
}