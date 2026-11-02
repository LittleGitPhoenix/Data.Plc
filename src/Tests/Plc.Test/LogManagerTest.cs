using Phoenix.Data.Plc.Logging;

namespace Phoenix.Data.Plc.Test;

[TestFixture]
public class LogManagerTest
{
	[Test]
	public void Check_If_NullLogger_Is_Default()
	{
		// Act
		var logger = LogManager.GetLogger();

		// Assert
		Assert.That(logger, Is.TypeOf<NullLogger>());
	}

	[Test]
	public void Check_If_NullLogger_Is_Applied_If_LogFactory_Is_Set_To_Null()
	{
		// Act
		LogManager.LoggerFactory = Mock.Of<ILogger>;
		LogManager.LoggerFactory = null;
		var logger = LogManager.GetLogger();

		// Assert
		Assert.That(logger, Is.TypeOf<NullLogger>());
	}
		
	/// <summary>
	/// Checks if logging can be enabled.
	/// </summary>
	[Test]
	public void Check_If_Logging_Can_Be_Enabled()
	{
		// Act
		LogManager.LogAllReadAndWriteOperations = false;
		LogManager.LogAllReadAndWriteOperations = true;
			
		// Assert
		Assert.True(LogManager.LogAllReadAndWriteOperations);
	}

	/// <summary>
	/// Checks if logging can be disabled.
	/// </summary>
	[Test]
	public void Check_If_Logging_Can_Be_Disabled()
	{
		// Act
		LogManager.LogAllReadAndWriteOperations = true;
		LogManager.LogAllReadAndWriteOperations = false;
			
		// Assert
		Assert.False(LogManager.LogAllReadAndWriteOperations);
	}
}