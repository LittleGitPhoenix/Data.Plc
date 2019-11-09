using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test
{
	[TestClass]
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