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

		/// <summary> The default polling frequency that will be used if not other default value is supplied. </summary>
		private const uint DefaultPollingFrequency = 300;

		/// <summary> The default polling frequency that is returned by <see cref="GetPollingFrequencyForPlcItem"/> if the <see cref="IPlcItem"/> is not listed. </summary>
		private TimeSpan? _defaultPollingFrequency;

		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

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
			if (!base.Contains(plcItem.Identifier))
			{
				// Try to get a default polling frequency or use the constant one.
				if (_defaultPollingFrequency is null)
				{
					if (base.Contains("__DEFAULT__"))
					{
						_defaultPollingFrequency = TimeSpan.FromMilliseconds(base["__DEFAULT__"].PollingFrequency);
					}
					else
					{
						_defaultPollingFrequency = TimeSpan.FromMilliseconds(PlcItemMonitorConfigurations.DefaultPollingFrequency);
					}
				}

				return _defaultPollingFrequency.Value;
			}
			else
			{
				return TimeSpan.FromMilliseconds(base[plcItem.Identifier].PollingFrequency);
			}
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