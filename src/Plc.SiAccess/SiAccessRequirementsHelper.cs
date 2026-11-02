#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Diagnostics;
using Softing.SiAccess;

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// This helper class provides static methods for providing requirements to <see cref="Softing.SiAccess"/>.
/// </summary>
public abstract class SiAccessRequirementsHelper
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields
		
	/// <summary> The <see cref="FileInfo"/> instance of the unmanaged siAccess assembly. Kept to later delete the file upon process exit. </summary>
	private static FileInfo? UnmanagedSiAccessAssemblyFile;

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
	static SiAccessRequirementsHelper()
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
	/// <remarks> This method sets a callback so that the unmanaged assembly file will be automatically deleted upon process exit. </remarks>
	public static void CopyUnmanagedAssemblies(string assemblyFileName, Stream? assemblyContent)
	{
		var workingDirectory = WorkingDirectory;
		var assemblyFile = new FileInfo(Path.Combine(workingDirectory.FullName, assemblyFileName));

		if (assemblyContent is null)
		{
#if DEBUG
			Debug.WriteLine($"The unmanaged siAccess assembly '{assemblyFile.Name}' does not exist as embedded resource. Therefore using any instance of '{nameof(SiAccessPlc)}' will probably fail.");
#else
			throw new ApplicationException($"The unmanaged siAccess assembly '{assemblyFile.Name}' does not exist as embedded resource. Please ensure its existence and then try again.");
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
			AutomaticallyDeleteUnmanagedAssembly();
				
			// Save a reference to the file, so that it can be deleted when the process ends.
			UnmanagedSiAccessAssemblyFile = assemblyFile;
		}
	}

	private static void AutomaticallyDeleteUnmanagedAssembly()
	{
		// Delete the unmanaged assembly once the whole process ends.
		AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
		{
			// Clean up resources of the siAccess library.
			//! This is something that AGLink did not require.
			AGL.Cleanup();

			if (UnmanagedSiAccessAssemblyFile?.Exists ?? false)
			{
				var command = $"timeout 5 /nobreak >nul | del /F /Q \"{UnmanagedSiAccessAssemblyFile.FullName}\" >nul";
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
	/// Activates siAccess via the <paramref name="licenseKey"/>.
	/// </summary>
	/// <param name="licenseKey"> The license key used for activation. Can be null or empty, in which case this function returns false. </param>
	/// <returns> True on success, otherwise false. </returns>
	public static bool Activate(string? licenseKey)
	{
		try
		{
			// Initializes the siAccess library. Must be called before any other API functions are called.
			//! This is something that AGLink did not require.
			AGL.Initialize(0);

			if (!String.IsNullOrWhiteSpace(licenseKey))
			{
				AGL.Activate(licenseKey);
				Debug.WriteLine($"siAccess has been activated with license key '{licenseKey}'.");
				return true;
			}
			else
			{
				Debug.WriteLine($"No license key for siAccess has been found.");
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
	/// Overrides <see cref="SiAccessPlc.ErrorMapping"/> with <paramref name="errorMapping"/>.
	/// </summary>
	/// <param name="errorMapping"> The new error mapping. </param>
	public static void OverrideErrorMapping(SiAccessErrorMapping errorMapping)
	{
		SiAccessPlc.ErrorMapping = errorMapping;
	}

	#endregion
}