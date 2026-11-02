using System.Diagnostics;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Test;

[TestFixture]
class BitCollectionTest
{
	/// <summary>
	/// Checks if two <see cref="BitCollection"/>s with the same data are considered equal.
	/// </summary>
	[Test]
	public void Same_Are_Equal()
	{
		// Arrange
		var first = new BitCollection(false, Enumerable.Range(0, 10).Select(index => (byte)index).ToArray());
		var second = new BitCollection(false, Enumerable.Range(0, 10).Select(index => (byte)index).ToArray());

		// Act
		var areEqual = first.Equals(second);

		// Assert
		Assert.True(areEqual);
	}

	/// <summary>
	/// Checks if two <see cref="BitCollection"/>s with different data are considered un-equal.
	/// </summary>
	[Test]
	public void Same_Are_Not_Equal()
	{
		// Arrange
		var first = new BitCollection(false, Enumerable.Range(0, 10).Select(index => (byte)index).ToArray());
		var second = new BitCollection(false, Enumerable.Range(10, 10).Select(index => (byte)index).ToArray());

		// Act
		var areEqual = first.Equals(second);

		// Assert
		Assert.False(areEqual);
	}

	/// <summary>
	/// Checks if a cloned <see cref="BitCollection"/>s is equal to the original.
	/// </summary>
	[Test]
	public void Clones_Are_Equal_But_Not_Same()
	{
		// Arrange
		var bitCollection = new BitCollection(false, Enumerable.Range(0, 10).Select(index => (byte)index).ToArray());
		var clone = bitCollection.Clone();

		// Act
		var areEqual = bitCollection.Equals(clone);

		// Assert
		Assert.True(areEqual);
		Assert.That(clone, Is.Not.SameAs(bitCollection));
	}

	/// <summary>
	/// Checks if new data is properly transferred to the underlying data of a <see cref="BitCollection"/>.
	/// </summary>
	[Test]
	public void New_Data_Is_Transferred()
	{
		// Arrange
		var bitCollection = new BitCollection(false, Enumerable.Range(0, 5).Select(index => false).ToArray());
		var newData = Enumerable.Range(0, (int)(bitCollection.Length)).Select(index => true).ToArray();
		var targetLength = newData.Length;

		// Act
		bitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.True(((bool[]) bitCollection).SequenceEqual(newData));
	}

	/// <summary>
	/// Checks changes in the underlying data of a <see cref="BitCollection"/> properly triggers change notification.
	/// </summary>
	[Test]
	public void Change_Notification_Is_Raised()
	{
		// Arrange
		BitChanges changes = null;
		var bitCollection = new BitCollection(false, Enumerable.Range(0, 10).Select(index => false).ToArray());
		bitCollection.BitsChanged += (sender, args) => changes = args.Changes;
		var newData = Enumerable.Range(0, 5).Select(index => true).Concat(Enumerable.Range(0, 5).Select(index => false)).ToArray();
		var targetChangesCount = newData.Count(boolean => boolean == true);

		// Act
		bitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.That(changes.Count, Is.EqualTo(targetChangesCount));
		var position = uint.MinValue;
		foreach (var change in changes.OrderBy(change => change.Key))
		{
			Assert.True(change.Key == position);
			Assert.False(change.Value.OldValue, $"Old value was not false at position: {position}.");
			Assert.True(change.Value.NewValue, $"New value was not true at position: {position}.");
			position++;
		}
	}

	/// <summary>
	/// Checks if change notification is raised if the underlying data of a <see cref="BitCollection"/> is extended.
	/// </summary>
	[Test]
	public void Change_Notification_Is_Raised_If_Dynamically_Expanded()
	{
		// Arrange
		BitChanges changes = null;
		var modifiableBitCollection = new BitCollection(true, Enumerable.Range(0, 5).Select(index => false).ToArray());
		modifiableBitCollection.BitsChanged += (sender, args) => changes = args.Changes;
		var newData = Enumerable.Range(0, (int)(modifiableBitCollection.Length * 2)).Select(index => false).ToArray(); //! Make this larger.
		var targetChangesCount = modifiableBitCollection.Length;

		// Act
		modifiableBitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.That(changes.Count, Is.EqualTo(targetChangesCount));
		var position = targetChangesCount;
		foreach (var change in changes.OrderBy(change => change.Key))
		{
			Assert.True(change.Key == position);
			Assert.Null(change.Value.OldValue, $"Old value was not null at position: {position}.");
			Assert.False(change.Value.NewValue, $"New value was not false at position: {position}.");
			position++;
		}
	}

