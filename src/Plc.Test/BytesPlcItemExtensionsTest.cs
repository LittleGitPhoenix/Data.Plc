using NUnit.Framework;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test
{
	[TestFixture]
	public class BytesPlcItemExtensionsTest
	{
		[Test]
		[TestCase("0xFF00AB", true)]
		[TestCase("255,0,127", true)]
		public void Check_Set_Value_From_String(string value, bool targetResult)
		{
			// Arrange
			var item = new BytesPlcItem(0, 0, true, new byte[0]);

			// Act
			var result = item.SetValue(value);

			// Assert
			Assert.That(result, Is.EqualTo(targetResult));
		}
	}
}