#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Text.RegularExpressions;
using Accon.AGLink;

namespace Phoenix.Data.Plc.AgLink;

/// <summary>
/// File that parses an AGLink error file and provides error message via <see cref="GetErrorMessageForCode"/>.
/// </summary>
public class AgLinkErrorMapping
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private static Regex CleanupRegEx;

	#endregion

	#region Properties

	internal Dictionary<int, string> ErrorCodeMapping { get; }

	#endregion

	#region (De)Constructors

	static AgLinkErrorMapping()
	{
		CleanupRegEx = new Regex(@"[\u000A\u000B\u000C\u000D\u2028\u2029\u0085]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="workingDirectory"> The current working directory of the running assembly. </param>
	[Obsolete("Use the new overload that accepts the content of the error file instead.", true)]
	public AgLinkErrorMapping(DirectoryInfo workingDirectory)
		: this(AgLinkErrorMapping.GetErrorMappingFile(workingDirectory)) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="errorFile"> The file containing the error messages. </param>
	[Obsolete("Use the new overload that accepts the content of the error file instead.", true)]
	public AgLinkErrorMapping(FileInfo? errorFile)
	{
		this.ErrorCodeMapping = new Dictionary<int, string>();
		foreach (var (code, errorMessage) in AgLinkErrorMapping.EnumerateErrorMappings(errorFile))
		{
			if (!this.ErrorCodeMapping.ContainsKey(code)) this.ErrorCodeMapping.Add(code, errorMessage);
		}
	}

	/// <summary>
	/// Constructor for a null-object.
	/// </summary>
	internal AgLinkErrorMapping()
		: this(new string[0]) { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="errorFileContent"> The content of an AGLink error file as a single string. </param>
	public AgLinkErrorMapping(string? errorFileContent)
		: this(errorFileContent?.Split('\n') ?? new string[0]) { }
		
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="errorFileContent"> The content of an AGLink error file line by line. </param>
	public AgLinkErrorMapping(ICollection<string> errorFileContent)
	{
		this.ErrorCodeMapping = new Dictionary<int, string>();
		foreach (var (code, errorMessage) in AgLinkErrorMapping.EnumerateErrorMappings(errorFileContent))
		{
			if (!this.ErrorCodeMapping.ContainsKey(code)) this.ErrorCodeMapping.Add(code, errorMessage);
		}
	}

	#endregion

	#region Methods

	[Obsolete("Not supported anymore.", true)]
	private static FileInfo? GetErrorMappingFile(DirectoryInfo workingDirectory)
	{
		return workingDirectory
				.EnumerateFiles("AGLink40_Error.txt", SearchOption.TopDirectoryOnly)
				.FirstOrDefault()
			;
	}

	[Obsolete("Not supported anymore.", true)]
	private static IEnumerable<(int Code, string ErrorMessage)> EnumerateErrorMappings(FileInfo? errorFile)
	{
		yield break;
		//if (errorFile is null) yield break;

		//StreamReader streamReader;
		//try
		//{
		//	// Read the file line by line, parsing and returning the error mappings from it. 
		//	streamReader = errorFile.OpenText();
		//}
		//catch (Exception ex)
		//{
		//	yield break;
		//}

		//try
		//{
		//	String? line;
		//	while ((line = streamReader.ReadLine()) != null)
		//	{
		//		if (line.StartsWith("0x"))
		//		{
		//			var data = line.Split(';');
		//			if (data.Length != 2) continue;
		//			int code;
		//			try
		//			{
		//				code = Convert.ToInt32(data[0], 16);
		//			}
		//			catch (Exception ex)
		//			{
		//				continue;
		//			}
		//			yield return (code, data[1]);
		//		}
		//	}
		//}
		//finally
		//{
		//	streamReader.Dispose();
		//}
	}
		
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
				var message = AgLinkErrorMapping.CleanupRegEx.Replace(data[1], string.Empty);
				yield return (code, message);
			}
		}
	}

	internal string GetErrorMessageForCode(int code)
	{
		this.ErrorCodeMapping.TryGetValue(code, out var message);
		if (String.IsNullOrWhiteSpace(message)) AGL4.GetErrorMsg(code, out message);
		if (String.IsNullOrWhiteSpace(message)) message = $"UNKNOWN: {code} [{code:X}]";
		return message;
	}

	#endregion
}