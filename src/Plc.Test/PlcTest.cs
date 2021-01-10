using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc.Test
{
	[TestFixture]
	public class PlcTest
	{
#if NET45
		private static Task CompletedTask = Task.FromResult(false);
#endif
		
		[Test]
		public void Open_Connection_Raises_Connected_Event()
		{
			// Create a mock of the abstract PLC base class and setup the needed abstract methods.
			var plcMock = new Mock<Plc>("MockPlc") { CallBase = true };
			plcMock
				.Setup(p => p.OpenConnection())
				.Returns(true)
				.Verifiable()
				;
			var plc = plcMock.Object;

			var connectedCounter = 0;
			var disconnectedCounter = 0;
			var interruptedCounter = 0;
			plc.Connected += (sender, state) => connectedCounter++;
			plc.Disconnected += (sender, state) => disconnectedCounter++;
			plc.Interrupted += (sender, state) => interruptedCounter++;

			// Connect to the plc.
			var success = plc.Connect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);

			success = plc.Connect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0);
			Assert.True(connectedCounter == 1); // Should still be just 1 because the state didn't change and change notification should not be raised.
			Assert.True(interruptedCounter == 0);
		}

		[Test]
		public void Close_Connection_Raises_Disconnected_Event()
		{
			// Create a mock of the abstract PLC base class and setup the needed abstract methods.
			var plcMock = new Mock<Plc>("MockPlc") { CallBase = true };
			plcMock
				.Setup(p => p.OpenConnection())
				.Returns(true)
				.Verifiable()
				;
			plcMock
				.Setup(p => p.CloseConnection())
				.Returns(true)
				.Verifiable()
				;
			var plc = plcMock.Object;

			var connectedCounter = 0;
			var disconnectedCounter = 0;
			var interruptedCounter = 0;
			plc.Connected += (sender, state) => connectedCounter++;
			plc.Disconnected += (sender, state) => disconnectedCounter++;
			plc.Interrupted += (sender, state) => interruptedCounter++;

			// Disconnect to the plc.
			var success = plc.Disconnect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0); // Should still be 0 because the state didn't change and change notification should not be raised.
			Assert.True(connectedCounter == 0);
			Assert.True(interruptedCounter == 0);

			// Connect to later check disconnection.
			success = plc.Connect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);

			// Finally disconnect and check the counter.
			success = plc.Disconnect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 1);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);
		}

		/// <summary>
		/// Checks if reading an <see cref="IPlcItem"/> is paused as long the connection to the plc hasn't been established and will automatically be continued if the connection is available.
		/// </summary>
		[Test]
		public async Task Read_Without_Connection()
		{
			// Arrange
			var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: byte.MaxValue);
			var plcMock = new Mock<Plc>("MockPlc") { CallBase = true };
			plcMock
				.Setup(p => p.OpenConnection())
				.Returns(true)
				.Verifiable()
				;
			plcMock
				.Setup(p => p.CloseConnection())
				.Returns(true)
				.Verifiable()
				;
			plcMock
				.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), CancellationToken.None))
#if NET45
				.Returns(CompletedTask)
#else
				.Returns(Task.CompletedTask)
