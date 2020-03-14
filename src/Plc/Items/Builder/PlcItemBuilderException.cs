#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items.Builder
{
	/// <summary>
	/// Specialized <see cref="ArgumentNullException"/> used for errors during validation of the <see cref="PlcItemConstructor"/>.
	/// </summary>
	public class PlcItemBuilderException : ArgumentNullException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameterName"> The name of the parameter that failed validation. </param>
		public PlcItemBuilderException(string parameterName) : base(parameterName) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameterName"> The name of the parameter that failed validation. </param>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		public PlcItemBuilderException(string parameterName, string message) : base(parameterName, message) { }
	}
}