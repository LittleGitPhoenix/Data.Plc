using System;

namespace Phoenix.Data.Plc.AgLink
{
	/// <summary>
	/// Interface for classes responsible for loading the unmanaged <c>AGLink</c> assemblies.
	/// </summary>
	public interface IAgLinkAssemblyProvider : IDisposable
	{
		/// <summary>
		/// This initializes (loads and saves) the files required by <c>AGLink</c>.
		/// </summary>
		void Initialize();
	}
}