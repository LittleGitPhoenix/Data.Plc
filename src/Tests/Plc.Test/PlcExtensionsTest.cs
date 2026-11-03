using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test;

[TestFixture]
public class PlcExtensionsTest
{
	#region WriteItemsInOrderAsync

	[Test]
	public async Task Check_WriteItemsInOrderAsync_Succeeds()
	{
		// Arrange
		var items = new IPlcItem[]
		{
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
		};
		var plc = Mock.Of<IPlc>();
		Mock.Get(plc)
			.SetupSequence(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None))
			.ReturnsAsync(true)
			.ReturnsAsync(true)
			;

		// Act
		var result = await plc.WriteItemsInOrderAsync(items);

		// Assert
		Assert.True(result);
		Mock.Get(plc).Verify(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None), Times.Exactly(items.Length));
	}

	[Test]
	public async Task Check_WriteItemsInOrderAsync_Fails()
	{
		// Arrange
		var items = new IPlcItem[]
		{
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
		};
		var plc = Mock.Of<IPlc>();
		Mock.Get(plc)
			.SetupSequence(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None))
			.ReturnsAsync(true)
			.ReturnsAsync(false)
			.ReturnsAsync(true)
			;

		// Act
		var result = await plc.WriteItemsInOrderAsync(items);

		// Assert
		Assert.False(result);
		Mock.Get(plc).Verify(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None), Times.Exactly(2));
	}

	[Test]
	public async Task Check_WriteItemsInOrderAsync_Fails_But_Continues()
	{
		// Arrange
		var items = new IPlcItem[]
		{
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
		};
		var plc = Mock.Of<IPlc>();
		Mock.Get(plc)
			.SetupSequence(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None))
			.ReturnsAsync(false)
			.ReturnsAsync(true)
			;

		// Act
		var result = await plc.WriteItemsInOrderAsync(items, continueOnError: true);

		// Assert
		Assert.False(result);
		Mock.Get(plc).Verify(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None), Times.Exactly(items.Length));
	}
		
	[Test]
	public void Check_WriteItemsInOrderAsync_Throws()
	{
		// Arrange
		var items = new IPlcItem[]
		{
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
		};
		var plc = Mock.Of<IPlc>();
		Mock.Get(plc)
			.SetupSequence(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None))
			.ReturnsAsync(true)
			.Returns(() => throw new WritePlcException(new IPlcItem[0], new (IPlcItem FailedItem, string ErrorMessage)[0]))
			.ReturnsAsync(true)
			;

		// Act + Assert
		Assert.ThrowsAsync<WritePlcException>(() => plc.WriteItemsInOrderAsync(items, throwOnError: true));
		Mock.Get(plc).Verify(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None), Times.Exactly(2));
	}
		
	[Test]
	public void Check_WriteItemsInOrderAsync_Throws_But_Continues()
	{
		// Arrange
		var items = new IPlcItem[]
		{
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
			new BytePlcItem(0,0),
		};
		var plc = Mock.Of<IPlc>();
		Mock.Get(plc)
			.SetupSequence(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None))
			.ReturnsAsync(true)
			.Returns(() => throw new WritePlcException(new IPlcItem[0], new (IPlcItem FailedItem, string ErrorMessage)[0]))
			.ReturnsAsync(true)
			;

		// Act + Assert
		Assert.ThrowsAsync<WritePlcException>(() => plc.WriteItemsInOrderAsync(items, continueOnError: true, throwOnError: true));
		Mock.Get(plc).Verify(p => p.WriteItemsAsync(It.IsAny<ICollection<IPlcItem>>(), CancellationToken.None), Times.Exactly(items.Length));
	}

	#endregion
}