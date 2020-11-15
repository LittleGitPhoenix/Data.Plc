#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accon.AGLink;

namespace Phoenix.Data.Plc.AgLink
{
	internal class AgLinkErrorMapping
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		internal Dictionary<int, string> ErrorCodeMapping { get; }

		#endregion

		#region (De)Constructors
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="workingDirectory"> The current working directory of the running assembly. </param>
		public AgLinkErrorMapping(DirectoryInfo workingDirectory)
			: this(AgLinkErrorMapping.GetErrorMappingFile(workingDirectory)) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="errorFile"> The file containing the error messages. </param>
		public AgLinkErrorMapping(FileInfo? errorFile)
		{
			this.ErrorCodeMapping = new Dictionary<int, string>();
			foreach (var (code, errorMessage) in AgLinkErrorMapping.EnumerateErrorMappings(errorFile))
			{
				if (!this.ErrorCodeMapping.ContainsKey(code)) this.ErrorCodeMapping.Add(code, errorMessage);
			}
		}

		#endregion

		#region Methods

		private static FileInfo? GetErrorMappingFile(DirectoryInfo workingDirectory)
		{
			return workingDirectory
				.EnumerateFiles("AGLink40_Error.txt", SearchOption.TopDirectoryOnly)
				.FirstOrDefault()
				;
		}

		private static IEnumerable<(int Code, string ErrorMessage)> EnumerateErrorMappings(FileInfo? errorFile)
		{
			if (errorFile is null) yield break;

			StreamReader streamReader;
			try
			{
				// Read the file line by line, parsing and returning the error mappings from it. 
				streamReader = errorFile.OpenText();
			}
			catch (Exception ex)
			{
				yield break;
			}

			try
			{
				String? line;
				while ((line = streamReader.ReadLine()) != null)
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
						yield return (code, data[1]);
					}
				}
			}
			finally
			{
				streamReader.Dispose();
			}
		}
		
		internal string GetErrorMessageForCode(int code)
		{
			ErrorCodeMapping.TryGetValue(code, out var message);
			if (String.IsNullOrWhiteSpace(message)) AGL4.GetErrorMsg(code, out message);
			if (String.IsNullOrWhiteSpace(message)) message = $"UNKNOWN: {code} [{code:X}]";
			return message;
		}

		#endregion
	}
}