﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Collections.Concurrent;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc;

/// <summary>
/// <para> Base plc class that provides functionality for: </para>
/// <para> • Thread safe connection establishment with automatic re-connect for interrupted connections. </para>
/// <para> • Thread safe reading and writing of <see cref="IPlcItem"/>s with automatic re-tries. </para>
/// </summary>
public abstract class Plc : IPlc, IFormattable
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

			this.Logger.Info($"Connection to {this:LOG} has been established.");

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

			this.Logger.Info($"Connection to {this:LOG} has been disconnected.");

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

			this.Logger.Error($"Connection to {this:LOG} has been interrupted.");

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

	/// <summary>
	/// This <see cref="CancellationTokenSource"/> that will be canceled when the instance is disposed.
	/// </summary>
	private readonly CancellationTokenSource _disposeTokenSource;
		
	private readonly CancellationToken _disposeToken;

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
	public int Id { get; }
		
	/// <inheritdoc />
	public string Name { get; }

	/// <inheritdoc />
	public PlcConnectionState ConnectionState { get; private set; }

	#endregion

	#region Enumerations

	/// <summary>
	/// Usage types of <see cref="IPlcItem"/>.
	/// </summary>
	protected internal enum PlcItemUsageType : byte
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
	/// <remarks> The id of the instance will be <c>-1</c> and its name an empty string. </remarks>
	protected Plc() : this(-1, String.Empty) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="id"> The id of the plc. This is purely used for identification and log purposes. </param>
	/// <remarks> The name of the instance will be an empty string. </remarks>
	protected Plc(int id) : this(id, String.Empty) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="name"> The name of the plc. This is purely used for identification and log purposes. </param>
	/// <remarks> The id of the instance will be <c>-1</c>. </remarks>
	protected Plc(string name) : this(-1, name) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plcInformation"> <see cref="IPlcInformation"/> used to create the instance. </param>
	protected Plc(IPlcInformation plcInformation) : this(plcInformation.Id, plcInformation.Name) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="id"> The id of the plc. This is purely used for identification and log purposes. </param>
	/// <param name="name"> The name of the plc. This is purely used for identification and log purposes. </param>
	protected Plc(int id, string name)
	{
		// Save parameters.
		this.Id = id;
		this.Name = name;
			
		// Initialize fields.
		this.ConnectionState = PlcConnectionState.Disconnected;
		_disposeTokenSource = new CancellationTokenSource();
		_disposeToken = _disposeTokenSource.Token;
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
			if (success)
			{
				this.OnConnected();
				return true;
			}
			else
			{
				try
				{
					this.Disconnect();
				}
				catch (Exception) { /* ignore */ }
				return false;
			}
		}
	}

	/// <summary>
	/// Establishes a connection to the plc.
	/// </summary>
	/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
	protected internal abstract bool OpenConnection();

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
	protected internal abstract bool CloseConnection();

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
			var message = $"Reading {{0}} from {this:LOG} returned '{{1}}'.";
			foreach (var plcItem in plcItems)
			{
				var item = $"{plcItem:LOG}";
				var value = $"{plcItem.Value:HEX}";
				this.Logger.Trace(message, item, value);
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
				var position = plcItems.IndexOf(dynamicPlcItem);
				plcItems.RemoveAt(position);
				plcItems.Insert(position, dynamicPlcItem.LengthPlcItem);

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
		var success = await this.WriteItemsAsync(plcItems.IsReadOnly ? new List<IPlcItem>(plcItems) : (IList<IPlcItem>) plcItems, cancellationToken);

		// Since this may be a costly operation, check if logging is even enabled.
		if (LogManager.LogAllReadAndWriteOperations)
		{
			var message = $"Writing '{{1}}' to {{0}} in {this:LOG} {(success ? "succeeded" : "failed")}.";
			foreach (var plcItem in plcItems)
			{
				var item = $"{plcItem:LOG}";
				var value = $"{plcItem.Value:HEX}";
				if (success)
				{
					this.Logger.Trace(message, item, value);
				}
				else
				{
					this.Logger.Error(message, item, value);
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
				// Remove the dynamic items and replace them with the flexible item that will be written first.
				var position = plcItems.IndexOf(dynamicPlcItem);
				plcItems.RemoveAt(position);
				plcItems.Insert(position, dynamicPlcItem.FlexiblePlcItem);

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
		// Allow only items that have a length greater than zero. This is mostly needed for dynamic items.
		plcItems = plcItems.Where(item => item.Value.Length > 0).ToList();
		if (!plcItems.Any()) return true;

		var cancellationTokenSource = this.BuildLinkedTokenSource(cancellationToken);
		cancellationToken = cancellationTokenSource.Token;
		try
		{
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
							throw new NotConnectedPlcException($"Cannot {usageType.ToString().ToLower()} the plc items ({itemDescriptions}) because {this:LOG} is not connected. All items will be put on hold.");
						}
					}

					await Task.Run(() => this.PerformReadWriteAsync(plcItems, usageType, cancellationToken), cancellationToken);
					break;
				}
				// This handles task cancellation.
				catch (OperationCanceledException)
				{
					if (_disposeToken.IsCancellationRequested)
					{
						if (usageType == PlcItemUsageType.Read) throw new DisposedReadPlcException(plcItems);
						else throw new DisposedWritePlcException(plcItems);
					}
					return false;
				}
				// Handle not connected exceptions by trying to re-connect and halting the items until a connection was established.
				catch (NotConnectedPlcException ex)
				{
					this.Logger.Error(ex.Message);

					// Create a cancellation token source for this handle callback that is linked to the external one.
					var sleepCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					_waitQueue.Enqueue(sleepCancellationTokenSource);
					var sleepCancellationToken = sleepCancellationTokenSource.Token;

					// Check if a reconnection could be useful.
					lock (_connectionStateChangeLock)
					{
						// Don't reconnect if the connection is disconnected to begin with.
						if (this.ConnectionState != PlcConnectionState.Disconnected)
						{
							this.OnInterrupted(executeReconnect: true);
						}
					}

					// Indefinitely wait until the task gets canceled.
					try
					{
						await Task.Delay(Timeout.Infinite, sleepCancellationToken);
					}
					catch (OperationCanceledException)
					{
						// Throw special dispose exception if the dispose token was canceled.
						if (_disposeToken.IsCancellationRequested)
						{
							if (usageType == PlcItemUsageType.Read) throw new DisposedReadPlcException(plcItems);
							else throw new DisposedWritePlcException(plcItems);
						}
						// Stop execution if the original (external) token was canceled.
						else if (cancellationToken.IsCancellationRequested)
						{
							return false;
						}
						// In all other cases just keep going.
						else
						{
							/* ignore */
						}
					}

					this.Logger.Info($"The previously suspended plc items of {this:LOG} will now be handled again.");
				}
				// Throw on read or write exceptions.
				catch (ReadOrWritePlcException)
				{
					throw;
				}
			}

			return true;
		}
		finally
		{
			cancellationTokenSource.Dispose();
			cancellationTokenSource = null;
#if DEBUG
			this.LinkedTokenWasCanceled();
#endif
		}
	}

