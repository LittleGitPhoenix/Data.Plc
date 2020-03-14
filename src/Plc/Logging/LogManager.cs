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
		
		private static readonly Func<string, ILogger> NullLoggerFactory = name => new NullLogger();

		#endregion

		#region Properties

		/// <summary>
		/// Is logging enabled or not.
		/// </summary>
		public static bool IsEnabled => LogManager.LoggerFactory != LogManager.NullLoggerFactory;

		/// <summary>
		/// Gets or sets the factory used to create new <see cref="ILogger"/>s.
		/// </summary>
		public static Func<string, ILogger> LoggerFactory
		{
			get => _loggerFactory ?? LogManager.NullLoggerFactory;
			set => _loggerFactory = value;
		}
		private static Func<string, ILogger> _loggerFactory;

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Static constructor
		/// </summary>
		static LogManager() { }

		#endregion

		#region Methods

		/// <summary>
		/// Gets an <see cref="ILogger"/> with the name of the class that invoked this method.
		/// </summary>
		/// <returns> A new <see cref="ILogger"/> instance. </returns>
		public static ILogger GetCurrentClassLogger()
			=> LogManager.GetLogger(LogManager.GetClassFullName());

		/// <summary>
		/// Get a new <see cref="ILogger"/> with the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name"> The name of the new <see cref="ILogger"/>. </param>
		/// <returns> A new <see cref="ILogger"/> instance. </returns>
		public static ILogger GetLogger(string name)
		{
			return LogManager.LoggerFactory.Invoke(name);
		}

		/// <summary> Gets the fully qualified name of the class invoking the LogManager, including the namespace but not the assembly. </summary>
		private static string GetClassFullName()
		{
			string className;
			Type declaringType;
			int framesToSkip = 2;

			do
			{
				var frame = new System.Diagnostics.StackFrame(framesToSkip, false);

				var method = frame.GetMethod();
				declaringType = method.DeclaringType;
				if (declaringType == null)
				{
					className = method.Name;
					break;
				}

				framesToSkip++;
				className = declaringType.FullName;
			} while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

			return className;
		}

		#endregion
	}
}