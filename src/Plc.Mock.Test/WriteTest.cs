using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
public sealed class ImplementationWriteTest : ImplementationWriteTest<MockPlc>
{
	public ImplementationWriteTest()
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