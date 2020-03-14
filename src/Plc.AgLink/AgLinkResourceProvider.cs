#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Provider class for embedded <c>AGLink</c> resources.
	/// </summary>
	internal sealed class AgLinkResourceProvider : IDisposable
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		
		private static int _alreadyInitialized;

		private readonly IAgLinkSetupAdapter _setupAdapter;

		private readonly AgLinkPlcConnectionData _connectionData;

		#endregion

		#region Properties
		
		/// <summary> The embedded unmanaged AGLink assembly. </summary>
		private FileInfo AssemblyFile { get; }

		/// <summary> The parameter file. </summary>
		private FileInfo ParameterFile { get; }

		#endregion

		#region (De)Constructors
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="setupAdapter"> An instance of <see cref="IAgLinkSetupAdapter"/> used for initial setup. </param>
		/// <param name="connectionData"> <see cref="AgLinkPlcConnectionData"/> used to change the <see cref="ParameterFile"/>. </param>
		public AgLinkResourceProvider(IAgLinkSetupAdapter setupAdapter, AgLinkPlcConnectionData connectionData)
		{
			// Save parameters.
			_setupAdapter = setupAdapter;
			_connectionData = connectionData;
			
			// Initialize fields.
			var assemblyDirectory = AgLinkResourceProvider.GetAssemblyDirectory();
			var tempDirectory = Path.GetTempPath();
			this.AssemblyFile = new FileInfo(Path.Combine(assemblyDirectory, setupAdapter.AssemblyFileName));
			this.ParameterFile = new FileInfo(Path.Combine(tempDirectory, setupAdapter.ParameterFileName));
		}

		#endregion

		#region Methods

		/// <summary>
		/// This initializes (loads and saves) the files required by <c>AGLink</c>.
		/// </summary>
		public void Initialize()
		{
			// Allow only one initialization ever.
			if (Interlocked.CompareExchange(ref _alreadyInitialized, 1, 0) == 1) return;

			// Get the assembly that contains the 
			var sourceAssembly = _setupAdapter.GetType().Assembly;

			// Get and save the embedded AGLink assembly and the parameter file.
			AgLinkResourceProvider.ExtractEmbeddedAssemblyToFile(_setupAdapter.SourceAssembly, _setupAdapter.ResourcePath, this.AssemblyFile);
			AgLinkResourceProvider.ExtractEmbeddedParameterFile(_setupAdapter.SourceAssembly, _setupAdapter.ResourcePath, this.ParameterFile, _connectionData.Ip, _connectionData.Rack, _connectionData.Slot);
			
			// Set further data.
			_setupAdapter.Activate();
			_setupAdapter.ChangeDirectoryOfParameterFile(this.ParameterFile.DirectoryName);
			_setupAdapter.ForceSynchronousFunctionCalls();
		}

		#region AGLink Assembly

		private static void ExtractEmbeddedAssemblyToFile(Assembly sourceAssembly, string resourcePath, FileInfo file)
		{
			var data = AgLinkResourceProvider.LoadInternalResource(file.Name, resourcePath, sourceAssembly);
			if (data == null) throw new AgLinkResourceProviderException(AgLinkResourceProviderExceptionReason.AssemblyFileNotFound, $"Could not load file '{file.Name}' from the internal resources.");
			AgLinkResourceProvider.SaveAssemblyFile(file, data);
		}

		/// <summary>
		/// Saves the <paramref name="data"/> into the <paramref name="file"/>.
		/// </summary>
		private static void SaveAssemblyFile(FileInfo file, byte[] data)
		{
			try
			{
				using (var fileStream = file.Open(FileMode.Create, FileAccess.Write))
				{
					fileStream.Write(data, 0, data.Length);
					fileStream.Flush();
				}
			}
			catch (IOException ex)
			{
				file.Refresh();
				if (!file.Exists) throw new AgLinkResourceProviderException(AgLinkResourceProviderExceptionReason.AssemblyFileNotSaved, $"Could not save file '{file.FullName}'. See the inner exception for further details.", ex);
			}

			file.Refresh();
		}

		#endregion

		#region Parameter File

		private static void ExtractEmbeddedParameterFile(Assembly sourceAssembly, string resourcePath, FileInfo parameterFile, string ip, int rack, int slot)
		{
			var data = AgLinkResourceProvider.LoadInternalResource(parameterFile.Name, resourcePath, sourceAssembly);
			if (data == null) throw new AgLinkResourceProviderException(AgLinkResourceProviderExceptionReason.ParameterFileNotFound, $"Could not load file '{parameterFile.Name}' from the internal resources.");
			AgLinkResourceProvider.SaveParameterFile(parameterFile, data, ip, rack, slot);
		}

		/// <summary>
		/// Saves the <paramref name="data"/> into the <paramref name="parameterFile"/> and replaces connection relevant data beforehand.
		/// </summary>
		private static void SaveParameterFile(FileInfo parameterFile, byte[] data, string ip, int rack, int slot)
		{
			try
			{
				// Get the parameter file from the internal resources.
				var content = System.Text.Encoding.UTF8.GetString(data);

				// Replace the content.
				content = content.Replace("{IP}", ip);
				content = content.Replace("{RACK}", rack.ToString());
				content = content.Replace("{SLOT}", slot.ToString());

				// Copy the parameter file to the temp directory.
				using (var writer = parameterFile.CreateText())
				{
					writer.Write(content);
					writer.Flush();
				}
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
				throw new AgLinkResourceProviderException(AgLinkResourceProviderExceptionReason.ParameterFileNotSaved, $"An error occured while saving '{parameterFile.FullName}'. See the inner exception for further details.", ex);
			}
		}

		#endregion

		#region Helper

		/// <summary>
		/// Gets the path where the current assembly resides.
		/// </summary>
		/// <returns> The current assemblies working path. </returns>
		private static string GetAssemblyDirectory()
		{
			var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);
			return Path.GetDirectoryName(path);
		}

		/// <summary>
		/// Loads the specified resource from the embedded resources of the passed assembly.
		/// </summary>
		/// <param name="resourceName"> The name of the resource. </param>
		/// <param name="path"> "." separated path to the embedded resource. </param>
		/// <param name="assembly"> An optional assembly from where to load the resource. If omitted, then <see cref="System.Reflection.Assembly.GetCallingAssembly"/> will be used. </param>
		/// <returns> The resource as <see cref="byte"/> array or <c>NULL</c>. </returns>
		private static byte[] LoadInternalResource(string resourceName, string path, System.Reflection.Assembly assembly = null)
		{
			try
			{
				if (assembly == null) assembly = System.Reflection.Assembly.GetCallingAssembly();
				using (var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{path}.{resourceName}"))
				{
					//  Either the file does not exist or it is not marked as embedded resource.
					if (stream == null) return null;

					//  Get byte array from the file from embedded resource.
					byte[] byteArray = new byte[stream.Length];
					stream.Read(buffer: byteArray, offset: 0, count: (int)stream.Length);
					return byteArray;
				}
			}
			catch
			{
				return null;
			}
		}

		#endregion

		#region IDisposable

		/// <inheritdoc />
		public void Dispose()
		{
			this.AssemblyFile.Refresh();
			this.ParameterFile.Refresh();

			//! Once loaded this assembly cannot be deleted while the application is still running.
			//if (this.AssemblyFile.Exists) this.AssemblyFile.Delete();
			if (this.ParameterFile.Exists) this.ParameterFile.Delete();

			_alreadyInitialized = 0;
		}

		#endregion

		#endregion
	}
}