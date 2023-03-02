#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder;

public static partial class PlcItemBuilderExtension
{
	public static IDWordDataBlockPlcItemConstructor ConstructDWordPlcItem(this IPlcItemBuilder builder, string identifier = null)
	{
		return new DWordPlcItemConstructor(identifier);
	}
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDWordDataBlockPlcItemConstructor
{
	IDWordPositionPlcItemConstructor AtDatablock(ushort dataBlock);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDWordPositionPlcItemConstructor
{
	IDWordLengthPlcItemConstructor AtPosition(ushort bytePosition);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDWordLengthPlcItemConstructor
{
	IDWordPlcItemCreator WithoutInitialValue();
	IDWordPlcItemCreator WithInitialValue(uint initialValue);
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IDWordPlcItemCreator
{
	DWordPlcItem Build();
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class DWordPlcItemConstructor
	: PlcItemConstructorBase<DWordPlcItem, uint>,
		IDWordDataBlockPlcItemConstructor,
		IDWordPositionPlcItemConstructor,
		IDWordLengthPlcItemConstructor,
		IDWordPlcItemCreator
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	public DWordPlcItemConstructor(string identifier)
		: base(identifier)
	{
		// Set default values.
		base.ForData();
		base.ForByteAmount(sizeof(uint));
	}

	#endregion

	#region Methods

	public new IDWordPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IDWordPositionPlcItemConstructor)base.AtDatablock(dataBlock);

	public new IDWordLengthPlcItemConstructor AtPosition(ushort bytePosition) => (IDWordLengthPlcItemConstructor)base.AtPosition(bytePosition);

	public IDWordPlcItemCreator WithoutInitialValue() => this.WithInitialValue(0);

	public new IDWordPlcItemCreator WithInitialValue(uint initialValue) => (IDWordPlcItemCreator)base.WithInitialValue(initialValue);

	public override DWordPlcItem Build()
	{
		this.Validate();
		// ReSharper disable PossibleInvalidOperationException
		//! The validation method takes care of no value being null.
		return new DWordPlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
		// ReSharper restore PossibleInvalidOperationException
	}

	#endregion
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member