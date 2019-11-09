using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IWordDataBlockPlcItemConstructor ConstructWordPlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new WordPlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IWordDataBlockPlcItemConstructor
	{
		IWordPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IWordPositionPlcItemConstructor
	{
		IWordLengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IWordLengthPlcItemConstructor
	{
		IWordPlcItemCreator WithOutInitialValue();
		IWordPlcItemCreator WithInitialValue(ushort initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IWordPlcItemCreator
	{
		WordPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class WordPlcItemConstructor
		: PlcItemConstructorBase<WordPlcItem, ushort>,
			IWordDataBlockPlcItemConstructor,
			IWordPositionPlcItemConstructor,
			IWordLengthPlcItemConstructor,
			IWordPlcItemCreator
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

		public WordPlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
			base.ForByteAmount(2);
		}

		#endregion

		#region Methods
		
		public new IWordPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IWordPositionPlcItemConstructor)base.AtDatablock(dataBlock);

		public new IWordLengthPlcItemConstructor AtPosition(ushort bytePosition) => (IWordLengthPlcItemConstructor)base.AtPosition(bytePosition);

		public IWordPlcItemCreator WithOutInitialValue() => this.WithInitialValue(0);

		public new IWordPlcItemCreator WithInitialValue(ushort initialValue) => (IWordPlcItemCreator)base.WithInitialValue(initialValue);
		
		public override WordPlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new WordPlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, base.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member