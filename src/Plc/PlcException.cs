#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// Different types of <see cref="ReadOrWritePlcException"/>s.
	/// </summary>
	public enum PlcExceptionType
	{
		/// <summary> An error occurred during a read operation. </summary>
		ReadError,
		/// <summary> An error occurred during a write operation. </summary>
		WriteError,
	}

	/// <summary>
	/// Base class for all exceptions used by the plc assembly.
	/// </summary>
	public class PlcException : Exception
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> A custom exception message. </param>
		public PlcException(string message) : base(message) { }
	}

	/// <summary>
	/// Special <see cref="PlcException"/> used by the plc assembly in cases the plc is not connected.
	/// </summary>
	public class NotConnectedPlcException : PlcException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> A custom exception message. </param>
		public NotConnectedPlcException(string message) : base(message) { }
	}

	/// <summary>
	/// Special <see cref="PlcException"/> used by the plc assembly in cases an error occurred while reading or writing.
	/// </summary>
	public class ReadOrWritePlcException : PlcException
	{
		/// <summary> The concrete <see cref="PlcExceptionType"/>. </summary>
		public PlcExceptionType ExceptionType { get; }

		/// <summary> All items where reading/writing succeeded. </summary>
		public ICollection<IPlcItem> ValidItems { get; }

		/// <summary> All items where reading/writing failed together with an error message. </summary>
		public ICollection<(IPlcItem FailedItem, string ErrorMessage)> FailedItems { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="exceptionType"> The concrete <see cref="PlcExceptionType"/>. </param>
		/// <param name="validItems"> <see cref="ValidItems"/> </param>
		/// <param name="failedItems"> <see cref="FailedItems"/> </param>
		public ReadOrWritePlcException(PlcExceptionType exceptionType, ICollection<IPlcItem> validItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)> failedItems)
			: this(exceptionType, validItems, failedItems, message: null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="exceptionType"> The concrete <see cref="PlcExceptionType"/>. </param>
		/// <param name="validItems"> <see cref="ValidItems"/> </param>
		/// <param name="failedItems"> <see cref="FailedItems"/> </param>
		/// <param name="message"> A custom exception message. </param>
		public ReadOrWritePlcException(PlcExceptionType exceptionType, ICollection<IPlcItem> validItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)> failedItems, string message)
			: base(message)
		{
			this.ExceptionType = exceptionType;
			this.ValidItems = validItems;
			this.FailedItems = failedItems;
		}
	}

	/// <summary>
	/// Special <see cref="ReadOrWritePlcException"/> used by the plc assembly in cases an error occurred while reading.
	/// </summary>
	public class ReadPlcException : ReadOrWritePlcException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="validItems"> <see cref="ReadOrWritePlcException.ValidItems"/> </param>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		public ReadPlcException(ICollection<IPlcItem> validItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)> failedItems)
			: this(validItems, failedItems, message: null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="validItems"> <see cref="ReadOrWritePlcException.ValidItems"/> </param>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		/// <param name="message"> A custom exception message. </param>
		public ReadPlcException(ICollection<IPlcItem> validItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)> failedItems, string message)
			: base(PlcExceptionType.ReadError, validItems, failedItems, message) { }
	}

	/// <summary>
	/// Special <see cref="ReadPlcException"/> used by the plc assembly in cases the underlying <see cref="IPlc"/> instance is already disposed.
	/// </summary>
	/// <remarks> <see cref="ReadOrWritePlcException.ValidItems"/> will always be an empty collection. </remarks>
	public class DisposedReadPlcException : ReadPlcException
	{
		private const string ItemMessage = "Plc instance has been disposed.";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		public DisposedReadPlcException(ICollection<IPlcItem> failedItems)
			: this(failedItems, message: null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		/// <param name="message"> A custom exception message. </param>
		public DisposedReadPlcException(ICollection<IPlcItem> failedItems, string message)
			: base(new IPlcItem[0], failedItems.Select(item => (item, ItemMessage)).ToArray(), message) { }
	}

	/// <summary>
	/// Special <see cref="ReadOrWritePlcException"/> used by the plc assembly in cases an error occurred while writing.
	/// </summary>
	public class WritePlcException : ReadOrWritePlcException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="validItems"> <see cref="ReadOrWritePlcException.ValidItems"/> </param>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		public WritePlcException(ICollection<IPlcItem> validItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)> failedItems)
			: this(validItems, failedItems, message: null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="validItems"> <see cref="ReadOrWritePlcException.ValidItems"/> </param>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		/// <param name="message"> A custom exception message. </param>
		public WritePlcException(ICollection<IPlcItem> validItems, ICollection<(IPlcItem FailedItem, string ErrorMessage)> failedItems, string message)
			: base(PlcExceptionType.WriteError, validItems, failedItems, message) { }
	}

	/// <summary>
	/// Special <see cref="WritePlcException"/> used by the plc assembly in cases the underlying <see cref="IPlc"/> instance is already disposed.
	/// </summary>
	/// <remarks> <see cref="ReadOrWritePlcException.ValidItems"/> will always be an empty collection. </remarks>
	public class DisposedWritePlcException : WritePlcException
	{
		private const string ItemMessage = "Plc instance has been disposed.";

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		public DisposedWritePlcException(ICollection<IPlcItem> failedItems)
			: this(failedItems, message: null) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="failedItems"> <see cref="ReadOrWritePlcException.FailedItems"/> </param>
		/// <param name="message"> A custom exception message. </param>
		public DisposedWritePlcException(ICollection<IPlcItem> failedItems, string message)
			: base(new IPlcItem[0], failedItems.Select(item => (item, ItemMessage)).ToArray(), message) { }
	}
}