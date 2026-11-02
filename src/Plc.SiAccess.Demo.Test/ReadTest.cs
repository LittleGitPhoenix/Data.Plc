using System.Net.NetworkInformation;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.SiAccess.Test;

[TestFixture]
public sealed class ImplementationReadTest : ImplementationReadTest<SiAccessPlc>
{
	private const string HostEnvironmentVariableName = "PLC_HOST";

	public ImplementationReadTest()
		: base
		(
			new DemoSiAccessPlc(new SiAccessPlcConnectionData(0, GetHost(HostEnvironmentVariableName), 0, 0))
		) { }

	/// <inheritdoc />
	protected override void CheckConnectivity()
	{
		var ping = new Ping();
		PingReply reply = null;
		try
		{
			reply = ping.Send(GetHost(HostEnvironmentVariableName), 1000);
		}
		catch { /* ignore */ }
		if (reply is null || reply.Status != IPStatus.Success) Assert.Ignore($"Establishing a connection to '{base.Plc}' failed. This implementation test will not be executed, because it needs special hardware which seems to be unavailable.");
	}
}