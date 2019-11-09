using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc.Test
{
	[TestClass]
	public class ChangeComparerTest
	{
		//[TestMethod]
		//[DataRow(new object(), new object())]
		//public void ChangeComparer_ReferenceQuality(object x, object y)
		//{
		//	var comparer = new ChangeComparer();

		//	var object1 = new object();
		//	var object2 = object1;
			
		//	Assert.IsTrue(comparer.Equals(x, y));
		//}

		[TestMethod]
		public void ChangeComparer_ReferenceEquality()
		{
			var comparer = new ChangeComparer();

			var object1 = new object();
			var object2 = object1;
			var object3 = new object();
			
			Assert.IsTrue(comparer.Equals(object1, object2));
			Assert.IsFalse(comparer.Equals(object1, object3));
		}

		[TestMethod]
		public void ChangeComparer_NullEquality()
		{
			var comparer = new ChangeComparer();

			var object1 = new object();
			object object2 = null;
			
			Assert.IsFalse(comparer.Equals(object1, object2));
			Assert.IsFalse(comparer.Equals(object2, object1));
		}

		[TestMethod]
		public void ChangeComparer_BooleanEquality()
		{
			var comparer = new ChangeComparer();

			var object1 = true;
			var object2 = true;
			var object3 = false;

			Assert.IsTrue(comparer.Equals(object1, object2));
			Assert.IsFalse(comparer.Equals(object1, object3));
		}
	}
}