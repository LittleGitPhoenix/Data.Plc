#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor.Polling
{
	/// <summary>
	/// Wrapper for an <see cref="IPlc"/> that uses the <see cref="PollingPlcMonitor"/> to monitor <see cref="IPlcItem"/>s.
	/// </summary>
	public sealed class PollingMonitorablePlc : IMonitorablePlc
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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plc"> The plc object that is used to forward all calls to the <see cref="IPlc"/> interface to. </param>
		public PollingMonitorablePlc(IPlc plc) : this(plc, null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plc"> The plc object that is used to forward all calls to the <see cref="IPlc"/> interface to. </param>
		/// <param name="plcItemMonitorConfigurations"> <see cref="PlcItemMonitorConfigurations"/> for specifying special polling frequencies for some <see cref="IPlcItem"/>s. </param>
		public PollingMonitorablePlc(IPlc plc, PlcItemMonitorConfigurations plcItemMonitorConfigurations)
		{
			// Save parameters.
			_decoratedPlc = plc;
			
			// Initialize fields.
			_plcMonitor = new PollingPlcMonitor(plc, plcItemMonitorConfigurations);
		}

		#endregion
		
		#region Forwarding - IPlc

		/// <summary> The decorated <see cref="IPlc"/> instance. </summary>
		private readonly IPlc _decoratedPlc;
		
		#region Event Forwarding

		/// <inheritdoc />
		public event EventHandler<PlcConnectionState> Connected
		{
			add => _decoratedPlc.Connected += value;
			remove => _decoratedPlc.Connected -= value;
		}

		/// <inheritdoc />
		public event EventHandler<PlcConnectionState> Disconnected
		{
			add => _decoratedPlc.Disconnected += value;
			remove => _decoratedPlc.Disconnected -= value;
		}

		/// <inheritdoc />
		public event EventHandler<PlcConnectionState> Interrupted
		{
			add => _decoratedPlc.Interrupted += value;
			remove => _decoratedPlc.Interrupted -= value;
		}

		#endregion

		#region Property Forwarding

		/// <inheritdoc />
		public int Id => _decoratedPlc.Id;
		
		/// <inheritdoc />
		public string Name => _decoratedPlc.Name;

		/// <inheritdoc />
		public PlcConnectionState ConnectionState => _decoratedPlc.ConnectionState;

		#endregion

		#region Method Forwarding

		#region Connection

		/// <inheritdoc />
		public bool Connect()
		{
			return _decoratedPlc.Connect();
		}

		/// <inheritdoc />
		public bool Disconnect()
			=> _decoratedPlc.Disconnect();

		/// <inheritdoc />
		public bool Reconnect()
			=> _decoratedPlc.Reconnect();

		#endregion

		#region Read
		
		/// <inheritdoc />
		public async Task ReadItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
			=> await _decoratedPlc.ReadItemsAsync(plcItems, cancellationToken);

		#endregion

		#region Write
		
		/// <inheritdoc />
		public async Task<bool> WriteItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
			=> await _decoratedPlc.WriteItemsAsync(plcItems, cancellationToken);
		
		#endregion

		#region IDisposable

		/// <inheritdoc />
		public void Dispose()
			=> _decoratedPlc.Dispose();

		#endregion

		#endregion

		#endregion

		#region Forwarding - IPlcMonitor

		private readonly PollingPlcMonitor _plcMonitor;
		
		/// <inheritdoc />
		public void Start()
			=> _plcMonitor.Start();

		/// <inheritdoc />
		public void Stop()
			=> _plcMonitor.Stop();

		/// <inheritdoc />
		public void MonitorItem(IPlcItem plcItem)
			=> _plcMonitor.MonitorItem(plcItem);

		/// <inheritdoc />
		public void UnMonitorItem(IPlcItem plcItem)
			=> _plcMonitor.UnMonitorItem(plcItem);

		#endregion
	}
}