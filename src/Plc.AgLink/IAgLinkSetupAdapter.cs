using System.Reflection;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Interface for classes that provide setup data to the <see cref="AgLinkResourceProvider"/>.
	/// </summary>
	public interface IAgLinkSetupAdapter
	{
		#region Properties

		/// <summary> The assembly that contains the embedded AGLink resources. </summary>
		Assembly SourceAssembly { get; }

		/// <summary> Path to the embedded AGLink resources. </summary>
		/// <remarks> This path uses "dot" separation (e.g. "Folder.AnotherFolder.LastFolder"). </remarks>
		string ResourcePath { get; }

		/// <summary> The name of the embedded AGLink assembly. </summary>
		/// <remarks> The name must be changed according to the current system environment (x86 | x64). </remarks>
		string AssemblyFileName { get; }

		/// <summary> The name of the embedded AGLink parameter file. </summary>
		string ParameterFileName { get; }

		/// <summary> An optional AGLink activation key. </summary>
		string LicenseKey { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Activate <c>AGLink</c> with the <see cref="LicenseKey"/>.
		/// </summary>
		/// <remarks> This should call <c>AGL4.Activate</c>. </remarks>
		void Activate();

		/// <summary>
		/// Change the path where <c>AGLink</c> will look for its parameter / configuration file.
		/// </summary>
		/// <remarks> This should call <c>AGL4.SetParaPath</c>. </remarks>
		void ChangeDirectoryOfParameterFile(string pathToParameterFile);

		/// <summary>
		/// Forces <c>AGLink</c> to make synchronous function calls for reading / writing.
		/// </summary>
		/// <remarks> This should call <c>AGL4.ReturnJobNr</c> with <c>False</c>. </remarks>
		void ForceSynchronousFunctionCalls();
		
		#endregion
	}
}