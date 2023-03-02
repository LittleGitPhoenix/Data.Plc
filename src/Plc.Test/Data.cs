namespace Phoenix.Data.Plc.Test;

public enum TestEnumeration : byte
{
	Value1 = 82,
	Value2 = 79,
	Value3 = 67,
	Value4 = 75,
}
	
public class Data
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	public ushort Datablock { get; }

	public ushort StartOfFixedBytes { get; }

	public ushort EndOfFixedBytes { get; }

	public ushort StartOfModifiableBytes { get; }

	public ushort EndOfModifiableBytes { get; }

	public byte[] TargetBytes { get; }

	public byte[] WriteBytes { get; }

	#endregion

	#region Properties

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	public Data()
	{
		this.Datablock = 25555;

		this.StartOfFixedBytes = 0;
		this.EndOfFixedBytes = 3;
		this.StartOfModifiableBytes = 4;
		this.EndOfModifiableBytes = 7;

		this.TargetBytes = new byte[]
		{
			82,
			79,
			67,
			75,
		};

		Random rnd = new Random();
		this.WriteBytes = new byte[]
		{
			(byte) rnd.Next(sbyte.MinValue, sbyte.MaxValue), // Random 7bit
			(byte) rnd.Next(sbyte.MinValue, sbyte.MaxValue), // Random 7bit
			(byte) rnd.Next(sbyte.MaxValue, byte.MaxValue),  // Random last 8bit
			(byte) rnd.Next(sbyte.MaxValue, byte.MaxValue),  // Random last 8bit
		};
	}

	#endregion

	#region Methods
	#endregion
}