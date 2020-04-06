using System;
using System.Linq;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test
{
	[TestFixture]
	public class ConversionTest
	{
		public Data Data { get; }

		public ConversionTest()
		{
			this.Data = new Data();
		}

		[Test]
		public void ConvertText()
		{
			var encoding = System.Text.Encoding.UTF8;
			var target = @"0123456789abcdefghijklmnopqrstuvwxyz,.-#+*~'<>|!""§$%&/()=?{[]}\´`^°äöü²³";
			var targetBytes = encoding.GetBytes(target);
			var asciiItem = new TextPlcItem(dataBlock: Data.Datablock, position: 0, initialValue: target, encoding: encoding);
			Assert.True(targetBytes.SequenceEqual((byte[])((IPlcItem)asciiItem).Value));


			// Check data to value.
			asciiItem = new TextPlcItem(dataBlock: Data.Datablock, position: 0, length: (ushort) targetBytes.Length, encoding);
			((IPlcItem) asciiItem).Value.TransferValuesFrom(targetBytes);
			Assert.True(target.Equals(asciiItem.Value, StringComparison.Ordinal));

			// Check overflow.
			asciiItem = new TextPlcItem(dataBlock: Data.Datablock, position: 0, length: 1, encoding);
			asciiItem.Value = "§"; // This char uses two bytes when encoded in UTF8 and therefore should be cut of because the item has a max length of one byte.
			Assert.True(encoding.GetBytes("§")[0] == ((byte[])((IPlcItem)asciiItem).Value)[0]);
		}
	}
}