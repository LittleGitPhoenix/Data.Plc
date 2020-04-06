using System;
using NUnit.Framework;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Test
{
	[TestFixture]
	public class ChangeComparerTest
	{
		//[Test]
		//[DataRow(new object(), new object())]
		//public void ChangeComparer_ReferenceQuality(object x, object y)
		//{
		//	var comparer = new ChangeComparer();

		//	var object1 = new object();
		//	var object2 = object1;
			
		//	Assert.True(comparer.Equals(x, y));
		//}

		[Test]
		public void ChangeComparer_ReferenceEquality()
		{
			var comparer = new ChangeComparer();

			var object1 = new object();
			var object2 = object1;
			var object3 = new object();
			
			Assert.True(comparer.Equals(object1, object2));
			Assert.False(comparer.Equals(object1, object3));
		}

		[Test]
		public void ChangeComparer_NullEquality()
		{
			var comparer = new ChangeComparer();

			var object1 = new object();
			object object2 = null;
			
			Assert.False(comparer.Equals(object1, object2));
			Assert.False(comparer.Equals(object2, object1));
		}

		[Test]
		public void ChangeComparer_BooleanEquality()
		{
			var comparer = new ChangeComparer();

			var object1 = true;
			var object2 = true;
			var object3 = false;

			Assert.True(comparer.Equals(object1, object2));
			Assert.False(comparer.Equals(object1, object3));
		}
	}
}