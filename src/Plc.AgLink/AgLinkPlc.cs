using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Base class for all <see cref="Plc"/> implementations utilizing <c>AGLink</c>.
	/// </summary>
	/// <typeparam name="TAgLinkPlc"> The concrete AGLink type. </typeparam>
	public abstract class AgLinkPlc<TAgLinkPlc> : Plc
		where TAgLinkPlc : IDisposable
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly IDisposable _agLinkAssemblyProvider;

		#endregion

		#region Properties

		/// <summary> Data used for connecting to the plc. </summary>
		protected AgLinkPlcConnectionData ConnectionData { get; }

		/// <summary> AGLink plc connection object. </summary>
		protected TAgLinkPlc UnderlyingPlc { get; set; }

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
		/// Constructor
		/// </summary>
		/// <param name="agLinkSetupAdapter"> An instance of <see cref="IAgLinkSetupAdapter"/> used for initial setup. </param>
		/// <param name="connectionData"> <see cref="AgLinkPlcConnectionData"/> </param>
		protected AgLinkPlc(IAgLinkSetupAdapter agLinkSetupAdapter, AgLinkPlcConnectionData connectionData)
			: base(name: connectionData.Name)
		{
			// Save parameters.
			this.ConnectionData = connectionData;

			// Initialize fields.

			// Setup AGLink.
			var agLinkAssemblyProvider = new AgLinkResourceProvider(agLinkSetupAdapter, connectionData);
			agLinkAssemblyProvider.Initialize();
			_agLinkAssemblyProvider = agLinkAssemblyProvider;
		}
		
		#endregion

		#region Methods

		#region Connection
		
		/// <inheritdoc />
		protected abstract override bool OpenConnection();

		/// <inheritdoc />
		protected abstract override bool CloseConnection();

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
		protected abstract Task PerformReadAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken);

		/// <summary>
		/// Writes the <see cref="IPlcItem.Value"/> of the <paramref name="plcItems"/> to the plc. 
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the write operation. </param>
		protected abstract Task PerformWriteAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken);
		
		#endregion

		#region Helper
		
		/// <summary>
		/// Sets the <paramref name="data"/> to the <paramref name="plcItem"/>s <see cref="IPlcItem{TValue}.Value"/>.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> whose value to set. </param>
		/// <param name="data"> The new byte data. </param>
		protected void TransferValue(IPlcItem plcItem, byte[] data)
		{
			if (!plcItem.Value.HandlesFullBytes && plcItem.Value.Length > 1)
			{
				var booleans = DataConverter.ToBooleans(data).Skip((byte) plcItem.BitPosition).Take((int) plcItem.Value.Length).ToArray();
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
		protected void VerifyAgLinkResult(int result, ICollection<IPlcItem> plcItems, PlcItemUsageType usageType)
		{
			var agLinkResult = AgLinkPlc<TAgLinkPlc>.ConvertToAgLinkResult(result);

			if (agLinkResult == AgLinkResult.Success)
			{
				// If handling the plc item was successful, then immediately return the plc items.
				return;
			}
			else
			{
				var itemDescriptions = Plc.GetPlcItemDescription(plcItems);
				if (agLinkResult == AgLinkResult.RecoverableError)
				{
					throw new PlcException(usageType == PlcItemUsageType.Read ? PlcExceptionType.ReadError : PlcExceptionType.WriteError, $"Could not {usageType.ToString().ToLower()} the '{itemDescriptions}' from the plc. AGLink returned error code '{result}'. Items will be handled again.");
				}
				else
				{
					throw new PlcException(PlcExceptionType.UnrecoverableConnection, $"Could not {usageType.ToString().ToLower()} the '{itemDescriptions}' from the plc. AGLink returned error code '{result}'. This is an unrecoverable error and the items will not be handled again.");
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
			else  return AgLinkResult.UnrecoverableError;
		}

		#endregion

		#region IDisposable
		
		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			this.UnderlyingPlc?.Dispose();
			_agLinkAssemblyProvider.Dispose();
		}

		#endregion

		#endregion

		#region Nested Types

		/// <summary>
		/// Wrapper for an <see cref="IPlcItem"/> that stores <see cref="Start"/> and <see cref="Amount"/> for reading multiple <c>AGL4.DATA_RW40</c> items at once.
		/// </summary>
		protected class ReadPlcItemWrapper
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