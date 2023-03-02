#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items;

/// <summary>
/// <see cref="EventArgs"/> for changes in the internal collection of the <see cref="BitCollection"/> class.
/// </summary>
public class BitsChangedEventArgs : EventArgs
{
	/// <summary>
	/// The collection of changes that occurred.
	/// </summary>
	/// <remarks> The key in the dictionary is the zero-based bit position. </remarks>
	public BitChanges Changes { get; }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="changes"> <see cref="Changes"/> </param>
	public BitsChangedEventArgs(BitChanges changes)
	{
		this.Changes = changes;
	}
}