#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Logging
{
	/// <summary>
	/// Manager for ILoggers. Used to create new ILoggers, and set up how ILoggers are created
	/// </summary>
	public static class LogManager
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		
		private static readonly Func<ILogger> NullLoggerFactory = () => new NullLogger();

		#endregion

		#region Properties

		/// <summary>
		/// Should every read / write operation be logged.
		/// </summary>
		/// <remarks> Default is <c>False</c> because this may be a costly operation. </remarks>
		public static bool LogAllReadAndWriteOperations { get; set; }

		/// <summary>
		/// Gets or sets the factory used to create new <see cref="ILogger"/>s.
		/// </summary>
		public static Func<ILogger> LoggerFactory
		{
			get => _loggerFactory ?? LogManager.NullLoggerFactory;
			set => _loggerFactory = value;
		}
		private static Func<ILogger> _loggerFactory;

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Static constructor
		/// </summary>
		static LogManager()
		{
			LogAllReadAndWriteOperations = false;
		}

		#endregion

		#region Methods
		
		/// <summary>
		/// Get a new <see cref="ILogger"/>.
		/// </summary>
		/// <returns> A new <see cref="ILogger"/> instance. </returns>
		public static ILogger GetLogger()
		{
			return LogManager.LoggerFactory.Invoke();
		}

		#endregion
	}
}