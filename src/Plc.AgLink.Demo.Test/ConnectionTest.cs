using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test
{
	[TestClass]
	public sealed class ImplementationConnectionTest : ImplementationConnectionTest<DemoAgLinkPlc>
	{
		public ImplementationConnectionTest()
			: base
			(
				new DemoAgLinkPlc(new AgLinkPlcConnectionData(name: "AGLinkTest@PLC1", ip: "PLC1", rack: 0, slot: 0))
			)
		{ }
	}
}