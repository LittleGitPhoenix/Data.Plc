#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Data.Plc.Items;

/// <summary>
/// Represents a single changed bit.
/// </summary>
public class BitChanges : System.Collections.ObjectModel.ReadOnlyDictionary<uint, (bool? OldValue, bool? NewValue)>
{
	/// <summary>
	/// Constructor
	/// </summary>
	public BitChanges() : base(new Dictionary<uint, (bool? OldValue, bool? NewValue)>()) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="capacity"> The initial number of elements that the <see cref="BitChanges"/> can contain. </param>
	public BitChanges(int capacity) : base(new Dictionary<uint, (bool? OldValue, bool? NewValue)>(capacity)) { }


	internal void Add(uint bitPosition, (bool? OldValue, bool? NewValue) change)
	{
		if (base.ContainsKey(bitPosition))
		{
			base.Dictionary[bitPosition] = change;
		}
		else
		{
			base.Dictionary.Add(bitPosition, change);
		}
	}
}