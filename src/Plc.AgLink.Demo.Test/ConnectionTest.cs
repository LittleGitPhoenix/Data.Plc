using System.Net.NetworkInformation;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test;

[TestFixture]
public sealed class ImplementationConnectionTest : ImplementationConnectionTest<AgLinkPlc>
{
	private static string Host = "PLC1518";

	public ImplementationConnectionTest()
		: base
		(
			new DemoAgLinkPlc(new AgLinkPlcConnectionData(0, ImplementationConnectionTest.Host, 0, 0)),
			new DemoAgLinkPlc(new AgLinkPlcConnectionData(0, ImplementationConnectionTest.Host, 0, 0))
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

	[Test]
	/// <inheritdoc />
	public override void Connect_Twice()
	{
		Assert.Ignore("This test currently fails for the AG-Link implementation. The reason is that the underlying 'IAGLink4' connection seems to be cached per device number (DevNr) and therefore no two connections to one and the same plc is possible.");
	}
}