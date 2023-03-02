#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Phoenix.Data.Plc.Items.Typed;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace Phoenix.Data.Plc.Items.Builder
{
	public static partial class PlcItemBuilderExtension
	{
		public static IBitTypePlcItemConstructor ConstructBitPlcItem(this IPlcItemBuilder builder, string identifier = null)
		{
			return new BitPlcItemConstructor(identifier);
		}
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitTypePlcItemConstructor
	{
		IBitDataBlockPlcItemConstructor ForData();
		IBitPositionPlcItemConstructor ForInput();
		IBitPositionPlcItemConstructor ForOutput();
		IBitPositionPlcItemConstructor ForFlags();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitDataBlockPlcItemConstructor
	{
		IBitPositionPlcItemConstructor AtDatablock(ushort dataBlock);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitPositionPlcItemConstructor
	{
		IBitLengthPlcItemConstructor AtPosition(ushort bytePosition);
		IBitLengthPlcItemConstructor AtPosition(ushort bytePosition, BitPosition bitPosition);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitLengthPlcItemConstructor
	{
		IBitPlcItemCreator AsSet();
		IBitPlcItemCreator AsUnset();
		IBitPlcItemCreator WithValue(bool value);
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public interface IBitPlcItemCreator
	{
		BitPlcItem Build();
	}

	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	public class BitPlcItemConstructor
		: PlcItemConstructorBase<BitPlcItem, bool>,
			IBitTypePlcItemConstructor,
			IBitDataBlockPlcItemConstructor,
			IBitPositionPlcItemConstructor,
			IBitLengthPlcItemConstructor,
			IBitPlcItemCreator
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

		public BitPlcItemConstructor(string identifier)
			: base(identifier)
		{
			// Set default values.
			/*!
			 * Using Marshal.SizeOf for 'bool' will not give the expected result of 1, as marshaling will get the size of the underlying unmanaged type 'BOOL' that has a size of 4.
			 * Same goes for 'char' that should have a size of 2 but its unmanged type 'SBYTE' has a size of 1.
			 * See: docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.unmanagedtype
			 */
			//base.ForBitAmount((ushort)System.Runtime.InteropServices.Marshal.SizeOf<bool>());
			base.ForBitAmount(sizeof(bool));
		}

		#endregion

		#region Methods

		public new IBitDataBlockPlcItemConstructor ForData() => (IBitDataBlockPlcItemConstructor)base.ForData();

		public new IBitPositionPlcItemConstructor ForFlags() => (IBitPositionPlcItemConstructor)base.ForFlags();

		public new IBitPositionPlcItemConstructor ForInput() => (IBitPositionPlcItemConstructor)base.ForInput();

		public new IBitPositionPlcItemConstructor ForOutput() => (IBitPositionPlcItemConstructor)base.ForOutput();

		public new IBitPositionPlcItemConstructor AtDatablock(ushort dataBlock) => (IBitPositionPlcItemConstructor)base.AtDatablock(dataBlock);

		public new IBitLengthPlcItemConstructor AtPosition(ushort bytePosition) => (IBitLengthPlcItemConstructor)base.AtPosition(bytePosition);

		public new IBitLengthPlcItemConstructor AtPosition(ushort bytePosition, BitPosition bitPosition) => (IBitLengthPlcItemConstructor)base.AtPosition(bytePosition, bitPosition);

		public IBitPlcItemCreator AsSet() => (IBitPlcItemCreator)base.WithInitialValue(true);

		public IBitPlcItemCreator AsUnset() => (IBitPlcItemCreator)base.WithInitialValue(false);

		public IBitPlcItemCreator WithValue(bool value) => (IBitPlcItemCreator)base.WithInitialValue(value);

		public override BitPlcItem Build()
		{
			this.Validate();
			// ReSharper disable PossibleInvalidOperationException
			//! The validation method takes care of no value being null.
			return new BitPlcItem(base.Type.Value, base.DataBlock.Value, base.Position.Value, base.BitPosition.Value, base.InitialValue, this.Identifier);
			// ReSharper restore PossibleInvalidOperationException
		}

		#endregion
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member