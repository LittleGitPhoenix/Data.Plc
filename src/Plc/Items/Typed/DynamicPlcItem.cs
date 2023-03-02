#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed;

/// <summary>
/// A special <see cref="PlcItem"/> that can be used for dynamic data where the length of the data is defined within the first few bytes of the item itself.
/// </summary>
public class DynamicPlcItem<TValue> : IDynamicPlcItem, IPlcItem<TValue>
{
	#region Delegates / Events

	/// <inheritdoc />
	event EventHandler<BitsChangedEventArgs> IPlcItem.ValueChanged
	{
		add => ((IPlcItem)this.FlexiblePlcItem).ValueChanged += value;
		remove => ((IPlcItem)this.FlexiblePlcItem).ValueChanged -= value;
	}

	/// <inheritdoc />
	event EventHandler<PlcItemChangedEventArgs<TValue>> IPlcItem<TValue>.ValueChanged
	{
		add => this.FlexiblePlcItem.ValueChanged += value;
		remove => this.FlexiblePlcItem.ValueChanged -= value;
	}

	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly Func<string, IPlcItem<TValue>> _flexiblePlcItemFactory;


	#endregion

	#region Properties

	#region Implementation of IDynamicPlcItem

	/// <inheritdoc cref="IDynamicPlcItem.LengthLimit"/>
	public uint? LengthLimit
	{
		get
		{
			if (_lengthLimit is null) return null;
			checked
			{
				try
				{
					return _lengthLimit.Value * this.LengthFactor;
				}
				catch (OverflowException)
				{
					//? If the limit overflows the largest number, then what should returned? Should the limit even be applied in such cases?
					//! Currently the limit will be removed if this happens.
					//return uint.MaxValue;
					return null;
				}
			}
		}
		set => _lengthLimit = value;
	}
	private uint? _lengthLimit;

	/// <inheritdoc />
	public byte LengthFactor
	{
		get => Math.Max((byte) 1, _lengthFactor);
		set => _lengthFactor = value;
	}
	private byte _lengthFactor;

	/// <inheritdoc />
	public INumericPlcItem LengthPlcItem { get; }

	/// <inheritdoc />
	IPlcItem IDynamicPlcItem.FlexiblePlcItem => this.FlexiblePlcItem;

	/// <see cref="IDynamicPlcItem.FlexiblePlcItem"/>
	public IPlcItem<TValue> FlexiblePlcItem { get; }

	#endregion

	#region Implementation of IPlcItem

	/// <inheritdoc />
	public string Identifier => this.FlexiblePlcItem.Identifier;

	/// <inheritdoc />
	public string PlcString => this.BuildPlcString();

	/// <inheritdoc />
	public PlcItemType Type => this.FlexiblePlcItem.Type;

	/// <inheritdoc />
	public ushort DataBlock => this.FlexiblePlcItem.DataBlock;

	/// <inheritdoc />
	public ushort Position => this.LengthPlcItem.Position;

	/// <inheritdoc />
	public BitPosition BitPosition => BitPosition.X0;

	/// <inheritdoc />
	/// <remarks> Accessing the value via <see cref="IPlcItem"/> will return the value of the <see cref="FlexiblePlcItem"/>. </remarks>
	BitCollection IPlcItem.Value => ((IPlcItem)this.FlexiblePlcItem).Value;

	/// <inheritdoc />
	/// <remarks> Accessing the value will return the value of the <see cref="FlexiblePlcItem"/>. </remarks>
	public TValue Value
	{
		get => this.FlexiblePlcItem.Value;
		set => this.FlexiblePlcItem.Value = value;
	}

	#endregion

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="lengthPlcItem"> <see cref="IDynamicPlcItem.LengthPlcItem"/> </param>
	/// <param name="flexiblePlcItemFactory"> <see cref="IDynamicPlcItem.FlexiblePlcItem"/> </param>
	/// <param name="lengthFactor"> <see cref="IDynamicPlcItem.LengthFactor"/> </param>
	/// <param name="lengthLimit"> <see cref="IDynamicPlcItem.LengthLimit"/> </param>
	/// <param name="identifier"> <see cref="IPlcItem.Identifier"/> </param>
	protected internal DynamicPlcItem(INumericPlcItem lengthPlcItem, Func<string, IPlcItem<TValue>> flexiblePlcItemFactory, byte lengthFactor, uint? lengthLimit, string identifier = default)
		: this(lengthPlcItem, flexiblePlcItemFactory.Invoke(identifier), lengthFactor, lengthLimit)
	{
		_flexiblePlcItemFactory = flexiblePlcItemFactory;
	}

	private DynamicPlcItem(INumericPlcItem lengthPlcItem, IPlcItem<TValue> flexiblePlcItem, byte lengthFactor, uint? lengthLimit)
	{
		// Save parameters.
		this.LengthFactor = lengthFactor;
		this.LengthLimit = lengthLimit;
		this.LengthPlcItem = lengthPlcItem;
		this.FlexiblePlcItem = flexiblePlcItem;

		// Initialize fields.

		this.LengthPlcItem.ValueChanged += (sender, args) =>
		{
			// Get the new value and change the length of the data item accordingly.
			var newFlexibleLength = DynamicPlcItem<TValue>.GetLengthForFlexibleItem(this.LengthPlcItem, this.LengthFactor, this.LengthLimit);
			((IPlcItem) this.FlexiblePlcItem).Value.Resize(newFlexibleLength * 8);
		};

		this.FlexiblePlcItem.ValueChanged += (sender, args) =>
		{
			// Get the new length and change the value of the length item accordingly.
			var newNumericValue = DynamicPlcItem<TValue>.GetValueForNumericItem(this.FlexiblePlcItem, this.LengthFactor, this.LengthLimit);
			this.LengthPlcItem.Value = newNumericValue;
		};
	}