#endif
				.Verifiable()
				;
			var plc = plcMock.Object;

			// Disconnect before reading.
			var success = plc.Disconnect();
			Assert.True(success);

			// Execute the read function.
			var readTask = plc.ReadItemAsync(byteItem);

			// Create a new task that automatically ends after some time.
			var waitTask = Task.Delay(3000);

			// Wait until one of the tasks finishes.
			await Task.WhenAny(new[] { readTask, waitTask });

			// The read task should still be running.
			Assert.False(readTask.Status == TaskStatus.RanToCompletion);

			// Connect to the plc.
			success = plc.Connect();
			Assert.True(success);

			// Now await the read task and check the result.
			var result = await readTask;
			Assert.True(byteItem.Value == result);
			Assert.True(byteItem.Value == byte.MaxValue);
		}

		//! This test cannot work anymore, since each error during reading or writing is automatically unrecoverable.
		///// <summary>
		///// Checks if reading an <see cref="IPlcItem"/> is automatically retried if the read operation throws an <see cref="PlcException"/> as long as it is not <see cref="PlcExceptionType.UnrecoverableConnection"/>.
		///// </summary>
		//[Test]
		//public async Task Read_With_Automatic_Retry()
		//{
		//	// Arrange
		//	byte iteration = 0;
		//	byte threshold = 5;
		//	var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: byte.MaxValue);
		//	var plcMock = new Mock<Plc>("MockPlc") { CallBase = true };
		//	plcMock
		//		.Setup(p => p.OpenConnection())
		//		.Returns(true)
		//		.Verifiable()
		//		;
		//	plcMock
		//		.Setup(p => p.CloseConnection())
		//		.Returns(true)
		//		.Verifiable()
		//		;
		//	plcMock
		//		.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), CancellationToken.None))
		//		.Returns
		//		(
		//			() =>
		//			{
		//				// Throw recoverable exceptions until the threshold is reached, so that automatic retry is triggered.
		//				if (iteration++ < threshold)
		//				{
		//					throw new PlcException(PlcExceptionType.ReadError, $"Iterations below {threshold} are recoverable errors.");
		//				}

		//				return Task.FromResult(true);
		//			}
		//		)
		//		.Verifiable()
		//		;
		//	var plc = plcMock.Object;

		//	var connectedCounter = 0;
		//	var disconnectedCounter = 0;
		//	var interruptedCounter = 0;
		//	plc.Connected += (sender, state) => connectedCounter++;
		//	plc.Disconnected += (sender, state) => disconnectedCounter++;
		//	plc.Interrupted += (sender, state) => interruptedCounter++;

		//	// Connect to the plc.
		//	var success = plc.Connect();
		//	Assert.True(success);

		//	// Execute the read function.
		//	var result = await plc.ReadItemAsync(byteItem);
		//	Assert.True(iteration == threshold + 1);
		//	Assert.True(interruptedCounter == threshold);
		//	Assert.True(byteItem.Value == result);
		//	Assert.True(byteItem.Value == byte.MaxValue);
		//}
		
		/// <summary>
		/// Checks if read operations are logged if <see cref="LogManager.LogAllReadAndWriteOperations"/> is enabled.
		/// </summary>
		[Test]
		public async Task Check_Logging_When_Reading()
		{
			// Arrange
			var logger = Mock.Of<ILogger>();
			LogManager.LoggerFactory = () => logger;
			LogManager.LogAllReadAndWriteOperations = true;
			var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: Byte.MaxValue);
			ICollection<IPlcItem> items = new IPlcItem[] { byteItem, byteItem.Clone(), byteItem.Clone() };
			var plcMock = new Mock<Plc>(Guid.NewGuid().ToString());
			plcMock
				.Setup(p => p.ReadItemsAsync(It.IsAny<IList<IPlcItem>>(), CancellationToken.None))
#if NET45
				.Returns(CompletedTask)
#else
				.Returns(Task.CompletedTask)
#endif
				;
			var plc = plcMock.Object;

			// Act
			await plc.ReadItemsAsync(items);

			// Assert
			Mock.Get(logger).Verify(l => l.Trace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(items.Count));
		}

		/// <summary>
		/// Checks if write operations are logged if <see cref="LogManager.LogAllReadAndWriteOperations"/> is enabled.
		/// </summary>
		[Test]
		public async Task Check_Logging_When_Writing()
		{
			// Arrange
			var logger = Mock.Of<ILogger>();
			LogManager.LoggerFactory = () => logger;
			LogManager.LogAllReadAndWriteOperations = true;
			var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: Byte.MaxValue);
			ICollection<IPlcItem> items = new IPlcItem[] { byteItem, byteItem.Clone(), byteItem.Clone() };
			var plcMock = new Mock<Plc>(Guid.NewGuid().ToString());
			plcMock
				.Setup(p => p.WriteItemsAsync(It.IsAny<IList<IPlcItem>>(), CancellationToken.None))
				.Returns(Task.FromResult(true))
				;
			var plc = plcMock.Object;

			// Act
			await plc.WriteItemsAsync(items);

			// Assert
			Mock.Get(logger).Verify(l => l.Trace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(items.Count));
		}
	}
}