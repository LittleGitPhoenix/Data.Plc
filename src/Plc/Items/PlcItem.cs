#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Text;

namespace Phoenix.Data.Plc.Items;

/// <summary>
/// Base implementation of an <see cref="IPlcItem"/>s.
/// </summary>
public class PlcItem : IPlcItem
{
	#region Delegates / Events

	/// <inheritdoc />
	public event EventHandler<BitsChangedEventArgs> ValueChanged
	{
		add => this.Value.BitsChanged += value;
		remove => this.Value.BitsChanged -= value;
	}
		
	#endregion

	#region Constants
	#endregion

	#region Fields
	#endregion

	#region Properties

	/// <inheritdoc />
	public string Identifier { get; }

	/// <inheritdoc />
	public string PlcString { get; }

	/// <inheritdoc />
	public PlcItemType Type { get; }
		
	/// <inheritdoc />
	public ushort DataBlock { get; }

	/// <inheritdoc />
	public ushort Position { get; }
		
	/// <inheritdoc />
	public BitPosition BitPosition { get; }

	/// <inheritdoc />
	public BitCollection Value { get; }

	#endregion

	#region (De)Constructors

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="type"> <see cref="Type"/> </param>
	/// <param name="dataBlock"> <see cref="DataBlock"/> </param>
	/// <param name="position"> <see cref="Position"/> </param>
	/// <param name="bitPosition"> <see cref="BitPosition"/> </param>
	/// <param name="bitAmount"> This is the amount of <c>bits</c> this item handles. </param>
	/// <param name="isFlexible"> Is the values <see cref="BitCollection"/> fixed or will it change size according to its data. </param>
	/// <param name="identifier"> <see cref="Identifier"/> </param>
	public PlcItem
	(
		PlcItemType type,
		ushort dataBlock,
		ushort position,
		BitPosition bitPosition,
		uint bitAmount,
		bool isFlexible,
		string identifier = default
	)
		: this
		(
			type,
			dataBlock,
			position,
			bitPosition,
			new BitCollection(isFlexible, new bool[bitAmount]),
			identifier
		)
	{ }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="type"> <see cref="Type"/> </param>
	/// <param name="dataBlock"> <see cref="DataBlock"/> </param>
	/// <param name="position"> <see cref="Position"/> </param>
	/// <param name="bitPosition"> <see cref="BitPosition"/> </param>
	/// <param name="initialValue"> The initial value of this item. </param>
	/// <param name="identifier"> <see cref="Identifier"/> </param>
	public PlcItem
	(
		PlcItemType type,
		ushort dataBlock,
		ushort position,
		BitPosition bitPosition,
		BitCollection initialValue,
		string identifier = default
	)
	{
		string BuildPlcString()
		{
			var bitAmount = initialValue.Length;
			var handlesFullBytes = bitAmount % 8 == 0;
			var builder = new StringBuilder();
			builder.Append(type == PlcItemType.Data ? $"DB{dataBlock}" : $"{type}");
			builder.Append(handlesFullBytes ? $",B{position},{bitAmount / 8}" : $",X{position}.{(byte) bitPosition},{bitAmount}");
			return builder.ToString();
		}
		this.PlcString = BuildPlcString();

		// Save parameters.
		this.Identifier = String.IsNullOrWhiteSpace(identifier) ? this.PlcString : identifier;
		this.Type = type;
		this.DataBlock = type == PlcItemType.Data ? dataBlock : (ushort) 0;
		this.Position = position;
		this.BitPosition = bitPosition;
		this.Value = initialValue;
	}
		
	#endregion

	#region Methods

	#region IEquatable

	/// <summary> The hash code of an instance of this class. </summary>
	private int HashCode
	{
		get
		{
			_hashCode ??= new { this.Type, this.DataBlock, this.Position, this.BitPosition, this.Value.Length }.GetHashCode();
			return _hashCode.Value;
		}
	}
	private int? _hashCode;

	/// <summary> The default hash method. </summary>
	/// <returns> A hash value for the current object. </returns>
	public override int GetHashCode()
	{
		return this.HashCode;
	}

	/// <summary>
	/// Compares this instance to another one.
	/// </summary>
	/// <param name="other"> The other instance to compare to. </param>
	/// <returns> <see langword="true"/> if this instance equals the <paramref name="other"/> instance, otherwise <see langword="false"/>. </returns>
	public override bool Equals(object other)
	{
		if (other is IPlcItem equatable) return this.Equals(equatable);
		return false;
	}

	/// <summary>
	/// Compares this instance to another one.
	/// </summary>
	/// <param name="other"> The other instance to compare to. </param>
	/// <returns> <see langword="true"/> if this instance equals the <paramref name="other"/> instance, otherwise <see langword="false"/>. </returns>
	public bool Equals(IPlcItem other)
	{
		return other?.GetHashCode() == this.GetHashCode();
	}

	/// <summary>
	/// Checks if the two <see cref="PlcItem"/>s are equal.
	/// </summary>
	/// <param name="x"> The first <see cref="PlcItem"/> to compare. </param>
	/// <param name="y"> The second <see cref="IPlcItem"/> to compare. </param>
	/// <returns> <see langword="true"/> if both instances are equal, otherwise <see langword="false"/>. </returns>
	public static bool operator ==(PlcItem x, IPlcItem y)
	{
		return x?.GetHashCode() == y?.GetHashCode();
	}

