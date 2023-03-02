using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc.Test;

[TestFixture]
public class PlcTest
{
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
	/// Checks if opening a connection fails, the <see cref="IPlc.Disconnect"/> method is implicitly invoked.
	/// </summary>
	[Test]
	public void Check_Failure_On_Connect_Calls_Disconnect()
	{
		// Arrange
		var plcMock = new Mock<Plc>("MockPlc") { CallBase = true };
		plcMock
			.Setup(mock => mock.OpenConnection())
			.Returns(false)
			.Verifiable()
			;
		plcMock
			.Setup(mock => mock.CloseConnection())
			.Returns(true)
			.Verifiable()
			;
		var plc = plcMock.Object;

		// Act
		var success = plc.Connect();

		// Assert
		Assert.False(success);
		plcMock.Verify(mock => mock.OpenConnection(), Times.Once);
		plcMock.Verify(mock => mock.CloseConnection(), Times.Once);
	}

	/// <summary>
	/// Checks if reading an <see cref="IPlcItem"/> is paused as long the connection to the plc hasn't been established and will automatically be continued if the connection is available.
	/// </summary>
	[Test]
	public async Task Check_If_Reading_Is_Paused_If_Not_Connected()
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
			.Returns(Task.CompletedTask)
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

	/// <summary>
	/// Checks if reading an <see cref="IPlcItem"/> stops, if the plc instance is disposed.
	/// </summary>
	[Test]
	public async Task Check_If_Reading_Is_Canceled_If_Plc_Is_Disposed()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;
			
		// Disconnect before reading and then wait a little bit.
		plc.Disconnect();
		var readTask = plc.ReadItemsAsync(new IPlcItem[] {byteItem});
		await Task.Delay(1000);
			
		// Act
		plc.Dispose();

