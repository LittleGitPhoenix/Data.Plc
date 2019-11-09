using System;

namespace Phoenix.Data.Plc.Logging
{
	/// <summary>
	/// <see cref="ILogger"/> implementation which does nothing.
	/// </summary>
	internal class NullLogger : ILogger
	{
		/// <inheritdoc />
		public void Trace(string format, params object[] args) { }

		/// <inheritdoc />
		public void Info(string format, params object[] args) { }

		/// <inheritdoc />
		public void Error(Exception exception) { }

		/// <inheritdoc />
		public void Error(Exception exception, string message, params object[] replacors) { }

		/// <inheritdoc />
		public void Error(string message, params object[] replacors) { }
	}
}