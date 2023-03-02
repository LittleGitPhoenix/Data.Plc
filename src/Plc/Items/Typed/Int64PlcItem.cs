#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Items.Typed;

/// <summary>
/// <see cref="IPlcItem"/> for an <see cref="Int64"/>.
/// </summary>
public class Int64PlcItem : TypedBytesPlcItem<Int64>, IDeepCloneable<Int64PlcItem>
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
	/// <param name="position"> The position where the <see cref="Int64"/> within the <see cref="IPlcItem.DataBlock"/> begins. </param>
	public Int64PlcItem(ushort dataBlock, ushort position, long initialValue = default, string identifier = default)
		: base
		(
			PlcItemType.Data,
			dataBlock,
			position,
			byteAmount: sizeof(long),
			false,
			initialValue,
			identifier
		) { }

	#endregion

	#region Methods

	#region Convert

	/// <inheritdoc />
	public override long ConvertFromData(BitCollection data)
	{
		return DataConverter.ToInt64(data, Endianness);
	}

	/// <inheritdoc />
	public override BitCollection ConvertToData(long value)
	{
		var bytes = DataConverter.ToBytes(value, Endianness);
		return new BitCollection(false, bytes);
	}

	#endregion

	#region Clone

	/// <inheritdoc />
	public new Int64PlcItem Clone() => this.Clone(null);

	/// <summary>
	/// Creates a deep copy of the current instance.
	/// </summary>
	/// <param name="identifier"> A new identifier for the clone. </param>
	/// <returns> A new <see cref="Int64PlcItem"/>. </returns>
	public new Int64PlcItem Clone(string identifier)
	{
		return new Int64PlcItem(base.DataBlock, base.Position, this.Value, identifier);
	}

	#endregion

	#endregion
}