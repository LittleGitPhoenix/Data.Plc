using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test;

[TestFixture]
public class BitPlcItemExtensionsTest
{
	[Test]
	[TestCase("True", true)]
	[TestCase("true", true)]
	[TestCase("ok", true)]
	[TestCase("yes", true)]
	[TestCase("1", true)]
	[TestCase("False", false)]
	[TestCase("Test", false)]
	public void Check_Set_Value_From_String(string value, bool target)
	{
		// Arrange
		var item = new BitPlcItem(0, 0, BitPosition.X0, initialValue: !target);

		// Act
		var newValue = item.SetValue(value);

		// Assert
		Assert.That(newValue, Is.EqualTo(target));
	}
}