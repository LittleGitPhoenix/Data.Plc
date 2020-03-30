using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc.Test
{
	[TestClass]
	public class LogManagerTest
	{
		[TestMethod]
		public void Check_If_NullLogger_Is_Default()
		{
			// Act
			var logger = LogManager.GetLogger();

			// Assert
			Assert.IsTrue(logger.GetType() == typeof(NullLogger));
		}

		[TestMethod]
		public void Check_If_NullLogger_Is_Applied_If_LogFactory_Is_Set_To_Null()
		{
			// Act
			LogManager.LoggerFactory = Mock.Of<ILogger>;
			LogManager.LoggerFactory = null;
			var logger = LogManager.GetLogger();

			// Assert
			Assert.IsTrue(logger.GetType() == typeof(NullLogger));
		}

		/// <summary>
		/// Checks if logging is disabled by default.
		/// </summary>
		[TestMethod]
		public void Check_If_Logging_Is_Disabled_By_Default()
		{
			// Assert
			Assert.IsFalse(LogManager.LogAllReadAndWriteOperations);
		}

		/// <summary>
		/// Checks if logging can be enabled.
		/// </summary>
		[TestMethod]
		public void Check_If_Logging_Can_Be_Enabled()
		{
			// Act
			LogManager.LogAllReadAndWriteOperations = true;
			
			// Assert
			Assert.IsTrue(LogManager.LogAllReadAndWriteOperations);
		}
	}
}