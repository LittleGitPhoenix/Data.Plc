#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;

namespace Phoenix.Data.Plc.Logging
{
	/// <summary>
	/// <see cref="ILogger"/> implementation which logs via <see cref="System.Diagnostics.Trace"/>.
	/// </summary>
	public class TraceLogger : ILogger
	{
		/// <inheritdoc />
		public void Trace(string message, params object[] replacors)
			=> this.Log(nameof(Trace), message, replacors);

		/// <inheritdoc />
		public void Info(string message, params object[] replacors)
			=> this.Log(nameof(Info), message, replacors);

		/// <inheritdoc />
		public void Error(Exception exception)
			=> this.Log(nameof(Error), exception.ToString());

		/// <inheritdoc />
		public void Error(Exception exception, string message, params object[] replacors)
			=> this.Log(nameof(Error), $"{message} ⇒ {exception}", replacors);

		/// <inheritdoc />
		public void Error(string message, params object[] replacors)
			=> this.Log(nameof(Error), message, replacors);

		private void Log(string level, string message, params object[] replacors)
		{
			message = $"{DateTime.Now:HH:mm:ss:ffff} - {level}: {String.Format(message, replacors)}";
			if (String.IsNullOrWhiteSpace(message)) return;

			// Log via trace.
			System.Diagnostics.Trace.WriteLine(message);
		}
	}
}