using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
public sealed class ImplementationConnectionTest : ImplementationConnectionTest<MockPlc>
{
	public ImplementationConnectionTest()
		: base
		(
			data => new MockPlc
			(
				initialDataBlocks: new Dictionary<ushort, byte[]>()
				{
					{data.Datablock, data.TargetBytes},
				}
			)
		)
	{ }
}