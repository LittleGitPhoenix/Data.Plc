using System.Net.NetworkInformation;
using Phoenix.Data.Plc.AgLink;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test;

[TestFixture]
public sealed class ImplementationMemoryTest : ImplementationMemoryTest<AgLinkPlc>
{
	private static string Host = "NB0791.lan";

	public ImplementationMemoryTest()
		: base
		(
			new DemoAgLinkPlc(new AgLinkPlcConnectionData(0, ImplementationMemoryTest.Host, 0, 0))
		)
	{ }

	/// <inheritdoc />
	protected override void CheckConnectivity()
	{
		var ping = new Ping();
		PingReply reply = null;
		try
		{
			reply = ping.Send(Host, 1000);
		}
		catch { /* ignore */ }
		if (reply is null || reply.Status != IPStatus.Success) Assert.Ignore($"Establishing a connection to '{base.Plc}' failed. This implementation test will not be executed, because it needs special hardware which seems to be unavailable.");
	}

	/// <summary>
	/// Checks that the <see cref="CancellationTokenSource"/> that is created while reading/writing is properly disposed.
	/// </summary>
	[Test]
	public async Task Check_Memory_Usage()
	{
		Assert.Ignore("This test takes relatively long if the connection is capsuled via vpn. Best to execute it only when the test machine and the plc physically connected to the same network.");
		await base.CheckMemoryUsage(10000, 500);
	}
}