using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
public sealed class ImplementationMemoryTest : ImplementationMemoryTest<MockPlc>
{
	public ImplementationMemoryTest()
		: base
		(
			data => new MockPlc()
		)
	{ }

	/// <summary>
	/// Checks that the <see cref="CancellationTokenSource"/> that is created while reading/writing is properly disposed.
	/// </summary>
	[Test]
	public async Task Check_Memory_Usage()
	{
		await base.CheckMemoryUsage(100000, 500);
	}
}