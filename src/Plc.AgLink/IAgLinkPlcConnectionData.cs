#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Interface for connection data needed by AGLink.
	/// </summary>
	public interface IAgLinkPlcConnectionData : IPlcInformation
	{
		/// <summary> The device number. </summary>
		byte DeviceNumber { get; }

		/// <summary> The ip address of the plc. </summary>
		string Ip { get; }

		/// <summary> The rack number of the plc. </summary>
		byte Rack { get; }

		/// <summary> The slot of the plc. </summary>
		byte Slot { get; }

		/// <summary> Timeout for establishing the connection. </summary>
		TimeSpan ConnectionTimeout { get; }
	}
}