	/// <summary>
	/// Checks if change notification is raised if the underlying data of a <see cref="BitCollection"/> is truncated.
	/// </summary>
	[Test]
	public void Change_Notification_Is_Raised_If_Dynamically_Truncated()
	{
		// Arrange
		BitChanges changes = null;
		var modifiableBitCollection = new BitCollection(true, Enumerable.Range(0, 10).Select(index => false).ToArray());
		modifiableBitCollection.BitsChanged += (sender, args) => changes = args.Changes;
		var newData = Enumerable.Range(0, (int)(modifiableBitCollection.Length / 2)).Select(index => false).ToArray(); //! Make this smaller.
		var targetChangesCount = newData.Length;

		// Act
		modifiableBitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.That(changes.Count, Is.EqualTo(targetChangesCount));
		var position = targetChangesCount;
		foreach (var change in changes.OrderBy(change => change.Key))
		{
			Assert.True(change.Key == position);
			Assert.False(change.Value.OldValue, $"Old value was not false at position: {position}.");
			Assert.Null(change.Value.NewValue, $"New value was not null at position: {position}.");
			position++;
		}
	}

	/// <summary>
	/// Checks if a <see cref="BitCollection"/> automatically increases the size of its underlying data structure.
	/// </summary>
	[Test]
	public void Size_Is_Increased()
	{
		// Arrange
		var modifiableBitCollection = new BitCollection(true, Enumerable.Range(0, 5).Select(index => false).ToArray());
		var newData = Enumerable.Range(0, (int) (modifiableBitCollection.Length * 2)).Select(index => false).ToArray(); //! Make this larger.
		var targetLength = newData.Length;

		// Act
		modifiableBitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.That(modifiableBitCollection.Length, Is.EqualTo(targetLength));
	}

	/// <summary>
	/// Checks if a <see cref="BitCollection"/> does not change the size of its underlying data structure if it is configured as fixed.
	/// </summary>
	[Test]
	public void Size_Is_Not_Increased()
	{
		// Arrange
		var fixedBitCollection = new BitCollection(false, Enumerable.Range(0, 5).Select(index => false).ToArray());
		var newData = Enumerable.Range(0, (int) (fixedBitCollection.Length * 2)).Select(index => false).ToArray(); //! Make this larger.
		var targetLength = fixedBitCollection.Length;

		// Act
		fixedBitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.That(fixedBitCollection.Length, Is.EqualTo(targetLength));
	}

	/// <summary>
	/// Checks if a <see cref="BitCollection"/> automatically reduces the size of its underlying data structure.
	/// </summary>
	[Test]
	public void Size_Is_Decreased()
	{
		// Arrange
		var modifiableBitCollection = new BitCollection(true, Enumerable.Range(0, 10).Select(index => false).ToArray());
		var newData = Enumerable.Range(0, (int)(modifiableBitCollection.Length / 2)).Select(index => false).ToArray(); //! Make this smaller.
		var targetLength = newData.Length;

		// Act
		modifiableBitCollection.TransferValuesFrom(newData, 0);

		// Assert
		Assert.That(modifiableBitCollection.Length, Is.EqualTo(targetLength));
	}

	/// <summary>
	/// Checks if change notification is analyzed and executed fast even for large <see cref="BitCollection"/>s.
	/// </summary>
	[Test]
	public void Change_Tracking_Is_Executed_Fast()
	{
		// Arrange
		var changes = 10000;
		var initialData = Enumerable.Range(0, changes).Select(index => false).ToArray();
		var modifiedData = Enumerable.Range(0, changes).Select(index => true).ToArray();
		var largeBitCollection = new BitCollection(false, initialData);
		var stopwatch = new Stopwatch();

		// Act
		stopwatch.Start();
		largeBitCollection.TransferValuesFrom(modifiedData);
		stopwatch.Stop();
			
		// Assert
		var elapsed = stopwatch.ElapsedMilliseconds;
		Console.WriteLine($"Change tracking of {changes} changes took {elapsed}ms.");
		Assert.That(elapsed, Is.LessThanOrEqualTo(20));
		Assert.True(((bool[]) largeBitCollection).All(b => true));
	}
}