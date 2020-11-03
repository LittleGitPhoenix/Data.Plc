#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Logging
{
	/// <summary>
	/// <see cref="ILogger"/> implementation which does nothing.
	/// </summary>
	internal class NullLogger : ILogger
	{
		/// <inheritdoc />
		public void Trace(string message, params object[] replacors) { }

		/// <inheritdoc />
		public void Info(string message, params object[] replacors) { }

		/// <inheritdoc />
		public void Error(Exception exception) { }

		/// <inheritdoc />
		public void Error(Exception exception, string message, params object[] replacors) { }

		/// <inheritdoc />
		public void Error(string message, params object[] replacors) { }
	}
}