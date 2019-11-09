using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Monitor;

namespace Phoenix.Data.Plc.Monitor.Polling
{
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

		private readonly Dictionary<int, PlcItemMonitorHandler> _plcItemHandlers;

		private readonly object _lock;
		
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
		{
			// Save parameters.
			_plc = plc;
			_plcItemMonitorConfigurations = plcItemMonitorConfigurations ?? new PlcItemMonitorConfigurations();

			// Initialize fields.
			_lock = new object();
			_plcItemHandlers = new Dictionary<int, PlcItemMonitorHandler>();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Start()
		{
			lock (_lock)
			{
				var handlers = this.GetHandlers();
				foreach (var handler in handlers) handler.Start();
			}
		}
		
		/// <inheritdoc />
		public void Stop()
		{
			lock (_lock)
			{
				var handlers = this.GetHandlers();
				foreach (var handler in handlers) handler.Stop();
			}
		}

		/// <inheritdoc />
		public void MonitorItem(IPlcItem plcItem)
		{
			// The hash code must consist of the hash from the plc item and the polling frequency.
			var plcItemHash = plcItem.GetHashCode();
			var pollingFrequency = _plcItemMonitorConfigurations.GetPollingFrequencyForPlcItem(plcItem);
			var hashValue = new {plcItemHash, pollingFrequency}.GetHashCode();

			lock (_lock)
			{
				// Check if such a handler exist.
				if (_plcItemHandlers.TryGetValue(hashValue, out var handler))
				{
					// YES: Just add the item.
					handler.AddPlcItem(plcItem);
				}
				else
				{
					// NO: Create and save the handler.
					handler = new PlcItemMonitorHandler(_plc, plcItem, pollingFrequency);
					_plcItemHandlers.Add(hashValue, handler);
				}
			}
		}

		/// <inheritdoc />
		public void UnMonitorItem(IPlcItem plcItem)
		{
			lock (_lock)
			{
				var hashValue = plcItem.GetHashCode();
				if (_plcItemHandlers.TryGetValue(hashValue, out var handler))
				{
					handler.RemovePlcItem(plcItem);
				}
			}
		}

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
	/// Handles monitoring all <see cref="IPlcItem"/>s that are equal to one another.
	/// </summary>
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

		private readonly IPlcItem _placeholderItem;

		private readonly List<IPlcItem> _plcItems;

		private CancellationTokenSource _cancellationTokenSource;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plc"> <see cref="IPlc"/> instance used to poll for changes in the monitored <see cref="IPlcItem"/>s. </param>
		/// <param name="plcItem"> The first <see cref="IPlcItem"/> to monitor. </param>
		/// <param name="pollingFrequency"> The frequency at which to poll for changes. </param>
		public PlcItemMonitorHandler(IPlc plc, IPlcItem plcItem, TimeSpan pollingFrequency)
		{
			// Save parameters.
			_plc = plc;
			_pollingFrequency = pollingFrequency;

			// Initialize fields.
			_lock = new object();
			//_cancellationLock = new object();

			// Create a placeholder plc item that is used to update the value.
			_placeholderItem = plcItem.Clone($"PLACEHOLDER_{plcItem.GetHashCode()}");

			// Add the passed plc item as first one to the internal collection.
			_plcItems = new List<IPlcItem>() { plcItem };

			// Start monitoring.
			this.Start();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts monitoring the internal placeholder plc item.
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

							// Wait for the next polling interval.
							await Task.Delay(_pollingFrequency, token);
						}
						catch (TaskCanceledException) { /* Ignore */ }
					}
				}, token
			);
		}

		/// <summary>
		/// Stops monitoring the internal placeholder plc item.
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
				// Don't allow the same item twice.
				//! The comparison must be done via referential equals, because the items themselves implement IEquatable and therefore may be equal, but not the same.
				foreach (var existingPlcItem in _plcItems)
				{
					if (Object.ReferenceEquals(existingPlcItem, plcItem)) return;
				}

				_plcItems.Add(plcItem);
			}
		}

		public void RemovePlcItem(IPlcItem plcItem)
		{
			lock (_lock)
			{
				_plcItems.Remove(plcItem);
			}
		}

		private async Task UpdateItemsAsync(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var newValue = await _plc.ReadItemAsync(_placeholderItem, token);

			// Make a copy of all monitored plc items in a thread safe manner.
			token.ThrowIfCancellationRequested();
			List<IPlcItem> plcItems;
			lock (_lock)
			{
				plcItems = new List<IPlcItem>(_plcItems);
			}

			// Update the value of the monitored items.
			//foreach (var plcItem in plcItems)
			//{
			//	token.ThrowIfCancellationRequested();
			//	plcItem.Value.TransferValuesFrom(newValue);
			//}
			Parallel.ForEach
			(
				plcItems, (plcItem) =>
				{
					token.ThrowIfCancellationRequested();
					plcItem.Value.TransferValuesFrom(newValue);
				}
			);
		}

		#endregion
	}
}