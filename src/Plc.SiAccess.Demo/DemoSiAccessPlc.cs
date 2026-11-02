#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// Special <see cref="SiAccessPlc"/> that uses the demo version of <see cref="Softing.SiAccess"/>.
/// </summary>
public sealed class DemoSiAccessPlc : SiAccessPlc
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

	static DemoSiAccessPlc()
	{
		// Extract the unmanaged assembly.
		var (unmanagedSiAccessAssemblyFile, resourceStream) = DemoSiAccessPlcResourceHelper.LoadUnmanagedAssembly();
		SiAccessRequirementsHelper.CopyUnmanagedAssemblies(unmanagedSiAccessAssemblyFile, resourceStream);

		// Try to get the license key and activate siAccess.
		var licenseKey = DemoSiAccessPlcResourceHelper.LoadLicenseKey();
		SiAccessRequirementsHelper.Activate(licenseKey);

		// Override error mapping from base class.
		var errorFileContent = DemoSiAccessPlcResourceHelper.LoadErrorMapping();
		SiAccessRequirementsHelper.OverrideErrorMapping(new SiAccessErrorMapping(errorFileContent));
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="connectionData"> <see cref="ISiAccessPlcConnectionData"/> </param>
	public DemoSiAccessPlc(ISiAccessPlcConnectionData connectionData)
		: base(connectionData) { }

	#endregion

	#region Methods
	#endregion
}