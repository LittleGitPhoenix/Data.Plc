using System;
using System.IO;

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

		/// <summary> Timeout for establishing the connection. </summary>
		public TimeSpan ConnectionTimeout { get; }
				
		/// <summary> The ip address of the plc. </summary>
		public string Ip { get; }

		/// <summary> The rack number of the plc. </summary>
		public int Rack { get; }

		/// <summary> The slot of the plc. </summary>
		public int Slot { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"> The name of the plc. </param>
		/// <param name="ip"> The ip address of the plc. </param>
		/// <param name="rack"> The rack number of the plc. </param>
		/// <param name="slot"> The slot of the plc. </param>
		/// <param name="connectionTimeout"> Timeout for establishing the connection. Default value is <c>5000</c> milliseconds. </param>
		/// <returns> <c>TRUE</c> if successful, otherwise <c>FALSE</c>. </returns>
		public AgLinkPlcConnectionData(string name, string ip, int rack, int slot, int connectionTimeout = 5000)
			: this(name, ip, rack, slot, TimeSpan.FromMilliseconds(connectionTimeout))
		{ }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"> The name of the plc. </param>
		/// <param name="ip"> The ip address of the plc. </param>
		/// <param name="rack"> The rack number of the plc. </param>
		/// <param name="slot"> The slot of the plc. </param>
		/// <param name="connectionTimeout"> <see cref="TimeSpan"/> for the timeout for establishing the connection. Default value is <c>3000</c> milliseconds. </param>
		/// <returns> <c>TRUE</c> if successful, otherwise <c>FALSE</c>. </returns>
		public AgLinkPlcConnectionData(string name, string ip, int rack, int slot, TimeSpan connectionTimeout)
		{
			// Save parameters.
			this.Name = name;
			this.Ip = ip;
			this.Rack = rack;
			this.Slot = slot;
			this.ConnectionTimeout = connectionTimeout;
		}

		#endregion

		#region Methods
		#endregion
	}
}