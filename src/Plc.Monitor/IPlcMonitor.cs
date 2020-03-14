#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor
{
	/// <summary>
	/// Interface for all plc monitor classes.
	/// </summary>
	public interface IPlcMonitor : IDisposable
	{
		/// <summary>
		/// Starts monitoring all available <see cref="IPlcItem"/>s.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops monitoring all available <see cref="IPlcItem"/>s.
		/// </summary>
		void Stop();

		/// <summary>
		/// Starts monitoring the <paramref name="plcItem"/>.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to monitor. </param>
		void MonitorItem(IPlcItem plcItem);

		/// <summary>
		/// Stops monitoring the <paramref name="plcItem"/>
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to stop monitoring. </param>
		void UnMonitorItem(IPlcItem plcItem);
	}
}