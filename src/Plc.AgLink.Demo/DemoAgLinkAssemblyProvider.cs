using System;
using System.IO;
using System.Threading;
using Accon.AGLink;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Provider class for the embedded unmanaged <see cref="Accon.AGLink"/> assembly.
	/// </summary>
	internal sealed class DemoAgLinkAssemblyProvider : IAgLinkAssemblyProvider
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		
		private static int _alreadyInitialized;

		private readonly AgLinkPlcConnectionData _connectionData;

		#endregion

		#region Properties

		/// <summary> An optional AGLink activation key. </summary>
		private static string Key { get; }

		/// <summary> Path to the embedded AGLink resource. </summary>
		/// <remarks> This path uses "dot" separation (e.g. "Folder.AnotherFolder.LastFolder"). </remarks>
		private static string ResourcePath { get; }

		/// <summary> The embedded unmanaged AGLink assembly. </summary>
		private static FileInfo AssemblyFile { get; }

		/// <summary> The parameter file. </summary>
		public static FileInfo ParameterFile { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Static constructor
		/// </summary>
		static DemoAgLinkAssemblyProvider()
		{
			var assemblyDirectory = DemoAgLinkAssemblyProvider.GetAssemblyDirectory();
			var tempDirectory = Path.GetTempPath();

			// Initialize fields.
			DemoAgLinkAssemblyProvider.Key = null;
			DemoAgLinkAssemblyProvider.ResourcePath = "Resources.AgLink";
			DemoAgLinkAssemblyProvider.AssemblyFile =  new FileInfo(Path.Combine(assemblyDirectory, $"AGLink40{(Environment.Is64BitProcess ? "_x64" : "")}.dll"));
			DemoAgLinkAssemblyProvider.ParameterFile =  new FileInfo(Path.Combine(tempDirectory, "AGLink40CfgDev0000.xml"));
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="connectionData"> <see cref="AgLinkPlcConnectionData"/> used to change the <see cref="ParameterFile"/>. </param>
		public DemoAgLinkAssemblyProvider(AgLinkPlcConnectionData connectionData)
		{
			_connectionData = connectionData;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Initialize()
		{
			// Allow only one initialization ever.
			if (Interlocked.CompareExchange(ref _alreadyInitialized, 1, 0) == 1) return;

			// Get and save the embedded AGLink assembly and the parameter file.
			DemoAgLinkAssemblyProvider.ExtractEmbeddedAssemblyToFile(DemoAgLinkAssemblyProvider.ResourcePath, DemoAgLinkAssemblyProvider.AssemblyFile);
			DemoAgLinkAssemblyProvider.ExtractEmbeddedParameterFile(DemoAgLinkAssemblyProvider.ResourcePath, DemoAgLinkAssemblyProvider.ParameterFile, _connectionData.Ip, _connectionData.Rack, _connectionData.Slot);
			
			// Set further data.
			if (!String.IsNullOrWhiteSpace(DemoAgLinkAssemblyProvider.Key)) AGL4.Activate(DemoAgLinkAssemblyProvider.Key);
			AGL4.SetParaPath(DemoAgLinkAssemblyProvider.ParameterFile.DirectoryName);
			AGL4.ReturnJobNr(false);
		}

		#region AGLink Assembly

		private static void ExtractEmbeddedAssemblyToFile(string resourcePath, FileInfo file)
		{
			var data = DemoAgLinkAssemblyProvider.LoadInternalResource(file.Name, resourcePath);
			if (data == null) throw new ApplicationException($"Could not load file '{file.Name}' from the internal resources.");
			DemoAgLinkAssemblyProvider.SaveAssemblyFile(file, data);
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
			catch (IOException)
			{
				file.Refresh();
				if (!file.Exists) throw;
			}

			file.Refresh();
		}

		#endregion

		#region Parameter File

		private static void ExtractEmbeddedParameterFile(string resourcePath, FileInfo parameterFile, string ip, int rack, int slot)
		{
			var data = DemoAgLinkAssemblyProvider.LoadInternalResource(parameterFile.Name, resourcePath);
			if (data == null) throw new ApplicationException($"Could not load file '{parameterFile.Name}' from the internal resources.");
			DemoAgLinkAssemblyProvider.SaveParameterFile(parameterFile, data, ip, rack, slot);
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
				throw;
				//Logger.Error(ex, $"An error occurred while settings plc connection data.");
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
		/// <param name="resourceName"> The name of the resource. All folders of the resource have to be supplied by changing them with dots too. </param>
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
			DemoAgLinkAssemblyProvider.AssemblyFile.Refresh();
			DemoAgLinkAssemblyProvider.ParameterFile.Refresh();

			//! Once loaded this assembly cannot be deleted while the application is still running.
			//if (DemoAgLinkAssemblyProvider.AssemblyFile.Exists) DemoAgLinkAssemblyProvider.AssemblyFile.Delete();
			if (DemoAgLinkAssemblyProvider.ParameterFile.Exists) DemoAgLinkAssemblyProvider.ParameterFile.Delete();

			_alreadyInitialized = 0;
		}

		#endregion

		#endregion
	}
}