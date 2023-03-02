#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Mock
{
	/// <summary>
	/// Manages several <see cref="DataRegion"/>s.
	/// </summary>
	internal sealed class DataRegionManager
	{
		#region Delegates / Events

		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		private readonly object _dataBlockLock;

		internal Lazy<DataRegion> InputDataHolder { get; }

		internal Lazy<DataRegion> OutputDataHolder { get; }

		internal Lazy<DataRegion> FlagsDataHolder { get; }

		internal Dictionary<ushort, DataRegion> DataBlockDataHolders { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		internal DataRegionManager() : this(null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="initialDataBlocks"> A collection of initial data. </param>
		internal DataRegionManager(Dictionary<ushort, byte[]> initialDataBlocks)
		{
			// Save parameters.

			// Initialize fields.
			_dataBlockLock = new object();
			this.InputDataHolder = new Lazy<DataRegion>(() => new DataRegion(), LazyThreadSafetyMode.ExecutionAndPublication);
			this.OutputDataHolder = new Lazy<DataRegion>(() => new DataRegion(), LazyThreadSafetyMode.ExecutionAndPublication);
			this.FlagsDataHolder = new Lazy<DataRegion>(() => new DataRegion(), LazyThreadSafetyMode.ExecutionAndPublication);
			this.DataBlockDataHolders = new Dictionary<ushort, DataRegion>();

			if (initialDataBlocks != null)
			{
				foreach (var initialDataBlock in initialDataBlocks)
				{
					this.DataBlockDataHolders.Add(initialDataBlock.Key, new DataRegion(initialDataBlock.Value));
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Reads the <paramref name="plcItem"/> from the underlying <see cref="DataRegion"/>s.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to read. </param>
		/// <returns> The value as <see cref="bool"/> array. </returns>
		internal bool[] Read(IPlcItem plcItem)
		{
			var dataHolder = this.GetDataRegion(plcItem);
			return dataHolder.Read(plcItem.Position * 8 + (byte) plcItem.BitPosition, (int) plcItem.Value.Length);
		}

		/// <summary>
		/// Writes the <paramref name="plcItem"/> to the underlying <see cref="DataRegion"/>s.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to write. </param>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		internal bool Write(IPlcItem plcItem)
		{
			var dataHolder = this.GetDataRegion(plcItem);
			return dataHolder.Write(plcItem.Value, (uint) (plcItem.Position * 8 + (byte) plcItem.BitPosition));
		}

		/// <summary>
		/// Gets the <see cref="DataRegion"/> for the <paramref name="plcItem"/>.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> for which to get the <see cref="DataRegion"/>. </param>
		/// <returns> The matching <see cref="DataRegion"/>. </returns>
		/// <remarks> The <see cref="DataRegion"/> will automatically be extended to satisfy the range of the <paramref name="plcItem"/>. </remarks>
		private DataRegion GetDataRegion(IPlcItem plcItem)
		{
			DataRegion dataRegion;
			switch (plcItem.Type)
			{
				case PlcItemType.Input:
				{
					dataRegion = this.InputDataHolder.Value;
					break;
				}
				case PlcItemType.Output:
				{
					dataRegion = this.OutputDataHolder.Value;
					break;
				}
				case PlcItemType.Flags:
				{
					dataRegion = this.FlagsDataHolder.Value;
					break;
				}
				default:
				case PlcItemType.Data:
				{
					lock (_dataBlockLock)
					{
						if (!this.DataBlockDataHolders.TryGetValue(plcItem.DataBlock, out dataRegion))
						{
							dataRegion = new DataRegion();
							this.DataBlockDataHolders.Add(plcItem.DataBlock, dataRegion);
						}
					}
					break;
				}
			}

			dataRegion.TryExtend((uint) (plcItem.Position * 8 + (byte) plcItem.BitPosition + plcItem.Value.Length));
			return dataRegion;
		}

		#endregion
	}
}