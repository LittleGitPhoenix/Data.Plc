#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc.Monitor.Polling;

/// <summary>
/// <see cref="IPlcMonitor"/> implementation that uses polling to monitor <see cref="IPlcItem"/>s.
/// </summary>
public class PollingPlcMonitor : IPlcMonitor
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	/// <summary> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </summary>
	private readonly IPlc _plc;
		
	/// <summary> <see cref="PlcItemMonitorConfigurations"/> specifying special polling frequencies for some <see cref="IPlcItem"/>s. </summary>
	private readonly PlcItemMonitorConfigurations _plcItemMonitorConfigurations;

	/// <summary> The default polling frequency of this monitor. If <see langword="null"/>, the static <see cref="PlcItemMonitorConfigurations.DefaultPollingFrequency"/> is used. </summary>
	private readonly TimeSpan? _defaultPollingFrequency;

	/// <summary> The handlers (one per polling frequency) that poll and update the monitored <see cref="IPlcItem"/>s. </summary>
	private readonly Dictionary<TimeSpan, PlcItemMonitorHandler> _plcItemHandlers;

	private readonly object _lock;

	/// <summary> Flag if the monitor is currently running. New handlers are only started if this is <see langword="true"/>. </summary>
	private bool _isRunning;
		
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
	public PollingPlcMonitor(IPlc plc) : this(plc, null) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
	/// <param name="plcItemMonitorConfigurations"> <see cref="PlcItemMonitorConfigurations"/> for specifying special polling frequencies for some <see cref="IPlcItem"/>s. </param>
	public PollingPlcMonitor(IPlc plc, PlcItemMonitorConfigurations plcItemMonitorConfigurations)
		: this(plc, plcItemMonitorConfigurations, (TimeSpan?) null) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
	/// <param name="defaultPollingFrequency"> The default polling frequency of this monitor that is used for all <see cref="IPlcItem"/>s that are not listed in the <see cref="PlcItemMonitorConfigurations"/>. Cannot be less than the minimum polling frequency. </param>
	public PollingPlcMonitor(IPlc plc, TimeSpan defaultPollingFrequency) : this(plc, null, defaultPollingFrequency) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
	/// <param name="plcItemMonitorConfigurations"> <see cref="PlcItemMonitorConfigurations"/> for specifying special polling frequencies for some <see cref="IPlcItem"/>s. </param>
	/// <param name="defaultPollingFrequency"> The default polling frequency of this monitor that is used for all <see cref="IPlcItem"/>s that are not listed in the <paramref name="plcItemMonitorConfigurations"/>. Cannot be less than the minimum polling frequency. </param>
	public PollingPlcMonitor(IPlc plc, PlcItemMonitorConfigurations plcItemMonitorConfigurations, TimeSpan defaultPollingFrequency)
		: this(plc, plcItemMonitorConfigurations, (TimeSpan?) defaultPollingFrequency) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
	/// <param name="plcItemMonitorConfigurations"> <see cref="PlcItemMonitorConfigurations"/> for specifying special polling frequencies for some <see cref="IPlcItem"/>s. </param>
	/// <param name="defaultPollingFrequency"> The default polling frequency of this monitor. If <see langword="null"/>, the static <see cref="PlcItemMonitorConfigurations.DefaultPollingFrequency"/> is used. </param>
	private PollingPlcMonitor(IPlc plc, PlcItemMonitorConfigurations plcItemMonitorConfigurations, TimeSpan? defaultPollingFrequency)
	{
		// Save parameters.
		_plc = plc;
		_plcItemMonitorConfigurations = plcItemMonitorConfigurations ?? [];
		_defaultPollingFrequency = defaultPollingFrequency;

		// Initialize fields.
		_lock = new object();
		_plcItemHandlers = [];
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Start()
	{
		lock (_lock)
		{
			_isRunning = true;
			var handlers = this.GetHandlers();
			foreach (var handler in handlers) handler.Start();
		}
	}
		
	/// <inheritdoc />
	public void Stop()
	{
		lock (_lock)
		{
			_isRunning = false;
			var handlers = this.GetHandlers();
			foreach (var handler in handlers) handler.Stop();
		}
	}

	/// <inheritdoc />
	public void MonitorItem(IPlcItem plcItem)
	{
		// All items with the same polling frequency are handled by the same handler.
		var pollingFrequency = this.GetPollingFrequency(plcItem);

		lock (_lock)
		{
			// Check if a handler for the polling frequency exists.
			if (!_plcItemHandlers.TryGetValue(pollingFrequency, out var handler))
			{
				// NO: Create and save the handler.
				handler = new PlcItemMonitorHandler(_plc, pollingFrequency);
				_plcItemHandlers.Add(pollingFrequency, handler);
			}

			handler.AddPlcItem(plcItem);

			// Only start the handler if the monitor is running.
			if (_isRunning) handler.Start();
		}
	}

	/// <inheritdoc />
	public void UnMonitorItem(IPlcItem plcItem)
	{
		lock (_lock)
		{
			// The polling frequency is determined the same way as when the item was added, so that the same handler is found.
			var pollingFrequency = this.GetPollingFrequency(plcItem);
			if (_plcItemHandlers.TryGetValue(pollingFrequency, out var handler))
			{
				handler.RemovePlcItem(plcItem);

				// A handler without any items must not keep polling the plc.
				if (!handler.HasPlcItems)
				{
					handler.Stop();
					_plcItemHandlers.Remove(pollingFrequency);
				}
			}
		}
	}

	/// <summary>
	/// Gets the polling frequency for the <paramref name="plcItem"/>.
	/// </summary>
	/// <param name="plcItem"> The <see cref="IPlcItem"/> whose polling frequency to get. </param>
	/// <returns> The configured polling frequency of the item or otherwise the default polling frequency of this monitor. </returns>
	private TimeSpan GetPollingFrequency(IPlcItem plcItem)
		=> _plcItemMonitorConfigurations.GetPollingFrequencyForPlcItem(plcItem, _defaultPollingFrequency ?? PlcItemMonitorConfigurations.DefaultPollingFrequency);

	/// <summary>
	/// Creates a copied collection of all currently available <see cref="PlcItemMonitorHandler"/>s.
	/// </summary>
	/// <returns> A collection of the active <see cref="PlcItemMonitorHandler"/>s. </returns>
	private ICollection<PlcItemMonitorHandler> GetHandlers()
	{
		lock (_lock)
		{
			return _plcItemHandlers.Values.ToArray();
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this.Stop();
	}

	#endregion
}

/// <summary>
/// Handles monitoring all <see cref="IPlcItem"/>s that share the same polling frequency.
/// </summary>
/// <remarks>
/// All distinct <see cref="IPlcItem"/>s are read with a single call per polling cycle. The read values are then transferred to all monitored instances.
/// </remarks>
internal sealed class PlcItemMonitorHandler
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	/// <summary> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </summary>
	private readonly IPlc _plc;

	/// <summary> The frequency at which to poll for changes. </summary>
	private readonly TimeSpan _pollingFrequency;

	private readonly object _lock;

	/// <summary> The monitored <see cref="IPlcItem"/>s grouped by the hash code of the items. </summary>
	private readonly Dictionary<int, MonitoredPlcItems> _monitoredPlcItems;

	private CancellationTokenSource _cancellationTokenSource;

	#endregion

	#region Properties

	/// <summary> Gets a value indicating whether this handler still monitors at least one <see cref="IPlcItem"/>. </summary>
	public bool HasPlcItems
	{
		get
		{
			lock (_lock)
			{
				return _monitoredPlcItems.Count > 0;
			}
		}
	}

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
	/// <param name="pollingFrequency"> The frequency at which to poll for changes. </param>
	public PlcItemMonitorHandler(IPlc plc, TimeSpan pollingFrequency)
	{
		// Save parameters.
		_plc = plc;
		_pollingFrequency = pollingFrequency;

		// Initialize fields.
		_lock = new object();
		_monitoredPlcItems = new Dictionary<int, MonitoredPlcItems>();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Starts polling all monitored plc items.
	/// </summary>
	public void Start()
	{
		CancellationToken token;
		lock (_lock)
		{
			// If the cancellation token source is available, then bail out.
			if (_cancellationTokenSource != null) return;

			// Create a cancellation token source if necessary.
			_cancellationTokenSource = new CancellationTokenSource();
			token = _cancellationTokenSource.Token;
		}

		Task.Run
		(
			async () =>
			{
				while (!token.IsCancellationRequested)
				{
					try
					{
						// Update the handler and therefore all the plc items.
						await this.UpdateItemsAsync(token);
					}
					catch (OperationCanceledException) when (token.IsCancellationRequested)
					{
						// Only cancellation ends the loop.
						break;
					}
					catch (Exception exception)
					{
						//! Any other exception must not end the polling, otherwise the items would never be polled again.
						this.LogUpdateFailure(exception);
					}

					try
					{
						// Wait for the next polling interval.
						await Task.Delay(_pollingFrequency, token);
					}
					catch (OperationCanceledException) { /* Ignore */ }
				}
			}, token
		);
	}

	/// <summary>
	/// Stops polling all monitored plc items.
	/// </summary>
	public void Stop()
	{
		lock (_lock)
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;
		}
	}

	/// <summary>
	/// Adds the <paramref name="plcItem"/> to the internal collection of monitored <see cref="IPlcItem"/>s.
	/// </summary>
	/// <param name="plcItem"> The additional plc item to monitor. </param>
	public void AddPlcItem(IPlcItem plcItem)
	{
		lock (_lock)
		{
			// Items that are equal to one another are read only once, so they share the same entry.
			var hash = plcItem.GetHashCode();
			if (!_monitoredPlcItems.TryGetValue(hash, out var monitoredPlcItems))
			{
				// Create a placeholder plc item that is used to read the value from the plc.
				monitoredPlcItems = new MonitoredPlcItems(plcItem.Clone($"PLACEHOLDER_{hash}"));
				_monitoredPlcItems.Add(hash, monitoredPlcItems);
			}

			// Don't allow the same item twice.
			//! The comparison must be done via referential equals, because the items themselves implement IEquatable and therefore may be equal, but not the same.
			foreach (var existingPlcItem in monitoredPlcItems.PlcItems)
			{
				if (Object.ReferenceEquals(existingPlcItem, plcItem)) return;
			}

			monitoredPlcItems.PlcItems.Add(plcItem);
		}
	}

	/// <summary>
	/// Removes the <paramref name="plcItem"/> from the internal collection of monitored <see cref="IPlcItem"/>s.
	/// </summary>
	/// <param name="plcItem"> The plc item to remove. </param>
	public void RemovePlcItem(IPlcItem plcItem)
	{
		lock (_lock)
		{
			var hash = plcItem.GetHashCode();
			if (!_monitoredPlcItems.TryGetValue(hash, out var monitoredPlcItems)) return;

			//! The removal must be done via referential equals, because the items themselves implement IEquatable and therefore may be equal, but not the same.
			var index = monitoredPlcItems.PlcItems.FindIndex(existingPlcItem => Object.ReferenceEquals(existingPlcItem, plcItem));
			if (index >= 0) monitoredPlcItems.PlcItems.RemoveAt(index);

			// Nothing needs to be read anymore if the last item has been removed.
			if (monitoredPlcItems.PlcItems.Count == 0) _monitoredPlcItems.Remove(hash);
		}
	}

	private async Task UpdateItemsAsync(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		// Make a copy of all monitored entries in a thread safe manner.
		MonitoredPlcItems[] entries;
		lock (_lock)
		{
			entries = _monitoredPlcItems.Values.ToArray();
		}
		if (entries.Length == 0) return;

		// Read all distinct items with a single call.
		var placeholderItems = entries.Select(entry => entry.PlaceholderItem).ToArray();
		try
		{
			await _plc.ReadItemsAsync(placeholderItems, token);
		}
		catch (ReadPlcException exception)
		{
			//! Failed items must not prevent the other items from being updated, so only the items that have been read successfully are processed further.
			this.LogUpdateFailure(exception);
			var validItems = new HashSet<IPlcItem>(exception.ValidItems, new ReferenceComparer());
			entries = entries.Where(entry => validItems.Contains(entry.PlaceholderItem)).ToArray();
		}

		// Collect all monitored items with their new value in a thread safe manner.
		token.ThrowIfCancellationRequested();
		var updates = new List<(IPlcItem PlcItem, BitCollection NewValue)>();
		lock (_lock)
		{
			foreach (var entry in entries)
			{
				foreach (var plcItem in entry.PlcItems)
				{
					updates.Add((plcItem, entry.PlaceholderItem.Value));
				}
			}
		}

		// Update the value of the monitored items.
		Parallel.ForEach
		(
			updates, (update) =>
			{
				if (token.IsCancellationRequested) return;

				//! Subscribers of the 'ValueChanged' event may throw. This must neither affect the other items nor the polling itself.
				try
				{
					update.PlcItem.Value.TransferValuesFrom(update.NewValue);
				}
				catch (Exception exception)
				{
					this.LogUpdateFailure(exception, update.PlcItem);
				}
			}
		);
	}

	#endregion

	#region Logging

	private void LogUpdateFailure(Exception exception, IPlcItem plcItem = null)
	{
		var message = plcItem is null
			? "Updating the monitored plc items failed. Polling will continue."
			: $"Updating the monitored plc item '{plcItem.Identifier}' failed. Polling will continue.";
		LogManager.GetLogger().Error(exception, message);
	}

	#endregion

	#region Nested Types

	/// <summary>
	/// All monitored <see cref="IPlcItem"/>s that are equal to one another and therefore share a single placeholder item that is used for reading.
	/// </summary>
	private sealed class MonitoredPlcItems
	{
		/// <summary> The item that is read from the plc. </summary>
		public IPlcItem PlaceholderItem { get; }

		/// <summary> The monitored items that receive the value of the <see cref="PlaceholderItem"/>. </summary>
		public List<IPlcItem> PlcItems { get; } = new List<IPlcItem>();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="placeholderItem"> <see cref="PlaceholderItem"/> </param>
		public MonitoredPlcItems(IPlcItem placeholderItem)
		{
			this.PlaceholderItem = placeholderItem;
		}
	}

	/// <summary>
	/// Compares <see cref="IPlcItem"/>s by reference, because the items themselves implement IEquatable.
	/// </summary>
	private sealed class ReferenceComparer : IEqualityComparer<IPlcItem>
	{
		/// <inheritdoc />
		public bool Equals(IPlcItem x, IPlcItem y) => Object.ReferenceEquals(x, y);

		/// <inheritdoc />
		public int GetHashCode(IPlcItem obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}

	#endregion
}