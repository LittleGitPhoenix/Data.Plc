using System;
using NUnit.Framework;

namespace Phoenix.Data.Plc.AgLink.Test
{
	[TestFixture]
	public sealed class DemoAgLinkPlcResourceHelperTest
	{
		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Check_If_Embedded_Unmanaged_Assembly_Is_Loaded(bool is64BitProcess)
		{
			// Act
			var (file, resourceStream) = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadUnmanagedAssembly(is64BitProcess);

			// Assert
			Assert.NotNull(file);
			Assert.NotNull(resourceStream);
		}
		
		[Test]
		public void Check_If_Embedded_License_File_Is_Loaded()
		{
			// Act
			var licenseKey = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadLicenseKey();

			// Assert
			Assert.NotNull(licenseKey);
		}
		
		[Test]
		public void Check_If_Embedded_Error_File_Is_Loaded()
		{
			// Act
			var errorFileContent = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadErrorMapping();

			// Assert
			Assert.IsNotEmpty(errorFileContent);
		}
	}
}