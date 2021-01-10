using System;
using NUnit.Framework;

namespace Phoenix.Data.Plc.AgLink.Test
{
	public class AgLinkErrorMappingTest
	{
		[Test]
		public void Check_Default_constructor_Yields_Nothing()
		{
			// Act
			var errorMapping = new AgLinkErrorMapping();

			// Assert
			Assert.IsEmpty(errorMapping.ErrorCodeMapping);
		}

		[Test]
		public void Check_Loading_Valid_Single_Error_File_Content()
		{
			// Arrange
			var content = "0x00001;Test1\n0x00002;Test2";

			// Act
			var errorMapping = new AgLinkErrorMapping(content);

			// Assert
			Assert.That(errorMapping.ErrorCodeMapping, Has.Count.EqualTo(2));
		}

		[Test]
		public void Check_Loading_Valid_Separated_Error_File_Content()
		{
			// Arrange
			var content = new string[]
			{
				"0x00001;Test1",
				"0x00002;Test2",
			};
			
			// Act
			var errorMapping = new AgLinkErrorMapping(content);

			// Assert
			Assert.That(errorMapping.ErrorCodeMapping, Has.Count.EqualTo(2));
		}
		
		[Test]
		public void Check_Loading_Empty_Single_Error_File_Content_Yields_Nothing()
		{
			// Arrange
			var content = String.Empty;

			// Act
			var errorMapping = new AgLinkErrorMapping(content);

			// Assert
			Assert.IsEmpty(errorMapping.ErrorCodeMapping);
		}

		[Test]
		public void Check_Loading_Empty_Separated_Error_File_Content_Yields_Nothing()
		{
			// Arrange
			var content = new string[0];

			// Act
			var errorMapping = new AgLinkErrorMapping(content);

			// Assert
			Assert.IsEmpty(errorMapping.ErrorCodeMapping);
		}

		[Test]
		public void Check_Duplicate_Entries_Are_Ignored()
		{
			// Arrange
			var content = new string[]
			{
				"0x00001;Test1",
				"0x00001;Test2",
			};

			// Act
			var errorMapping = new AgLinkErrorMapping(content);

			// Assert
			Assert.That(errorMapping.ErrorCodeMapping, Has.Count.EqualTo(1));
		}

		[Test]
		public void Check_Code_Lookup()
		{
			// Arrange
			var content = new string[]
			{
				"0x00001;Test1",
				"0x00002;Test2",
			}; 
			var errorMapping = new AgLinkErrorMapping(content);

			// Act
			var message = errorMapping.GetErrorMessageForCode(1);

			// Assert
			Assert.That(message, Is.EqualTo("Test1"));
		}

		[Test]
		[TestCase("\n")]
		[TestCase("\r")]
		public void Check_Message_Cleanup(string suffix)
		{
			// Arrange
			var content = new string[]
			{
				$"0x00001;Test1{suffix}",
			}; 
			var errorMapping = new AgLinkErrorMapping(content);

			// Act
			var message = errorMapping.GetErrorMessageForCode(1);

			// Assert
			Assert.That(message, Is.EqualTo("Test1"));
		}
	}
}