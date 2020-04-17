using System;
using System.Net.NetworkInformation;
using NUnit.Framework;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test
{
	[TestFixture]
	public sealed class ImplementationReadTest : ImplementationReadTest<AgLinkPlc>
	{
		private static string Host = "PLC2";

		public ImplementationReadTest()
			: base
			(
				new AgLinkPlc(new AgLinkPlcConnectionData(0, ImplementationReadTest.Host, 0, 0))
			) { }

		/// <inheritdoc />
		protected override void CheckConnectivity()
		{
			var ping = new Ping();
			var reply = ping.Send(ImplementationReadTest.Host, 500);
			if (reply is null || reply.Status != IPStatus.Success) Assert.Ignore($"Establishing a connection to '{base.Plc}' failed. This implementation test will not be executed, because it needs special hardware which seems to be currently unavailable.");
		}
	}
}