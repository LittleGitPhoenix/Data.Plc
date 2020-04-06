using System;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Monitor.Polling.Test
{
	[TestFixture]
	public class PlcItemMonitorConfigurationsTest
	{
		/// <summary>
		/// Checks if the <see cref="PlcItemMonitorConfigurations.DefaultPollingFrequency"/> is used for items that don't provide a custom <see cref="PlcItemMonitorConfiguration"/>.
		/// </summary>
		[Test]
		public void Check_Default_Frequency()
		{
			//Arrange
			var item = Moq.Mock.Of<IPlcItem>();
			Moq.Mock.Get(item).SetupGet(plcItem => plcItem.Identifier).Returns(Guid.NewGuid().ToString);
			var configurations = new PlcItemMonitorConfigurations();

			// Act
			var frequency = configurations.GetPollingFrequencyForPlcItem(item);

			// Assert
			Assert.AreEqual(PlcItemMonitorConfigurations.DefaultPollingFrequency, frequency);
		}

		/// <summary>
		/// Checks if a custom polling frequency is returned if a custom <see cref="PlcItemMonitorConfiguration"/> has been provided for an <see cref="IPlcItem"/>.
		/// </summary>
		[Test]
		public void Check_Custom_Frequency()
		{
			//Arrange
			var itemIdentifier = Guid.NewGuid().ToString();
			var itemFrequency = TimeSpan.FromMilliseconds(100);
			var item = Moq.Mock.Of<IPlcItem>();
			Moq.Mock.Get(item).SetupGet(plcItem => plcItem.Identifier).Returns(itemIdentifier);
			var configurations = new PlcItemMonitorConfigurations(new[] {new PlcItemMonitorConfiguration(itemIdentifier, (uint) itemFrequency.TotalMilliseconds),});

			// Act
			var frequency = configurations.GetPollingFrequencyForPlcItem(item);

			// Assert
			Assert.AreEqual(itemFrequency, frequency);
		}

		/// <summary>
		/// Checks if the <see cref="PlcItemMonitorConfigurations.MinimumPollingFrequency"/> cannot be undershot.
		/// </summary>
		[Test]
		public void Polling_Frequency_Cannot_Be_Undershot()
		{
			// Act
			var frequency = PlcItemMonitorConfigurations.ValidatePollingFrequency(TimeSpan.FromMilliseconds(10));

			// Assert
			Assert.AreEqual(PlcItemMonitorConfigurations.MinimumPollingFrequency, frequency);
		}
	}
}