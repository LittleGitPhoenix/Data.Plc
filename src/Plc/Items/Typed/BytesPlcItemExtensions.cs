#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed;

/// <summary>
/// Contains extension methods for <see cref="BytesPlcItem"/>s.
/// </summary>
public static class BytesPlcItemExtensions
{
	/// <summary>
	/// Sets the <see cref="IPlcItem.Value"/> of the <paramref name="item"/> to <paramref name="value"/>.
	/// </summary>
	/// <param name="item"> The extended <see cref="BytesPlcItem"/>. </param>
	/// <param name="value">
	/// <para> The string value to set. Supported are the following formats: </para>
	/// <para> • HEX: FF12AC | 0xFF12AC... | 0xFF 12 AC... | #FF-12-AC... </para>
	/// <para> • Bytes: 255,0,127... | 255, 0, 127... | 255;0;127... | 255-0-127... | 255 0 127 </para>
	/// </param>
	/// <returns> True on success, otherwise false. </returns>
	public static bool SetValue(this BytesPlcItem item, string value)
	{
		// Try to convert the string into data.
		var success = DataConverter.TryGetBytesFromString(value, out var data);
		if (!success) return false;

		// Set the value.
		item.Value = data;
		return true;
	}
}