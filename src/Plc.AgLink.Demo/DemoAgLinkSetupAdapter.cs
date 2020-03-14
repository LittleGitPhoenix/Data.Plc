#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Accon.AGLink;

namespace Phoenix.Data.Plc.AgLink
{
	/// <inheritdoc />
	internal class DemoAgLinkSetupAdapter : IAgLinkSetupAdapter
	{
		#region Properties

		/// <inheritdoc />
		public Assembly SourceAssembly { get; } = typeof(DemoAgLinkSetupAdapter).Assembly;

		/// <inheritdoc />
		public string ResourcePath { get; } = "Resources.AgLink";
		
		/// <inheritdoc />
		public string AssemblyFileName { get; } = $"AGLink40{(Environment.Is64BitProcess ? "_x64" : "")}.dll";

		/// <inheritdoc />
		public string ParameterFileName { get; } = "AGLink40CfgDev0000.xml";

		/// <inheritdoc />
		public string LicenseKey { get; } = null;

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Activate()
		{
			if (!String.IsNullOrWhiteSpace(this.LicenseKey)) AGL4.Activate(this.LicenseKey);
		}

		/// <inheritdoc />
		public void ChangeDirectoryOfParameterFile(string pathToParameterFile)
		{
			AGL4.SetParaPath(pathToParameterFile);
		}

		/// <inheritdoc />
		public void ForceSynchronousFunctionCalls()
		{
			AGL4.ReturnJobNr(false);
		}
		
		#endregion
	}
}