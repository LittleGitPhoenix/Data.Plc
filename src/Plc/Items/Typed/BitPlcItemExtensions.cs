#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// Contains extension methods for <see cref="BitPlcItem"/>s.
	/// </summary>
	public static class BitPlcItemExtensions
	{
		/// <summary>
		/// Sets the <see cref="IPlcItem.Value"/> of the <paramref name="item"/> to <paramref name="value"/>.
		/// </summary>
		/// <param name="item"> The extended <see cref="BitPlcItem"/>. </param>
		/// <param name="value">
		/// <para> The string value to set. Only the following values are treated as true, comparison is case-insensitive: </para>
		/// <para> • true </para>
		/// <para> • yes </para>
		/// <para> • ok </para>
		/// <para> • 1 </para>
		/// </param>
		/// <returns> The <paramref name="item"/>s new value. </returns>
		public static bool SetValue(this BitPlcItem item, string value)
		{
			// Try to convert the string into data.
			value = value.ToLower();
			var boolean = value == "true"
						|| value == "yes"
						|| value == "ok"
						|| value == "1"
				? true
				: false
				;
			
			// Set the value.
			return item.Value = boolean;
		}
	}
}