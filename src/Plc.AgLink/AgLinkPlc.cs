#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items;
using Accon.AGLink;

#nullable enable

namespace Phoenix.Data.Plc.AgLink;

/// <summary>
/// <see cref="Plc"/> implementation utilizing <c>AGLink</c>.
/// </summary>
/// <remarks> Since <c>AGLink</c> needs additional components, this class cannot be instantiated anymore. Instead inherit from this class an provide the requirements in the static constructor of the sub-class. </remarks>
public abstract class AgLinkPlc : Plc
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> Data used for connecting to the plc. </summary>
	private IAgLinkPlcConnectionData ConnectionData { get; }

	/// <summary> AGLink plc connection object. </summary>
	private IAGLink4? UnderlyingPlc { get; set; }

	/// <summary> <see cref="AgLinkErrorMapping"/> used to resolve error codes to messages. </summary>
	protected internal static AgLinkErrorMapping ErrorMapping { get; internal set; }

	#endregion

	#region Enumerations

	private enum AgLinkResult
	{
		Success = 0,
		Error = 1,
	}

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Static constructor
	/// </summary>
	static AgLinkPlc()
	{
		// Setup null-error mapping.
		AgLinkPlc.ErrorMapping = new AgLinkErrorMapping();
			
		// Setup other AGLink properties.
		AGL4.ReturnJobNr(false);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="connectionData"> <see cref="IAgLinkPlcConnectionData"/> </param>
	protected AgLinkPlc(IAgLinkPlcConnectionData connectionData)
		: base(connectionData.Id, connectionData.Name)
	{
		// Save parameters.
		this.ConnectionData = connectionData;

		// Initialize fields.
	}

	#endregion

	#region Methods

	#region Connection

	#region Connection Callbacks

	private void OnConnectionErrorOccurred(IAGLink4 sender, ConnectionEventArgs args)
		=> base.OnInterrupted(executeReconnect: true);

	private void OnConnectAborted(IAGLink4 sender, ConnectionEventArgs args)
		=> base.OnInterrupted(executeReconnect: true);

	#endregion

	/// <inheritdoc />
	protected sealed override bool OpenConnection()
	{
		if (base.ConnectionState == PlcConnectionState.Connected) return true;

		// Create a new AGLink-PLC instance.
		var connectionData = this.ConnectionData;
		var plc = AGL4ConnectionFactory.CreateInstanceAndConfigureTcpIp
		(
			devNr: connectionData.DeviceNumber % (byte.MaxValue + 1), //! Testing showed that any number larger than 255 leads to a 'System.AccessViolationException'.
			entry: 0,
			plcNr: 0,
			rackNr: connectionData.Rack,
			slotNr: connectionData.Slot,
			plc_class: AGL4.PLC_Class.ePLC_1200, //? Which type is correct?
			ip: connectionData.Ip,
			port: 0, //! This value seems to be ignored, as any test with different values always succeeded.
			timeout: (int)Math.Abs(this.ConnectionData.ConnectionTimeout.TotalMilliseconds),
			reportType: AsyncReportType.Callbacks,
			result: out var result
		);
		if (plc is null)
		{
			base.Logger.Error($"Creating an instance of the underlying plc connection '{nameof(IAGLink4)}' failed. Error was '{AgLinkPlc.ErrorMapping.GetErrorMessageForCode(result)}'.");
			return false;
		}
		plc.Name = this.ConnectionData.Name;
		plc.AutoReconnect = false; // Seems not to work, so disable it and handle reconnection manually via the base class.

		// Attach handlers to the event-callbacks when the connection has been interrupted.
		plc.OnConnectionErrorOccured += this.OnConnectionErrorOccurred;
		plc.OnConnectAborted += this.OnConnectAborted;

		// Establish the connection.
		this.UnderlyingPlc = plc;
		return this.UnderlyingPlc.Connect();
	}

	/// <inheritdoc />
	protected sealed override bool CloseConnection()
	{
		if (base.ConnectionState == PlcConnectionState.Disconnected) return true;
		if (this.UnderlyingPlc is null) return true;

		IAGLink4 plc = this.UnderlyingPlc;
		this.UnderlyingPlc = null;

		// Remove the handlers to the event-callbacks when the connection has been interrupted.
		plc.OnConnectionErrorOccured -= this.OnConnectionErrorOccurred;
		plc.OnConnectAborted -= this.OnConnectAborted;

		var result = plc.Disconnect();
		plc.Dispose();

		return result;
	}

	#endregion

	#region Read / Write

	/// <inheritdoc />
	protected sealed override async Task PerformReadWriteAsync(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType, CancellationToken cancellationToken)
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
	private async Task PerformReadAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
	{
		const PlcItemUsageType usageType = PlcItemUsageType.Read;
		var underlyingPlc = AgLinkPlc.VerifyConnectivity(this, plcItems, usageType);

		// Create the mapping.
		var (mapping, allAgLinkItems) = AgLinkPlc.CreateMappingAndAgLinkItems(plcItems, usageType);

		// Read from the plc.
		//! The return value may be zero even if some or all of the items failed. To get the real result the 'Result' property of the AGLink item (AGL4.DATA_RW40.Result) must be checked.
		var readResult = await Task.Run(() => underlyingPlc.ReadMixEx(allAgLinkItems, allAgLinkItems.Length), cancellationToken);

		// Verify the total result.
		//! Ignore the total result and inspect the result of each item individually.
		var result = AgLinkPlc.ConvertToAgLinkResult(readResult);
		//if (result != AgLinkResult.Success)
		//{
		//	var errorMessage = AgLinkPlc.ErrorMapping.GetErrorMessageForCode(readResult);
		//	var items = plcItems.Select(item => (item, "General reading error.")).ToArray();
		//	throw new ReadPlcException(new IPlcItem[0], items, $"Could not read any items from {this:LOG}. AGLink returned error code '{result}' ({errorMessage}).");
		//}

		// Verify the result of all items and transfer the value.
		var (validItems, failedItems) = AgLinkPlc.VerifyPlcItemResults(mapping, allAgLinkItems, true);
		if (failedItems.Any()) throw new ReadPlcException(validItems, failedItems, $"Some of the items couldn't be read. See the '{nameof(ReadOrWritePlcException.FailedItems)}' property for further information.");
	}

	/// <summary>
	/// Writes the <see cref="IPlcItem.Value"/> of the <paramref name="plcItems"/> to the plc. 
	/// </summary>
	/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
	/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the write operation. </param>
	private async Task PerformWriteAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
	{
		const PlcItemUsageType usageType = PlcItemUsageType.Write;
		var underlyingPlc = AgLinkPlc.VerifyConnectivity(this, plcItems, usageType);

		// Create the mapping.
		var (mapping, allAgLinkItems) = AgLinkPlc.CreateMappingAndAgLinkItems(plcItems, usageType);

		// Write to the plc.
		var writeResult = await Task.Run(() => underlyingPlc.WriteMixEx(allAgLinkItems, allAgLinkItems.Length), cancellationToken);

		// Verify the total result.
		//! Ignore the total result and inspect the result of each item individually.
		var result = AgLinkPlc.ConvertToAgLinkResult(writeResult);
		//if (result != AgLinkResult.Success)
		//{
		//	var errorMessage = AgLinkPlc.ErrorMapping.GetErrorMessageForCode(writeResult);
		//	var items = plcItems.Select(item => (item, "General writing error.")).ToArray();
		//	throw new WritePlcException(new IPlcItem[0], items, $"Could not write any items to {this:LOG}. AGLink returned error code '{result}' ({errorMessage}).");
		//}

		// Verify the result of all items.
		var (validItems, failedItems) = AgLinkPlc.VerifyPlcItemResults(mapping, allAgLinkItems, false);
		if (failedItems.Any()) throw new WritePlcException(validItems, failedItems, $"Some of the items couldn't be written. See the '{nameof(ReadOrWritePlcException.FailedItems)}' property for further information.");
	}

	#endregion
		
	#region Helper

	/// <summary>
	/// Verifies that <paramref name="plc.UnderlyingPlc"/> is not <c>Null</c> before reading or writing.
	/// </summary>
	/// <remarks> Normally the base class already handles cases where the plc connection is not yet established, but just in case and to keep the compiler from complaining. </remarks>
	/// <exception cref="NotConnectedPlcException"> Thrown if <paramref name="plc.UnderlyingPlc"/> is <c>Null</c>. </exception>
	private static IAGLink4 VerifyConnectivity(AgLinkPlc plc, ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
	{
		var underlyingPlc = plc.UnderlyingPlc;
		if (underlyingPlc is null)
		{
			var itemDescriptions = Plc.GetPlcItemDescription(plcItems);
			throw new NotConnectedPlcException( $"Cannot {usageType.ToString().ToLower()} the plc items ({itemDescriptions}) because {plc:LOG} is not connected. All items will be put on hold.");
		}

		return underlyingPlc;
	}

	/// <summary>
	/// Creates all needed <see cref="AGL4.DATA_RW40"/> items for the <paramref name="plcItems"/> along with a <see cref="ReadPlcItemWrapper"/>  for each.
	/// </summary>
	/// <param name="plcItems"> The plc items for which to create the AGLink items. </param>
	/// <param name="usageType"> The <see cref="Phoenix.Data.Plc.Plc.PlcItemUsageType"/> of the items. </param>
	/// <returns>
	/// <para> A <see cref="ValueTuple"/> with: </para>
	/// <para> Mapping: The <see cref="ReadPlcItemWrapper"/>s. </para>
	/// <para> AllAgLinkItems: The <see cref="AGL4.DATA_RW40"/> items. </para>
	/// </returns>
	private static (ICollection<ReadPlcItemWrapper> Mapping, AGL4.DATA_RW40[] AllAgLinkItems) CreateMappingAndAgLinkItems(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
	{
		var mapping = plcItems
				.Select(plcItem => new ReadPlcItemWrapper(plcItem))
				.ToArray()
			;

		// Create all needed AGLink items.
		//! Directly storing the AGLink items in the ReadPlcItemWrapper instance won't work, as AGL4.DATA_RW40 is a struct that would consequently be copied on assignment.
		//! Therefore the ReadPlcItemWrapper only contains the start position and the amount of items in the returned AGLink items array.
		var previousAmount = 0;
		var allAgLinkItems = mapping
				.SelectMany
				(
					readPlcItemWrapper =>
					{
						var agLinkItems = AgLinkPlc.CreateAgLinkItems(readPlcItemWrapper.PlcItem, usageType).ToArray();

						readPlcItemWrapper.Start = (previousAmount += agLinkItems.Length) - 1;
						readPlcItemWrapper.Amount = agLinkItems.Length;

						return agLinkItems;
					}
				)
				.ToArray()
			;

		return (mapping, allAgLinkItems);
	}

	/// <summary>
	/// Creates AGLink plcItems from <paramref name="plcItem"/>. Some <see cref="IPlcItem"/>s can only be handled by multiple AGLink items.
	/// </summary>
	/// <param name="plcItem"> The <see cref="IPlcItem"/> used to build the AGLink item. </param>
	/// <param name="usageType"> The <see cref="Plc.PlcItemUsageType"/> of the <paramref name="plcItem"/>. </param>
	/// <returns> New AGLink items of type <see cref="AGL4.DATA_RW40"/>. </returns>
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
			agLinkItem.OpAnz = (ushort) DataHelper.GetByteAmountForBits(plcItem.Value.Length);
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

	/// <summary>
	/// Verifies the result of all <see cref="AGL4.DATA_RW40"/> items linked to an <see cref="IPlcItem"/> and sorts them into two lists.
	/// </summary>
	/// <param name="mapping"> The <see cref="ReadPlcItemWrapper"/>s used to identify which AGLink items belong to which plc item. </param>
	/// <param name="allAgLinkItems"> The AGLink items themselves. </param>
	/// <param name="transferData"> Optional flag if the data from the AGLink items should be transferred to the plc items. </param>
	/// <returns>
	/// <para> A <see cref="ValueTuple"/> with: </para>
	/// <para> ValidItems: The <see cref="IPlcItem"/>s that where valid. </para>
	/// <para> FailedItems: The <see cref="IPlcItem"/> that where not valid together with the error message. </para>
	/// </returns>
	private static (ICollection<IPlcItem> ValidItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)>) VerifyPlcItemResults(ICollection<ReadPlcItemWrapper> mapping, ICollection<AGL4.DATA_RW40> allAgLinkItems, bool transferData)
	{
		var validItems = new List<IPlcItem>(mapping.Count);
		var failedItems = new List<(IPlcItem FailedItem, string ErrorMessage)>(0); //! Assume that no errors occur to save memory.

		// Iterate each plc item and get the data for all AGLink items that where needed to handle it.
		foreach (var (plcItem, start, amount) in mapping)
		{
			// Get all ag link items of this plc item.
			var agLinkItems = allAgLinkItems.Skip(start).Take(amount).ToArray();

			// Check if any of those have failed.
			var itemResult = agLinkItems
					.Select(agLinkItem => agLinkItem.Result)
					.FirstOrDefault(resultCode => resultCode != 0)
				;
				
			var result = AgLinkPlc.ConvertToAgLinkResult(itemResult);
			if (result != AgLinkResult.Success)
			{
				failedItems.Add((plcItem, AgLinkPlc.ErrorMapping.GetErrorMessageForCode(itemResult)));
			}
			else
			{
				validItems.Add(plcItem);
				if (transferData)
				{
					var data = agLinkItems.SelectMany(agLinkItem => agLinkItem.B).ToArray();
					AgLinkPlc.TransferValue(plcItem, data);
				}
			}
		}

		return (validItems, failedItems);
	}

	/// <summary>
	/// Sets the <paramref name="data"/> to the <paramref name="plcItem"/>s <see cref="IPlcItem{TValue}.Value"/>.
	/// </summary>
	/// <param name="plcItem"> The <see cref="IPlcItem"/> whose value to set. </param>
	/// <param name="data"> The new byte data. </param>
	private static void TransferValue(IPlcItem plcItem, byte[] data)
	{
		if (!plcItem.Value.HandlesFullBytes && plcItem.Value.Length > 1)
		{
			var booleans = DataConverter.ToBooleans(data).Skip((byte)plcItem.BitPosition).Take((int)plcItem.Value.Length).ToArray();
			plcItem.Value.TransferValuesFrom(booleans);
		}
		else
		{
			plcItem.Value.TransferValuesFrom(data);
		}
	}
		
	/// <summary>
	/// Converts the passed <c>AGLink</c> result into a <see cref="AgLinkResult"/>.
	/// </summary>
	/// <param name="result"> The result as <see cref="int"/>. </param>
	/// <returns> A <see cref="AgLinkResult"/>. </returns>
	private static AgLinkResult ConvertToAgLinkResult(int result)
	{
		if (result == 0) return AgLinkResult.Success;
		else return AgLinkResult.Error;
	}

	#endregion
		
	#region IDisposable

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		this.UnderlyingPlc?.Dispose();
	}

	#endregion

	#endregion

	#region Nested Types

	/// <summary>
	/// Wrapper for an <see cref="IPlcItem"/> that stores <see cref="Start"/> and <see cref="Amount"/> for reading multiple <c>AGL4.DATA_RW40</c> items at once.
	/// </summary>
	private class ReadPlcItemWrapper
	{
		/// <summary>
		/// The wrapped <see cref="IPlcItem"/>.
		/// </summary>
		public IPlcItem PlcItem { get; }

		/// <summary> When reading / writing (multiple) <see cref="IPlcItem"/> those items will be split up into an array of <c>AGL4.DATA_RW40</c> structures. This is the index of the first such structure within the array. </summary>
		public int Start { private get; set; }

		/// <summary> When reading / writing (multiple) <see cref="IPlcItem"/> those items will be split up into an array of <c>AGL4.DATA_RW40</c> structures. This is the amount of how many consecutive structures where needed to handle the item. </summary>
		public int Amount { private get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plcItem"> The item to wrap. </param>
		public ReadPlcItemWrapper(IPlcItem plcItem)
		{
			this.PlcItem = plcItem;
			this.Start = -1;
			this.Amount = -1;
		}

		/// <summary>
		/// Tuple-Deconstructor
		/// </summary>
		public void Deconstruct(out IPlcItem plcItem, out int start, out int amount)
		{
			plcItem = this.PlcItem;
			start = this.Start;
			amount = this.Amount;
		}

		/// <inheritdoc />
		public override string ToString() => $"{this.PlcItem}: Start: {this.Start} | Amount: {this.Amount}";
	}

	#endregion
}