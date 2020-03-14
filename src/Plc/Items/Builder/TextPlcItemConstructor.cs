#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.Text;
using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static ITextEncodingPlcItemConstructor ConstructTextPlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new TextPlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ITextEncodingPlcItemConstructor
	{
		ITextDataBlockPlcItemConstructor WithEncoding(Encoding encoding);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ITextDataBlockPlcItemConstructor
	{
		ITextPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ITextPositionPlcItemConstructor
	{
		ITextLengthPlcItemConstructor AtPosition(ushort bytePosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ITextLengthPlcItemConstructor
	{
		ITextPlcItemCreator WithLength(ushort length);
		ITextPlcItemCreator WithInitialValue(string initialValue);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface ITextPlcItemCreator
	{
		TextPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	internal class TextPlcItemConstructor
		: PlcItemConstructorBase<TextPlcItem, string>,
			ITextEncodingPlcItemConstructor,
			ITextDataBlockPlcItemConstructor,
			ITextPositionPlcItemConstructor,
			ITextLengthPlcItemConstructor,
			ITextPlcItemCreator
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties
		
		protected Encoding Encoding { get; set; }

		#endregion

		#region (De)Constructors

		public TextPlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			base.ForData();
		}

		#endregion

		#region Methods

		public ITextDataBlockPlcItemConstructor WithEncoding(Encoding encoding)
		{
			this.Encoding = encoding;
			return this;
		}

		public new ITextPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (ITextPositionPlcItemConstructor)base.AtDatablock(dataBlock);

		public new ITextLengthPlcItemConstructor AtPosition(ushort bytePosition) => (ITextLengthPlcItemConstructor)base.AtPosition(bytePosition);

		public ITextPlcItemCreator WithLength(ushort length) => (ITextPlcItemCreator) base.ForByteAmount(length);

		public new ITextPlcItemCreator WithInitialValue(string initialValue) => (ITextPlcItemCreator) base.WithInitialValue(initialValue);

		protected override void Validate()
		{
			if (this.Encoding is null) throw new PlcItemBuilderException(nameof(this.Encoding));
			base.Validate();
		}

		public override TextPlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			if (this.InitialValue is null)
			{
				return new TextPlcItem(base.DataBlock.Value, base.Position.Value, (ushort) base.ByteAmount.Value, this.Encoding, base.Identifier);
			}
			else
			{
				return new TextPlcItem(base.DataBlock.Value, base.Position.Value, base.InitialValue, this.Encoding, false, base.Identifier);
			}
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member