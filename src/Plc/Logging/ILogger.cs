#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Logging
{
	/// <summary>
	/// Logger used for internal logging.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Creates a trace log message.
		/// </summary>
		/// <param name="message"> The message itself. </param>
		/// <param name="replacors"> Optional parameters used as replacors in <paramref name="message"/>. </param>
		void Trace(string message, params object[] replacors);

		/// <summary>
		/// Creates an information log message.
		/// </summary>
		/// <param name="message"> The message itself. </param>
		/// <param name="replacors"> Optional parameters used as replacors in <paramref name="message"/>. </param>
		void Info(string message, params object[] replacors);

		/// <summary>
		/// Creates an exception log message.
		/// </summary>
		/// <param name="exception"> The <see cref="Exception"/> to log. </param>
		void Error(Exception exception);

		/// <summary>
		/// Creates an exception log message.
		/// </summary>
		/// <param name="exception"> The <see cref="Exception"/> to log. </param>
		/// <param name="message"> An optional message. </param>
		/// <param name="replacors"> Optional parameters used as replacors in <paramref name="message"/>. </param>
		void Error(Exception exception, string message, params object[] replacors);

		/// <summary>
		/// Creates an exception log message.
		/// </summary>
		/// <param name="message"> An optional message. </param>
		/// <param name="replacors"> Optional parameters used as replacors in <paramref name="message"/>. </param>
		void Error(string message, params object[] replacors);
	}
}