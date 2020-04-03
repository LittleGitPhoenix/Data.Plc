using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test
{
	[TestFixture]
	public sealed class ImplementationReadTest : ImplementationReadTest<MockPlc>
	{
		public ImplementationReadTest()
			: base
			(
				data => new MockPlc
				(
					initialDataBlocks: new Dictionary<ushort, byte[]>()
					{
						{data.Datablock, data.TargetBytes},
					}
				)
			) { }
	}
}