﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Data.Plc.Monitor.Polling
{
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
	}
}