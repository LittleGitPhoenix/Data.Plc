#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Reflection;

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// This helper class can be inherited for loading siAccess requirements from embedded resources.
/// </summary>
/// <remarks>
/// <para> Even though it only provides static methods, it must be inherited so that those methods can be accessed. </para>
/// <para> The reason is to not clutter VS IntelliSense with those static methods if they would be public. </para>
/// </remarks>
public abstract class SiAccessPlcEmbeddedRequirementsHelper
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
	#endregion

	#region Methods
		
	/// <summary>
	/// Gets a list of all embedded resources of the passed <paramref name="assembly"/>.
	/// </summary>
	/// <param name="assembly"> The assembly from where to load the resource names. If omitted, then <see cref="Assembly.GetCallingAssembly"/> will be used. </param>
	/// <param name="writeNamesToDebug"> Should all the names be written to the debug output. Default is <see langword="false"/>. </param>
	/// <returns> A collection with all the resource names. </returns>
	protected static ICollection<string> GetAlResourceNames(Assembly? assembly = null, bool writeNamesToDebug = false)
	{
		// Getting the calling assembly if it is not passed to this method must be done directly here and not in another method of this assembly, as the calling assembly would then the current one.
		assembly ??= Assembly.GetCallingAssembly();

		var resources = assembly.GetManifestResourceNames();
		if (writeNamesToDebug) foreach (var resource in resources) System.Diagnostics.Debug.WriteLine(resource);
		return resources;
	}

	/// <summary>
	/// Loads the specified resource as <see cref="System.Text.Encoding.UTF8"/> encoded string from the embedded resources of the passed <paramref name="assembly"/>.
	/// </summary>
	/// <param name="resourceName"> The name of the resource. This must include all folders to the resource file. </param>
	/// <param name="assembly"> An optional assembly from where to load the resource. If omitted, then <see cref="Assembly.GetCallingAssembly"/> will be used. </param>
	/// <returns> The resource as <see cref="System.Text.Encoding.UTF8"/> encoded <see cref="string"/> or <see langword="null"/>. </returns>
	/// <example> ResourceHelper.LoadInternalResourceAsString("Resources\\Interop.IWshRuntimeLibrary.dll"); </example>
	protected static string? LoadInternalResourceAsString(string? resourceName, Assembly? assembly = null)
	{
		// Getting the calling assembly if it is not passed to this method must be done directly here and not in another method of this assembly, as the calling assembly would then the current one.
		assembly ??= Assembly.GetCallingAssembly();

		try
		{
			using var stream = LoadInternalResourceAsStream(resourceName, assembly);
				
			//  Either the file does not exist or it is not marked as embedded resource.
			if (stream is null) return null;

			//  Get byte array from the file from embedded resource.
			using var reader = new System.IO.StreamReader(stream, System.Text.Encoding.UTF8);
			var content = reader.ReadToEnd();
			return content;
		}
		catch (Exception ex)
		{
			if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
			return null;
		}
	}

	/// <summary>
	/// Loads the specified resource from the embedded resources of the passed <paramref name="assembly"/>.
	/// </summary>
	/// <param name="resourceName"> The name of the resource. This must include all folders to the resource file. </param>
	/// <param name="assembly"> An optional assembly from where to load the resource. If omitted, then <see cref="Assembly.GetCallingAssembly"/> will be used. </param>
	/// <returns> The resource as <see cref="System.IO.Stream"/> or <see langword="null"/>. </returns>
	/// <example> ResourceHelper.LoadInternalResourceAsStream("Resources\\Interop.IWshRuntimeLibrary.dll"); </example>
	protected static System.IO.Stream? LoadInternalResourceAsStream(string? resourceName, Assembly? assembly = null)
	{
		if (resourceName is null) return null;
			
		// Getting the calling assembly if it is not passed to this method must be done directly here and not in another method of this assembly, as the calling assembly would then the current one.
		assembly ??= Assembly.GetCallingAssembly();

		try
		{
			var stream = assembly.GetManifestResourceStream(resourceName);
			return stream;
		}
		catch (Exception ex)
		{
			if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
			return null;
		}
	}
		
	#endregion
}