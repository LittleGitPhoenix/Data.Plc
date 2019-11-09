using System;
using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IEnumDataBlockPlcItemConstructor<TEnum> ConstructEnumPlcItem<TEnum>(this IPlcItemBuilder builder, string identifier = null) where TEnum : Enum
		{
			return new EnumPlcItemConstructor<TEnum>(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IEnumDataBlockPlcItemConstructor<TEnum> where TEnum : Enum
	{
		IEnumPositionPlcItemConstructor<TEnum> AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IEnumPositionPlcItemConstructor<TEnum> where TEnum : Enum
	{
		IEnumLengthPlcItemConstructor<TEnum> AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IEnumLengthPlcItemConstructor<TEnum> where TEnum : Enum
	{
		IEnumPlcItemCreator<TEnum> WithoutInitialValue();
		IEnumPlcItemCreator<TEnum> WithInitialValue(TEnum initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IEnumPlcItemCreator<TEnum> where TEnum : Enum
	{
		EnumPlcItem<TEnum> Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class EnumPlcItemConstructor<TEnum>
		: PlcItemConstructorBase<EnumPlcItem<TEnum>, TEnum>,
			IEnumDataBlockPlcItemConstructor<TEnum>,
			IEnumPositionPlcItemConstructor<TEnum>,
			IEnumLengthPlcItemConstructor<TEnum>,
			IEnumPlcItemCreator<TEnum>
		where TEnum : Enum
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

		public EnumPlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
		}

		#endregion

		#region Methods

		public new IEnumPositionPlcItemConstructor<TEnum> AtDatablock(ushort dataBlock) => (IEnumPositionPlcItemConstructor<TEnum>) base.AtDatablock(dataBlock);

		public new IEnumLengthPlcItemConstructor<TEnum> AtPosition(ushort bytePosition) => (IEnumLengthPlcItemConstructor<TEnum>) base.AtPosition(bytePosition);

#if NET45
		public IEnumPlcItemCreator<TEnum> WithoutInitialValue() => (IEnumPlcItemCreator<TEnum>) base.ForByteAmount((ushort) System.Runtime.InteropServices.Marshal.SizeOf(typeof(TEnum)));
#else
		public IEnumPlcItemCreator<TEnum> WithoutInitialValue() => (IEnumPlcItemCreator<TEnum>) base.ForByteAmount((ushort) System.Runtime.InteropServices.Marshal.SizeOf<TEnum>());
#endif

		public new IEnumPlcItemCreator<TEnum> WithInitialValue(TEnum initialValue) => (IEnumPlcItemCreator<TEnum>) base.WithInitialValue(initialValue);
		
		public override EnumPlcItem<TEnum> Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new EnumPlcItem<TEnum>(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member