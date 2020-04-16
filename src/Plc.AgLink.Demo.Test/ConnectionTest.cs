using System;
using System.Collections.Generic;
using NUnit.Framework;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test
{
	[TestFixture]
	public sealed class ImplementationConnectionTest : ImplementationConnectionTest<AgLinkPlc>
	{
		public ImplementationConnectionTest()
			: base
			(
				new AgLinkPlc(new AgLinkPlcConnectionData(0, "PLC2", 0, 0))
			)
		{ }
	}
}