	#endregion

	#region Methods

	#region Implementation of IEquatable<IPlcItem>

	/// <summary> The default hash method. </summary>
	/// <returns> A hash value for the current object. </returns>
	public override int GetHashCode()
	{
		return this.LengthPlcItem.GetHashCode();
	}

	/// <summary>
	/// Compares this instance to another one.
	/// </summary>
	/// <param name="other"> The other instance to compare to. </param>
	/// <returns> <c>TRUE</c> if this instance equals the <paramref name="other"/> instance, otherwise <c>FALSE</c>. </returns>
	public override bool Equals(object other)
	{
		if (other is IPlcItem equatable) return this.Equals(equatable);
		return false;
	}

	/// <inheritdoc />
	/// <remarks>
	/// <para> Two IDynamicPlcItem are equal, if they both have the same LengthPlcItem. </para>
	/// <para> Using the FlexiblePlcItem for equality is not possible, as the bit-length of each item is part of its hash code. But in case of flexible items this length may get changed. </para>
	/// </remarks>
	public bool Equals(IPlcItem other)
	{
		// 
		if (!(other is IDynamicPlcItem dynamicOther)) return false;
		return this.LengthPlcItem.Equals(dynamicOther.LengthPlcItem);
	}

	#endregion

	#region Implementation of IDeepCloneable<out IPlcItem>

	/// <inheritdoc/>
	/// <returns> A new <see cref="IDynamicPlcItem"/>. </returns>
	IPlcItem IDeepCloneable<IPlcItem>.Clone() => (IPlcItem)this.Clone();

	IPlcItem IPlcItem.Clone(string identifier) => (IPlcItem)this.Clone(identifier);

	/// <summary>
	/// Creates a deep copy of the current instance. 
	/// </summary>
	public IDynamicPlcItem Clone() => this.Clone(null);

	/// <summary>
	/// Creates a deep copy of the current instance. 
	/// </summary>
	/// <param name="identifier"> A new identifier for the clone. </param>
	/// <returns> A new <see cref="IDynamicPlcItem"/>. </returns>
	public IDynamicPlcItem Clone(string identifier)
	{
		return new DynamicPlcItem<TValue>(this.LengthPlcItem.Clone(identifier), _flexiblePlcItemFactory.Invoke(identifier), this.LengthFactor, this.LengthLimit);
	}

	#endregion

	#region Implementation of IPlcItem<TValue>

	/// <inheritdoc />
	public TValue ValidateNewValue(TValue newValue) => this.FlexiblePlcItem.ValidateNewValue(newValue);

	/// <inheritdoc />
	public TValue ConvertFromData(BitCollection data) => this.FlexiblePlcItem.ConvertFromData(data);

	/// <inheritdoc />
	public BitCollection ConvertToData(TValue value) => this.FlexiblePlcItem.ConvertToData(value);

	#endregion

	#region Helper

	/// <summary>
	/// Gets the new length of the <see cref="FlexiblePlcItem"/> based on the data in <see cref="LengthPlcItem"/>.
	/// </summary>
	internal static uint GetLengthForFlexibleItem(INumericPlcItem lengthPlcItem, byte lengthFactor, uint? lengthLimit)
	{
		// Get the length of the length item.
		uint newLength = lengthPlcItem.Value;

		// Apply the length factor.
		checked
		{
			try
			{
				newLength *= lengthFactor;
			}
			catch (OverflowException)
			{
				newLength = uint.MaxValue;
			}
		}

		// Apply the limit.
		return lengthLimit is null ? newLength : Math.Min(newLength, lengthLimit.Value);
	}

	/// <summary>
	/// Gets the new length value for the <see cref="LengthPlcItem"/> based on the data in <see cref="FlexiblePlcItem"/>.
	/// </summary>
	internal static uint GetValueForNumericItem(IPlcItem dataPlcItem, byte lengthFactor, uint? lengthLimit)
	{
		// Get the amount of bytes of the flexible item.
		var newLength = dataPlcItem.Value.ByteLength;

		// The length factor has to be divided from the length of the data.
		newLength /= lengthFactor;
			
		// Apply the limit.
		return lengthLimit is null ? newLength : Math.Min(newLength, lengthLimit.Value);
	}

	private string BuildPlcString()
	{
		var type = this.Type;
		var dataBlock = this.DataBlock;
		var position = this.Position;
		var fixedLength = ((IPlcItem) this.LengthPlcItem).Value.ByteLength;
		var dynamicLength = ((IPlcItem) this.FlexiblePlcItem).Value.ByteLength;
			
		return $"{(type == PlcItemType.Data ? $"DB{dataBlock}" : $"{type}")},B{position},[{fixedLength}+{dynamicLength}]";
	}

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	public override string ToString()
		=> this.ToString("N");

	/// <inheritdoc cref="IFormattable.ToString(string, System.IFormatProvider)"/>
	public string ToString(string format)
		=> this.ToString(format, null);

	/// <inheritdoc />
	public string ToString(string format, IFormatProvider formatProvider)
		=> PlcItem.ToString(this, format);

	#endregion

	#endregion
}