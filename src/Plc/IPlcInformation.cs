namespace Phoenix.Data.Plc
{
	/// <summary>
	/// General information about a plc instance.
	/// </summary>
	public interface IPlcInformation
	{
		/// <summary> The name of the plc. </summary>
		/// <remarks> May ´be used for identification and log purposes. </remarks>
		string Name { get; }
	}
}