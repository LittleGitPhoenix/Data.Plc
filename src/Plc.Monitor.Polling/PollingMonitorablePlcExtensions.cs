#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Data.Plc.Monitor.Polling;

/// <summary>
/// Provides extension methods for <see cref="IPlc"/>.
/// </summary>
public static class PollingMonitorablePlcExtensions
{
	/// <summary>
	/// Creates a <see cref="IMonitorablePlc"/> utilizing the <paramref name="plc"/>.
	/// </summary>
	/// <param name="plc"> The <see cref="IPlc"/> object that is used for monitoring. </param>
	/// <returns> An <see cref="IMonitorablePlc"/> instance. </returns>
	public static IMonitorablePlc MakeMonitorable(this IPlc plc)
	{
		return new PollingMonitorablePlc(plc);
	}

	/// <summary>
	/// Creates a <see cref="IMonitorablePlc"/> utilizing the <paramref name="plc"/>.
	/// </summary>
	/// <param name="plc"> The <see cref="IPlc"/> object that is used for monitoring. </param>
	/// <param name="plcItemMonitorConfigurations"> <see cref="PlcItemMonitorConfigurations"/> for specifying special polling frequencies for some <see cref="Phoenix.Data.Plc.Items.IPlcItem"/>s. </param>
	/// <returns> An <see cref="IMonitorablePlc"/> instance. </returns>
	public static IMonitorablePlc MakeMonitorable(this IPlc plc, PlcItemMonitorConfigurations plcItemMonitorConfigurations)
	{
		return new PollingMonitorablePlc(plc, plcItemMonitorConfigurations);
	}

	/// <summary>
	/// Creates a <see cref="IMonitorablePlc"/> utilizing the <paramref name="plc"/>.
	/// </summary>
	/// <param name="plc"> The <see cref="IPlc"/> object that is used for monitoring. </param>
	/// <param name="defaultPollingFrequency"> The default polling frequency of the monitor that is used for all <see cref="Phoenix.Data.Plc.Items.IPlcItem"/>s that are not listed in the <see cref="PlcItemMonitorConfigurations"/>. </param>
	/// <returns> An <see cref="IMonitorablePlc"/> instance. </returns>
	public static IMonitorablePlc MakeMonitorable(this IPlc plc, TimeSpan defaultPollingFrequency)
	{
		return new PollingMonitorablePlc(plc, defaultPollingFrequency);
	}

	/// <summary>
	/// Creates a <see cref="IMonitorablePlc"/> utilizing the <paramref name="plc"/>.
	/// </summary>
	/// <param name="plc"> The <see cref="IPlc"/> object that is used for monitoring. </param>
	/// <param name="plcItemMonitorConfigurations"> <see cref="PlcItemMonitorConfigurations"/> for specifying special polling frequencies for some <see cref="Phoenix.Data.Plc.Items.IPlcItem"/>s. </param>
	/// <param name="defaultPollingFrequency"> The default polling frequency of the monitor that is used for all <see cref="Phoenix.Data.Plc.Items.IPlcItem"/>s that are not listed in the <paramref name="plcItemMonitorConfigurations"/>. </param>
	/// <returns> An <see cref="IMonitorablePlc"/> instance. </returns>
	public static IMonitorablePlc MakeMonitorable(this IPlc plc, PlcItemMonitorConfigurations plcItemMonitorConfigurations, TimeSpan defaultPollingFrequency)
	{
		return new PollingMonitorablePlc(plc, plcItemMonitorConfigurations, defaultPollingFrequency);
	}
}