#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor.Polling
{
	/// <summary>
	/// Collection of <see cref="PlcItemMonitorConfiguration"/>s.
	/// </summary>
	public class PlcItemMonitorConfigurations : System.Collections.ObjectModel.KeyedCollection<string, PlcItemMonitorConfiguration>
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		/// <summary> Lock for synchronizing access to <see cref="DefaultPollingFrequency"/> </summary>
		private static readonly object Lock;

		/// <summary> The minimum polling frequency in milliseconds that can be used. </summary>
		internal static readonly TimeSpan MinimumPollingFrequency;

		#endregion

		#region Properties

		/// <summary> The default polling frequency that is returned by <see cref="GetPollingFrequencyForPlcItem"/> if the <see cref="IPlcItem"/> is not listed. </summary>
		/// <remarks> Cannot be less than <see cref="MinimumPollingFrequency"/>. </remarks>
		public static TimeSpan DefaultPollingFrequency
		{
			get
			{
				lock (Lock)
				{
					return _defaultPollingFrequency;
				}
			}
			set
			{
				lock (Lock)
				{
					value = PlcItemMonitorConfigurations.ValidatePollingFrequency(value);
					_defaultPollingFrequency = value;
				}
			}
		}
		private static TimeSpan _defaultPollingFrequency;

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		static PlcItemMonitorConfigurations()
		{
			Lock = new object();
			MinimumPollingFrequency = TimeSpan.FromMilliseconds(50);
			DefaultPollingFrequency = TimeSpan.FromMilliseconds(300);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public PlcItemMonitorConfigurations() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="configurations"> Collection of <see cref="PlcItemMonitorConfiguration"/>s. </param>
		public PlcItemMonitorConfigurations(IEnumerable<PlcItemMonitorConfiguration> configurations)
		{
			foreach (var configuration in configurations)
			{
				base.Add(configuration);
			}
		}
		#endregion

		#region Methods

		/// <summary>
		/// Gets the polling frequency for the <paramref name="plcItem"/>.
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> whose polling frequency to get. </param>
		/// <returns> The polling frequency. </returns>
		public TimeSpan GetPollingFrequencyForPlcItem(IPlcItem plcItem)
		{
			// Use the defined value for the item if available.
			if (plcItem.Identifier != null && base.Contains(plcItem.Identifier)) return PlcItemMonitorConfigurations.ValidatePollingFrequency(TimeSpan.FromMilliseconds(base[plcItem.Identifier].PollingFrequency));

			// Otherwise the default polling frequency is used.
			return PlcItemMonitorConfigurations.DefaultPollingFrequency;
		}

		/// <summary>
		/// Checks if the <paramref name="pollingFrequency"/> does not undershot the <see cref="MinimumPollingFrequency"/>.
		/// </summary>
		/// <param name="pollingFrequency"> The polling frequency to validate. </param>
		/// <returns> A valid polling frequency. </returns>
		internal static TimeSpan ValidatePollingFrequency(TimeSpan pollingFrequency)
		{
			return pollingFrequency > MinimumPollingFrequency ? pollingFrequency : MinimumPollingFrequency;
		}

		/// <summary>
		/// Implicit cast operator so that a normal <see cref="Dictionary{TKey,TValue}"/> can be used to construct a <see cref="PlcItemMonitorConfigurations"/> object.
		/// </summary>
		/// <param name="configurations"></param>
		public static implicit operator PlcItemMonitorConfigurations(Dictionary<string, uint> configurations)
		{
			return new PlcItemMonitorConfigurations(configurations.Select(pair => new PlcItemMonitorConfiguration(pair.Key, pair.Value)));
		}

		#region KeyedCollection

		/// <inheritdoc />
		protected override string GetKeyForItem(PlcItemMonitorConfiguration item)
		{
			return item.PlcItemIdentifier;
		}

		#endregion

		#endregion
	}
}