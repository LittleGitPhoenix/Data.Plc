using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.Mock;
using Phoenix.Data.Plc.Test;

namespace Phoenix.Data.Plc.Monitor.Polling.Test
{
	[TestClass]
	public class PollingPlcMonitorTest
	{
		public Data.Plc.Test.Data Data { get; }

		public PollingPlcMonitorTest()
		{
			this.Data = new Data.Plc.Test.Data();
		}

		[TestMethod]
		public async Task MonitorChanges()
		{
			var changes = 100;
			var target = (changes * (changes + 1)) / 2; // Gauﬂsche Summenformel
			var monitoredChanges = 0;
			var collectedValue = 0;

			var monitorItemIdentifier = "MonitoredItem";
			uint monitorItemInterval = 50;

			var monitoredPlc = new MockPlc().MakeMonitorable
			(
				new Dictionary<string, uint>
				{
					{monitorItemIdentifier, monitorItemInterval}
				}
			);
			
			// Create the item that must be monitored.
			var monitorItem = new BytePlcItem(dataBlock: Data.Datablock, position: Data.StartOfModifiableBytes, identifier: monitorItemIdentifier);

			// Create the item that is used to manipulate the value.
			IPlcItem writeItem = monitorItem.Clone("ChangeItem");

			// Set a callback for changes to the items.
			monitorItem.ValueChanged += (sender, args) =>
			{
				monitoredChanges++;
				collectedValue += args.NewValue;
			};

			try
			{
				// Connect to the plc.
				monitoredPlc.Connect();
				
				// Start monitoring those items.
				monitoredPlc.MonitorItem(monitorItem);
				monitoredPlc.Start();

				// Manipulate the monitored value.
				for (byte i = 1; i <= changes; i++)
				{
					writeItem.Value.TransferValuesFrom(new[] { i });
					await monitoredPlc.WriteItemAsync(writeItem);
					await Task.Delay((int)monitorItemInterval * 2); // This must be at least the double amount of the polling interval.
				}

				// Stop monitoring.
				monitoredPlc.Stop();

				// Further manipulate the value to check if this is not monitored.
				writeItem.Value.TransferValuesFrom(new[] { byte.MinValue });
				writeItem.Value.TransferValuesFrom(new[] { byte.MaxValue });

				// Check if all changes where registered.
				Assert.AreEqual(changes, monitoredChanges);
				Assert.AreEqual(target, collectedValue);
				Assert.AreEqual(changes, monitorItem.Value);
			}
			finally
			{
				monitoredPlc.Dispose();
			}
		}

		[TestMethod]
		public async Task MonitorDifferentIntervals()
		{
			var monitoredChangesOfFirstItem = 0;
			var monitoredChangesOfSecondItem = 0;

			var firstMonitorItemIdentifier = "Monitored item #01";
			uint firstMonitorItemInterval = 50;
			var secondMonitorItemIdentifier = "Monitored item #02";
			uint secondMonitorItemInterval = 200;

			var changes = 100;
			var secondChanges = changes / ((secondMonitorItemInterval / firstMonitorItemInterval) / 2);

			var monitoredPlc = new MockPlc().MakeMonitorable
			(
				new Dictionary<string, uint>
				{
					{firstMonitorItemIdentifier, firstMonitorItemInterval}
					,{secondMonitorItemIdentifier, secondMonitorItemInterval}
				}
			);

			// Create items that must be monitored.
			var firstMonitoredItem = new BytePlcItem(dataBlock: Data.Datablock, position: Data.StartOfModifiableBytes, identifier: firstMonitorItemIdentifier);
			var secondMonitoredItem = firstMonitoredItem.Clone(secondMonitorItemIdentifier);

			// Create the item that is used to manipulate the value.
			IPlcItem writeItem = firstMonitoredItem.Clone("ChangeItem");

			// Set a callback for changes to the items.
			firstMonitoredItem.ValueChanged += (sender, args) =>
			{
				monitoredChangesOfFirstItem++;
			};
			secondMonitoredItem.ValueChanged += (sender, args) =>
			{
				monitoredChangesOfSecondItem++;
			};

			try
			{
				// Connect to the plc.
				monitoredPlc.Connect();

				// Start monitoring those items.
				monitoredPlc.MonitorItem(firstMonitoredItem);
				monitoredPlc.MonitorItem(secondMonitoredItem);
				monitoredPlc.Start();

				// Manipulate the monitored value.
				for (byte i = 1; i <= changes; i++)
				{
					writeItem.Value.TransferValuesFrom(new[] { i });
					await monitoredPlc.WriteItemAsync(writeItem);
					await Task.Delay((int)firstMonitorItemInterval * 2); // This must be at least the double amount of the polling interval.
				}

				// Stop monitoring.
				monitoredPlc.Stop();

				// Check if all changes where registered.
				Assert.AreEqual(changes, monitoredChangesOfFirstItem);
				Assert.IsTrue(monitoredChangesOfSecondItem >= secondChanges);
			}
			finally
			{
				monitoredPlc.Dispose();
			}
		}

		[TestMethod]
		public async Task MonitorDifferentIdenticalItems()
		{
			var firstMonitoredItem = new BytesPlcItem(dataBlock: Data.Datablock, position: Data.StartOfFixedBytes, byteAmount: 2);
			var secondMonitoredItem = new UInt16PlcItem(dataBlock: Data.Datablock, position: Data.StartOfFixedBytes);
			var thirdMonitoredItem = new BitsPlcItem(dataBlock: Data.Datablock, position: Data.StartOfFixedBytes, bitPosition: BitPosition.X0, bitAmount: 16);

			var differentReadItems = new HashSet<IPlcItem>(new ReferenceComparer());
			var mockPlc = new MockPlc();
			Func<ICollection<IPlcItem>, CancellationToken, Task> performReadItemsAsyncMock = (plcItems, cancellationToken) =>
			{
				foreach (var plcItem in plcItems)
				{
					differentReadItems.Add(plcItem);
				}
				return mockPlc.ReadItemsAsync(plcItems, cancellationToken);
			};

			// Create a mock of MockPlc that signals which items are read.
			var plcMock = new Mock<IPlc>(behavior: MockBehavior.Strict)
			{
				CallBase = true,
			};
			plcMock
				.Setup(x => x.ReadItemsAsync(It.IsAny<IPlcItem[]>(), It.IsAny<CancellationToken>()))
				.Returns(performReadItemsAsyncMock)
				.Verifiable()
				;

			// Create the monitor with the mocked instance.
			var monitoredPlc = new PollingMonitorablePlc(plcMock.Object);

			// Start monitoring those items.
			monitoredPlc.MonitorItem(firstMonitoredItem);
			monitoredPlc.MonitorItem(secondMonitoredItem);
			monitoredPlc.MonitorItem(thirdMonitoredItem);
			monitoredPlc.Start();
			await Task.Delay(1000);
			monitoredPlc.Stop();
			Assert.AreEqual(1, differentReadItems.Count);
		}

		private class ReferenceComparer : IEqualityComparer<IPlcItem>
		{
			#region Implementation of IEqualityComparer<in object>

			/// <inheritdoc />
			public bool Equals(IPlcItem x, IPlcItem y)
			{
				return Object.ReferenceEquals(x, y);
			}

			/// <inheritdoc />
			public int GetHashCode(IPlcItem obj)
			{
				return obj.GetHashCode();
			}

			#endregion
		}
	}
}