#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Connection data needed by AGLink for establishing a connection to a plc.
	/// </summary>
	public sealed class AgLinkPlcConnectionData
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary> The name of the plc. </summary>
		public string Name { get; }

		/// <summary> The device number. </summary>
		public int DeviceNumber { get; }

		/// <summary> The ip address of the plc. </summary>
		public string Ip { get; }
		
		/// <summary> The rack number of the plc. </summary>
		public byte Rack { get; }

		/// <summary> The slot of the plc. </summary>
		public byte Slot { get; }

		/// <summary> Timeout for establishing the connection. </summary>
		public TimeSpan ConnectionTimeout { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="deviceNumber"> <see cref="DeviceNumber"/> </param>
		/// <param name="ip"> <see cref="Ip"/> </param>
		/// <param name="rack"> <see cref="Rack"/> </param>
		/// <param name="slot"> <see cref="Slot"/> </param>
		/// <param name="connectionTimeout"> Optional timeout for establishing the connection. Default value is 5000 milliseconds. </param>
		/// <returns> <c>TRUE</c> if successful, otherwise <c>FALSE</c>. </returns>
		public AgLinkPlcConnectionData(int deviceNumber, string ip, byte rack, byte slot, TimeSpan connectionTimeout = default)
			: this(null, deviceNumber, ip, rack, slot, connectionTimeout) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"> The name of the plc instance. If this is <c>Null</c> or whitespace then the default value will be used. </param>
		/// <param name="deviceNumber"> <see cref="DeviceNumber"/> </param>
		/// <param name="ip"> <see cref="Ip"/> </param>
		/// <param name="rack"> <see cref="Rack"/> </param>
		/// <param name="slot"> <see cref="Slot"/> </param>
		/// <param name="connectionTimeout"> Optional timeout for establishing the connection. Default value is 5000 milliseconds. </param>
		/// <returns> <c>TRUE</c> if successful, otherwise <c>FALSE</c>. </returns>
		public AgLinkPlcConnectionData(string name, int deviceNumber, string ip, byte rack, byte slot, TimeSpan connectionTimeout = default)
		{
			// Save parameters.
			this.Name = String.IsNullOrWhiteSpace(name) ? $"AGLinkPlc@{ip}" : name;
			this.DeviceNumber = deviceNumber;
			this.Ip = ip;
			this.Rack = rack;
			this.Slot = slot;
			this.ConnectionTimeout = connectionTimeout == default ? TimeSpan.FromMilliseconds(5000) : connectionTimeout;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: Name: {this.Name} | Device: {this.DeviceNumber} | IP: {this.Ip} | Rack: {this.Rack} | Slot: {this.Slot} | ConnectionTimeOut: {this.ConnectionTimeout}]";

		#endregion
	}
}