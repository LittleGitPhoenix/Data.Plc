#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections;
using System.Collections.Concurrent;
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
	public sealed class BitCollection : IEnumerable, IEquatable<BitCollection>, IFormattable, IDeepCloneable<BitCollection>
	{
		#region Delegates / Events

		/// <summary>
		/// Raised if at least one of the bits was changed.
		/// </summary>
		public event EventHandler<BitsChangedEventArgs> BitsChanged;
		private void OnBitsChanged(BitChanges changes)
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

		private readonly ConcurrentQueue<BitChanges> _eventQueue;

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
					this.OnBitsChanged(new BitChanges() {{index, (oldValue, value)}});
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
		public uint Length => (uint)this.Bits.Length;

		/// <summary> The total amount of bytes this <see cref="BitCollection"/> contains. </summary>
		public uint ByteLength => (uint)this.Bytes.Length;

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
		/// <param name="automaticallyAdaptSize"> Will the size of the internal storage be adapted automatically according to new values. </param>
		/// <param name="bytes"> A byte array of booleans that will be interpreted as single <see cref="Boolean"/>s. </param>
		public BitCollection(bool automaticallyAdaptSize, byte[] bytes) : this(null, bytes, automaticallyAdaptSize) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="automaticallyAdaptSize"> Will the size of the internal storage be adapted automatically according to new values. </param>
		/// <param name="booleans"> A <see cref="bool"/> array of booleans used to initialize the internal collection. </param>
		public BitCollection(bool automaticallyAdaptSize, params bool[] booleans) : this(booleans, null, automaticallyAdaptSize) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="automaticallyAdaptSize"> Will the size of the internal storage be adapted automatically according to new values. </param>
		/// <param name="bitCollections"> Several other <see cref="BitCollection"/> that will build a new one. </param>
		public BitCollection(bool automaticallyAdaptSize, params BitCollection[] bitCollections) : this(bitCollections.SelectMany(collection => (bool[])collection).ToArray(), null, automaticallyAdaptSize) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="booleans"> A <see cref="bool"/> array used as internal collection. </param>
		/// <param name="bytes"> A <see cref="Byte"/> array that is synchronized with <paramref name="booleans"/> used for faster casting. </param>
		/// <param name="automaticallyAdaptSize"> Will the size of the internal storage be adapted automatically according to new values. </param>
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
			_eventQueue = new ConcurrentQueue<BitChanges>();
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
				var changes = this.Resize_Internal(newLength)/*.ToArray()*/;
				if (!changes.Any()) return;

				this.OnBitsChanged(changes);
			}
		}

		/// <summary>
		/// Resizes the internal <see cref="bool"/> array.
		/// </summary>
		/// <param name="newLength"> The new length for <see cref="Bits"/>. </param>
		/// <returns> A list of the <see cref="BitChanges"/> that happened because of resizing. </returns>
		private BitChanges Resize_Internal(uint newLength)
		{
			lock (_valueAccessLockObject)
			{
				if (this.Length == newLength) return new BitChanges();

				var oldBits = _bits;
				var oldLength = this.Length;

				Array.Resize(ref _bits, (int) newLength);
				this.SynchronizeBytes();

				//? Should this raise change notification? Maybe only if the array got truncated?
				//! A Change is a change...so raise for both!
				if (oldLength < newLength)
				{
					// Array has been extended.

					//! Use arrays instead of Linq for performance reasons.
					//return Enumerable
					//	.Range(start: (int) oldLength, count: (int) (newLength - oldLength))
					//	.Select(index => new BitChange((uint) index, null, false))
					//	.ToList()
					//	;
					var capacity = (int) (newLength - oldLength);
					var changes = new BitChanges(capacity);
					for (uint index = oldLength; index < oldLength + capacity; index++)
					{
						changes.Add(index, (null, false));
					}
					return changes;
				}
				else
				{
					// Array has been truncated.

					//! Use arrays instead of Linq for performance reasons.
					//return Enumerable
					//	.Range(start: (int) newLength, count: (int) (oldLength - newLength))
					//	.Select(index => new BitChange((uint) index, oldBits[index], null))
					//	.ToList()
					//	;
					var capacity = (int) (oldLength - newLength);
					var changes = new BitChanges(capacity);
					for (uint index = newLength; index < newLength + capacity; index++)
					{
						changes.Add(index, (oldBits[index], null));
					}
					return changes;
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
		{
			//! Creating a new boolean array and set all values to true if needed is faster than the linq query.
			////var booleans = value ? this.Bits.Select(bit => true).ToArray() : new bool[this.Bits.Length];
			var booleans = new bool[this.Bits.Length];
			if (value) for (int i = 0; i < booleans.Length; i++) booleans[i] = true;

			this.TransferValuesFrom(booleans);
		}

		/// <summary>
		/// Transfers the bits from another <see cref="BitCollection"/> to this one.
		/// </summary>
		/// <param name="other"> The other <see cref="BitCollection"/>. </param>
		/// <param name="startPosition"> The bit-based position within the internal collection where to start transferring the other <see cref="BitCollection"/>. </param>
		public void TransferValuesFrom(BitCollection other, uint startPosition = 0)
			=> this.TransferValuesFrom(other.Bits, startPosition);

		/// <summary>
		/// Transfers the <paramref name="bytes"/> to the internal boolean collection.
		/// </summary>
		/// <param name="bytes"> The <see cref="Byte"/> array to apply. </param>
		/// <param name="startPosition"> The byte-based position within the internal collection where to start transferring the <paramref name="bytes"/>. </param>
		public void TransferValuesFrom(byte[] bytes, uint startPosition = 0)
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
				var changes = this.AutomaticallyAdaptSize ? this.Resize_Internal((uint) booleans.Length) : new BitChanges(Math.Max(Math.Min(sizeof(ushort) * 8, booleans.Length), booleans.Length / 4)); // In general assume that a quarter of all bits changed. Only for small collections assume everything changed.

				for (var bitPosition = startPosition; bitPosition < Math.Min(this.Bits.Length, booleans.Length) + startPosition; bitPosition++)
				{
					var currentValue = this.Bits[bitPosition];
					var newValue = booleans[bitPosition - startPosition];

					if (currentValue != newValue)
					{
						changes.Add(bitPosition, (currentValue, newValue));
						this.Bits[bitPosition] = newValue;
					}
				}
				
				this.OnBitsChanged(changes);
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

		/// <inheritdoc />
		public override int GetHashCode()
			// ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode → This class has no immutable properties and therefore no proper implementation of 'GetHashCode' can be made. For equality checks this is not necessary as equality is not determined via hash code but rather via comparing the sequences of the underlying data.
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
		/// Implicit cast operator to <see cref="bool"/> array.
		/// </summary>
		public static implicit operator bool[](BitCollection bitCollection)
		{
			return bitCollection.Bits;
		}

		/// <summary>
		/// Implicit cast operator to <see cref="Byte"/> array.
		/// </summary>
		public static implicit operator byte[](BitCollection bitCollection)
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
		public override string ToString()
			=> this.ToString("N");


		/// <inheritdoc cref="IFormattable.ToString(string,System.IFormatProvider)"/>
		public string ToString(string format)
			=> this.ToString(format, null);

		/// <inheritdoc />
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrWhiteSpace(format)) format = "N";
			//formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;
			
			switch (format.ToUpperInvariant())
			{
				case "S":
				case "LOG":
					return $"{nameof(BitCollection)} (Bits: {this.Length})";

				case "F":
				case "FULL":
					return $"[<{this.GetType().Name}> :: Dynamic: {this.AutomaticallyAdaptSize} | {String.Join(",", this.Bytes)}]";

				case "N":
				case "NORMAL":
				default:
					return $"[<{this.GetType().Name}> :: {String.Join(",", this.Bytes)}]";

				case "BYTE":
					return String.Join(",", this.Bytes);

				case "HEX":
					return BitConverter.ToString(this.Bytes);

				case "BASE64":
					return Convert.ToBase64String(this.Bytes);
			}
		}

		#endregion
	}
}