	/// <summary>
	/// Checks if the two <see cref="PlcItem"/>s are NOT equal.
	/// </summary>
	/// <param name="x"> The first <see cref="PlcItem"/> to compare. </param>
	/// <param name="y"> The second <see cref="IPlcItem"/> to compare. </param>
	/// <returns> <see langword="true"/> if both instances are NOT equal, otherwise <see langword="false"/>. </returns>
	public static bool operator !=(PlcItem x, IPlcItem y)
	{
		return !(x == y);
	}

	#endregion

	#region IDeepCloneable

	/// <summary>
	/// Creates a deep copy of the current <see cref="IPlcItem"/>
	/// </summary>
	/// <returns> A cloned <see cref="IPlcItem"/>. </returns>
	/// <remarks> The clone will always by a normal (non-typed) <see cref="PlcItem"/>. Therefor its <see cref="IPlcItem.Value"/> will be a <see cref="BitCollection"/> and NOT whatever the original item value type is. </remarks>
	public IPlcItem Clone()
		=> this.Clone(null);

	/// <summary>
	/// Creates a deep copy of the current <see cref="IPlcItem"/>
	/// </summary>
	/// <param name="identifier"> An optional new <see cref="IPlcItem.Identifier"/>. </param>
	/// <returns> A cloned <see cref="IPlcItem"/>. </returns>
	/// <remarks> The clone will always by a normal (non-typed) <see cref="PlcItem"/>. Therefor its <see cref="IPlcItem.Value"/> will be a <see cref="BitCollection"/> and NOT whatever the original item value type is. </remarks>
	public IPlcItem Clone(string identifier)
	{
		var newIdentifier = identifier ?? $"Clone[{Guid.NewGuid()}]: {this.Identifier}";
		return new PlcItem(this.Type, this.DataBlock, this.Position, this.BitPosition, this.Value.Clone(), newIdentifier);
	}

	#endregion

	#region Helper

	private static readonly Dictionary<System.Type, string> TypeTranslations = new Dictionary<System.Type, string>
	{
		{typeof(int), "int"},
		{typeof(uint), "uint"},
		{typeof(long), "long"},
		{typeof(ulong), "ulong"},
		{typeof(short), "short"},
		{typeof(ushort), "ushort"},
		{typeof(byte), "byte"},
		{typeof(sbyte), "sbyte"},
		{typeof(bool), "bool"},
		{typeof(float), "float"},
		{typeof(double), "double"},
		{typeof(decimal), "decimal"},
		{typeof(char), "char"},
		{typeof(string), "string"},
		{typeof(object), "object"},
		{typeof(void), "void"}
	};
		
	/// <summary>
	/// Returns the full name of the passed type.
	/// </summary>
	/// <param name="type"> The type to handle. </param>
	/// <returns> The full name of the type with resolved generics. </returns>
	protected internal static string GetFullName(Type type)
	{
		if (TypeTranslations.TryGetValue(type, out var name))
		{
			return name;
		}
		else if (type.IsArray)
		{
			return GetFullName(type.GetElementType()) + "[]";
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>))
		{
			return GetFullName(type.GetGenericArguments()[0]) + "?";
		}
		else if (type.IsGenericType)
		{
			return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(GetFullName)) + ">";
		}
		else
		{
			return type.Name;
		}
	}

	internal static StringBuilder GetIdentifierBuilder(IPlcItem plcItem)
	{
		var identifierBuilder = new StringBuilder(plcItem.Identifier);
		if (!plcItem.Identifier.Equals(plcItem.PlcString)) identifierBuilder.Append($" ({plcItem.PlcString})");
		return identifierBuilder;
	}

	#endregion

	/// <summary>
	/// Returns a string that represents the current object.
	/// </summary>
	public override string ToString()
		=> this.ToString("N");

	/// <inheritdoc cref="IFormattable.ToString(string, System.IFormatProvider)"/>
	public string ToString(string format)
		=> this.ToString(format, null);

	/// <inheritdoc />
	public string ToString(string format, IFormatProvider formatProvider)
		=> ToString(this, format);
		
	/// <inheritdoc cref="IFormattable.ToString(string, System.IFormatProvider)"/>
	internal static string ToString(IPlcItem plcItem, string format)
	{
		if (String.IsNullOrWhiteSpace(format)) format = "N";

		switch (format.ToUpperInvariant())
		{
			case "S":
			case "LOG":
				return GetIdentifierBuilder(plcItem).ToString();

			case "F":
			case "FULL":
				return $"[<{GetFullName(plcItem.GetType())}> :: {plcItem.Type} | {GetIdentifierBuilder(plcItem)} | {plcItem.Value:HEX}]";

			case "N":
			case "NORMAL":
			default:
				return $"[<{GetFullName(plcItem.GetType())}> :: {plcItem.Type} | {GetIdentifierBuilder(plcItem)}]";
		}
	}

	#endregion
}