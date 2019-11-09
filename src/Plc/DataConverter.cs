using System;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// Helper class for conversion of bits and bytes.
	/// </summary>
	public static class DataConverter
	{
		#region To: Byte
		
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

		#region To: Byte-Array

		/// <summary>
		/// Converts the passed <see cref="Boolean"/>s into a <see cref="Byte"/> array.
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

		#endregion

		#region To: Boolean-Array

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
	}
}