#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items;
using Softing.SiAccess;

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// <see cref="Plc"/> implementation utilizing <c>siAccess</c>.
/// </summary>
/// <remarks> Since <c>siAccess</c> needs additional components, this class cannot be instantiated anymore. Instead, inherit from this class and provide the requirements in the static constructor of the subclass. </remarks>
public abstract class SiAccessPlc : Plc
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> Data used for connecting to the plc. </summary>
	private ISiAccessPlcConnectionData ConnectionData { get; }

	/// <summary> siAccess plc connection object. </summary>
	private ISiAccess? UnderlyingPlc { get; set; }

	/// <summary> <see cref="SiAccessErrorMapping"/> used to resolve error codes to messages. </summary>
	protected internal static SiAccessErrorMapping ErrorMapping { get; internal set; }

	#endregion

	#region Enumerations

	private enum CommunicationResult
	{
		Success = 0,
		Error = 1,
	}

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Static constructor
	/// </summary>
	static SiAccessPlc()
	{
		// Setup null-error mapping.
		ErrorMapping = new ();
			
		// Setup other siAccess properties.
		AGL.ReturnJobNr(false);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="connectionData"> <see cref="ISiAccessPlcConnectionData"/> </param>
	protected SiAccessPlc(ISiAccessPlcConnectionData connectionData)
		//! siAccess's own 'Timeout' (set from ConnectionData.ConnectionTimeout during OpenConnection) is documented to apply to communication functions too.
		//! But has been observed in production to NOT reliably abort 'ReadMixEx'/'WriteMixEx' when the underlying TCP connection dies.
		//! Therefore, an explicit guard timeout is passed to the base class. A margin is added over the raw siAccess timeout to avoid racing against legitimate siAccess-internal retries.
		: base(connectionData.Id, connectionData.Name, connectionData.ConnectionTimeout + TimeSpan.FromSeconds(5))
	{
		// Save parameters.
		this.ConnectionData = connectionData;

		// Initialize fields.
	}

	#endregion

	#region Methods

	#region Connection

	#region Connection Callbacks

	private void OnConnectionErrorOccurred(ISiAccess sender, ConnectionEventArgs args)
		=> base.OnInterrupted(executeReconnect: true);

	private void OnConnectAborted(ISiAccess sender, ConnectionEventArgs args)
		=> base.OnInterrupted(executeReconnect: true);

	#endregion

	/// <inheritdoc />
	protected sealed override bool OpenConnection()
	{
		if (base.ConnectionState == PlcConnectionState.Connected) return true;

		// Create a new siAccess-PLC instance.
		var connectionData = this.ConnectionData;
		var plc = AGLConnectionFactory.CreateInstanceAndConfigureTcpIp
		(
			devNr: connectionData.DeviceNumber % (byte.MaxValue + 1), //! Testing showed that any number larger than 255 leads to a 'System.AccessViolationException'.
			entry: 0,
			plcNr: 0,
			rackNr: connectionData.Rack,
			slotNr: connectionData.Slot,
			plc_class: AGL.PLC_Class.ePLC_1200, //? Which type is correct?
			ip: connectionData.Ip,
			port: 0, //! This value seems to be ignored, as any test with different values always succeeded.
			timeout: (int) Math.Abs(this.ConnectionData.ConnectionTimeout.TotalMilliseconds),
			reportType: AsyncReportType.Callbacks,
			result: out var result
		);
		if (plc is null)
		{
			base.Logger.Error($"Creating an instance of the underlying plc connection '{nameof(ISiAccess)}' failed. Error was '{ErrorMapping.GetErrorMessageForCode(result)}'.");
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

		ISiAccess plc = this.UnderlyingPlc;
		this.UnderlyingPlc = null;

		// Remove the handlers to the event-callbacks when the connection has been interrupted.
		plc.OnConnectionErrorOccured -= this.OnConnectionErrorOccurred;
		plc.OnConnectAborted -= this.OnConnectAborted;

		var result = plc.Disconnect();
		plc.Dispose();

		return result;
	}

	/// <inheritdoc />
	/// <remarks>
	/// A hung 'ReadMixEx'/'WriteMixEx' call may never return (observed in production to block for over 10 days), so the underlying connection object cannot be safely awaited on or disposed of at this point.
	/// Instead, it is abandoned here (handlers detached, reference dropped) without calling <see cref="ISiAccess.Disconnect"/> or <b>ISiAccess.Dispose</b>, accepting a bounded resource leak of that single instance,
	/// so that reconnection can proceed immediately via a fresh instance.
	/// </remarks>
	protected override void OnCommunicationTimeout()
	{
		var plc = this.UnderlyingPlc;
		if (plc is null) return;

		// Imminently abandon the underlying plc instance and let it be garbage collected (after the orphaned read/write call has completed, if ever).
		// Setting it to null as soon as possible means that other calls to 'PerformReadWriteAsync' will fail fast with a 'NotConnectedPlcException' instead of potentially blocking on the orphaned read/write call.
		this.UnderlyingPlc = null;

		// Detach the handlers so the stale (possibly still busy) instance can no longer invoke reconnect logic on our behalf.
		plc.OnConnectionErrorOccured -= this.OnConnectionErrorOccurred;
		plc.OnConnectAborted -= this.OnConnectAborted;

		//! Deliberately NOT calling 'plc.Disconnect()' or 'plc.Dispose()' here, since the orphaned read/write call may still be running against this instance and might never complete.
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
		var underlyingPlc = VerifyConnectivity(this, plcItems, usageType);

		// Create the mapping.
		var (mapping, allSiAccessItems) = CreateMappingAndSiAccessItems(plcItems, usageType);

		// Read from the plc.
		//! The return value may be zero even if some or all of the items failed. To get the real result the 'Result' property of the siAccess item (AGL.DATA_RW40.Result) must be checked.
		var readResult = await Task.Run(() => underlyingPlc.ReadMixEx(allSiAccessItems, allSiAccessItems.Length), cancellationToken);

		// If the underlying plc connection object has since been abandoned (e.g. because a previous guard-timeout for this very operation already fired and triggered a reconnect),
		// then this is a stale result arriving after the fact. Applying it now could overwrite fresher values from a subsequent retry, so it must be discarded instead.
		if (!ReferenceEquals(underlyingPlc, this.UnderlyingPlc))
		{
			base.Logger.Info($"A stale read operation on {this:LOG} completed after its underlying connection had already been abandoned (most likely due to a previous communication timeout). Its result will be discarded.");
			return;
		}

		// Verify the total result.
		//! Ignore the total result and inspect the result of each item individually.
		var result = ConvertToCommunicationResult(readResult);
		//if (result != CommunicationResult.Success)
		//{
		//	var errorMessage = ErrorMapping.GetErrorMessageForCode(readResult);
		//	var items = plcItems.Select(item => (item, "General reading error.")).ToArray();
		//	throw new ReadPlcException(new IPlcItem[0], items, $"Could not read any items from {this:LOG}. siAccess returned error code '{result}' ({errorMessage}).");
		//}

		// Verify the result of all items and transfer the value.
		var (validItems, failedItems) = VerifyPlcItemResults(mapping, allSiAccessItems, true);
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
		var underlyingPlc = VerifyConnectivity(this, plcItems, usageType);

		// Create the mapping.
		var (mapping, allSiAccessItems) = CreateMappingAndSiAccessItems(plcItems, usageType);

		// Write to the plc.
		var writeResult = await Task.Run(() => underlyingPlc.WriteMixEx(allSiAccessItems, allSiAccessItems.Length), cancellationToken);

		// If the underlying plc connection object has since been abandoned (e.g. because a previous guard-timeout for this very operation already fired and triggered a reconnect),
		// then this is a stale result arriving after the fact. Its outcome is unreliable at this point (a subsequent retry may already have written fresher values), so it must be discarded instead.
		if (!ReferenceEquals(underlyingPlc, this.UnderlyingPlc))
		{
			var itemDescriptions = String.Join(", ", plcItems.Select(plcItem => $"{plcItem:FULL}"));
			base.Logger.Error($"A stale write operation on {this:LOG} completed after its underlying connection had already been abandoned (most likely due to a previous communication timeout). This means that potentially deprecated data has been written to the plc for the following items: {itemDescriptions}.");
			return;
		}

		// Verify the total result.
		//! Ignore the total result and inspect the result of each item individually.
		var result = ConvertToCommunicationResult(writeResult);
		//if (result != CommunicationResult.Success)
		//{
		//	var errorMessage = ErrorMapping.GetErrorMessageForCode(writeResult);
		//	var items = plcItems.Select(item => (item, "General writing error.")).ToArray();
		//	throw new WritePlcException(new IPlcItem[0], items, $"Could not write any items to {this:LOG}. siAccess returned error code '{result}' ({errorMessage}).");
		//}

		// Verify the result of all items.
		var (validItems, failedItems) = VerifyPlcItemResults(mapping, allSiAccessItems, false);
		if (failedItems.Any()) throw new WritePlcException(validItems, failedItems, $"Some of the items couldn't be written. See the '{nameof(ReadOrWritePlcException.FailedItems)}' property for further information.");
	}

	#endregion
		
	#region Helper

	/// <summary>
	/// Verifies that <paramref name="plc.UnderlyingPlc"/> is not <see langword="null"/> before reading or writing.
	/// </summary>
	/// <remarks> Normally the base class already handles cases where the plc connection is not yet established, but just in case and to keep the compiler from complaining. </remarks>
	/// <exception cref="NotConnectedPlcException"> Thrown if <paramref name="plc.UnderlyingPlc"/> is <see langword="null"/>. </exception>
	private static ISiAccess VerifyConnectivity(SiAccessPlc plc, ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
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
	/// Creates all needed <see cref="AGL.DATA_RW40"/> items for the <paramref name="plcItems"/> along with a <see cref="ReadPlcItemWrapper"/>  for each.
	/// </summary>
	/// <param name="plcItems"> The plc items for which to create the siAccess items. </param>
	/// <param name="usageType"> The <see cref="Phoenix.Data.Plc.Plc.PlcItemUsageType"/> of the items. </param>
	/// <returns>
	/// <para> A <see cref="ValueTuple"/> with: </para>
	/// <para> Mapping: The <see cref="ReadPlcItemWrapper"/>s. </para>
	/// <para> AllSiAccessItems: The <see cref="AGL.DATA_RW40"/> items. </para>
	/// </returns>
	private static (ICollection<ReadPlcItemWrapper> Mapping, AGL.DATA_RW40[] AllSiAccessItems) CreateMappingAndSiAccessItems(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
	{
		var mapping = plcItems
			.Select(plcItem => new ReadPlcItemWrapper(plcItem))
			.ToArray()
			;

		// Create all needed siAccess items.
		//! Directly storing the siAccess items in the ReadPlcItemWrapper instance won't work, as AGL.DATA_RW40 is a struct that would consequently be copied on assignment.
		//! Therefore, the ReadPlcItemWrapper only contains the start position and the amount of items in the returned siAccess items array.
		var previousAmount = 0;
		var allSiAccessItems = mapping
			.SelectMany
			(
				readPlcItemWrapper =>
				{
					var siAccessItems = CreateSiAccessItems(readPlcItemWrapper.PlcItem, usageType).ToArray();

					readPlcItemWrapper.Start = (previousAmount += siAccessItems.Length) - 1;
					readPlcItemWrapper.Amount = siAccessItems.Length;

					return siAccessItems;
				}
			)
			.ToArray()
			;

		return (mapping, allSiAccessItems);
	}

	/// <summary>
	/// Creates siAccess plcItems from <paramref name="plcItem"/>. Some <see cref="IPlcItem"/>s can only be handled by multiple siAccess items.
	/// </summary>
	/// <param name="plcItem"> The <see cref="IPlcItem"/> used to build the siAccess item. </param>
	/// <param name="usageType"> The <see cref="Plc.PlcItemUsageType"/> of the <paramref name="plcItem"/>. </param>
	/// <returns> New siAccess items of type <see cref="AGL.DATA_RW40"/>. </returns>
	private static IEnumerable<AGL.DATA_RW40> CreateSiAccessItems(IPlcItem plcItem, PlcItemUsageType usageType)
	{
		var siAccessItem = new AGL.DATA_RW40
		{
			Offset = plcItem.Position
		};
		switch (plcItem.Type)
		{
			case PlcItemType.Input:
				siAccessItem.DBNr = 0;
				siAccessItem.OpArea = AGL.AREA_IN;
				break;
			case PlcItemType.Output:
				siAccessItem.DBNr = 0;
				siAccessItem.OpArea = AGL.AREA_OUT;
				break;
			case PlcItemType.Flags:
				siAccessItem.DBNr = 0;
				siAccessItem.OpArea = AGL.AREA_FLAG;
				break;
			default:
			case PlcItemType.Data:
				siAccessItem.DBNr = plcItem.DataBlock;
				siAccessItem.OpArea = AGL.AREA_DATA;
				break;
		}

		if (plcItem.Value.HandlesFullBytes || (usageType == PlcItemUsageType.Read && plcItem.Value.Length > 1))
		{
			siAccessItem.OpType = AGL.TYP_BYTE;
			siAccessItem.BitNr = 0;
			siAccessItem.OpAnz = (ushort) DataHelper.GetByteAmountForBits(plcItem.Value.Length);
			siAccessItem.B = usageType == PlcItemUsageType.Write ? plcItem.Value : new byte[siAccessItem.OpAnz];
			
			yield return siAccessItem;
		}
		else
		{
			siAccessItem.OpType = AGL.TYP_BIT;
			siAccessItem.OpAnz = 1;

			for (byte bitPosition = 0; bitPosition < plcItem.Value.Length; bitPosition++)
			{
				var bitSiAccessItem = siAccessItem; // Value type will be copied on assignment.
				bitSiAccessItem.BitNr = (ushort)(bitPosition + plcItem.BitPosition);
				// Get the relevant bit of this item and set the siAccess byte accordingly.						
				bitSiAccessItem.B = usageType == PlcItemUsageType.Write ? [plcItem.Value[bitPosition] ? (byte) 1 : (byte) 0] : new byte[1];

				yield return bitSiAccessItem;
			}
		}
	}

	/// <summary>
	/// Verifies the result of all <see cref="AGL.DATA_RW40"/> items linked to an <see cref="IPlcItem"/> and sorts them into two lists.
	/// </summary>
	/// <param name="mapping"> The <see cref="ReadPlcItemWrapper"/>s used to identify which siAccess items belong to which plc item. </param>
	/// <param name="allSiAccessItems"> The siAccess items themselves. </param>
	/// <param name="transferData"> Optional flag if the data from the siAccess items should be transferred to the plc items. </param>
	/// <returns>
	/// <para> A <see cref="ValueTuple"/> with: </para>
	/// <para> ValidItems: The <see cref="IPlcItem"/>s that where valid. </para>
	/// <para> FailedItems: The <see cref="IPlcItem"/> that where not valid together with the error message. </para>
	/// </returns>
	private static (ICollection<IPlcItem> ValidItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)>) VerifyPlcItemResults(ICollection<ReadPlcItemWrapper> mapping, ICollection<AGL.DATA_RW40> allSiAccessItems, bool transferData)
	{
		var validItems = new List<IPlcItem>(mapping.Count);
		var failedItems = new List<(IPlcItem FailedItem, string ErrorMessage)>(0); //! Assume that no errors occur to save memory.

		// Iterate each plc item and get the data for all siAccess items that where needed to handle it.
		foreach (var (plcItem, start, amount) in mapping)
		{
			// Get all siAccess items of this plc item.
			var siAccessItems = allSiAccessItems.Skip(start).Take(amount).ToArray();

			// Check if any of those have failed.
			var itemResult = siAccessItems
				.Select(siAccessItem => siAccessItem.Result)
				.FirstOrDefault(resultCode => resultCode != 0)
				;
				
			var result = ConvertToCommunicationResult(itemResult);
			if (result != CommunicationResult.Success)
			{
				failedItems.Add((plcItem, ErrorMapping.GetErrorMessageForCode(itemResult)));
			}
			else
			{
				validItems.Add(plcItem);
				if (transferData)
				{
					var data = siAccessItems.SelectMany(siAccessItem => siAccessItem.B).ToArray();
					TransferValue(plcItem, data);
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
	/// Converts the passed <c>siAccess</c> result into a <see cref="CommunicationResult"/>.
	/// </summary>
	/// <param name="result"> The result as <see cref="int"/>. </param>
	/// <returns> A <see cref="CommunicationResult"/>. </returns>
	private static CommunicationResult ConvertToCommunicationResult(int result)
	{
		return result == 0 ? CommunicationResult.Success : CommunicationResult.Error;
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
	/// Wrapper for an <see cref="IPlcItem"/> that stores <see cref="Start"/> and <see cref="Amount"/> for reading multiple <see cref="AGL.DATA_RW40"/> items at once.
	/// </summary>
	private class ReadPlcItemWrapper
	{
		/// <summary>
		/// The wrapped <see cref="IPlcItem"/>.
		/// </summary>
		public IPlcItem PlcItem { get; }

		/// <summary> When reading / writing (multiple) <see cref="IPlcItem"/> those items will be split up into an array of <see cref="AGL.DATA_RW40"/> structures. This is the index of the first such structure within the array. </summary>
		public int Start { private get; set; }

		/// <summary> When reading / writing (multiple) <see cref="IPlcItem"/> those items will be split up into an array of <see cref="AGL.DATA_RW40"/> structures. This is the amount of how many consecutive structures where needed to handle the item. </summary>
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