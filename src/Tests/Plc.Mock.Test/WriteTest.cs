using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
public sealed class WriteTest : WriteTest<MockPlc>
{
	public WriteTest()
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