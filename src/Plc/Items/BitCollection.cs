#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// Represents a collection of bits.
	/// </summary>
	/// <remarks>
	/// <para> This class can be directly used as: </para>
	/// <para> • <see cref="Boolean"/> array </para>
	/// <para> • <see cref="Byte"/> array </para>
	/// </remarks>
	public sealed class BitCollection : IEnumerable, IEquatable<BitCollection>, IDeepCloneable<BitCollection>
	{
		#region Delegates / Events

		/// <summary>
		/// Raised if at least one of the bits was changed.
		/// </summary>
		public event EventHandler<BitsChangedEventArgs> BitsChanged;
		private void OnBitsChanged(params BitChange[] changes)
		{
			if (!changes.Any()) return;

			lock (_valueAccessLockObject)
			{
				this.SynchronizeBytes();

				// Enqueue the event.
				_eventQueue.Enqueue(changes);
			}

			// Execute all changed events.
			lock (_valueChangeLockObject)
			{
				while (_eventQueue.TryDequeue(out var changeValue))
				{
					// Raise the event.
					this.BitsChanged?.Invoke(this, new BitsChangedEventArgs(changeValue));
				}
			}
		}

		#endregion

		#region Constants
		#endregion

		#region Fields
		
		private readonly object _valueAccessLockObject;

		private readonly object _valueChangeLockObject;

		private readonly ConcurrentQueue<BitChange[]> _eventQueue;

		#endregion

		#region Properties

		/// <summary> Will the size of the internal storage be adapted automatically according to new values. </summary>
		public bool AutomaticallyAdaptSize { get; }

		/// <summary> Indexer </summary>
		public bool this[uint index]
		{
			get
			{
				lock (_valueAccessLockObject)
				{
					return this.Bits[index];
				}
			}
			set
			{
				lock (_valueAccessLockObject)
				{
					if (this.Bits[index] == value) return;

					var oldValue = this.Bits[index];
					this.Bits[index] = value;
					this.OnBitsChanged(new BitChange(index, oldValue, value));
				}
			}
		}

		private bool[] Bits
		{
			get
			{
				lock (_valueAccessLockObject)
				{
					return _bits;
				}
			}
		}
		private bool[] _bits;

		private byte[] Bytes
		{
			get
			{
				lock (_valueAccessLockObject)
				{
					return _bytes;
				}
			}
			set
			{
				lock (_valueAccessLockObject)
				{
					_bytes = value;
				}
			}
		}
		private byte[] _bytes;

		/// <summary> The total amount of bits this <see cref="BitCollection"/> contains. </summary>
		public uint Length => (uint) this.Bits.Length;

		/// <summary> The total amount of bytes this <see cref="BitCollection"/> contains. </summary>
		public uint ByteLength => (uint) this.Bytes.Length;

		/// <summary> Does this <see cref="BitCollection"/> handle full bytes or single bits. </summary>
		/// <remarks> Full bytes are identified by bit amount and modulo 8. </remarks>
		public bool HandlesFullBytes => this.Length % 8 == 0;

		/// <summary> Does the underlying collection contain real data or just zeros. </summary>
		public bool ContainsOnlyZeros => this.Bits.All(b => b == false);

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="bytes"> A byte array of booleans that will be interpreted as single <see cref="Boolean"/>s. </param>
		public BitCollection(bool automaticallyAdaptSize, byte[] bytes) : this(null, bytes, automaticallyAdaptSize) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="booleans"> A <see cref="bool"/> array of booleans used to initialize the internal collection. </param>
		public BitCollection(bool automaticallyAdaptSize, params bool[] booleans) : this(booleans, null, automaticallyAdaptSize) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="bitCollections"> Several other <see cref="BitCollection"/> that will build a new one. </param>
		public BitCollection(bool automaticallyAdaptSize, params BitCollection[] bitCollections) : this(bitCollections.SelectMany(collection => (bool[])collection).ToArray(), null, automaticallyAdaptSize) { }
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="booleans"> A <see cref="bool"/> array used as internal collection. </param>
		/// <param name="bytes"> A <see cref="Byte"/> array that is synchronized with <paramref name="booleans"/> used for faster casting. </param>
		///// <param name="bitPosition"> An offset value for the internal collection of bits. </param>
		private BitCollection(bool[] booleans, byte[] bytes, bool automaticallyAdaptSize)
		{
			if (booleans is null && bytes is null) throw new ArgumentNullException($"At least one of the two parameters '{nameof(booleans)}' and '{nameof(bytes)}' mustn't be NULL.");

			// Save parameters.
			_bits = booleans ?? DataConverter.ToBooleans(bytes);
			_bytes = bytes ?? DataConverter.ToBytes(_bits);
			this.AutomaticallyAdaptSize = automaticallyAdaptSize;

			// Initialize fields.
			_valueAccessLockObject = new object();
			_valueChangeLockObject = new object();
			_eventQueue = new ConcurrentQueue<BitChange[]>();
		}

		#endregion

		#region Methods

		#region Change Size

		/// <summary>
		/// Resizes the internal <see cref="bool"/> array.
		/// </summary>
		/// <param name="newLength"> The new length for <see cref="Bits"/>. </param>
		public void Resize(uint newLength)
		{
			lock (_valueAccessLockObject)
			{
				var changes = this.Resize_Internal(newLength).ToArray();
				if (!changes.Any()) return;
				
				this.OnBitsChanged(changes);
			}
		}

		/// <summary>
		/// Resizes the internal <see cref="bool"/> array.
		/// </summary>
		/// <param name="newLength"> The new length for <see cref="Bits"/>. </param>
		/// <returns> A list of the <see cref="BitChange"/>s that happened because of resizing. </returns>
		private List<BitChange> Resize_Internal(uint newLength)
		{
			lock (_valueAccessLockObject)
			{
				if (this.Length == newLength) return new List<BitChange>();

				var oldBits = _bits;
				var oldLength = this.Length;

				Array.Resize(ref _bits, (int)newLength);
				this.SynchronizeBytes();

				//? Should this raise change notification? Maybe only if the array got truncated?
				//! A Change is a change...so raise for both!
				if (oldLength < newLength)
				{
					// Array has been extended.
					return Enumerable
						.Range(start: (int) oldLength, count: (int) (newLength - oldLength))
						.Select(index => new BitChange((uint) index, null, false))
						.ToList()
						;
				}
				else
				{
					// Array has been truncated.
					return Enumerable
						.Range(start: (int) newLength, count: (int) (oldLength - newLength))
						.Select(index => new BitChange((uint) index, oldBits[index], null))
						.ToList()
						;
				}
			}
		}

		#endregion

		#region Change Bits

		/// <summary>
		/// Sets all bits to <paramref name="value"/>
		/// </summary>
		/// <param name="value"> The new value. </param>
		public void SetAllBitsTo(bool value)
			=> this.TransferValuesFrom(value ? this.Bits.Select(bit => true).ToArray() : new bool[this.Bits.Length]);

		/// <summary>
		/// Transfers the bits from another <see cref="BitCollection"/> to this one.
		/// </summary>
		/// <param name="other"> The other <see cref="BitCollection"/>. </param>
		/// <param name="startPosition"> The bit-based position within the internal collection where to start transferring the other <see cref="BitCollection"/>. </param>
		/// /// <param name="adaptSize"> Should the size of the internal storage be adapted according to the size of the <paramref name="other"/>. Default is <c>False</c>. </param>
		public void TransferValuesFrom(BitCollection other, uint startPosition = 0, bool adaptSize = false)
			=> this.TransferValuesFrom(other.Bits, startPosition);

		/// <summary>
		/// Transfers the <paramref name="bytes"/> to the internal boolean collection.
		/// </summary>
		/// <param name="bytes"> The <see cref="Byte"/> array to apply. </param>
		/// <param name="startPosition"> The byte-based position within the internal collection where to start transferring the <paramref name="bytes"/>. </param>
		/// <param name="adaptSize"> Should the size of the internal storage be adapted according to the size of the <paramref name="bytes"/>. Default is <c>False</c>. </param>
		public void TransferValuesFrom(byte[] bytes, uint startPosition = 0, bool adaptSize = false)
			=> this.TransferValuesFrom(DataConverter.ToBooleans(bytes), startPosition * 8);

		/// <summary>
		/// Transfers the <paramref name="booleans"/> to the internal boolean collection.
		/// </summary>
		/// <param name="booleans"> The <see cref="bool"/> array to apply. </param>
		/// <param name="startPosition"> The bit-based position within the internal collection where to start transferring the <paramref name="booleans"/>. </param>
		public void TransferValuesFrom(bool[] booleans, uint startPosition = 0)
		{
			lock (_valueAccessLockObject)
			{
				// Change the size if needed.
				var changes = this.AutomaticallyAdaptSize ? this.Resize_Internal((uint) booleans.Length) : new List<BitChange>();
				
				for (var bitPosition = startPosition; bitPosition < Math.Min(this.Bits.Length, booleans.Length) + startPosition; bitPosition++)
				{
					var currentValue = this.Bits[bitPosition];
					var newValue = booleans[bitPosition - startPosition];

					if (currentValue != newValue)
					{
						var change = changes.SingleOrDefault(bitChange => bitChange.Position == bitPosition);
						if (change is null)
						{
							changes.Add(new BitChange(bitPosition, currentValue, newValue));
						}
						else
						{
							change.NewValue = newValue;
						}
						this.Bits[bitPosition] = newValue;
					}
				}

				this.OnBitsChanged(changes.ToArray());
			}
		}

		#endregion
		
		#region IEnumerable

		/// <inheritdoc />
		public IEnumerator GetEnumerator()
		{
			return this.Bits.GetEnumerator();
		}

		#endregion

		#region IEquatable
		
		//! This class has no immutable properties and therefore no proper implementation of 'GetHashCode' can be made.
		/// <inheritdoc />
		public override int GetHashCode()
			=> base.GetHashCode();

		/// <summary>
		/// Compares this instance to another one.
		/// </summary>
		/// <param name="other"> The other instance to compare to. </param>
		/// <returns> <c>TRUE</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>FALSE</c>. </returns>
		public override bool Equals(object other)
		{
			if (other is BitCollection equatable) return this.Equals(equatable);
			return false;
		}

		/// <summary>
		/// Compares this instance to another one.
		/// </summary>
		/// <param name="other"> The other instance to compare to. </param>
		/// <returns> <c>TRUE</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>FALSE</c>. </returns>
		public bool Equals(BitCollection other)
		{
			if (other is null) return false;
			return this.SequenceEqual(other);
		}

		/// <summary>
		/// Compares the current instance to another <see cref="BitCollection"/>.
		/// </summary>
		/// <param name="other"> The other <see cref="BitCollection"/> to compare to. </param>
		/// <returns> <c>True</c> if both <see cref="BitCollection"/> are sequentially equal, otherwise <c>False</c>. </returns>
		public bool SequenceEqual(BitCollection other)
		{
			return this.Bytes.SequenceEqual(other.Bytes);
		}

		/// <summary>
		/// Checks if the two <see cref="BitCollection"/>s are equal.
		/// </summary>
		/// <param name="x"> The first <see cref="BitCollection"/> to compare. </param>
		/// <param name="y"> The second <see cref="BitCollection"/> to compare. </param>
		/// <returns> <c>TRUE</c> if both instances are equal, otherwise <c>FALSE</c>. </returns>
		public static bool operator ==(BitCollection x, BitCollection y)
		{
			if (x is null && y is null) return true;
			if (x is null) return false;
			return x.Equals(y);
		}

		/// <summary>
		/// Checks if the two <see cref="BitCollection"/>s are NOT equal.
		/// </summary>
		/// <param name="x"> The first <see cref="BitCollection"/> to compare. </param>
		/// <param name="y"> The second <see cref="BitCollection"/> to compare. </param>
		/// <returns> <c>TRUE</c> if both instances are NOT equal, otherwise <c>FALSE</c>. </returns>
		public static bool operator !=(BitCollection x, BitCollection y)
		{
			return !(x == y);
		}

		#endregion

		#region IDeepCloneable

		private bool[] CloneBits()
		{
			lock (_valueAccessLockObject)
			{
				var clonedBits = new bool[this.Bits.Length];
				this.Bits.CopyTo(clonedBits, 0);
				return clonedBits;
			}
		}

		/// <inheritdoc />
		public BitCollection Clone()
		{
			return new BitCollection(this.AutomaticallyAdaptSize, this.CloneBits());
		}

		#endregion

		#region Casting Operators

		/// <summary>
		/// Explicit cast operator to <see cref="bool"/> array.
		/// </summary>
		public static implicit operator bool[] (BitCollection bitCollection)
		{
			return bitCollection.Bits;
		}

		/// <summary>
		/// Explicit cast operator to <see cref="Byte"/> array.
		/// </summary>
		public static implicit operator byte[] (BitCollection bitCollection)
		{
			return bitCollection.Bytes;
		}

		#endregion

		#region Helper

		/// <summary>
		/// Updates the <see cref="Bytes"/> to match the <see cref="Bits"/>.
		/// </summary>
		private void SynchronizeBytes()
		{
			// Always change the bytes too.
			this.Bytes = DataConverter.ToBytes(this.Bits);
		}

		#endregion

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: {String.Join(",", this.Bytes)}]";

		#endregion
	}

	/// <summary>
	/// <see cref="EventArgs"/> for changes in the internal collection of the <see cref="BitCollection"/> class.
	/// </summary>
	public class BitsChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The collection of <see cref="BitChange"/>s that occured.
		/// </summary>
		public ICollection<BitChange> Changes { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="changes"> <see cref="Changes"/> </param>
		public BitsChangedEventArgs(params BitChange[] changes)
		{
			this.Changes = changes;
		}
	}

	/// <summary>
	/// Represents a single changed bit.
	/// </summary>
	public class BitChange
	{
		/// <summary> The bit position. </summary>
		public uint Position { get; }

		/// <summary> The previous value or <c>Null</c> if the bit has been added. </summary>
		public bool? OldValue { get; }

		/// <summary> The new value or <c>Null</c> if the bit has been removed. </summary>
		/// <remarks> The setter is internal, so that multiple changes to the same bit can be updated. </remarks>
		public bool? NewValue { get; internal set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="position"> <see cref="Position"/> </param>
		/// <param name="oldValue"> <see cref="OldValue"/> </param>
		/// <param name="newValue"> <see cref="NewValue"/> </param>
		public BitChange(uint position, bool? oldValue, bool? newValue)
		{
			this.Position = position;
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: {this.Position} | {(this.OldValue is null ? "UNAVAILABLE" : this.OldValue.Value.ToString())} → {(this.NewValue is null ? "REMOVED" : this.NewValue.Value.ToString())}]";
	}
}