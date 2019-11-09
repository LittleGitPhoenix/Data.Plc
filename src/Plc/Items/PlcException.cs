using System;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// Different types of plc exceptions.
	/// </summary>
	public enum PlcExceptionType
	{
		/// <summary> The plc is not connected. </summary>
		NotConnected,
		/// <summary> An error occured (typically while reading or writing) that cannot be fixed by simply reconnecting to the plc. </summary>
		UnrecoverableConnection,
		/// <summary> An error occured during a read operation. </summary>
		ReadError,
		/// <summary> An error occured during a write operation. </summary>
		WriteError,
	}

	/// <summary>
	/// Custom exception class used by the plc assembly.
	/// </summary>
	public class PlcException : Exception
	{
		/// <summary> The concrete <see cref="PlcExceptionType"/>. </summary>
		public PlcExceptionType ExceptionType { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="exceptionType"> The concrete <see cref="PlcExceptionType"/>. </param>
		public PlcException(PlcExceptionType exceptionType)
		{
			this.ExceptionType = exceptionType;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="exceptionType"> The concrete <see cref="PlcExceptionType"/>. </param>
		/// <param name="message"> A custom exception message. </param>
		public PlcException(PlcExceptionType exceptionType, string message) : base(message)
		{
			this.ExceptionType = exceptionType;
		}
	}
}