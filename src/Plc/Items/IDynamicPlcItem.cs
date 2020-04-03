#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// A special <see cref="PlcItem"/> that can be used for dynamic data where the length of the data is defined within the first few bytes of the item itself.
	/// </summary>
	/// <remarks> Set this to <c>Null</c> to prevent limiting. </remarks>
	public interface IDynamicPlcItem : IPlcItem
	{
		/// <summary> This is a limit that will be applied to the length being read or written. </summary>
		uint? LengthLimit { get; }

		/// <summary> The <see cref="IPlcItem"/> that defines the length of this dynamic item. </summary>
		INumericPlcItem LengthPlcItem { get; }

		/// <summary> The <see cref="IPlcItem"/> that holds the real value of this dynamic item. </summary>
		IPlcItem FlexiblePlcItem { get; }
	}
}