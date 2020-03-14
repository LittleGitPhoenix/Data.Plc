#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// Special <see cref="IPlcItem"/> used as part of an <see cref="IDynamicPlcItem"/>.
	/// </summary>
	public interface INumericPlcItem : IPlcItem
	{
		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="INumericPlcItem"/>. </returns>
		new INumericPlcItem Clone(string identifier);
	}
}