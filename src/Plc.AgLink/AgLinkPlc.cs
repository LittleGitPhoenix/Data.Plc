#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Phoenix.Data.Plc.Items;
using Accon.AGLink;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// <see cref="Plc"/> implementation utilizing <c>AGLink</c>.
	/// </summary>
	public sealed class AgLinkPlc : Plc
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> Data used for connecting to the plc. </summary>
		private AgLinkPlcConnectionData ConnectionData { get; }

		/// <summary> AGLink plc connection object. </summary>
		private IAGLink4? UnderlyingPlc { get; set; }

		#endregion

		#region Enumerations

		private enum AgLinkResult
		{
			UnrecoverableError = -1,
			Success = 0,
			RecoverableError = 1,
		}

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Static constructor
		/// </summary>
		static AgLinkPlc()
		{
#if (NET46 || NETSTANDARD)
			/*!
			In .Net Standard it is possible that this assembly is part of a single part executable.
			In such cases the content of the application is extracted to a temporary folder and therefore the AGLink assemblies and license file will be located in this temporary directory.			
			When using 'Directory.GetCurrentDirectory()' those files then won't be found.
			*/
			var workingDirectory = new DirectoryInfo(System.AppContext.BaseDirectory);
#else
			var workingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
#endif

			// Verify existence of unmanaged AGLink assemblies.
			var unmanagedAgLinkAssemblyFile = new FileInfo(Path.Combine(workingDirectory.FullName, $"AGLink40{(Environment.Is64BitProcess ? "_x64" : "")}.dll"));
			if (!unmanagedAgLinkAssemblyFile.Exists)
			{
#if DEBUG
				Debug.WriteLine($"The unmanaged AGLink assembly '{unmanagedAgLinkAssemblyFile.Name}' does not exist in the working directory '{workingDirectory.FullName}'. Therefore using any instance of '{nameof(AgLinkPlc)}' will probably fail.");
#else
				throw new ApplicationException($"The unmanaged AGLink assembly '{unmanagedAgLinkAssemblyFile.Name}' does not exist in the working directory '{workingDirectory.FullName}'. Please ensure its existence and then try again.");
#endif
			}

			// Try to get the license key and activate AGLink.
			var licenseKey = AgLinkPlc.GetLicenseKey(workingDirectory);
			if (licenseKey != null)
			{
				AGL4.Activate(licenseKey);
				Debug.WriteLine($"AgLink has been activated with license key '{licenseKey}'.");
			}
			else
			{
				Debug.WriteLine($"No license key for AgLink has been found.");
			}

			// Setup other AGLink properties.
			AGL4.ReturnJobNr(false);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="connectionData"> <see cref="AgLinkPlcConnectionData"/> </param>
		public AgLinkPlc(AgLinkPlcConnectionData connectionData)
			: base(name: connectionData.Name)
		{
			// Save parameters.
			this.ConnectionData = connectionData;

			// Initialize fields.
		}

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
				result: out _
			);
			if (plc is null)
			{
				base.Logger.Error($"Creating an instance of the underlying plc connection '{nameof(IAGLink4)}' failed.");
				return false;
			}
			plc.Name = this.ConnectionData.Name;
			plc.AutoReconnect = false; // Seems not to work, so disable it and handle reconnection manually via the base class.

			// Attach handlers to the event-callbacks when the connection has been interrupted.
			plc.OnConnectionErrorOccured += this.OnConnectionErrorOccured;
			plc.OnConnectAborted += this.OnConnectAborted;

			// Establish the connection.
			this.UnderlyingPlc = plc;
			return this.UnderlyingPlc.Connect();
		}

		/// <inheritdoc />
		protected override bool CloseConnection()
		{
			if (base.ConnectionState == PlcConnectionState.Disconnected) return true;
			if (this.UnderlyingPlc is null) return true;

			IAGLink4 plc = this.UnderlyingPlc;
			this.UnderlyingPlc = null;

			// Remove the handlers to the event-callbacks when the connection has been interrupted.
			plc.OnConnectionErrorOccured -= this.OnConnectionErrorOccured;
			plc.OnConnectAborted -= this.OnConnectAborted;

			var result = plc.Disconnect();
			plc.Dispose();

			return result;
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
		private async Task PerformReadAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
		{
			const PlcItemUsageType usageType = PlcItemUsageType.Read;
			var underlyingPlc = AgLinkPlc.VerifyConnectivity(this, plcItems, usageType);

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

			// Read from the plc.
			var result = await Task.Run(() => underlyingPlc.ReadMixEx(allAgLinkItems, allAgLinkItems.Length), cancellationToken);
			
			// Verify the result.
			this.VerifyAgLinkResult(result, plcItems, usageType);

			// Iterate each plc item and get the data for all AGLink items that where needed to handle it.
			foreach (var (plcItem, start, amount) in mapping)
			{
				var data = allAgLinkItems.Skip(start).Take(amount).SelectMany(agLinkItem => agLinkItem.B).ToArray();
				this.TransferValue(plcItem, data);
			}
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

			var agLinkItems = plcItems
				.SelectMany(plcItem => AgLinkPlc.CreateAgLinkItems(plcItem, usageType).ToArray())
				.ToArray()
				;

			// Write to the plc.
			var result = await Task.Run(() => underlyingPlc.WriteMixEx(agLinkItems, agLinkItems.Length), cancellationToken);

			// Verify the result.
			this.VerifyAgLinkResult(result, plcItems, usageType);
		}

		#endregion

		#region Helper

		/// <summary>
		/// Verifies that <paramref name="plc.UnderlyingPlc"/> is not <c>Null</c> before reading or writing.
		/// </summary>
		/// <remarks> Normally the base class already handles cases where the plc connection is not yet established, but just in case and to keep the compiler from complaining. </remarks>
		/// <exception cref="PlcException"> Thrown if <paramref name="plc.UnderlyingPlc"/> is <c>Null</c>. The exception type will be <see cref="PlcExceptionType.NotConnected"/>. </exception>
		private static IAGLink4 VerifyConnectivity(AgLinkPlc plc, ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
		{
			var underlyingPlc = plc.UnderlyingPlc;
			if (underlyingPlc is null)
			{
				var itemDescriptions = Plc.GetPlcItemDescription(plcItems);
				throw new PlcException(PlcExceptionType.NotConnected, $"Cannot {usageType.ToString().ToLower()} the plc items ({itemDescriptions}) because {plc:LOG} is not connected. All items will be put on hold.");
			}

			return underlyingPlc;
		}

		private static string? GetLicenseKey(DirectoryInfo workingDirectory)
		{
			//var regEx = new Regex(@"^aglink\.license$", RegexOptions.IgnoreCase);
			var licenseFile = workingDirectory
				.EnumerateFiles("aglink.license", SearchOption.TopDirectoryOnly)
				.FirstOrDefault()
				;

			if (licenseFile is null) return null;

			using var reader = licenseFile.OpenText();
			var licenseKey = reader.ReadToEnd();
			return String.IsNullOrWhiteSpace(licenseKey) ? null : licenseKey;
		}

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
		/// Sets the <paramref name="data"/> to the <paramref name="plcItem"/>s <see cref="IPlcItem{TValue}.Value"/>.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> whose value to set. </param>
		/// <param name="data"> The new byte data. </param>
		private void TransferValue(IPlcItem plcItem, byte[] data)
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
		/// Verifies the <paramref name="result"/> of the handled <paramref name="plcItems"/>.
		/// </summary>
		/// <param name="result"> The result as <see cref="AgLinkResult"/>. </param>
		/// <param name="plcItems"> The handled <see cref="IPlcItem"/>s. </param>
		/// <param name="usageType"> The <see cref="Plc.PlcItemUsageType"/> of the <paramref name="plcItems"/>. </param>
		/// <exception cref="PlcException"> Thrown if <paramref name="result"/> is not <see cref="AgLinkResult.Success"/>. </exception>
		private void VerifyAgLinkResult(int result, ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
		{
			var agLinkResult = AgLinkPlc.ConvertToAgLinkResult(result);

			if (agLinkResult == AgLinkResult.Success)
			{
				// If handling the plc item was successful, then immediately return the plc items.
				return;
			}
			else
			{
				AGL4.GetErrorMsg(result, out var errorMessage);
				var itemDescriptions = Plc.GetPlcItemDescription(plcItems);
				if (agLinkResult == AgLinkResult.RecoverableError)
				{
					throw new PlcException(usageType == PlcItemUsageType.Read ? PlcExceptionType.ReadError : PlcExceptionType.WriteError, $"Could not {usageType.ToString().ToLower()} the '{itemDescriptions}' from {this:LOG}. AGLink returned error code '{result}'. Items will be handled again.");
				}
				else
				{
					throw new PlcException(PlcExceptionType.UnrecoverableConnection, $"Could not {usageType.ToString().ToLower()} the '{itemDescriptions}' from {this:LOG}. AGLink returned error code '{result}' ({errorMessage}). This is an unrecoverable error and the items will not be handled again.");
				}
			}
		}

		/// <summary>
		/// Converts the passed <c>AGLink</c> result into a <see cref="AgLinkResult"/>.
		/// </summary>
		/// <param name="result"> The result as <see cref="int"/>. </param>
		/// <returns> A <see cref="AgLinkResult"/>. </returns>
		private static AgLinkResult ConvertToAgLinkResult(int result)
		{
			/*TODO
			 * Divide the result code into recoverable and unrecoverable errors and throw an appropriate exception.
			 * To decide which code is recoverable, somehow get all relevant return values from the AGLink assembly.
			 */
			if (result == 0) return AgLinkResult.Success;
			else return AgLinkResult.UnrecoverableError;
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
}