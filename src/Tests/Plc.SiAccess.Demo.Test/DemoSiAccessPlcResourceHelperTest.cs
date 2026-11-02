namespace Phoenix.Data.Plc.SiAccess.Test;

[TestFixture]
public sealed class DemoSiAccessPlcResourceHelperTest
{
	[Test]
	[TestCase(true)]
	[TestCase(false)]
	public void Check_If_Embedded_Unmanaged_Assembly_Is_Loaded(bool is64BitProcess)
	{
		// Act
		var (file, resourceStream) = DemoSiAccessPlcResourceHelper.LoadUnmanagedAssembly(is64BitProcess);

		// Assert
		Assert.NotNull(file);
		Assert.NotNull(resourceStream);
	}
		
	[Test]
	public void Check_If_Embedded_License_File_Is_Loaded()
	{
		// Act
		var licenseKey = DemoSiAccessPlcResourceHelper.LoadLicenseKey();

		// Assert
		Assert.NotNull(licenseKey);
	}
		
	[Test]
	public void Check_If_Embedded_Error_File_Is_Loaded()
	{
		// Act
		var errorFileContent = DemoSiAccessPlcResourceHelper.LoadErrorMapping();

		// Assert
		Assert.IsNotEmpty(errorFileContent);
	}
}