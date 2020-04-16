using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.AgLink.Test
{
	[TestFixture]
	public sealed class ImplementationWriteTest : ImplementationWriteTest<AgLinkPlc>
	{
		public ImplementationWriteTest()
			: base
			(
				new AgLinkPlc(new AgLinkPlcConnectionData(0, "PLC2", 0, 0))
			)
		{ }
	}
}