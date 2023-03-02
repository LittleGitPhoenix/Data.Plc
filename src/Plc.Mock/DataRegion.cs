#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Mock;

/// <summary>
/// Represent a region of data.
/// </summary>
internal class DataRegion
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly object _lock;
		
	#endregion

	#region Properties

	internal BitCollection BitCollection { get; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	internal DataRegion()
		: this(new byte[0])
	{ }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="initialData"> Initial data for this region. </param>
	internal DataRegion(byte[] initialData)
	{
		// Save parameters.

		// Initialize fields.
		_lock = new object();
		this.BitCollection = new BitCollection(false, initialData);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Tries to extend the <see cref="DataRegion"/> to the <paramref name="newSize"/>.
	/// </summary>
	/// <param name="newSize"> The new size of the region. </param>
	internal void TryExtend(uint newSize)
	{
		lock (_lock)
		{
			// Only extended the bit collection.
			if (this.BitCollection.Length < newSize) this.BitCollection.Resize(newSize);
		}
	}

	/// <summary>
	/// Reads the data from <paramref name="start"/> with <paramref name="length"/>.
	/// </summary>
	/// <param name="start"> The start index from where to read the data. </param>
	/// <param name="length"> The length to read. </param>
	/// <returns> The data as <see cref="bool"/> array. </returns>
	internal bool[] Read(int start, int length)
	{
		lock (_lock)
		{
			return ((bool[]) this.BitCollection).Skip(start).Take(length).ToArray();
		}
	}

	/// <summary>
	/// Writes the <paramref name="data"/> beginning at <paramref name="start"/>.
	/// </summary>
	/// <param name="data"> The <see cref="BitCollection"/> to write. </param>
	/// <param name="start"> The starting position. </param>
	/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
	internal bool Write(BitCollection data, uint start)
	{
		lock (_lock)
		{
			this.BitCollection.TransferValuesFrom(data, start);
		}
		return true;
	}

	#endregion
}