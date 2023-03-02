using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
public sealed class ImplementationReadTest : ImplementationReadTest<MockPlc>
{
	public ImplementationReadTest()
		: base
		(
			data => new MockPlc
			(
				initialDataBlocks: new Dictionary<ushort, byte[]>()
				{
					{data.Datablock, data.TargetBytes},
				}
			)
		) { }

	#region Overrides of ImplementationReadTest<MockPlc>

	/// <inheritdoc />
	public override void ReadUndefinedDatablock()
	{
		Assert.Ignore("The MockPlc doesn't have any undefined datablock, as they will be created upon first interaction.");
	}

	#endregion
}