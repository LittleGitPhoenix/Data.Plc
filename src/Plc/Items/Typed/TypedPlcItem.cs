#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed;

/// <summary>
/// Abstract class for <see cref="IPlcItem"/>s with a concrete type.
/// </summary>
/// <typeparam name="TValue"> The type of the <see cref="Value"/> of the item. </typeparam>
public abstract class TypedPlcItem<TValue> : PlcItem, IPlcItem<TValue>
{
	#region Delegates / Events
		
	/// <inheritdoc />
	public new event EventHandler<PlcItemChangedEventArgs<TValue>> ValueChanged;
	private void OnValueChanged(TValue oldValue, TValue newValue)
	{
		this.ValueChanged?.Invoke(this, new PlcItemChangedEventArgs<TValue>(this, oldValue, newValue));
	}

	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties
		
	/// <inheritdoc />
	public new TValue Value
	{
		get => _value;
		set
		{
			value = this.ValidateNewValue(value);
			var data = this.ConvertToData(value);
			base.Value.TransferValuesFrom(data);
		}
	}
	private TValue _value;

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="type"> <see cref="IPlcItem.Type"/> </param>
	/// <param name="dataBlock"> <see cref="IPlcItem.DataBlock"/> </param>
	/// <param name="position"> <see cref="IPlcItem.Position"/> </param>
	/// <param name="bitPosition"> <see cref="IPlcItem.BitPosition"/> </param>
	/// <param name="bitAmount"> This is the amount of <c>bits</c> this item handles. </param>
	/// <param name="isFlexible"> Is the values <see cref="BitCollection"/> fixed or will it change size according to its data. </param>
	/// <param name="initialValue"> The initial value of this item. </param>
	/// <param name="identifier"> <see cref="IPlcItem.Identifier"/> </param>
	protected TypedPlcItem
	(
		PlcItemType type,
		ushort dataBlock,
		ushort position,
		BitPosition bitPosition,
		uint bitAmount,
		bool isFlexible,
		TValue initialValue = default,
		string identifier = default
	)
		: base(type, dataBlock, position, bitPosition, bitAmount, isFlexible, identifier)
	{
			
		// Override the initial value if it differs from the default value of its type.
		if (!Object.Equals(initialValue, default(TValue)))
		{
			// Both the field and the property must be set. Later is needed so that the base value is identical.
			_value = this.Value = initialValue;
		}
		//! This has been disabled because it was basically useless. In case of a string based TextPlcItem, this would still fail, because the underlying encoding is still not available until the ctor has been run completely.
		//// Otherwise check if the value is a reference type, in which case NULL is not allowed.
		//else if (!typeof(TValue).IsValueType && initialValue == null)
		//{
		//	try
		//	{
		//		// ReSharper disable once VirtualMemberCallInConstructor → Since this may fail, the catch will then throw a more meaningful exception.
		//		_value = this.ConvertFromData(base.Value);
		//	}
		//	catch
		//	{
		//		throw new NotSupportedException($"The initial value of type '{typeof(TValue)}' may not be NULL. Calling the conversion method from the base class seems to have failed.");
		//	}
		//}

		// React to the value changed event from the underlying item.
		base.ValueChanged += (sender, args) =>
		{
			if (!(sender is BitCollection bitCollection)) throw new ArgumentException($"The event sender should be of type '{nameof(BitCollection)}' but actually is '{sender.GetType()}'.");

			var oldValue = _value;
			_value = this.ConvertFromData(bitCollection);
			this.OnValueChanged(oldValue, _value);
		};
	}

	#endregion

	#region Methods

	#region Validation

	/// <inheritdoc />
	public virtual TValue ValidateNewValue(TValue newValue) => newValue;

	#endregion

	#region Convert

	/// <inheritdoc />
	public abstract TValue ConvertFromData(BitCollection data);
		
	/// <inheritdoc />
	public abstract BitCollection ConvertToData(TValue value);
		
	#endregion

	#endregion
}