#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor
{
	/// <summary>
	/// Interface for an <see cref="IPlc"/> that can be used to monitor <see cref="IPlcItem"/>s.
	/// </summary>
	public interface IMonitorablePlc : IPlc, IPlcMonitor { }
}