#if DEBUG
	/// <summary>
	/// This method is only used to check if the internally created CancellationTokenSource during read or write operations is disposed.
	/// </summary>
	internal virtual void LinkedTokenWasCanceled() { }
#endif

	/// <summary>
	/// Asynchronously reads or writes the <paramref name="plcItems"/> to / from the plc.
	/// </summary>
	/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
	/// <param name="usageType"> The <see cref="PlcItemUsageType"/> defining whether a read or a write operation is performed. </param>
	/// <param name="cancellationToken"> A <see cref="CancellationToken"/> for cancelling the operation. </param>
	protected internal abstract Task PerformReadWriteAsync(ICollection<IPlcItem> plcItems, PlcItemUsageType usageType, CancellationToken cancellationToken);

	#endregion

	#region Helper

	/// <summary>
	/// Creates a linked <see cref="CancellationTokenSource"/> from <paramref name="cancellationToken"/> and the internal <see cref="_disposeToken"/>.
	/// </summary>
	/// <param name="cancellationToken"> The external <see cref="CancellationToken"/> used to create a linked token source. </param>
	/// <returns> A new <see cref="CancellationTokenSource"/>. </returns>
	internal CancellationTokenSource BuildLinkedTokenSource(CancellationToken cancellationToken)
	{
		CancellationTokenSource source;
		try
		{
			source = CancellationTokenSource.CreateLinkedTokenSource(_disposeToken, cancellationToken);
		}
		catch (ObjectDisposedException)
		{
			// If any of the cancellation tokens, used to create the new token source, has been disposed, then create a new and already canceled token source.
			source = new CancellationTokenSource();
			source.Cancel();
		}
		return source;
	}

	/// <summary>
	/// Returns a readable representation of the <paramref name="plcItems"/>.
	/// </summary>
	/// <param name="plcItems"> The <see cref="IPlcItem"/>s to get a readable representation from. </param>
	/// <returns> A string representation. </returns>
	public static string GetPlcItemDescription(IEnumerable<IPlcItem> plcItems)
	{
		return String.Join(",", plcItems.Select(plcItem => $"{plcItem:LOG}"));
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
			try
			{
				_disposeTokenSource?.Cancel();
				_disposeTokenSource?.Dispose();
			}
			catch (ObjectDisposedException) { /* ignore → Could be thrown if 'IPlc.Dispose' is called multiple times. */ }
		}
	}

	#endregion

	#region IFormattable

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	public override string ToString()
		=> this.ToString("N");

	/// <inheritdoc cref="IFormattable.ToString(string,System.IFormatProvider)"/>
	public string ToString(string format)
		=> this.ToString(format, null);

	/// <inheritdoc />
	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (String.IsNullOrWhiteSpace(format)) format = "N";
		formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;

		switch (format.ToUpperInvariant())
		{
			case "S":
			case "LOG":
				return this.Name;

			case "F":
			case "FULL":
				return $"[<{this.GetType().Name}> :: Id: {this.Id} | Name: {this.Name} | State: {this.ConnectionState}]";
				
			case "N":
			case "NORMAL":
			default:
				return $"[<{this.GetType().Name}> :: Name: {this.Name}]";
		}
	}

	#endregion

	#endregion
}