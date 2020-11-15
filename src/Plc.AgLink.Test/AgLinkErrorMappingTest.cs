using System;
using System.IO;
using NUnit.Framework;
using Phoenix.Data.Plc.AgLink;

namespace Phoenix.Data.Plc.AgLink.Test
{
	public class AgLinkErrorMappingTest
	{
		void ExecuteTestWithWorkingDirectory(Action<DirectoryInfo> callback)
		{
			var workingDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), $".test_{Guid.NewGuid()}"));
			if (!workingDirectory.Exists)
			{
				workingDirectory.Create();
				workingDirectory.Refresh();
			}
			try
			{
				callback.Invoke(workingDirectory);
			}
			finally
			{
				if (workingDirectory.Exists) workingDirectory.Delete(true);
			}
		}

		[Test]
		public void Check_Loading_Empty_Error_File_Yields_Nothing()
		{
			this.ExecuteTestWithWorkingDirectory
			(
				workingDirectory =>
				{
					// Arrange
					//File.WriteAllLines(Path.Combine(workingDirectory.FullName, "empty.txt"));
					var errorFile = new FileInfo(Path.Combine(workingDirectory.FullName, "empty.txt"));
					errorFile.Create().Dispose();
					errorFile.Refresh();

					// Act
					var errorMapping = new AgLinkErrorMapping(errorFile);

					// Assert
					Assert.IsEmpty(errorMapping.ErrorCodeMapping);
				}
			);
		}

		[Test]
		public void Check_Loading_Valid_Error_File()
		{
			this.ExecuteTestWithWorkingDirectory
			(
				workingDirectory =>
				{
					// Arrange
					var filePath = Path.Combine(workingDirectory.FullName, "errors.txt");
					var content = new string[]
					{
						"0x00001;Test1",
						"0x00002;Test2",
					};
					File.WriteAllLines(filePath, content);
					var errorFile = new FileInfo(filePath);
					
					// Act
					var errorMapping = new AgLinkErrorMapping(errorFile);

					// Assert
					Assert.That(errorMapping.ErrorCodeMapping, Has.Count.EqualTo(2));
				}
			);
		}

		[Test]
		public void Check_Duplicate_Entries_Are_Ignored()
		{
			this.ExecuteTestWithWorkingDirectory
			(
				workingDirectory =>
				{
					// Arrange
					var filePath = Path.Combine(workingDirectory.FullName, "errors.txt");
					var content = new string[]
					{
						"0x00001;Test1",
						"0x00001;Test2",
					};
					File.WriteAllLines(filePath, content);
					var errorFile = new FileInfo(filePath);
					
					// Act
					var errorMapping = new AgLinkErrorMapping(errorFile);

					// Assert
					Assert.That(errorMapping.ErrorCodeMapping, Has.Count.EqualTo(1));
				}
			);
		}

		[Test]
		public void Check_Code_Lookup()
		{
			this.ExecuteTestWithWorkingDirectory
			(
				workingDirectory =>
				{
					// Arrange
					var filePath = Path.Combine(workingDirectory.FullName, "errors.txt");
					var content = new string[]
					{
						"0x00001;Test1",
						"0x00002;Test2",
					};
					File.WriteAllLines(filePath, content);
					var errorFile = new FileInfo(filePath);
					var errorMapping = new AgLinkErrorMapping(errorFile);
					
					// Act
					var message = errorMapping.GetErrorMessageForCode(1);

					// Assert
					Assert.That(message, Is.EqualTo("Test1"));
				}
			);
		}
	}
}