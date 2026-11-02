#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// This is a helper class that that provides embedded resources for siAccess.
/// </summary>
internal sealed class DemoSiAccessPlcResourceHelper : SiAccessPlcEmbeddedRequirementsHelper
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> The <see cref="Assembly"/> of this class. </summary>
	private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

	/// <summary> The names of all embedded resources of this assembly. Obtained via <see cref="SiAccessPlcEmbeddedRequirementsHelper.GetAlResourceNames"/>. </summary>
	private static ICollection<string> ResourceNames { get; } = SiAccessPlcEmbeddedRequirementsHelper.GetAlResourceNames(CurrentAssembly);

	#endregion
	
	#region Methods

	/// <summary>
	/// Loads the embedded unmanaged assembly.
	/// </summary>
	/// <param name="is64BitProcess"> Optional boolean if the 64-bit version of the unmanaged assembly should be loaded. By default, this is determined by <see cref="Environment.Is64BitProcess"/>. </param>
	/// <returns> The file name of the unmanaged assembly and its content as a <see cref="Stream"/> reset to <see cref="SeekOrigin.Begin"/>. This stream is not closed and needs to be disposed! </returns>
	internal static (string FileName, Stream? ResourceStream) LoadUnmanagedAssembly(bool? is64BitProcess = null)
	{
		is64BitProcess ??= Environment.Is64BitProcess;
		var currentAssembly = CurrentAssembly;
		var resourceNames = ResourceNames;
		var unmanagedAssemblyFileName = $"siAccess{(is64BitProcess.Value ? "_x64" : "_x86")}.dll";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(unmanagedAssemblyFileName.ToLower()));
		var resourceStream = SiAccessPlcEmbeddedRequirementsHelper.LoadInternalResourceAsStream(resourceName, currentAssembly);
		if (resourceStream?.CanSeek ?? false) resourceStream.Seek(0, SeekOrigin.Begin);
		return (unmanagedAssemblyFileName, resourceStream);
	}

	/// <summary>
	/// Loads the embedded license key.
	/// </summary>
	/// <returns> The license key or null. </returns>
	internal static string? LoadLicenseKey()
	{
		var currentAssembly = CurrentAssembly;
		var resourceNames = ResourceNames;
		var licenseFileName = "siAccess.license";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(licenseFileName.ToLower()));
		var licenseKey = SiAccessPlcEmbeddedRequirementsHelper.LoadInternalResourceAsString(resourceName, currentAssembly);
		return licenseKey;
	}

	/// <summary>
	/// Loads the embedded error file.
	/// </summary>
	/// <returns> The error file as single string or null. </returns>
	internal static string? LoadErrorMapping()
	{
		var currentAssembly = CurrentAssembly;
		var resourceNames = ResourceNames;
		var errorFileName = "siAccess_Error.txt";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(errorFileName.ToLower()));
		var errorFileContent = SiAccessPlcEmbeddedRequirementsHelper.LoadInternalResourceAsString(resourceName, currentAssembly);
		return errorFileContent;
	}

	#endregion
}