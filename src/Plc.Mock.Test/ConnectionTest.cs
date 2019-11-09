using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Implementation.Test;

namespace Phoenix.Data.Plc.Mock.Test
{
	[TestClass]
	public sealed class ImplementationConnectionTest : ImplementationConnectionTest<MockPlc>
	{
		public ImplementationConnectionTest()
			: base
			(
				data => new MockPlc
				(
					initialDataBlocks: new Dictionary<ushort, byte[]>()
					{
						{data.Datablock, data.TargetBytes},
					}
				)
			)
		{ }
	}
}