using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test
{
	[TestFixture]
	public sealed class ImplementationReadTest : ImplementationReadTest<DemoAgLinkPlc>
	{
		public ImplementationReadTest()
			: base
			(
				new DemoAgLinkPlc(new AgLinkPlcConnectionData(name: "AGLinkTest@PLC1", ip: "PLC1", rack: 0, slot: 0))
			) { }
	}
}