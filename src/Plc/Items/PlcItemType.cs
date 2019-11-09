namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// Defines types for <see cref="IPlcItem"/>s.
	/// </summary>
	public enum PlcItemType : byte
	{
		/// <summary> Input sockets </summary>
		Input,
		/// <summary> Output sockets </summary>
		Output,
		/// <summary> Flags area </summary>
		Flags,
		/// <summary> Regular data blocks </summary>
		Data,
	}
}