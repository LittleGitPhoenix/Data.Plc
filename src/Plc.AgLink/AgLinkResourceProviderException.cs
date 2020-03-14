#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Defines different reasons why an <see cref="AgLinkResourceProviderException"/> has been thrown.
	/// </summary>
	public enum AgLinkResourceProviderExceptionReason
	{
		/// <summary> The <c>AGLink</c> assembly could not be found in the embedded resources. </summary>
		AssemblyFileNotFound,
		/// <summary> The <c>AGLink</c> assembly could not be saved to the local file system. </summary>
		AssemblyFileNotSaved,
		/// <summary> The <c>AGLink</c> parameter file could not be found in the embedded resources. </summary>
		ParameterFileNotFound,
		/// <summary> The <c>AGLink</c> parameter file could not be saved to the local file system. </summary>
		ParameterFileNotSaved,
	}

	/// <summary>
	/// Exception used by <see cref="AgLinkResourceProvider"/>.
	/// </summary>
	public class AgLinkResourceProviderException : Exception
	{
		/// <summary> The reason of this exception. </summary>
		public AgLinkResourceProviderExceptionReason Reason { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reason"> <see cref="Reason"/> </param>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		public AgLinkResourceProviderException(AgLinkResourceProviderExceptionReason reason, string message) : base(message)
		{
			this.Reason = reason;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="reason"> <see cref="Reason"/> </param>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		/// <param name="innerException"> <see cref="Exception.InnerException"/> </param>
		public AgLinkResourceProviderException(AgLinkResourceProviderExceptionReason reason, string message, Exception innerException) : base(message, innerException)
		{
			this.Reason = reason;
		}
	}
}