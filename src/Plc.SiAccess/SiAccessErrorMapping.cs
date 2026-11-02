#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Text.RegularExpressions;
using Softing.SiAccess;

namespace Phoenix.Data.Plc.SiAccess;

/// <summary>
/// File that parses an siAccess error file and provides error message via <see cref="GetErrorMessageForCode"/>.
/// </summary>
public class SiAccessErrorMapping
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private static readonly Regex CleanupRegEx = new(@"[\u000A\u000B\u000C\u000D\u2028\u2029\u0085]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

	#endregion

	#region Properties

	internal Dictionary<int, string> ErrorCodeMapping { get; }

	#endregion

	#region (De)Constructors
	
	/// <summary>
	/// Constructor for a null-object.
	/// </summary>
	internal SiAccessErrorMapping()
		: this([]) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="errorFileContent"> The content of an siAccess error file as a single string. </param>
	public SiAccessErrorMapping(string? errorFileContent)
		: this(errorFileContent?.Split('\n') ?? []) { }
		
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="errorFileContent"> The content of an siAccess error file line by line. </param>
	public SiAccessErrorMapping(ICollection<string> errorFileContent)
	{
		this.ErrorCodeMapping = new Dictionary<int, string>();
		foreach (var (code, errorMessage) in EnumerateErrorMappings(errorFileContent))
		{
			if (!this.ErrorCodeMapping.ContainsKey(code)) this.ErrorCodeMapping.Add(code, errorMessage);
		}
	}

	#endregion

	#region Methods
	
	private static IEnumerable<(int Code, string ErrorMessage)> EnumerateErrorMappings(ICollection<string> errorFileContent)
	{
		foreach (var line in errorFileContent)
		{
			if (line.StartsWith("0x"))
			{
				var data = line.Split(';');
				if (data.Length != 2) continue;
				int code;
				try
				{
					code = Convert.ToInt32(data[0], 16);
				}
				catch (Exception ex)
				{
					continue;
				}
				var message = CleanupRegEx.Replace(data[1], string.Empty);
				yield return (code, message);
			}
		}
	}

	internal string GetErrorMessageForCode(int code)
	{
		this.ErrorCodeMapping.TryGetValue(code, out var message);
		if (String.IsNullOrWhiteSpace(message)) AGL.GetErrorMsg(code, out message);
		if (String.IsNullOrWhiteSpace(message)) message = $"UNKNOWN: {code} [{code:X}]";
		return message;
	}

	#endregion
}