#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items;

// TODO: Implement logic that creates a readable string from the Value.
/// <summary>
/// Untyped interface for plc items.
/// </summary>
/// <remarks> Mostly used for documentation purposes. </remarks>
public interface IPlcItem : IEquatable<IPlcItem>, IFormattable, IDeepCloneable<IPlcItem>
{
	#region Delegates / Events

	/// <summary>
	/// Raised if the <see cref="Value"/> changed.
	/// </summary>
	event EventHandler<BitsChangedEventArgs> ValueChanged;

	#endregion

	#region Properties
		
	/// <summary> A custom name for this item that can be used for identification. </summary>
	string Identifier { get; }

	/// <summary> A string representation of the item based on plc syntax. </summary>
	string PlcString { get; }

	/// <summary> The <see cref="PlcItemType"/> of the item. </summary>
	PlcItemType Type { get; }

	/// <summary> The plc datablock of the item. This is <c>0</c> for all types except <see cref="PlcItemType.Data"/>. </summary>
	ushort DataBlock { get; }

	/// <summary> This is the zero-based byte-position within the corresponding plc region. </summary>
	ushort Position { get; }

	/// <summary>
	/// This is the position of the first <c>bit</c>. For items handling whole bytes this is <c>0</c>.
	/// </summary>
	BitPosition BitPosition { get; }
		
	/// <summary> The value of the plc item. </summary>
	BitCollection Value { get; }

	#endregion

	#region Methods

	/// <inheritdoc cref="IFormattable.ToString(string,System.IFormatProvider)"/>
	/// <remarks> This is using <see cref="System.Globalization.CultureInfo.CurrentCulture"/> as format provider.  </remarks>
	string ToString(string format);

	/// <summary>
	/// Creates a deep copy of the current instance.
	/// </summary>
	/// <param name="identifier"> A new identifier for the clone. </param>
	/// <returns> A new <see cref="IPlcItem"/>. </returns>
	IPlcItem Clone(string identifier);

	#endregion
}
	
/// <summary>
/// Typed interface for <see cref="IPlcItem"/>s.
/// </summary>
/// <typeparam name="TValue"> The type of the <see cref="Value"/> of the item. </typeparam>
public interface IPlcItem<TValue> : IPlcItem
{
	/// <summary>
	/// Raised if the <see cref="Value"/> changed.
	/// </summary>
	new event EventHandler<PlcItemChangedEventArgs<TValue>> ValueChanged;

	/// <summary> The value of the plc item. </summary>
	new TValue Value { get; set; }

	/// <summary>
	/// Called whenever <see cref="Value"/> will get changed.
	/// </summary>
	/// <param name="newValue"> The new value that should be applied. </param>
	/// <returns> The new value that will be applied. </returns>
	/// <remarks> This should be used for reference types that shouldn't allow <c>Null</c> as a valid value. </remarks>
	TValue ValidateNewValue(TValue newValue);

	/// <summary>
	/// Converts the <see cref="IPlcItem.Value"/> into <typeparamref name="TValue"/>.
	/// </summary>
	/// <param name="data"> The <see cref="BitCollection"/> to convert to <typeparamref name="TValue"/>. </param>
	/// <returns> <typeparamref name="TValue"/> </returns>
	/// <remarks> Used when <c>Get</c>ting the value. </remarks>
	TValue ConvertFromData(BitCollection data);

	/// <summary>
	/// Converts the <typeparamref name="TValue"/> into <see cref="IPlcItem.Value"/>.
	/// </summary>
	/// <param name="value"> The <typeparamref name="TValue"/> to convert into a <see cref="BitCollection"/>. </param>
	/// <returns> <see cref="BitCollection"/> </returns>
	/// <remarks> Used when <c>Set</c>ting the value. </remarks>
	BitCollection ConvertToData(TValue value);
}