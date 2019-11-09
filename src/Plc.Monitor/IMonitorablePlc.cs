using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor
{
	/// <summary>
	/// Interface for an <see cref="IPlc"/> that can be used to monitor <see cref="IPlcItem"/>s.
	/// </summary>
	public interface IMonitorablePlc : IPlc, IPlcMonitor { }
}