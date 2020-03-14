#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// Helper class containing methods for <see cref="byte"/> arrays.
	/// </summary>
	public static class DataHelper
	{
		/// <summary>
		/// Checks if the bit at <paramref name="position"/> within the <paramref name="byte"/> is either <c>True</c> or <c>False</c>.
		/// </summary>
		public static bool IsBitSet(byte @byte, byte position)
		{
			if (position > 8) throw new ArgumentOutOfRangeException(paramName: nameof(position), message: $"A byte is only 8 bits long. So position '{position}' is out of its range.");
			return (@byte & (1 << position)) != 0;
		}

		/// <summary>
		/// Checks if the bit positioned at <paramref name="bytePosition"/> and <paramref name="bitPosition"/> within the <paramref name="bytes"/> is either <c>True</c> or <c>False</c>.
		/// </summary>
		/// <param name="bytes"> The complete <see cref="Byte"/> array. </param>
		/// <param name="bytePosition"></param>
		/// <param name="bitPosition"></param>
		/// <returns> The relevant <see cref="bool"/>. </returns>
		public static bool IsBitSet(byte[] bytes, ushort bytePosition, byte bitPosition)
		{
			if (bytePosition > bytes.Length) throw new ArgumentOutOfRangeException(paramName: nameof(bytePosition), message: $"The byte position '{bytePosition}' is out of range for the data length of '{bytes.Length}'.");

			var @byte = bytes[bytePosition];
			return DataHelper.IsBitSet(@byte, bitPosition);
		}

		/// <summary>
		/// Creates a new <see cref="Byte"/> array where the bit at <paramref name="position"/> is set to <paramref name="value"/>.
		/// </summary>
		/// <param name="position"> The bit position within the byte. </param>
		/// <param name="value"> The value of the bit. </param>
		/// <returns> A <see cref="Byte"/> array containing a single bit. </returns>
		public static byte[] CreateBytesWithBit(int position, bool value)
		{
			byte @byte = 0;
			if (value)
			{
				// Left-Shift 1, then bitwise OR
				@byte = (byte)(@byte | (1 << position));
			}
			else
			{
				// Left-Shift 1, then take complement, then bitwise AND
				@byte = (byte)(@byte & ~(1 << position));
			}

			return new[] { @byte };
		}

		/// <summary>
		/// Gets the amount of <see cref="byte"/>s need to contain the <paramref name="bitAmount"/>.
		/// </summary>
		/// <param name="bitAmount"> The amount of bits. </param>
		/// <returns> The needed amount of <see cref="byte"/>s. </returns>
		public static uint GetByteAmountForBits(uint bitAmount)
		{
			return bitAmount / 8 + Math.Min(1, bitAmount % 8);
		}
	}
}