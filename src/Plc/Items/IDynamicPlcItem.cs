using System;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// A special <see cref="PlcItem"/> that can be used for dynamic data where the length of the data is defined within the first few bytes of the item itself.
	/// </summary>
	public interface IDynamicPlcItem : IPlcItem
	{
		/// <summary> The <see cref="IPlcItem"/> that defines the length of this dynamic item. </summary>
		INumericPlcItem LengthPlcItem { get; }

		/// <summary> The <see cref="IPlcItem"/> that holds the real value of this dynamic item. </summary>
		IPlcItem FlexiblePlcItem { get; }
	}
}