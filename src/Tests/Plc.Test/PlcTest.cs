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
		var waitTask = Task.Delay(1000);

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
			.Setup(mock => mock.OpenConnection())
			.Returns(true)
			.Verifiable()
			;
		plcMock
			.Setup(mock => mock.CloseConnection())
			.Returns(true)
			.Verifiable()
			;
		plcMock
			.Setup(mock => mock.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), CancellationToken.None))
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;
			
		// Disconnect before reading and then wait a little bit.
		plc.Disconnect();
		var readTask = plc.ReadItemsAsync(new IPlcItem[] {byteItem});
		await Task.Delay(500);
			
		// Act
		plc.Dispose();

		// Assert
		Exception exception = null;
		try
		{
			await readTask;
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		Assert.That(exception, Is.TypeOf<DisposedReadPlcException>());
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
				await Task.Delay(500);
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
		var waitTask = Task.Delay(1000);

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
			.Setup(mock => mock.OpenConnection())
			.Returns(true)
			.Verifiable()
			;
		plcMock
			.Setup(mock => mock.CloseConnection())
			.Returns(true)
			.Verifiable()
			;
		plcMock
			.Setup(mock => mock.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), CancellationToken.None))
			.Returns(Task.CompletedTask)
			.Verifiable()
			;
		var plc = (IPlc) plcMock.Object;
			
		// Disconnect before writing and then wait a little bit.
		plc.Disconnect();
		var writeTask = plc.WriteItemsAsync(new IPlcItem[] {byteItem});
		await Task.Delay(500);
			
		// Act
		plc.Dispose();

		// Assert
		Exception exception = null;
		try
		{
			await writeTask;
		}
		catch (Exception ex)
		{
			exception = ex;
		}

		Assert.That(exception, Is.TypeOf<DisposedWritePlcException>());
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
				await Task.Delay(500);
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
	/// Checks if a read operation that takes longer than the configured communication timeout is treated as unresponsive: the connection is marked as interrupted,
	/// <see cref="Plc.OnCommunicationTimeout"/> is invoked, and the affected items are automatically retried once the connection has been reestablished.
	/// </summary>
	[Test]
	public async Task Check_If_Reading_Is_Retried_After_Communication_Timeout()
	{
		// Arrange
		var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: byte.MaxValue);
		var communicationTimeout = TimeSpan.FromMilliseconds(200);
		var plcMock = new Mock<Plc>(1, "MockPlc", (TimeSpan?) communicationTimeout) { CallBase = true };
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
		var callCounter = 0;
		plcMock
			.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()))
			.Returns
			(
				async () =>
				{
					if (Interlocked.Increment(ref callCounter) == 1)
					{
						// Simulate a hung operation that takes far longer than the configured communication timeout.
						await Task.Delay(TimeSpan.FromSeconds(2));
					}
				}
			)
			.Verifiable()
			;
		plcMock
			.Setup(p => p.OnCommunicationTimeout())
			.Verifiable()
			;
		var plc = plcMock.Object;

		var interruptedCounter = 0;
		plc.Interrupted += (sender, state) => interruptedCounter++;

		// Connect to the plc.
		var success = plc.Connect();
		Assert.True(success);

		// Act: Execute the read function. The first (simulated hung) attempt should time out, triggering an automatic reconnect and retry.
		var result = await plc.ReadItemAsync(byteItem);

		// Assert
		Assert.True(interruptedCounter == 1);
		plcMock.Verify(p => p.OnCommunicationTimeout(), Times.Once);
		plcMock.Verify(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
		Assert.True(byteItem.Value == result);
	}

	/// <summary>
	/// Checks if a write operation that takes longer than the configured communication timeout is treated as unresponsive: the connection is marked as interrupted,
	/// <see cref="Plc.OnCommunicationTimeout"/> is invoked, and the affected items are automatically retried once the connection has been reestablished.
	/// </summary>
	[Test]
	public async Task Check_If_Writing_Is_Retried_After_Communication_Timeout()
	{
		// Arrange
		var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: byte.MaxValue);
		var communicationTimeout = TimeSpan.FromMilliseconds(200);
		var plcMock = new Mock<Plc>(1, "MockPlc", (TimeSpan?) communicationTimeout) { CallBase = true };
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
		var callCounter = 0;
		plcMock
			.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()))
			.Returns
			(
				async () =>
				{
					if (Interlocked.Increment(ref callCounter) == 1)
					{
						// Simulate a hung operation that takes far longer than the configured communication timeout.
						await Task.Delay(TimeSpan.FromSeconds(2));
					}
				}
			)
			.Verifiable()
			;
		plcMock
			.Setup(p => p.OnCommunicationTimeout())
			.Verifiable()
			;
		var plc = plcMock.Object;

		var interruptedCounter = 0;
		plc.Interrupted += (sender, state) => interruptedCounter++;

		// Connect to the plc.
		var success = plc.Connect();
		Assert.True(success);

		// Act: Execute the write function. The first (simulated hung) attempt should time out, triggering an automatic reconnect and retry.
		var writeSuccess = await plc.WriteItemAsync(byteItem);

		// Assert
		Assert.True(writeSuccess);
		Assert.True(interruptedCounter == 1);
		plcMock.Verify(p => p.OnCommunicationTimeout(), Times.Once);
		plcMock.Verify(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
	}

	/// <summary>
	/// Checks that no communication timeout guard is applied if none was configured, even if the read operation takes a long time.
	/// </summary>
	[Test]
	public async Task Check_If_No_Communication_Timeout_Is_Applied_By_Default()
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
			.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()))
			.Returns(async () => await Task.Delay(TimeSpan.FromMilliseconds(300)))
			.Verifiable()
			;
		plcMock
			.Setup(p => p.OnCommunicationTimeout())
			.Verifiable()
			;
		var plc = plcMock.Object;

		var interruptedCounter = 0;
		plc.Interrupted += (sender, state) => interruptedCounter++;

		// Connect to the plc.
		var success = plc.Connect();
		Assert.True(success);

		// Act
		var result = await plc.ReadItemAsync(byteItem);

		// Assert
		Assert.True(interruptedCounter == 0);
		plcMock.Verify(p => p.OnCommunicationTimeout(), Times.Never);
		plcMock.Verify(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()), Times.Once);
		Assert.True(byteItem.Value == result);
	}

	/// <summary>
	/// Checks that a regular external cancellation of a read operation is not misinterpreted as a communication timeout, even if the operation is still pending when the guard's
	/// internal <see cref="Task.Delay(TimeSpan, CancellationToken)"/> completes as canceled due to the very same token being canceled.
	/// </summary>
	[Test]
	public async Task Check_If_External_Cancellation_Is_Not_Treated_As_Communication_Timeout()
	{
		// Arrange
		var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: byte.MaxValue);
		var communicationTimeout = TimeSpan.FromSeconds(2); // Deliberately much longer than the cancellation below, so only cancellation - and not the guard timeout - can trigger completion.
		var plcMock = new Mock<Plc>(1, "MockPlc", (TimeSpan?) communicationTimeout) { CallBase = true };
		plcMock
			.Setup(p => p.OpenConnection())
			.Returns(true)
			.Verifiable()
			;
		plcMock
			.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()))
			.Returns(async (ICollection<IPlcItem> items, Plc.PlcItemUsageType usageType, CancellationToken token) => await Task.Delay(TimeSpan.FromSeconds(5), token))
			.Verifiable()
			;
		plcMock
			.Setup(p => p.OnCommunicationTimeout())
			.Verifiable()
			;
		var plc = plcMock.Object;

		var interruptedCounter = 0;
		plc.Interrupted += (sender, state) => interruptedCounter++;

		// Connect to the plc.
		var success = plc.Connect();
		Assert.True(success);

		// Act: Cancel well before both the simulated operation and the configured communication timeout would elapse.
		using var externalTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

		// Assert: A canceled read simply stops silently (per 'ExecuteReadWriteAsync's cancellation handling) instead of surfacing as a communication timeout.
		var result = await plc.ReadItemAsync(byteItem, externalTokenSource.Token);
		Assert.True(interruptedCounter == 0);
		plcMock.Verify(p => p.OnCommunicationTimeout(), Times.Never);
		Assert.True(result == byteItem.Value); // Value should remain unchanged since the read never completed.
	}

	/// <summary>
	/// Checks that a negative <c>communicationTimeout</c> constructor argument (other than <see cref="Timeout.InfiniteTimeSpan"/>) is treated as if no timeout was configured,
	/// instead of eventually causing an <see cref="ArgumentOutOfRangeException"/> when used internally with <see cref="Task.Delay(TimeSpan, CancellationToken)"/>.
	/// </summary>
	[Test]
	public async Task Check_If_Negative_Communication_Timeout_Is_Treated_As_Infinite()
	{
		// Arrange
		var byteItem = new BytePlcItem(dataBlock: 0, position: 0, initialValue: byte.MaxValue);
		var negativeCommunicationTimeout = TimeSpan.FromSeconds(-5);
		var plcMock = new Mock<Plc>(1, "MockPlc", (TimeSpan?) negativeCommunicationTimeout) { CallBase = true };
		plcMock
			.Setup(p => p.OpenConnection())
			.Returns(true)
			.Verifiable()
			;
		plcMock
			.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()))
			.Returns(async () => await Task.Delay(TimeSpan.FromMilliseconds(300)))
			.Verifiable()
			;
		plcMock
			.Setup(p => p.OnCommunicationTimeout())
			.Verifiable()
			;
		var plc = plcMock.Object;

		var interruptedCounter = 0;
		plc.Interrupted += (sender, state) => interruptedCounter++;

		// Connect to the plc.
		var success = plc.Connect();
		Assert.True(success);

		// Act
		var result = await plc.ReadItemAsync(byteItem);

		// Assert
		Assert.True(interruptedCounter == 0);
		plcMock.Verify(p => p.OnCommunicationTimeout(), Times.Never);
		plcMock.Verify(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()), Times.Once);
		Assert.True(byteItem.Value == result);
	}

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

	#region Reading of dynamic items

	/// <summary>
	/// Creates a connected <see cref="Plc"/> whose read and write operations fail for all passed items that are listed in <paramref name="failingPlcItems"/>. All other items are read successfully.
	/// </summary>
	/// <param name="failingPlcItems"> The items that cannot be read. </param>
	/// <param name="readPlcItems"> Receives a record of all items of every call to <see cref="Plc.PerformReadWriteAsync"/>. </param>
	private static Plc CreatePlcWithFailingItems(ICollection<IPlcItem> failingPlcItems, out List<IPlcItem[]> readPlcItems)
	{
		var calls = new List<IPlcItem[]>();
		readPlcItems = calls;
		var plcMock = new Mock<Plc>("MockPlc") { CallBase = true };
		plcMock.Setup(p => p.OpenConnection()).Returns(true);
		plcMock.Setup(p => p.CloseConnection()).Returns(true);
		plcMock
			.Setup(p => p.PerformReadWriteAsync(It.IsAny<ICollection<IPlcItem>>(), It.IsAny<Plc.PlcItemUsageType>(), It.IsAny<CancellationToken>()))
			.Returns<ICollection<IPlcItem>, Plc.PlcItemUsageType, CancellationToken>
			(
				(plcItems, usageType, _) =>
				{
					var items = plcItems.ToArray();
					lock (calls) calls.Add(items);
					var failedItems = items.Where(item => failingPlcItems.Any(failing => Object.ReferenceEquals(failing, item))).ToArray();
					if (failedItems.Any())
					{
						var validItems = items.Except(failedItems, new ReferenceComparer()).ToArray();
						var failures = failedItems.Select(item => (item, "Failed on purpose.")).ToArray();
						if (usageType == Plc.PlcItemUsageType.Write) throw new WritePlcException(validItems, failures, "Some of the items couldn't be written.");
						throw new ReadPlcException(validItems, failures, "Some of the items couldn't be read.");
					}
					return Task.CompletedTask;
				}
			);
		var plc = plcMock.Object;
		Assert.That(plc.Connect(), Is.True);
		return plc;
	}

	private static DynamicBytesPlcItem CreateDynamicItem(ushort position, string identifier)
		=> new DynamicBytesPlcItem(new BytePlcItem(dataBlock: 0, position: position), new byte[] { 1, 2 }, identifier);

	private sealed class ReferenceComparer : IEqualityComparer<IPlcItem>
	{
		public bool Equals(IPlcItem x, IPlcItem y) => Object.ReferenceEquals(x, y);
		public int GetHashCode(IPlcItem obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}

	/// <summary>
	/// Checks that a failure while reading the flexible part of a dynamic item is reported for the dynamic item itself and that all other items are reported as valid.
	/// </summary>
	[Test]
	public async Task Read_FailingFlexiblePartOfDynamicItem_ReportsAllItemsOfTheCaller()
	{
		// Arrange
		var fixedItem = new BytePlcItem(dataBlock: 0, position: 0, identifier: "Fixed");
		var failingDynamicItem = CreateDynamicItem(10, "FailingDynamic");
		var healthyDynamicItem = CreateDynamicItem(20, "HealthyDynamic");
		var plc = CreatePlcWithFailingItems(new IPlcItem[] { failingDynamicItem.FlexiblePlcItem }, out _);

		// Act
		var exception = await Assert.ThrowsAsync<ReadPlcException>(() => plc.ReadItemsAsync(new List<IPlcItem> { fixedItem, failingDynamicItem, healthyDynamicItem }));

		// Assert
		Assert.That(exception.FailedItems.Select(failed => failed.FailedItem), Is.EquivalentTo(new IPlcItem[] { failingDynamicItem }).Using(new ReferenceComparer()));
		Assert.That(exception.ValidItems, Is.EquivalentTo(new IPlcItem[] { fixedItem, healthyDynamicItem }).Using(new ReferenceComparer()));
	}

	/// <summary>
	/// Checks that a failure while reading the length of a dynamic item neither prevents reading the flexible part of the other dynamic items nor is reported for them.
	/// </summary>
	[Test]
	public async Task Read_FailingLengthOfDynamicItem_StillReadsOtherDynamicItems()
	{
		// Arrange
		var fixedItem = new BytePlcItem(dataBlock: 0, position: 0, identifier: "Fixed");
		var failingDynamicItem = CreateDynamicItem(10, "FailingDynamic");
		var healthyDynamicItem = CreateDynamicItem(20, "HealthyDynamic");
		var plc = CreatePlcWithFailingItems(new IPlcItem[] { failingDynamicItem.LengthPlcItem }, out var calls);

		// Act
		var exception = await Assert.ThrowsAsync<ReadPlcException>(() => plc.ReadItemsAsync(new List<IPlcItem> { fixedItem, failingDynamicItem, healthyDynamicItem }));

		// Assert
		Assert.That(exception.FailedItems.Select(failed => failed.FailedItem), Is.EquivalentTo(new IPlcItem[] { failingDynamicItem }).Using(new ReferenceComparer()));
		Assert.That(exception.ValidItems, Is.EquivalentTo(new IPlcItem[] { fixedItem, healthyDynamicItem }).Using(new ReferenceComparer()));
		Assert.That(calls, Has.Count.EqualTo(2), "The flexible parts must be read although the first phase failed.");
		Assert.That(calls[1], Is.EquivalentTo(new IPlcItem[] { healthyDynamicItem.FlexiblePlcItem }).Using(new ReferenceComparer()), "Only the flexible part of the dynamic item whose length was read must be read.");
	}

	/// <summary>
	/// Checks that failures of both phases are reported together.
	/// </summary>
	[Test]
	public async Task Read_FailuresInBothPhases_AreReportedTogether()
	{
		// Arrange
		var failingFixedItem = new BytePlcItem(dataBlock: 0, position: 0, identifier: "FailingFixed");
		var failingLengthItem = CreateDynamicItem(10, "FailingLength");
		var failingFlexibleItem = CreateDynamicItem(20, "FailingFlexible");
		var healthyDynamicItem = CreateDynamicItem(30, "HealthyDynamic");
		var plc = CreatePlcWithFailingItems(new IPlcItem[] { failingFixedItem, failingLengthItem.LengthPlcItem, failingFlexibleItem.FlexiblePlcItem }, out _);

		// Act
		var exception = await Assert.ThrowsAsync<ReadPlcException>(() => plc.ReadItemsAsync(new List<IPlcItem> { failingFixedItem, failingLengthItem, failingFlexibleItem, healthyDynamicItem }));

		// Assert
		Assert.That(exception.FailedItems.Select(failed => failed.FailedItem), Is.EquivalentTo(new IPlcItem[] { failingFixedItem, failingLengthItem, failingFlexibleItem }).Using(new ReferenceComparer()));
		Assert.That(exception.ValidItems, Is.EquivalentTo(new IPlcItem[] { healthyDynamicItem }).Using(new ReferenceComparer()));
	}

	#endregion

	#region Writing of dynamic items

	/// <summary>
	/// Checks that a failure while writing the flexible part of a dynamic item is reported for the dynamic item itself, that all other items are reported as valid and that the length of the failed item is not written.
	/// </summary>
	[Test]
	public async Task Write_FailingFlexiblePartOfDynamicItem_ReportsAllItemsOfTheCaller()
	{
		// Arrange
		var fixedItem = new BytePlcItem(dataBlock: 0, position: 0, identifier: "Fixed");
		var failingDynamicItem = CreateDynamicItem(10, "FailingDynamic");
		var healthyDynamicItem = CreateDynamicItem(20, "HealthyDynamic");
		var plc = CreatePlcWithFailingItems(new IPlcItem[] { failingDynamicItem.FlexiblePlcItem }, out var calls);

		// Act
		var exception = await Assert.ThrowsAsync<WritePlcException>(() => plc.WriteItemsAsync(new List<IPlcItem> { fixedItem, failingDynamicItem, healthyDynamicItem }));

		// Assert
		Assert.That(exception.FailedItems.Select(failed => failed.FailedItem), Is.EquivalentTo(new IPlcItem[] { failingDynamicItem }).Using(new ReferenceComparer()));
		Assert.That(exception.ValidItems, Is.EquivalentTo(new IPlcItem[] { fixedItem, healthyDynamicItem }).Using(new ReferenceComparer()));
		Assert.That(calls, Has.Count.EqualTo(2), "The length parts must be written although the first phase failed.");
		Assert.That(calls[1], Is.EquivalentTo(new IPlcItem[] { healthyDynamicItem.LengthPlcItem }).Using(new ReferenceComparer()), "The length of an item whose data could not be written must not be written, because it would not match the data in the plc.");
	}

	/// <summary>
	/// Checks that a failure while writing the length of a dynamic item is reported for the dynamic item itself.
	/// </summary>
	[Test]
	public async Task Write_FailingLengthOfDynamicItem_ReportsAllItemsOfTheCaller()
	{
		// Arrange
		var fixedItem = new BytePlcItem(dataBlock: 0, position: 0, identifier: "Fixed");
		var failingDynamicItem = CreateDynamicItem(10, "FailingDynamic");
		var healthyDynamicItem = CreateDynamicItem(20, "HealthyDynamic");
		var plc = CreatePlcWithFailingItems(new IPlcItem[] { failingDynamicItem.LengthPlcItem }, out _);

		// Act
		var exception = await Assert.ThrowsAsync<WritePlcException>(() => plc.WriteItemsAsync(new List<IPlcItem> { fixedItem, failingDynamicItem, healthyDynamicItem }));

		// Assert
		Assert.That(exception.FailedItems.Select(failed => failed.FailedItem), Is.EquivalentTo(new IPlcItem[] { failingDynamicItem }).Using(new ReferenceComparer()));
		Assert.That(exception.ValidItems, Is.EquivalentTo(new IPlcItem[] { fixedItem, healthyDynamicItem }).Using(new ReferenceComparer()));
	}

	/// <summary>
	/// Checks that failures of both phases are reported together.
	/// </summary>
	[Test]
	public async Task Write_FailuresInBothPhases_AreReportedTogether()
	{
		// Arrange
		var failingFixedItem = new BytePlcItem(dataBlock: 0, position: 0, identifier: "FailingFixed");
		var failingFlexibleItem = CreateDynamicItem(10, "FailingFlexible");
		var failingLengthItem = CreateDynamicItem(20, "FailingLength");
		var healthyDynamicItem = CreateDynamicItem(30, "HealthyDynamic");
		var plc = CreatePlcWithFailingItems(new IPlcItem[] { failingFixedItem, failingFlexibleItem.FlexiblePlcItem, failingLengthItem.LengthPlcItem }, out _);

		// Act
		var exception = await Assert.ThrowsAsync<WritePlcException>(() => plc.WriteItemsAsync(new List<IPlcItem> { failingFixedItem, failingFlexibleItem, failingLengthItem, healthyDynamicItem }));

		// Assert
		Assert.That(exception.FailedItems.Select(failed => failed.FailedItem), Is.EquivalentTo(new IPlcItem[] { failingFixedItem, failingFlexibleItem, failingLengthItem }).Using(new ReferenceComparer()));
		Assert.That(exception.ValidItems, Is.EquivalentTo(new IPlcItem[] { healthyDynamicItem }).Using(new ReferenceComparer()));
	}

	#endregion
}