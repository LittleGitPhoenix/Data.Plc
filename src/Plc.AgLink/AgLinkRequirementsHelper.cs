#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Diagnostics;
using Accon.AGLink;

#nullable enable

namespace Phoenix.Data.Plc.AgLink;

/// <summary>
/// This helper class provides static methods for providing requirements to <see cref="Accon.AGLink"/>.
/// </summary>
public abstract class AgLinkRequirementsHelper
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
		
	/// <summary> The <see cref="FileInfo"/> instance of the unmanaged AGLink assembly. Kept to later delete the file upon process exit. </summary>
	private static FileInfo? _unmanagedAgLinkAssemblyFile;

	/// <summary>
	/// The working directory of the running application.
	/// </summary>
	/// <remarks>
	/// <para> In .NetCore 3.x it is possible that this assembly is part of a single part executable. </para>
	/// <para> In such cases the content of the application is extracted to a temporary folder. </para>
	/// <para> This property will be that temporary directory. It is obtained via <see cref="System.AppContext.BaseDirectory"/>. </para>
	/// </remarks>
	private static readonly DirectoryInfo WorkingDirectory;

	#endregion

	#region Properties

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Static constructor
	/// </summary>
	static AgLinkRequirementsHelper()
	{
#if (NET45)
			WorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
#else
		WorkingDirectory = new DirectoryInfo(System.AppContext.BaseDirectory);
#endif
	}

	#endregion

	#region Methods

	/// <summary>
	/// Creates a new file for the unmanaged assembly.
	/// </summary>
	/// <param name="assemblyFileName"> The name of the unmanaged assembly file. </param>
	/// <param name="assemblyContent"> The content as a <see cref="Stream"/>. </param>
	/// <remarks> This methods sets a callback so that the unmanaged assembly file will be automatically deleted upon process exit. </remarks>
	public static void CopyUnmanagedAssemblies(string assemblyFileName, Stream? assemblyContent)
	{
		var workingDirectory = AgLinkRequirementsHelper.WorkingDirectory;
		var assemblyFile = new FileInfo(Path.Combine(workingDirectory.FullName, assemblyFileName));

		if (assemblyContent is null)
		{
#if DEBUG
			Debug.WriteLine($"The unmanaged AGLink assembly '{assemblyFile.Name}' does not exist as embedded resource. Therefore using any instance of '{nameof(AgLinkPlc)}' will probably fail.");
#else
				throw new ApplicationException($"The unmanaged AGLink assembly '{assemblyFile.Name}' does not exist as embedded resource. Please ensure its existence and then try again.");
#endif
		}
		else
		{
			using (assemblyContent)
			{
				using var fileStream = assemblyFile.Open(FileMode.Create);
				assemblyContent.CopyTo(fileStream);
				fileStream.Flush();
			}

			// Setup auto deletion of this file.
			AgLinkRequirementsHelper.AutomaticallyDeleteUnmanagedAssembly();
				
			// Save a reference to the file, so that it can be deleted when the process ends.
			_unmanagedAgLinkAssemblyFile = assemblyFile;
		}
	}

	private static void AutomaticallyDeleteUnmanagedAssembly()
	{
		// Delete the unmanaged assembly once the whole process ends.
		AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
		{
			if (_unmanagedAgLinkAssemblyFile?.Exists ?? false)
			{
				var command = $"timeout 5 /nobreak >nul | del /F /Q \"{_unmanagedAgLinkAssemblyFile.FullName}\" >nul";
				var process = new System.Diagnostics.Process()
				{
					StartInfo = new System.Diagnostics.ProcessStartInfo
					{
						WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
						FileName = "cmd.exe",
						Arguments = $"/C {command}",
						CreateNoWindow = true,
					}
				};
				process.Start();
			}
		};
	}
		
	/// <summary>
	/// Activates AGLink via the <paramref name="licenseKey"/>.
	/// </summary>
	/// <param name="licenseKey"> The license key used for activation. Can be null or empty, in which case this function returns false. </param>
	/// <returns> True on success, otherwise false. </returns>
	public static bool ActivateAgLink(string? licenseKey)
	{
		try
		{
			if (!String.IsNullOrWhiteSpace(licenseKey))
			{
				AGL4.Activate(licenseKey);
				Debug.WriteLine($"AgLink has been activated with license key '{licenseKey}'.");
				return true;
			}
			else
			{
				Debug.WriteLine($"No license key for AgLink has been found.");
				return false;
			}
		}
		catch
		{
			if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
			return false;
		}
	}

	/// <summary>
	/// Overrides <see cref="AgLinkPlc.ErrorMapping"/> with <paramref name="agLinkErrorMapping"/>.
	/// </summary>
	/// <param name="agLinkErrorMapping"> The new error mapping. </param>
	public static void OverrideErrorMapping(AgLinkErrorMapping agLinkErrorMapping)
	{
		AgLinkPlc.ErrorMapping = agLinkErrorMapping;
	}

	#endregion
}