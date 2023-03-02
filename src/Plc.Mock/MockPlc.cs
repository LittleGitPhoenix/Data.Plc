#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Mock
{
	/// <summary>
	/// <see cref="Plc"/> implementation for mocking / test purposes.
	/// </summary>
	public class MockPlc : Plc
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly DataRegionManager _dataRegionManager;

		#endregion

		#region Properties

		/// <summary> The input area of this <see cref="MockPlc"/>. </summary>
		public BitCollection InputArea => _dataRegionManager.InputDataHolder.IsValueCreated ? _dataRegionManager.InputDataHolder.Value.BitCollection : null;

		/// <summary> The output area of this <see cref="MockPlc"/>. </summary>
		public BitCollection OutputArea => _dataRegionManager.OutputDataHolder.IsValueCreated ? _dataRegionManager.OutputDataHolder.Value.BitCollection : null;

		/// <summary> The flags area of this <see cref="MockPlc"/>. </summary>
		public BitCollection FlagsArea => _dataRegionManager.FlagsDataHolder.IsValueCreated ? _dataRegionManager.FlagsDataHolder.Value.BitCollection : null;

		/// <summary> Collection of all available data blocks of this <see cref="MockPlc"/>. </summary>
		public Dictionary<ushort, BitCollection> DataBlocks =>
			_dataRegionManager
				.DataBlockDataHolders
				.ToDictionary
				(
					pair => pair.Key,
					pair => pair.Value.BitCollection
				);

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public MockPlc()
			: this(null) { }
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="initialDataBlocks"> Initial data. </param>
		public MockPlc(Dictionary<ushort, byte[]> initialDataBlocks)
			: this(-1, initialDataBlocks) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plcId"> The id of the plc. </param>
		public MockPlc(int plcId)
			: this(plcId, null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plcId"> The id of the plc. </param>
		/// <param name="initialDataBlocks"> Initial data. </param>
		public MockPlc(int plcId, Dictionary<ushort, byte[]> initialDataBlocks)
			: base(plcId, nameof(MockPlc))
		{
			// Save parameters.

			// Initialize fields.
			_dataRegionManager = new DataRegionManager(initialDataBlocks);
		}

		#endregion

		#region Methods
		
		#region Connection
		
		/// <inheritdoc />
		protected override bool OpenConnection()
		{
			return true;
		}

		/// <inheritdoc />
		protected override bool CloseConnection()
		{
			return true;
		}

		#endregion

		#region Read / Write

		/// <inheritdoc />
		protected override async Task PerformReadWriteAsync(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType, CancellationToken cancellationToken)
		{
			try
			{
				if (usageType == PlcItemUsageType.Read)
				{
					await this.PerformReadAsync(plcItems, cancellationToken);
				}
				else
				{
					await this.PerformWriteAsync(plcItems, cancellationToken);
				}
			}
			catch (NullReferenceException)
			{
				throw;
			}
		}

		/// <summary>
		/// Reads the value of the <paramref name="plcItems"/> from the plc. 
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to read. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the read operation. </param>
		private Task PerformReadAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
		{
			foreach (var plcItem in plcItems)
			{
				plcItem.Value.TransferValuesFrom(_dataRegionManager.Read(plcItem));
			}

#if NET45
			return Task.FromResult(false);
#else
			return Task.CompletedTask;
#endif
		}

		/// <summary>
		/// Writes the <see cref="IPlcItem.Value"/> of the <paramref name="plcItems"/> to the plc. 
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the write operation. </param>
		private Task PerformWriteAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
		{
			var result = true;
			foreach (var plcItem in plcItems)
			{
				result &= _dataRegionManager.Write(plcItem);
			}

			return Task.FromResult(result);
		}

#endregion

#endregion
	}
}