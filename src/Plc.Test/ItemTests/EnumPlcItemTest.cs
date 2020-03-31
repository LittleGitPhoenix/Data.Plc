using System;
using Phoenix.Data.Plc.Items.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Phoenix.Data.Plc.Items;
using Phoenix.Data.Plc.Items.Typed;

namespace Phoenix.Data.Plc.Test.ItemTests
{
	[TestClass]
	[TestCategory("Item Test")]
	public sealed class EnumPlcItemTest
	{
		public enum TestEnumeration : byte
		{
			Value1 = 10,
			Value2 = 20,
			Value3 = 30,
			Value4 = 40,
		}

		public enum UnknownEnumeration : byte
		{
			Unknown = 0
		}

		public enum UndefinedEnumeration : byte
		{
			Undefined = 0
		}

		[TestMethod]
		public void Check_ItemBuilder()
		{
			EnumPlcItem<TestEnumeration> item = new Items.Builder.PlcItemBuilder()
				.ConstructEnumPlcItem<TestEnumeration>("Enumeration")
				.AtDatablock(0)
				.AtPosition(0)
				.WithInitialValue(TestEnumeration.Value1)
				.Build()
				;
			Assert.AreEqual((uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(TestEnumeration).GetEnumUnderlyingType()), ((IPlcItem)item).Value.ByteLength);

			System.Diagnostics.Debug.WriteLine(item.ToString());
		}

		[TestMethod]
		public void Check_Clone()
		{
			var item = new EnumPlcItem<TestEnumeration>(dataBlock: 0, position: 0);
			var clone = item.Clone();

			// Check equality (Equals is overriden).
			Assert.AreEqual(item, clone);
			
			// Check the value.
			Assert.IsTrue(item.Value == clone.Value);
			
			// Check if both items are different references.
			Assert.IsFalse(Object.ReferenceEquals(item, clone));
		}

		[TestMethod]
		public void Check_ToData()
		{
			var item = new EnumPlcItem<TestEnumeration>(dataBlock: 0, position: 0);

			// Check: Enumeration → Data
			var target = TestEnumeration.Value4;
			item.Value = target;
			Assert.AreEqual(target, item.Value);
			Assert.AreEqual((byte) target, ((byte[]) ((IPlcItem) item).Value)[0]);
		}

		[TestMethod]
		public void Check_FromData()
		{
			var item = new EnumPlcItem<TestEnumeration>(dataBlock: 0, position: (ushort)0);

			// Check: Data → Enumeration
			var target = TestEnumeration.Value3;
			((IPlcItem) item).Value.TransferValuesFrom(new byte[] {(Byte) target});
			Assert.AreEqual(target, item.Value);
			Assert.AreEqual((byte) target, ((byte[]) ((IPlcItem) item).Value)[0]);
		}

		[TestMethod]
		public void Check_NotDefined()
		{
			var item = new EnumPlcItem<TestEnumeration>(dataBlock: 0, position: (ushort)0);

			// Check: Undefined
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => ((IPlcItem)item).Value.TransferValuesFrom(new byte[] { byte.MaxValue }));
		}

		[TestMethod]
		public void Check_Unknown()
		{
			var item = new EnumPlcItem<UnknownEnumeration>(dataBlock: 0, position: (ushort)0);

			// Check: Data → Enumeration
			((IPlcItem) item).Value.TransferValuesFrom(new byte[] {byte.MaxValue});
			Assert.AreEqual(UnknownEnumeration.Unknown, item.Value);
			Assert.AreEqual(byte.MaxValue, ((byte[]) ((IPlcItem) item).Value)[0]);
		}

		[TestMethod]
		public void Check_Undefined()
		{
			var item = new EnumPlcItem<UndefinedEnumeration>(dataBlock: 0, position: (ushort)0);

			// Check: Data → Enumeration
			((IPlcItem) item).Value.TransferValuesFrom(new byte[] {byte.MinValue});
			Assert.AreEqual(UndefinedEnumeration.Undefined, item.Value);
			Assert.AreEqual(byte.MinValue, ((byte[]) ((IPlcItem) item).Value)[0]);
		}
	}
}