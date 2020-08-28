#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Connection data needed by AGLink for establishing a connection to a plc.
	/// </summary>
	public sealed class AgLinkPlcConnectionData : IAgLinkPlcConnectionData
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <inheritdoc />
		public int Id { get; }

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public byte DeviceNumber { get; }

		/// <inheritdoc />
		public string Ip { get; }

		/// <inheritdoc />
		public byte Rack { get; }

		/// <inheritdoc />
		public byte Slot { get; }

		/// <inheritdoc />
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
		public AgLinkPlcConnectionData(byte deviceNumber, string ip, byte rack, byte slot, TimeSpan connectionTimeout = default)
			: this(-1, null, deviceNumber, ip, rack, slot, connectionTimeout) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id"> The id of the plc. </param>
		/// <param name="name"> The name of the plc instance. If this is <c>Null</c> or whitespace then the default value <c>AGLinkPlc@{ip}</c> will be used. </param>
		/// <param name="deviceNumber"> <see cref="DeviceNumber"/> </param>
		/// <param name="ip"> <see cref="Ip"/> </param>
		/// <param name="rack"> <see cref="Rack"/> </param>
		/// <param name="slot"> <see cref="Slot"/> </param>
		/// <param name="connectionTimeout"> Optional timeout for establishing the connection. Default value is 5000 milliseconds. </param>
		/// <returns> <c>TRUE</c> if successful, otherwise <c>FALSE</c>. </returns>
		public AgLinkPlcConnectionData(int id, string name, byte deviceNumber, string ip, byte rack, byte slot, TimeSpan connectionTimeout = default)
		{
			// Save parameters.
			this.Id = id;
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
		public override string ToString() => $"[<{this.GetType().Name}> :: Id: {this.Id} | Name: {this.Name} | Device: {this.DeviceNumber} | IP: {this.Ip} | Rack: {this.Rack} | Slot: {this.Slot} | ConnectionTimeOut: {this.ConnectionTimeout}]";

		#endregion
	}
}