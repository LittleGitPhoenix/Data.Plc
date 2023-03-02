#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed;

/// <summary>
/// <see cref="IPlcItem"/> for an <see cref="Int16"/>.
/// </summary>
public sealed class Int16PlcItem : TypedBytesPlcItem<Int16>, IDeepCloneable<Int16PlcItem>
{
	#region Delegates / Events
	#endregion

	#region Constants

	internal static DataConverter.Endianness Endianness = DataConverter.Endianness.LittleEndian;

	#endregion

	#region Fields
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	/// <inheritdoc />
	/// <param name="position"> The position where the <see cref="Int16"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
	public Int16PlcItem(ushort dataBlock, ushort position, short initialValue = default, string identifier = default)
		: base
		(
			PlcItemType.Data,
			dataBlock,
			position,
			byteAmount: sizeof(short),
			false,
			initialValue,
			identifier
		) { }

	#endregion

	#region Methods

	#region Convert

	/// <inheritdoc />
	public override short ConvertFromData(BitCollection data)
	{
		return DataConverter.ToInt16(data, Endianness);
	}

	/// <inheritdoc />
	public override BitCollection ConvertToData(short value)
	{
		var bytes = DataConverter.ToBytes(value, Endianness);
		return new BitCollection(false, bytes);
	}

	#endregion

	#region Clone

	/// <inheritdoc />
	public new Int16PlcItem Clone() => this.Clone(null);

	/// <summary>
	/// Creates a deep copy of the current instance.
	/// </summary>
	/// <param name="identifier"> A new identifier for the clone. </param>
	/// <returns> A new <see cref="Int16PlcItem"/>. </returns>
	public new Int16PlcItem Clone(string identifier)
	{
		return new Int16PlcItem(base.DataBlock, base.Position, this.Value, identifier);
	}

	#endregion

	#endregion
}