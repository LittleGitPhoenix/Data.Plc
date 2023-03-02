#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using Accon.AGLink;

namespace Phoenix.Data.Plc.AgLink;

/// <summary>
/// Special <see cref="AgLinkPlc"/> that uses the demo version of <see cref="AGLink4"/>.
/// </summary>
public sealed class DemoAgLinkPlc : AgLinkPlc
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	static DemoAgLinkPlc()
	{
		// Extract the unmanaged assembly.
		var (unmanagedAgLinkAssemblyFile, resourceStream) = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadUnmanagedAssembly();
		AgLinkRequirementsHelper.CopyUnmanagedAssemblies(unmanagedAgLinkAssemblyFile, resourceStream);

		// Try to get the license key and activate AGLink.
		var licenseKey = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadLicenseKey();
		AgLinkRequirementsHelper.ActivateAgLink(licenseKey);

		// Override error mapping from base class.
		var errorFileContent = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadErrorMapping();
		AgLinkRequirementsHelper.OverrideErrorMapping(new AgLinkErrorMapping(errorFileContent));
	}
		
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="connectionData"> <see cref="IAgLinkPlcConnectionData"/> </param>
	public DemoAgLinkPlc(IAgLinkPlcConnectionData connectionData)
		: base(connectionData) { }

	#endregion

	#region Methods
	#endregion
}