#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// <para> Base plc class that provides functionality for: </para>
	/// <para> • Thread safe connection establishment with automatic re-connect for interrupted connections. </para>
	/// <para> • Thread safe reading and writing of <see cref="IPlcItem"/>s with automatic re-tries. </para>
	/// </summary>
	public abstract class Plc : IPlc
	{
		#region Delegates / Events

		/// <inheritdoc />
		public event EventHandler<PlcConnectionState> Connected;
		
		/// <summary>
		/// Invoked if the connection to the plc has been established.
		/// </summary>
		protected void OnConnected()
		{
			lock (_connectionStateChangeLock)
			{
				if (this.ConnectionState == PlcConnectionState.Connected) return;

				this.Logger.Info($"Connection to the plc has been established.");

				this.ConnectionState = PlcConnectionState.Connected;
				this.Connected?.Invoke(this, this.ConnectionState);
			
				this.RestartHandleTasks();
			}
		}

		/// <inheritdoc />
		public event EventHandler<PlcConnectionState> Disconnected;

		/// <summary>
		/// Invoked if the connection to the plc has been disconnected.
		/// </summary>
		protected void OnDisconnected()
		{
			lock (_connectionStateChangeLock)
			{
				if (this.ConnectionState == PlcConnectionState.Disconnected) return;

				this.Logger.Info($"Connection to the plc has been disconnected.");

				this.ConnectionState = PlcConnectionState.Disconnected;
				this.Disconnected?.Invoke(this, this.ConnectionState);
			}
		}

		/// <inheritdoc />
		public event EventHandler<PlcConnectionState> Interrupted;

		/// <summary>
		/// Invoked if the connection to the plc has been lost.
		/// </summary>
		protected void OnInterrupted(bool executeReconnect)
		{
			lock (_connectionStateChangeLock)
			{
				if (this.ConnectionState == PlcConnectionState.Interrupted) return;

				this.Logger.Error($"Connection to the plc has been interrupted.");

				this.ConnectionState = PlcConnectionState.Interrupted;
				this.Interrupted?.Invoke(this, this.ConnectionState);

				// Reestablish the connection.
				if (executeReconnect) this.ReestablishConnection();
			}
		}

		#endregion

		#region Constants
		#endregion

		#region Fields

		private int _isReconnecting;

		private readonly object _connectionStateChangeLock;

		/// <summary> Contains <see cref="CancellationTokenSource"/> objects for all <see cref="IPlcItem"/>s whose handling has been put on hold. </summary>
		private readonly ConcurrentQueue<CancellationTokenSource> _waitQueue;
		
		#endregion

		#region Properties

		/// <summary>
		/// Default <see cref="ILogger"/> instance.
		/// </summary>
		protected ILogger Logger => _logger.Value;
		private readonly Lazy<ILogger> _logger = new Lazy<ILogger>(LogManager.GetLogger);

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public PlcConnectionState ConnectionState { get; private set; }

		#endregion

		#region Enumerations

		/// <summary>
		/// Usage types of <see cref="IPlcItem"/>.
		/// </summary>
		protected enum PlcItemUsageType : byte
		{
			/// <summary> The plc item will be read. </summary>
			Read,
			/// <summary> The plc item will be written. </summary>
			Write,
		}

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"> The name of the plc. This is purely used for identification and log purposes. </param>
		protected Plc(string name)
		{
			// Save parameters.
			this.Name = name;

			// Initialize fields.
			this.ConnectionState = PlcConnectionState.Disconnected;
			_isReconnecting = 0;
			_connectionStateChangeLock = new object();
			_waitQueue = new ConcurrentQueue<CancellationTokenSource>();
		}

		#endregion

		#region Methods

		#region Connection

		#region Connect

		/// <inheritdoc />
		public bool Connect()
		{
			lock (_connectionStateChangeLock)
			{
				var success = this.OpenConnection();
				if (success) this.OnConnected();
				return success;
			}
		}

		/// <summary>
		/// Establishes a connection to the plc.
		/// </summary>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		protected abstract bool OpenConnection();

		#endregion

		#region Disconnect
		
		/// <inheritdoc />
		public bool Disconnect()
		{
			lock (_connectionStateChangeLock)
			{				
				var success = this.CloseConnection();
				if (success) this.OnDisconnected();
				return success;
			}
		}

		/// <summary>
		/// Disconnects the link to the plc.
		/// </summary>
		/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
		protected abstract bool CloseConnection();

		#endregion

		#region Reconnect

		/// <inheritdoc />
		public bool Reconnect()
		{
			lock (_connectionStateChangeLock)
			{
				this.Disconnect();
				return this.Connect();
			}
		}

		/// <summary>
		/// Continuously tries to reestablish the connection as long as the <see cref="ConnectionState"/> is still <see cref="PlcConnectionState.Interrupted"/>.
		/// </summary>
		private void ReestablishConnection()
		{
			// One time is enough.
			if (Interlocked.CompareExchange(ref _isReconnecting, 1, 0) == 1) return;

			Task.Run(async () =>
			{
				do
				{
					if (this.ConnectionState != PlcConnectionState.Interrupted) break;
					
					// Directly close the connection to prevent change notifications.
					lock (_connectionStateChangeLock)
					{
						this.CloseConnection();
					}
					if (this.ConnectionState != PlcConnectionState.Interrupted) break;

					await Task.Delay(1000);
					if (this.ConnectionState != PlcConnectionState.Interrupted) break;

					// To reestablish the connection use the normal functionality so that change notifications will be raised on success.
					this.Connect();

				} while (true);

				_isReconnecting = 0;
			});
			
			//TODO Check if this functions leaves even though the above task still runs. This should be the case because the task is not awaited.
		}

		#endregion

		#endregion

		#region Read

		/// <inheritdoc />
		public async Task ReadItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
		{
			await this.ReadItemsAsync(plcItems.IsReadOnly ? new List<IPlcItem>(plcItems) : (IList<IPlcItem>) plcItems, cancellationToken);

			// Since this may be a costly operation, check if logging is even enabled.
			if (LogManager.LogAllReadAndWriteOperations)
			{
				foreach (var plcItem in plcItems)
				{
					var value = String.Join(",", (byte[]) plcItem.Value);
					var message = $"Reading {{0}} returned '{{1}}'.";
					this.Logger.Trace(message, plcItem, value);
				}
			}
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal virtual async Task ReadItemsAsync(IList<IPlcItem> plcItems, CancellationToken cancellationToken = default)
		{
			// Get and iterate all dynamic items.
			var flexiblePlcItems = new List<IPlcItem>();
			var dynamicPlcItems = plcItems.OfType<IDynamicPlcItem>().ToList();
			dynamicPlcItems.ForEach
			(
				dynamicPlcItem =>
				{
					// Remove the dynamic items and replace them with a just the numeric item that will be read to get the length.
					plcItems.Remove(dynamicPlcItem);
					plcItems.Add(dynamicPlcItem.LengthPlcItem);

					// Save the flexible part of the dynamic item separately, so that they can be read after the numerical item has been updated.
					flexiblePlcItems.Add(dynamicPlcItem.FlexiblePlcItem);
				}
			);

			if (cancellationToken.IsCancellationRequested) return;
			await this.ExecuteReadWriteAsync(plcItems, PlcItemUsageType.Read, cancellationToken);
			
			if (cancellationToken.IsCancellationRequested) return;
			await this.ExecuteReadWriteAsync(flexiblePlcItems, PlcItemUsageType.Read, cancellationToken);
		}
		
		#endregion

		#region Write

		/// <inheritdoc />
		public async Task<bool> WriteItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
		{
			var success = await this.WriteItemsAsync(plcItems.IsReadOnly ? new List<IPlcItem>(plcItems) : (IList<IPlcItem>)plcItems, cancellationToken);

			// Since this may be a costly operation, check if logging is even enabled.
			if (LogManager.LogAllReadAndWriteOperations)
			{
				foreach (var plcItem in plcItems)
				{
					var value = String.Join(",", (byte[])plcItem.Value);
					var message = $"Writing '{{1}}' to {{0}} {(success ? "succeeded" : "failed")}.";
					if (success)
					{
						this.Logger.Trace(message, plcItem, value);
					}
					else
					{
						this.Logger.Error(message, plcItem, value);
					}
				}
			}

			return success;
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		internal virtual async Task<bool> WriteItemsAsync(IList<IPlcItem> plcItems, CancellationToken cancellationToken = default)
		{
			// Get and iterate all dynamic items.
			var lengthPlcItems = new List<IPlcItem>();
			var dynamicPlcItems = plcItems.OfType<IDynamicPlcItem>().ToList();
			dynamicPlcItems.ForEach
			(
				dynamicPlcItem =>
				{
					// Remove the dynamic items and replace them with a just the flexible item that will be written first.
					plcItems.Remove(dynamicPlcItem);
					plcItems.Add(dynamicPlcItem.FlexiblePlcItem);

					// Save the numeric part of the dynamic item separately, so that they can be written afterwards.
					lengthPlcItems.Add(dynamicPlcItem.LengthPlcItem);
				}
			);

			if (cancellationToken.IsCancellationRequested) return false;
			var success = await this.ExecuteReadWriteAsync(plcItems, PlcItemUsageType.Write, cancellationToken);
			
			if (cancellationToken.IsCancellationRequested) return false;
			success &= await this.ExecuteReadWriteAsync(lengthPlcItems, PlcItemUsageType.Write, cancellationToken);
			
			return success;
		}
		
		#endregion

		#region Read / Write
		
		/// <summary>
		/// Reads or writes the <paramref name="plcItems"/> according to <paramref name="usageType"/> with automatic reconnection attempts.
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to read or write. </param>
		/// <param name="usageType"> The <see cref="PlcItemUsageType"/> defining whether a read or a write operation is performed. </param>
		/// <returns> An awaitable <see cref="Task"/> containing <c>True</c> on success, otherwise <c>False</c>. </returns>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the operation. </param>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private async Task<bool> ExecuteReadWriteAsync(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType, CancellationToken cancellationToken)
		{
			if (!plcItems.Any()) return true;

			while (true)
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();

					lock (_connectionStateChangeLock)
					{
						// If no active connection is available, then throw an exception.
						/*!
						 * This means, that reading/writing items while the plc is not connected, will result in the calling function to indefinitely wait,
						 * because no reconnect will be made when the connection was deliberately not established.
						 */
						if (this.ConnectionState == PlcConnectionState.Disconnected)
						{
							var itemDescriptions = Plc.GetPlcItemDescription(plcItems);
							throw new PlcException(PlcExceptionType.NotConnected, $"Cannot {usageType.ToString().ToLower()} the plc items ({itemDescriptions}) because the plc is not connected. All items will be put on hold.");
						}
					}

					await Task.Run(() => this.PerformReadWriteAsync(plcItems, usageType, cancellationToken), cancellationToken);
					break;
				}
				// This handles task cancellation.
				catch (TaskCanceledException)
				{
					return false;
				}
				// Catch only recoverable exceptions.
				catch (PlcException ex) when (ex.ExceptionType != PlcExceptionType.UnrecoverableConnection)
				{
					this.Logger.Error(ex.Message);

					// Create a cancellation token source for this handle callback that is linked to the external one.
					var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					_waitQueue.Enqueue(cancellationTokenSource);
					var sleepCancellationToken = cancellationTokenSource.Token;
					
					// Check if a reconnection could be useful.
					lock (_connectionStateChangeLock)
					{
						// Don't reconnect if the connection is disconnected to begin with.
						if (this.ConnectionState != PlcConnectionState.Disconnected)
						{
							this.OnInterrupted(executeReconnect: true);
						}
					}

					// Indefinite wait until the task gets canceled.
					try
					{
						await Task.Delay(Timeout.Infinite, sleepCancellationToken);
					}
					catch (TaskCanceledException)
					{
						// Stop execution if the original (external) token was canceled.
						if (cancellationToken.IsCancellationRequested) return false;
					}

					//TODO Check if cancellation happened because of disposing.

					this.Logger.Info("The previously suspended plc items will now be handled again.");
				}
			}

			return true;
		}

		/// <summary>
		/// Asynchronously reads or writes the <paramref name="plcItems"/> to / from the plc.
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
		/// <param name="usageType"> The <see cref="PlcItemUsageType"/> defining whether a read or a write operation is performed. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the operation. </param>
		protected abstract Task PerformReadWriteAsync(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType, CancellationToken cancellationToken);

		#endregion
		
		#region Helper

		/// <summary>
		/// Returns a readable representation of the <paramref name="plcItems"/>.
		/// </summary>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to get a readable representation from. </param>
		/// <returns> A string representation. </returns>
		public static string GetPlcItemDescription(IEnumerable<IPlcItem> plcItems)
		{
			return String.Join(",", plcItems.Select(plcItem => plcItem.ToString()));
		}

		private void RestartHandleTasks()
		{
			// Start all waiting handle callbacks.
			while (_waitQueue.Count > 0)
			{
				if (_waitQueue.TryDequeue(out var cancellationTokenSource))
				{
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
				}
			}
		}

		#endregion

		#region IDisposable

		/// <inheritdoc />
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc cref="Dispose()"/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.Disconnect();
			}
		}

		#endregion

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: Name: {this.Name}]";

		#endregion
	}
}