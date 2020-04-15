#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// A special <see cref="PlcItem"/> that can be used for dynamic data where the length of the data is defined within the first few bytes of the item itself.
	/// </summary>
	public interface IDynamicPlcItem : IPlcItem
	{
		/// <summary> This factor will be applied to the length of <see cref="LengthPlcItem"/> and the <see cref="LengthLimit"/>. </summary>
		/// <remarks> It should be used if the <see cref="LengthPlcItem"/> does not provide an absolute byte amount, but rather an amount of items. </remarks>
		/// <example> If <see cref="LengthPlcItem"/> specifies an amount of items where each item is 4 byte, then this <see cref="LengthFactor"/> should be 4.  </example>
		byte LengthFactor { get; }

		/// <summary> This is a limit that will be applied to the length being read or written. </summary>
		/// <remarks> Set this to <c>Null</c> to prevent limiting. </remarks>
		uint? LengthLimit { get; }

		/// <summary> The <see cref="IPlcItem"/> that defines the length of this dynamic item. </summary>
		INumericPlcItem LengthPlcItem { get; }

		/// <summary> The <see cref="IPlcItem"/> that holds the real value of this dynamic item. </summary>
		IPlcItem FlexiblePlcItem { get; }
	}
}