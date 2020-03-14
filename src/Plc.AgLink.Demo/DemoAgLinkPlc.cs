using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accon.AGLink;
using System.Threading;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.AgLink
{
	public sealed class DemoAgLinkPlc : AgLinkPlc<IAGLink4>
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		/// <inheritdoc />
		public DemoAgLinkPlc(AgLinkPlcConnectionData connectionData)
			: base(new DemoAgLinkSetupAdapter(), connectionData)
		{ }

		#endregion

		#region Methods

		#region Connection

		#region Connection Callbacks

		private void OnConnectionErrorOccured(IAGLink4 sender, ConnectionEventArgs args)
			=> base.OnInterrupted(executeReconnect: true);

		private void OnConnectAborted(IAGLink4 sender, ConnectionEventArgs args)
			=> base.OnInterrupted(executeReconnect: true);

		#endregion

		/// <inheritdoc />
		protected override bool OpenConnection()
		{
			if (base.ConnectionState == PlcConnectionState.Connected) return true;

			// Create a new AGLink-PLC instance and connect to it.
			var plc = AGL4ConnectionFactory.CreateInstance(AsyncReportType.Callbacks);
			plc.Name = ConnectionData.Name;
			plc.AutoReconnect = false; // Seems not to work, so disable it and handle reconnection manually via the base class.
			//plc.ReconnectRetries = -1;
			//plc.ReconnectTimeout = RECONNECT_TIMEOUT
			plc.DevNr = 0;
			plc.PlcNr = 0;
			plc.RackNr = ConnectionData.Rack;
			plc.SlotNr = ConnectionData.Slot;
			plc.Timeout = (int) Math.Abs(ConnectionData.ConnectionTimeout.TotalMilliseconds);

			// Attach handlers to the event-callbacks when the connection has been interrupted.
			plc.OnConnectionErrorOccured += this.OnConnectionErrorOccured;
			plc.OnConnectAborted += this.OnConnectAborted;

			// Establish the connection.
			base.UnderlyingPlc = plc;
			return base.UnderlyingPlc.Connect();
		}

		/// <inheritdoc />
		protected override bool CloseConnection()
		{
			if (base.ConnectionState == PlcConnectionState.Disconnected) return true;
			if (base.UnderlyingPlc is null) return true;

			IAGLink4 plc = base.UnderlyingPlc;
			base.UnderlyingPlc = null;

			// Remove the handlers to the event-callbacks when the connection has been interrupted.
			plc.OnConnectionErrorOccured -= this.OnConnectionErrorOccured;
			plc.OnConnectAborted -= this.OnConnectAborted;

			var result = plc.Disconnect();
			plc.Dispose();

			return result;
		}

		#endregion

		#region Read / Write
		
		/// <summary>
		/// Reads the value of the <paramref name="plcItems"/> from the plc. 
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to read. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the read operation. </param>
		protected override async Task PerformReadAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
		{
			const PlcItemUsageType usageType = PlcItemUsageType.Read;

			/*
			 * Create special plc items that will store which AGLink items are used for reading.
			 * This is done via position information (start and amount) of the AGLink items array because those items are structs
			 * and therefore cannot be used on a reference based solution.
			 */
			var mapping = plcItems
				.Select(plcItem => new ReadPlcItemWrapper(plcItem))
				.ToArray()
				;

			// Create all needed AGLink items.
			var previousAmount = 0;
			var allAgLinkItems = mapping
				.SelectMany(readPlcItemWrapper =>
				{
					var agLinkItems = DemoAgLinkPlc.CreateAgLinkItems(readPlcItemWrapper.PlcItem, usageType).ToArray();

					readPlcItemWrapper.Start = (previousAmount += agLinkItems.Length) - 1;
					readPlcItemWrapper.Amount = agLinkItems.Length;

					return agLinkItems;
				})
				.ToArray()
				;

			// Read from the plc.
			var result = await Task.Run(() => base.UnderlyingPlc.ReadMixEx(allAgLinkItems, allAgLinkItems.Length), cancellationToken); ;

			// Verify the result.
			base.VerifyAgLinkResult(result, plcItems, usageType);

			// Iterate each plc item and get the data for all AGLink items that where needed to handle it.
			foreach (var (plcItem, start, amount) in mapping)
			{
				var data = allAgLinkItems.Skip(start).Take(amount).SelectMany(agLinkItem => agLinkItem.B).ToArray();
				base.TransferValue(plcItem, data);
			}
		}

		/// <summary>
		/// Writes the <see cref="IPlcItem.Value"/> of the <paramref name="plcItems"/> to the plc. 
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the write operation. </param>
		protected override async Task PerformWriteAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
		{
			const PlcItemUsageType usageType = PlcItemUsageType.Write;

			var agLinkItems = plcItems
				.SelectMany(plcItem => DemoAgLinkPlc.CreateAgLinkItems(plcItem, usageType).ToArray())
				.ToArray()
				;

			// Write to the plc.
			var result = await Task.Run(() => base.UnderlyingPlc.WriteMixEx(agLinkItems, agLinkItems.Length), cancellationToken);

			// Verify the result.
			base.VerifyAgLinkResult(result, plcItems, usageType);
		}

		#endregion

		#region Helper

		/// <summary>
		/// Creates an AGLink plcItem from <paramref name="plcItem"/>.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> used to build the AGLink item. </param>
		/// <param name="usageType"> The <see cref="Plc.PlcItemUsageType"/> of the <paramref name="plcItem"/>. </param>
		/// <returns> A new AGLink item of type <see cref="AGL4.DATA_RW40"/>. </returns>
		private static IEnumerable<AGL4.DATA_RW40> CreateAgLinkItems(IPlcItem plcItem, PlcItemUsageType usageType)
		{
			var agLinkItem = new AGL4.DATA_RW40
			{
				Offset = plcItem.Position
			};
			switch (plcItem.Type)
			{
				case PlcItemType.Input:
					agLinkItem.DBNr = 0;
					agLinkItem.OpArea = AGL4.AREA_IN;
					break;
				case PlcItemType.Output:
					agLinkItem.DBNr = 0;
					agLinkItem.OpArea = AGL4.AREA_OUT;
					break;
				case PlcItemType.Flags:
					agLinkItem.DBNr = 0;
					agLinkItem.OpArea = AGL4.AREA_FLAG;
					break;
				default:
				case PlcItemType.Data:
					agLinkItem.DBNr = plcItem.DataBlock;
					agLinkItem.OpArea = AGL4.AREA_DATA;
					break;
			}

			if (plcItem.Value.HandlesFullBytes || (usageType == PlcItemUsageType.Read && plcItem.Value.Length > 1))
			{
				agLinkItem.OpType = AGL4.TYP_BYTE;
				agLinkItem.BitNr = 0;
				agLinkItem.OpAnz = (ushort)DataHelper.GetByteAmountForBits(plcItem.Value.Length);
				if (usageType == PlcItemUsageType.Write)
				{
					agLinkItem.B = plcItem.Value;
				}
				else
				{
					agLinkItem.B = new byte[agLinkItem.OpAnz];
				}

				yield return agLinkItem;
			}
			else
			{
				agLinkItem.OpType = AGL4.TYP_BIT;
				agLinkItem.OpAnz = 1;

				for (byte bitPosition = 0; bitPosition < plcItem.Value.Length; bitPosition++)
				{
					var bitAgLinkItem = agLinkItem; // Value type will be copied on assignment.
					bitAgLinkItem.BitNr = (ushort)(bitPosition + plcItem.BitPosition);
					if (usageType == PlcItemUsageType.Write)
					{
						// Get the relevant bit of this item and set the AGLink byte accordingly.						
						bitAgLinkItem.B = new byte[] { plcItem.Value[bitPosition] ? (byte)1 : (byte)0 };
					}
					else
					{
						bitAgLinkItem.B = new byte[1];
					}

					yield return bitAgLinkItem;
				}
			}
		}
		
		#endregion

		#endregion
	}
}