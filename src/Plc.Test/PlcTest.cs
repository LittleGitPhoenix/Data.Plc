using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc.Test
{
	[TestClass]
	public class PlcTest
	{
		public Data Data { get; }

		public PlcTest()
		{
			this.Data = new Data();
		}

		///// <summary>
		///// Tests the internal reconnection for handling read / write of <see cref="IPlcItem"/>s.
		///// </summary>
		//[TestMethod]
		//public async Task Reconnect()
		//{
		//	// Build a handler method that throws an recoverable error for a defined amount.
		//	byte iteration = 0;
		//	byte threshold = 5;
		//	Func<bool> disconnectCallback = () =>
		//	{
		//		//if (iteration++ < threshold) throw new PlcException(PlcExceptionType.ReadError, $"Iterations below {threshold} are recoverable errors.");
		//		return true;
		//	};

		//	// Create mock connection data.
		//	var connectionDataMock = new Mock<IPlcConnectionData>();
		//	connectionDataMock.SetupGet(data => data.Name).Returns("Test");
		//	connectionDataMock.SetupGet(data => data.ConnectionTimeout).Returns(5000);
		//	connectionDataMock.SetupGet(data => data.ReconnectionTimeout).Returns(1000);
		//	var connectionData = connectionDataMock.Object;

		//	// Create a mock plc instance that will automatically reconnect upon a recoverable error.
		//	var plcMock = new Mock<Plc<IPlcConnectionData>>(behavior: MockBehavior.Strict, args: new object[] { connectionData })
		//	{
		//		CallBase = true,
		//	};
		//	plcMock.SetupProperty(p => p.IsConnected, initialValue: true);
		//	plcMock.Setup(p => p.Connect()).Callback(disconnectCallback);
		//	plcMock.Setup(p => p.Disconnect()).Callback(disconnectCallback);
		//	var plc = plcMock.Object;

		//	// Get the method to test.
		//	var plcType = plc.GetType();
		//	var methodInfo = plcType.GetMethod("Reconnect", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
		//	Assert.IsNotNull(methodInfo);

		//	// Await method execution and compare the iteration.
		//	await (Task)methodInfo.Invoke(obj: plc, parameters: null);
		//	//Assert.IsTrue(iteration == threshold + 1);
		//}

		///// <summary>
		///// Tests the internal reconnection for handling read / write of <see cref="IPlcItem"/>s.
		///// </summary>
		//[TestMethod]
		//public async Task Retry_RecoverableError()
		//{
		//	// Build a handler method that throws an recoverable error for a defined amount.
		//	byte iteration = 0;
		//	byte threshold = 5;
		//	Action callback = () =>
		//	{
		//		if (iteration++ < threshold) throw new PlcException(PlcExceptionType.ReadError, $"Iterations below {threshold} are recoverable errors.");
		//	};

		//	//// Create mock connection data.
		//	//var connectionDataMock = new Mock<IPlcConnectionData>();
		//	//connectionDataMock.SetupGet(data => data.Name).Returns("Test");
		//	//connectionDataMock.SetupGet(data => data.ConnectionTimeout).Returns(5000);
		//	//connectionDataMock.SetupGet(data => data.ReconnectionTimeout).Returns(1000);
		//	//var connectionData = connectionDataMock.Object;

		//	// Create a mock plc instance that will automatically reconnect upon a recoverable error.
		//	var plcMock = new Mock<Plc>(behavior: MockBehavior.Strict, args: new object[] { "MockPlc" });
		//	var plc = plcMock.Object;

		//	// Get the method to test.
		//	var plcType = plc.GetType();
		//	var methodInfo = plcType.GetMethod("ExecuteReadWrite", BindingFlags.NonPublic | BindingFlags.Instance);
		//	Assert.IsNotNull(methodInfo);

		//	// Await method execution and compare the iteration.
		//	await (Task)methodInfo.Invoke(obj: plc, parameters: new object[] { callback });
		//	Assert.IsTrue(iteration == threshold + 1);
		//}

		///// <summary>
		///// Tests the internal reconnection for handling read / write of <see cref="IPlcItem"/>s.
		///// </summary>
		//[TestMethod]
		//public async Task Retry_UnrecoverableError()
		//{
		//	// Build a handler method that throws an unrecoverable error.
		//	void PlcExceptionCallback() => throw new PlcException(PlcExceptionType.UnrecoverableConnection, $"This is an unrecoverable error.");

		//	// Build a handler method that throws some other error.
		//	void OtherExceptionCallback() => throw new ApplicationException($"This is an unrecoverable error.");

		//	//// Create mock connection data.
		//	//var connectionDataMock = new Mock<IPlcConnectionData>();
		//	//connectionDataMock.SetupGet(data => data.Name).Returns("Test");
		//	//connectionDataMock.SetupGet(data => data.ConnectionTimeout).Returns(5000);
		//	//connectionDataMock.SetupGet(data => data.ReconnectionTimeout).Returns(1000);
		//	//var connectionData = connectionDataMock.Object;

		//	// Create a mock plc instance that will automatically reconnect upon a recoverable error.
		//	var plcMock = new Mock<Plc>(behavior: MockBehavior.Strict, args: new object[] { "MockPlc" });
		//	var plc = plcMock.Object;

		//	// Get the method to test.
		//	var plcType = plc.GetType();
		//	var methodInfo = plcType.GetMethod("ExecuteReadWrite", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance);
		//	Assert.IsNotNull(methodInfo);

		//	// Await method execution and check if the expected error was raised.
		//	await Assert.ThrowsExceptionAsync<PlcException>(async () =>
		//	{
		//		await (Task) methodInfo.Invoke(obj: plc, parameters: new object[] {(Action) PlcExceptionCallback});
		//	});
		//	await Assert.ThrowsExceptionAsync<ApplicationException>(async () =>
		//	{
		//		await (Task)methodInfo.Invoke(obj: plc, parameters: new object[] { (Action) OtherExceptionCallback });
		//	});
		//}

		[TestMethod]
		public void Open_Connection_Raises_Connected_Event()
		{
			// Create a mock of the abstract PLC base class and setup the needed abstract methods.
			var plcMock = new Mock<Plc>(behavior: MockBehavior.Strict, args: new object[] { "MockPlc" })
			{
				CallBase = true,
			};
			plcMock
				.Protected()
				.Setup<bool>("OpenConnection")
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
			Assert.IsTrue(success);
			Assert.IsTrue(disconnectedCounter == 0);
			Assert.IsTrue(connectedCounter == 1);
			Assert.IsTrue(interruptedCounter == 0);

			success = plc.Connect();
			Assert.IsTrue(success);
			Assert.IsTrue(disconnectedCounter == 0);
			Assert.IsTrue(connectedCounter == 1); // Should still be just 1 because the state didn't change and change notification should not be raised.
			Assert.IsTrue(interruptedCounter == 0);
		}

		[TestMethod]
		public void Close_Connection_Raises_Disconnected_Event()
		{
			// Create a mock of the abstract PLC base class and setup the needed abstract methods.
			var plcMock = new Mock<Plc>(behavior: MockBehavior.Strict, args: new object[] { "MockPlc" })
			{
				CallBase = true,
			};
			plcMock
				.Protected()
				.Setup<bool>("OpenConnection")
				.Returns(true)
				.Verifiable()
				;
			plcMock
				.Protected()
				.Setup<bool>("CloseConnection")
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
			Assert.IsTrue(success);
			Assert.IsTrue(disconnectedCounter == 0); // Should still be 0 because the state didn't change and change notification should not be raised.
			Assert.IsTrue(connectedCounter == 0);
			Assert.IsTrue(interruptedCounter == 0);

			// Connect to later check disconnection.
			success = plc.Connect();
			Assert.IsTrue(success);
			Assert.IsTrue(disconnectedCounter == 0);
			Assert.IsTrue(connectedCounter == 1);
			Assert.IsTrue(interruptedCounter == 0);

			// Finally disconnect and check the counter.
			success = plc.Disconnect();
			Assert.IsTrue(success);
			Assert.IsTrue(disconnectedCounter == 1);
			Assert.IsTrue(connectedCounter == 1);
			Assert.IsTrue(interruptedCounter == 0);
		}

		[TestMethod]
		public async Task Read_Without_Connection()
		{
			var byteItem = new BytePlcItem(dataBlock: Data.Datablock, position: 0, initialValue: Data.TargetBytes[0]);

			// Get the protected and therefore not accessible PlcItemUsageType enumeration.
			var enumType = typeof(Plc).GetNestedType("PlcItemUsageType", BindingFlags.NonPublic);
			var plcUsageType = Enum.ToObject(enumType, 0 /* Read */);

			// Create a mock of the abstract PLC base class and setup the needed abstract methods.
			var plcMock = new Mock<Plc>(behavior: MockBehavior.Strict, args: new object[] { "MockPlc" })
			{
				CallBase = true,
			};
			plcMock
				.Protected()
				.Setup<bool>("OpenConnection")
				.Returns(true)
				.Verifiable()
				;
			plcMock
				.Protected()
				.Setup<bool>("CloseConnection")
				.Returns(true)
				.Verifiable()
				;			
			plcMock
				.Protected()
				.Setup<Task>("PerformReadWriteAsync", ItExpr.IsAny<ICollection<IPlcItem>>(), plcUsageType, CancellationToken.None)
				.Returns(Task.CompletedTask)
				.Verifiable()
				;
			
			var plc = plcMock.Object;

			// Disconnect before reading.
			var success = plc.Disconnect();
			Assert.IsTrue(success);

			// Execute the read function.
			var readTask = plc.ReadItemAsync(byteItem);

			// Create a new task that automatically ends after some time.
			var waitTask = Task.Delay(3000);

			// Wait until one of the tasks finishes.
			await Task.WhenAny(new[] { readTask, waitTask });

			// The read task should still be running.
			Assert.IsFalse(readTask.Status == TaskStatus.RanToCompletion);

			// Connect to the plc.
			success = plc.Connect();
			Assert.IsTrue(success);

			// Now await the read task and check the result.
			var result = await readTask;
			Assert.IsTrue(byteItem.Value == result);
			Assert.IsTrue(byteItem.Value == Data.TargetBytes[0]);			
		}

		[TestMethod]
		public async Task Read_With_Automatic_Retry()
		{
			// Build a handler method that throws an recoverable error for a defined amount.
			byte iteration = 0;
			byte threshold = 5;
			
			// ReSharper disable once ConvertToLocalFunction
			Func<object, object, object, Task> performReadWriteAsyncMock = (plcItems, usageType, cancellationToken) =>
			{
				if (iteration++ < threshold)
				{
					throw new PlcException(PlcExceptionType.ReadError, $"Iterations below {threshold} are recoverable errors.");
				}

				return Task.FromResult(true);
			};
			
			var byteItem = new BytePlcItem(dataBlock: Data.Datablock, position: 0, initialValue: Data.TargetBytes[0]);

			// Get the protected and therefore not accessible PlcItemUsageType enumeration.
			var enumType = typeof(Plc).GetNestedType("PlcItemUsageType", BindingFlags.NonPublic);
			var plcUsageType = Enum.ToObject(enumType, 0 /* Read */);

			// Create a mock of the abstract PLC base class and setup the needed abstract methods.
			var plcMock = new Mock<Plc>(behavior: MockBehavior.Strict, args: new object[] { "MockPlc" })
			{
				CallBase = true,
			};
			plcMock
				.Protected()
				.Setup<bool>("OpenConnection")
				.Returns(true)
				.Verifiable()
				;
			plcMock
				.Protected()
				.Setup<bool>("CloseConnection")
				.Returns(true)
				.Verifiable()
				;			
			plcMock
				.Protected()
				.Setup<Task>("PerformReadWriteAsync", ItExpr.IsAny<ICollection<IPlcItem>>(), plcUsageType, CancellationToken.None)
				.Returns(performReadWriteAsyncMock)
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
			Assert.IsTrue(success);
			
			// Execute the read function.
			var result = await plc.ReadItemAsync(byteItem); ;
			Assert.IsTrue(iteration == threshold + 1);
			Assert.IsTrue(interruptedCounter == threshold);
			Assert.IsTrue(byteItem.Value == result);
			Assert.IsTrue(byteItem.Value == Data.TargetBytes[0]);			
		}

		class TestLogger : ILogger
		{
			public string LastMessage { get; private set; }

			#region Implementation of ILogger

			/// <inheritdoc />
			public void Trace(string message, params object[] replacors)
			{
				this.LastMessage = String.Format(message, replacors);
			}

			/// <inheritdoc />
			public void Info(string message, params object[] replacors)
			{
				this.LastMessage = String.Format(message, replacors);
			}

			/// <inheritdoc />
			public void Error(Exception exception)
			{
				this.LastMessage = $"{exception.Message}";
			}

			/// <inheritdoc />
			public void Error(Exception exception, string message, params object[] replacors)
			{
				this.LastMessage = $"{exception.Message} - {String.Format(message, replacors)}";
			}

			/// <inheritdoc />
			public void Error(string message, params object[] replacors)
			{
				this.LastMessage = String.Format(message, replacors);
			}

			#endregion
		}

		/// <summary>
		/// Checks if read operations are logged if <see cref="LogManager.LogAllReadAndWriteOperations"/> is enabled.
		/// </summary>
		[TestMethod]
		public async Task Check_Logging_When_Reading()
		{
			// Arrange
			var logger = Mock.Of<ILogger>();
			LogManager.LoggerFactory = () => logger;
			LogManager.LogAllReadAndWriteOperations = true;
			var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: Byte.MaxValue);
			ICollection<IPlcItem> items = new IPlcItem[] { byteItem , byteItem .Clone(), byteItem.Clone() };
			var plcMock = new Mock<Plc>(Guid.NewGuid().ToString());
			plcMock
				.Setup(p => p.ReadItemsAsync(It.IsAny<IList<IPlcItem>>(), CancellationToken.None))
				.Returns(Task.CompletedTask)
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
		[TestMethod]
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