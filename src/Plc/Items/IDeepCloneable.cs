#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// Generic interface for deep cloning objects.
	/// </summary>
	/// <typeparam name="T"> The type that will be cloned. </typeparam>
	public interface IDeepCloneable<out T> where T : class
	{
		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <returns> A new instance of <typeparamref name="T"/>. </returns>
		T Clone();
	}
}