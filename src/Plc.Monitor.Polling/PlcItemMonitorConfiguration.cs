#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor.Polling
{
	/// <summary>
	/// A single configuration specifying a different <see cref="PollingFrequency"/> for a monitored <see cref="IPlcItem"/>.
	/// </summary>
	public class PlcItemMonitorConfiguration : IEquatable<PlcItemMonitorConfiguration>
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		/// <summary>
		/// The <see cref="IPlcItem.Identifier"/> used for matching the configuration with the monitored items.
		/// </summary>
		public string PlcItemIdentifier { get; }

		/// <summary>
		/// The polling frequency.
		/// </summary>
		public uint PollingFrequency { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plcItemIdentifier"> <see cref="PlcItemIdentifier"/> </param>
		/// <param name="pollingFrequency"> <see cref="PollingFrequency"/> </param>
		public PlcItemMonitorConfiguration(string plcItemIdentifier, uint pollingFrequency)
		{
			this.PlcItemIdentifier = plcItemIdentifier;
			this.PollingFrequency = pollingFrequency;
		}

		#endregion

		#region Methods

		#region IEquatable

		/// <summary> The hash code of an instance of this class. </summary>
		private int HashCode
		{
			get
			{
				_hashCode ??= this.PlcItemIdentifier.GetHashCode();
				return _hashCode.Value;
			}
		}
		private int? _hashCode;

		/// <summary> The default hash method. </summary>
		/// <returns> A hash value for the current object. </returns>
		public override int GetHashCode()
		{
			return this.HashCode;
		}

		/// <summary>
		/// Compares this instance to another one.
		/// </summary>
		/// <param name="other"> The other instance to compare to. </param>
		/// <returns> <c>TRUE</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>FALSE</c>. </returns>
		public override bool Equals(object other)
		{
			if (other is PlcItemMonitorConfiguration equatable) return this.Equals(equatable);
			return false;
		}

		/// <summary>
		/// Compares this instance to another one.
		/// </summary>
		/// <param name="other"> The other instance to compare to. </param>
		/// <returns> <c>TRUE</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>FALSE</c>. </returns>
		public bool Equals(PlcItemMonitorConfiguration other)
		{
			return other?.GetHashCode() == this.GetHashCode();
		}

		/// <summary>
		/// Checks if the two <see cref="PlcItemMonitorConfiguration"/>s are equal.
		/// </summary>
		/// <param name="x"> The first <see cref="PlcItemMonitorConfiguration"/> to compare. </param>
		/// <param name="y"> The second <see cref="PlcItemMonitorConfiguration"/> to compare. </param>
		/// <returns> <c>TRUE</c> if both instances are equal, otherwise <c>FALSE</c>. </returns>
		public static bool operator ==(PlcItemMonitorConfiguration x, PlcItemMonitorConfiguration y)
		{
			return x?.GetHashCode() == y?.GetHashCode();
		}

		/// <summary>
		/// Checks if the two <see cref="PlcItemMonitorConfiguration"/>s are NOT equal.
		/// </summary>
		/// <param name="x"> The first <see cref="PlcItemMonitorConfiguration"/> to compare. </param>
		/// <param name="y"> The second <see cref="PlcItemMonitorConfiguration"/> to compare. </param>
		/// <returns> <c>TRUE</c> if both instances are NOT equal, otherwise <c>FALSE</c>. </returns>
		public static bool operator !=(PlcItemMonitorConfiguration x, PlcItemMonitorConfiguration y)
		{
			return !(x == y);
		}

		#endregion

		#endregion
	}
}