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