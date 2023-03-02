#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Data.Plc.AgLink;

/// <summary>
/// This is a helper class that that provides embedded resources for AGLink.
/// </summary>
internal sealed class DemoAgLinkPlcEmbeddedRequirementsHelper : AgLinkPlcEmbeddedRequirementsHelper
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <summary> The <see cref="Assembly"/> of the this class. </summary>
	private static Assembly CurrentAssembly { get; }

	/// <summary> The names of all embedded resources of this assembly. Obtained via <see cref="AgLinkPlcEmbeddedRequirementsHelper.GetAlResourceNames"/>. </summary>
	private static ICollection<string> ResourceNames { get; }
		
	#endregion

	#region (De)Constructors

	static DemoAgLinkPlcEmbeddedRequirementsHelper()
	{
		CurrentAssembly = Assembly.GetExecutingAssembly();
		ResourceNames = AgLinkPlcEmbeddedRequirementsHelper.GetAlResourceNames(CurrentAssembly);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Loads the embedded unmanaged assembly.
	/// </summary>
	/// <param name="is64BitProcess"> Optional boolean if the 64-bit version of the unmanaged assembly should be loaded. By default this is determined by <see cref="Environment.Is64BitProcess"/>. </param>
	/// <returns> The file name of the unmanaged assembly and its content as a <see cref="Stream"/> reset to <see cref="SeekOrigin.Begin"/>. This stream is not closed and needs to be disposed! </returns>
	internal static (string FileName, Stream? ResourceStream) LoadUnmanagedAssembly(bool? is64BitProcess = null)
	{
		is64BitProcess ??= Environment.Is64BitProcess;
		var currentAssembly = DemoAgLinkPlcEmbeddedRequirementsHelper.CurrentAssembly;
		var resourceNames = DemoAgLinkPlcEmbeddedRequirementsHelper.ResourceNames;
		var unmanagedAssemblyFileName = $"AGLink40{(is64BitProcess.Value ? "_x64" : "")}.dll";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(unmanagedAssemblyFileName.ToLower()));
		var resourceStream = AgLinkPlcEmbeddedRequirementsHelper.LoadInternalResourceAsStream(resourceName, currentAssembly);
		if (resourceStream?.CanSeek ?? false) resourceStream.Seek(0, SeekOrigin.Begin);
		return (unmanagedAssemblyFileName, resourceStream);
	}

	/// <summary>
	/// Loads the embedded license key.
	/// </summary>
	/// <returns> The license key or null. </returns>
	internal static string? LoadLicenseKey()
	{
		var currentAssembly = DemoAgLinkPlcEmbeddedRequirementsHelper.CurrentAssembly;
		var resourceNames = DemoAgLinkPlcEmbeddedRequirementsHelper.ResourceNames;
		var licenseFileName = "AGLink.license";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(licenseFileName.ToLower()));
		var licenseKey = AgLinkPlcEmbeddedRequirementsHelper.LoadInternalResourceAsString(resourceName, currentAssembly);
		return licenseKey;
	}

	/// <summary>
	/// Loads the embedded error file.
	/// </summary>
	/// <returns> The error file as single string or null. </returns>
	internal static string? LoadErrorMapping()
	{
		var currentAssembly = DemoAgLinkPlcEmbeddedRequirementsHelper.CurrentAssembly;
		var resourceNames = DemoAgLinkPlcEmbeddedRequirementsHelper.ResourceNames;
		var errorFileName = "AGLink40_Error.txt";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(errorFileName.ToLower()));
		var errorFileContent = AgLinkPlcEmbeddedRequirementsHelper.LoadInternalResourceAsString(resourceName, currentAssembly);
		return errorFileContent;
	}

	#endregion
}