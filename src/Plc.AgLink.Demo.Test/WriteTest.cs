using System.Net.NetworkInformation;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test;

[TestFixture]
public sealed class ImplementationWriteTest : ImplementationWriteTest<AgLinkPlc>
{
	private static string Host = "NB0791.lan";

	public ImplementationWriteTest()
		: base
		(
			new DemoAgLinkPlc(new AgLinkPlcConnectionData(0, ImplementationWriteTest.Host, 0, 0))
		) { }


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
}