		// Assert
		Assert.ThrowsAsync<DisposedReadPlcException>(() => readTask);
		Assert.That(readTask.Status, Is.EqualTo(TaskStatus.Faulted));
	}

	/// <summary>
	/// Checks if reading an <see cref="IPlcItem"/> from a disposed <see cref="IPlc"/> throws a <see cref="ReadOrWritePlcException"/>.
	/// </summary>
	[Test]
	public void Check_If_Reading_From_A_Disposed_Plc_Throws()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;
			
		// Act
		plc.Dispose();
			
		// Assert
		Assert.ThrowsAsync<DisposedReadPlcException>(() => plc.ReadItemsAsync(new IPlcItem[] {byteItem}));
	}

	/// <summary>
	/// Checks if disposing a <see cref="IPlc"/> while some <see cref="IPlcItem"/> have been put on hold throws a <see cref="ReadOrWritePlcException"/>.
	/// </summary>
	[Test]
	public void Check_If_Disposing_A_Plc_With_Waiting_Items_For_Reading_Throws()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;
		var readTask = plc.ReadItemsAsync(new IPlcItem[] {byteItem});
			
		// Act
		Task.Run
		(
			async () =>
			{
				await Task.Delay(1000);
				plc.Dispose();
			}
		);

		// Assert
		Assert.ThrowsAsync<DisposedReadPlcException>(() => readTask);
	}

	/// <summary>
	/// Checks if writing an <see cref="IPlcItem"/> is paused as long the connection to the plc hasn't been established and will automatically be continued if the connection is available.
	/// </summary>
	[Test]
	public async Task Check_If_Writing_Is_Paused_If_Not_Connected()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;

		// Disconnect before reading.
		var success = plc.Disconnect();
		Assert.True(success);

		// Execute the write function.
		var writeTask = plc.WriteItemAsync(byteItem);

		// Create a new task that automatically ends after some time.
		var waitTask = Task.Delay(3000);

		// Wait until one of the tasks finishes.
		await Task.WhenAny(new[] { writeTask, waitTask });

		// The write task should still be running.
		Assert.False(writeTask.Status == TaskStatus.RanToCompletion);

		// Connect to the plc.
		success = plc.Connect();
		Assert.True(success);

		// Now await the write task and check the result.
		var result = await writeTask;
		Assert.True(result);
		Assert.True(byteItem.Value == byte.MaxValue);
	}

	/// <summary>
	/// Checks if writing an <see cref="IPlcItem"/> is canceled if the plc instance is disposed.
	/// </summary>
	[Test]
	public async Task Check_If_Writing_Is_Canceled_If_Plc_Is_Disposed()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;
			
		// Disconnect before writing and then wait a little bit.
		plc.Disconnect();
		var writeTask = plc.WriteItemsAsync(new IPlcItem[] {byteItem});
		await Task.Delay(1000);
			
		// Act
		plc.Dispose();

		// Assert
		Assert.ThrowsAsync<DisposedWritePlcException>(() => writeTask);
		Assert.That(writeTask.Status, Is.EqualTo(TaskStatus.Faulted));
	}

	/// <summary>
	/// Checks if writing an <see cref="IPlcItem"/> to a disposed <see cref="IPlc"/> throws a <see cref="ReadOrWritePlcException"/>.
	/// </summary>
	[Test]
	public void Check_If_Writing_To_A_Disposed_Plc_Throws()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc)plcMock.Object;
		plc.Dispose();

		// Act
		plc.Dispose();
			
		// Assert
		Assert.ThrowsAsync<DisposedWritePlcException>(() => plc.WriteItemsAsync(new IPlcItem[] { byteItem }));
	}

	/// <summary>
	/// Checks if disposing a <see cref="IPlc"/> while some <see cref="IPlcItem"/> have been put on hold throws a <see cref="ReadOrWritePlcException"/>.
	/// </summary>
	[Test]
	public void Check_If_Disposing_A_Plc_With_Waiting_Items_For_Writing_Throws()
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc)plcMock.Object;
		var writeTask = plc.WriteItemsAsync(new IPlcItem[] { byteItem });

		// Act
		Task.Run
		(
			async () =>
			{
				await Task.Delay(1000);
				plc.Dispose();
			}
		);

		// Assert
		Assert.ThrowsAsync<DisposedWritePlcException>(() => writeTask);
	}

	[Test]
	public void Check_Merged_Token_Is_Canceled_On_External_Token()
	{
		var externalTokenSource = new CancellationTokenSource(500);
		var externalToken = externalTokenSource.Token;
		var internalTokenSource = new CancellationTokenSource();
		var internalToken = internalTokenSource.Token;

		var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalToken).Token;

		Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(Timeout.InfiniteTimeSpan, mergedToken));
	}

	[Test]
	public void Check_Merged_Token_Is_Canceled_On_Internal_Token()
	{
		var externalTokenSource = new CancellationTokenSource();
		var externalToken = externalTokenSource.Token;
		var internalTokenSource = new CancellationTokenSource(500);
		var internalToken = internalTokenSource.Token;

		var mergedToken = CancellationTokenSource.CreateLinkedTokenSource(externalToken, internalToken).Token;

		Assert.ThrowsAsync<TaskCanceledException>(() => Task.Delay(Timeout.InfiniteTimeSpan, mergedToken));
	}

#if DEBUG
	/// <summary>
	/// Checks that the <see cref="CancellationTokenSource"/> that is created while reading/writing is properly disposed.
	/// </summary>
	[Test]
	public async Task Check_That_The_Internal_CancellationTokenSource_Is_Disposed_After_Reading_Or_Writing()
	{
		// Arrange
		using var externalTokenSource = new CancellationTokenSource();
		var externalToken = externalTokenSource.Token;
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
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		plcMock
			.Setup(p => p.LinkedTokenWasCanceled())
			.Verifiable()
			;
		using var plc = plcMock.Object;
		plc.Connect();

		// Act
		await plc.ReadItemAsync(byteItem, CancellationToken.None);

		// Assert
		plcMock.Verify(p => p.LinkedTokenWasCanceled(), Times.Once);
		Assert.False(externalTokenSource.IsCancellationRequested);
		Assert.False(externalToken.IsCancellationRequested);
	}
#endif

	/// <summary>
	/// Checks that the <see cref="CancellationTokenSource"/> that is created while reading/writing is properly disposed.
	/// </summary>
	[Test]
	public void Check_Memory_Usage()
	{
		Assert.Ignore("The mocked plc instance needed to perform this test, seems to have massive memory leaks itself. So this test has been moved to the real implementations.");
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