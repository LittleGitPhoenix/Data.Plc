#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// Defines different connection states for the plc.
	/// </summary>
	public enum PlcConnectionState : byte
	{
		/// <summary> The plc is disconnected. </summary>
		Disconnected = 0,
		/// <summary> The plc is connected. </summary>
		Connected = 1,
		/// <summary> The connection to the plc has been interrupted. </summary>
		Interrupted = 2,
	}

	/// <summary>
	/// Special event arguments for changes in the connection state of a plc.
	/// </summary>
	public class PlcConnectionStateChangedEventArgs : EventArgs
	{
		/// <summary> The new <see cref="PlcConnectionState"/>. </summary>
		public PlcConnectionState ConnectionState { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="connectionState"> The new <see cref="PlcConnectionState"/>. </param>
		public PlcConnectionStateChangedEventArgs(PlcConnectionState connectionState)
		{
			this.ConnectionState = connectionState;
		}
	}
}