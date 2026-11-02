#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// Interface for connection data needed by siAccess.
/// </summary>
public interface ISiAccessPlcConnectionData : IPlcInformation
{
	/// <summary> The device number. </summary>
	byte DeviceNumber { get; }

	/// <summary> The ip address of the plc. </summary>
	string Ip { get; }

	/// <summary> The rack number of the plc. </summary>
	byte Rack { get; }

	/// <summary> The slot of the plc. </summary>
	byte Slot { get; }

	/// <summary> Timeout used both for establishing the connection and for siAccess communication functions (i.e. read/write calls). </summary>
	/// <remarks>
	/// siAccess does not always reliably enforce this for communication calls if the underlying TCP connection dies silently.
	/// See <see cref="SiAccessPlc.PerformReadWriteAsync"/> for an additional application-level guard against this.
	/// </remarks>
	TimeSpan ConnectionTimeout { get; }
}