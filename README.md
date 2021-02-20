# Phoenix.Data.Plc

Contains assemblies for communicating with a plc.
___

# Table of content

[toc]
___

# Usage

To get data from or write data to a plc, two things are needed.

- An instance of a specific [`IPlc`](#IPlc-Implementations).
- At least one [`IPlcItem`](#PlcItems).

The first is responsible for establishing the connection to the plc and how to read or write.  
The later defines what kind of data and where to read it from or write it to the plc.
___

# IPlc Implementations

The following specific implementations for accessing a plc are currently available as separate NuGet packages:

## Plc.Mock

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

This provides a mocked plc, that stores its data in-memory. It can be used for test and simulation purposes.

### Initialization

When creating an instance of the `MockPlc` class, initially available datablocks can be specified optionally. This implementation automatically creates or expands datablocks as they are accessed. 

``` csharp
var initialDataBlocks = new Dictionary<ushort, byte[]>()
	{
		{65, new byte[] {0,1,2,3, 255} },
		{1245, new byte[] {0,1,2,3, 255} },
	};
IPlc plc = new MockPlc(initialDataBlocks);
```

:grey_exclamation: Like all other `IPlc` implementations this one has to be disposed once it is not used anymore.

## Plc.AgLink

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

This implementation utilizes the proprietary **AGLink** library owned by **Delta Logic** for communicating with the plc via **S7 TCP/IP**.

:grey_exclamation: **AGLink** is a commercial product owned by **Delta Logic**. Using the ***Plc.AgLink*** package requires their software, so make sure you are allowed to.

### Build

:grey_exclamation: This repository does not contain the necessary **AGLink** libraries and auxiliary files needed to build the projects. Those files must be provided individually.

#### Plc.AgLink

To get the project ***Plc.AgLink*** to build, at least the .Net wrapper assembly **AGL4DotNET.4.dll** must be put into the _⬙\AgLink_ folder of the project. Optionally the **AGL4DotNET.4.xml** documentation file can be added too. Afterwards this project should be compilable. Bear in mind that the wrapper assembly will not be copied to the output folder during compilation, nor will it be part of the created **NuGet** package. Its only purpose within the ***Plc.AgLink*** project is to get it to properly build. Supplying all needed **AGLink** libraries and files must be done via other means. One way is described [below](#Plc.AgLink.Demo).

#### Plc.AgLink.Demo

**AGLink** requires some additional files to properly run. The below table lists all those files.

|        File        |      Required      |     Origin     |                       Description                        |
| :----------------: | :----------------: | :------------: | :------------------------------------------------------: |
|  AGL4DotNET.4.dll  | :heavy_check_mark: | AGLink package |                  .Net wrapper assembly                   |
|  AGL4DotNET.4.xml  | :heavy_minus_sign: | AGLink package |         .Net documentation file (can be omitted)         |
|    AGLink40.dll    | :heavy_check_mark: | AGLink package |    Native connection assembly for 32 bit architecture    |
|  AGLink40_x64.dll  | :heavy_check_mark: | AGLink package |    Native connection assembly for 64 bit architecture    |
| AGLink40_Error.txt | :heavy_minus_sign: | AGLink package |  Contains error code to message mapping(can be omitted)  |
|   AGLink.license   | :grey_exclamation: |     custom     | Contains the license key (can, but shouldn't be omitted) |

Prior to ***Phoenix.Data.Plc.AgLink v4.x*** those files where provided via a custom build target. The demo project ***Phoenix.Plc.AgLink.Demo*** contained an example how to do this in a custom project. This mechanism has been deprecated, because of the new way **.Net 5** handles execution of applications published as single file. The result was a major change in the architecture of the `AgLinkPlc` class. Formerly this class was responsible for loading the unmanaged assemblies, the license key and even the error file. This no longer applies.

`AgLinkPlc` is now an abstract base class that has to be inherited by a custom class whose sole purpose should be, to provide the necessary files for **AGLink** to run. The proposed way of providing those files is creating a new project containing the inheriting class of `AgLinkPlc` along with all required files as an embedded resource of that assembly. The main Project ***Phoenix.Data.Plc.AgLink*** has two helper classes that have been created to help in this case.

The first is `AgLinkRequirementsHelper` which has the following static methods that should be called for providing the requirements:

- ````csharp
static void CopyUnmanagedAssemblies(FileInfo assemblyFile, Stream assemblyContent)
	````
	Copy the unmanaged assembly to the working directory. The unmanaged assembly will be deleted automatically once the application ends. This is done by `AgLinkPlc`.
	
- 	```csharp
	static bool ActivateAgLink(string? licenseKey)
	```
	Provide the license key for activation. This can be omitted, in which case **AGLink** would operate in demo mode.

- 	````csharp
	static void OverrideErrorMapping(AgLinkErrorMapping agLinkErrorMapping)
	````
	Override the default null-object error mapping. This can be omitted, in which case error codes from **AGLink** cannot be translated into readable messages.

The second one `AgLinkPlcEmbeddedRequirementsHelper` is for dealing with locating and extracting embedded resources from an assembly. It has the following static, but protected properties.

> Those functions are protected as to not clutter **IntelliSense** with too much functions. Therefore `AgLinkPlcEmbeddedRequirementsHelper` must be inherited.

- ````csharp
	static ICollection<string> GetAlResourceNames(Assembly? assembly = null, bool writeNamesToDebug = false)
	````
	Gets a list of all embedded resources of an assembly.

- ````csharp
	static string? LoadInternalResourceAsString(string? resourceName, Assembly? assembly = null)
	````
	Loads the internal resource as a string. Can be used for the license key and the error messages.

- ````csharp
	static System.IO.Stream? LoadInternalResourceAsStream(string? resourceName, Assembly? assembly = null)
	````
	Loads the internal resource as a stream. Can be used for the unmanaged assembly.

Like before the project ***Phoenix.Plc.AgLink.Demo*** shows an example on how best to setup a custom implementation where the required files are an embedded resources of the project. The new sub-class **`DemoAgLinkPlc`** in this project provides all requirements from its static constructor with the help of the above described `AgLinkRequirementsHelper` and a custom implementation of `AgLinkPlcEmbeddedRequirementsHelper` named **`DemoAgLinkPlcEmbeddedRequirementsHelper`**. Below is the code of both classes.

Here the `DemoAgLinkPlc` class provides the resources for **AGLink**:

```csharp
public sealed class DemoAgLinkPlc : AgLinkPlc
{
	static DemoAgLinkPlc()
	{
		// Extract the unmanaged assembly.
		var (unmanagedAgLinkAssemblyFile, resourceStream) = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadUnmanagedAssembly();
		AgLinkRequirementsHelper.CopyUnmanagedAssemblies(unmanagedAgLinkAssemblyFile, resourceStream);

		// Try to get the license key and activate AGLink.
		var licenseKey = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadLicenseKey();
		AgLinkRequirementsHelper.ActivateAgLink(licenseKey);

		// Override error mapping from base class.
		var errorFileContent = DemoAgLinkPlcEmbeddedRequirementsHelper.LoadErrorMapping();
		AgLinkRequirementsHelper.OverrideErrorMapping(new AgLinkErrorMapping(errorFileContent));
	}
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="connectionData"> <see cref="IAgLinkPlcConnectionData"/> </param>
	public DemoAgLinkPlc(IAgLinkPlcConnectionData connectionData)
		: base(connectionData) { }
}
```

`DemoAgLinkPlcEmbeddedRequirementsHelper` loads the embedded resources:

```csharp
internal sealed class DemoAgLinkPlcEmbeddedRequirementsHelper : AgLinkPlcEmbeddedRequirementsHelper
{
	#region Properties

	/// <summary> The <see cref="Assembly"/> of the this class. </summary>
	private static Assembly CurrentAssembly { get; }

	/// <summary> The names of all embedded resources of this assembly. Obtained via <see cref="AgLinkPlcEmbeddedRequirementsHelper.GetAlResourceNames"/>. </summary>
	private static ICollection<string> ResourceNames { get; }
	
	#endregion

	#region (De)Constructors

	static DemoAgLinkPlcEmbeddedRequirementsHelper()
	{
		CurrentAssembly = Assembly.GetExecutingAssembly();
		ResourceNames = AgLinkPlcEmbeddedRequirementsHelper.GetAlResourceNames(CurrentAssembly);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Loads the embedded unmanaged assembly.
	/// </summary>
	/// <param name="is64BitProcess"> Optional boolean if the 64-bit version of the unmanaged assembly should be loaded. By default this is determined by <see cref="Environment.Is64BitProcess"/>. </param>
	/// <returns> The file name of the unmanaged assembly and its content as a <see cref="Stream"/> reset to <see cref="SeekOrigin.Begin"/>. This stream is not closed and needs to be disposed! </returns>
	internal static (string FileName, Stream? ResourceStream) LoadUnmanagedAssembly(bool? is64BitProcess = null)
	{
		is64BitProcess ??= Environment.Is64BitProcess;
		var currentAssembly = DemoAgLinkPlcEmbeddedRequirementsHelper.CurrentAssembly;
		var resourceNames = DemoAgLinkPlcEmbeddedRequirementsHelper.ResourceNames;
		var unmanagedAssemblyFileName = $"AGLink40{(is64BitProcess.Value ? "_x64" : "")}.dll";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(unmanagedAssemblyFileName.ToLower()));
		var resourceStream = AgLinkPlcEmbeddedRequirementsHelper.LoadInternalResourceAsStream(resourceName, currentAssembly);
		if (resourceStream?.CanSeek ?? false) resourceStream.Seek(0, SeekOrigin.Begin);
		return (unmanagedAssemblyFileName, resourceStream);
	}

	/// <summary>
	/// Loads the embedded license key.
	/// </summary>
	/// <returns> The license key or null. </returns>
	internal static string? LoadLicenseKey()
	{
		var currentAssembly = DemoAgLinkPlcEmbeddedRequirementsHelper.CurrentAssembly;
		var resourceNames = DemoAgLinkPlcEmbeddedRequirementsHelper.ResourceNames;
		var licenseFileName = "AGLink.license";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(licenseFileName.ToLower()));
		var licenseKey = AgLinkPlcEmbeddedRequirementsHelper.LoadInternalResourceAsString(resourceName, currentAssembly);
		return licenseKey;
	}

	/// <summary>
	/// Loads the embedded error file.
	/// </summary>
	/// <returns> The error file as single string or null. </returns>
	internal static string? LoadErrorMapping()
	{
		var currentAssembly = DemoAgLinkPlcEmbeddedRequirementsHelper.CurrentAssembly;
		var resourceNames = DemoAgLinkPlcEmbeddedRequirementsHelper.ResourceNames;
		var errorFileName = "AGLink40_Error.txt";
		var resourceName = resourceNames.FirstOrDefault(name => name.ToLower().Contains(errorFileName.ToLower()));
		var errorFileContent = AgLinkPlcEmbeddedRequirementsHelper.LoadInternalResourceAsString(resourceName, currentAssembly);
		return errorFileContent;
	}

	#endregion
}
```

### Initialization

To create an instance of any `AGLinkPlc` class, the `AgLinkPlcConnectionData` has to be supplied. It basically contains the IP address and some additional information about the plc.

``` csharp
var connectionData = new AgLinkPlcConnectionData(deviceNumber: 0, ip: "127.0.0.2", rack: 0, slot: 0);
IPlc plc = new DemoAgLinkPlc(connectionData);
```

:grey_exclamation: Like all other `IPlc` implementations this one has to be disposed once it is not used anymore.
___

# PlcItems

An `IPlcItem` contains all data needed to read or write to the plc.

| Property | Data Type | Description |
| :- | :- | :- |
| Type | Enum `PlcItemType` | Input, Output, Flags, Data |
| DataBlock | `UInt16` | The datablock of the item. This is 0 for all types except `PlcItemType.Data`. |
| Position | `UInt16` | The zero-based byte-position. |
| BitPosition | Enum `BitPosition` | X0, X1, ... , X7 |
| Value | `BitCollection` | Specialized class that holds the bits and bytes of the item. |


To make working with plc items easier, specialized items for the most common data types exist in the namespace ***Phoenix.Data.Plc.Items.Typed***. Those items automatically convert the underlying `BitCollection` into more specific types. A hopefully complete list of all available items can be found [here](#Typed-plc-items).

## Initialization

Creating a new `IPlcItem` can be done either by using the constructor of any specific item or more guided via builder pattern.

**Constructor**
``` csharp
var item = new Utf8PlcItem(0, 4, 10, identifier: "UTF-8");
```

**Builder**
``` csharp
var itemBuilder = new Phoenix.Data.Plc.Items.Builder.PlcItemBuilder();
var item = itemBuilder
	.ConstructUtf8PlcItem("UTF-8")
	.AtDatablock(0)
	.AtPosition(4)
	.WithLength(10)
	.Build()
	;
```

## Reading / Writing plc items

Reading or writing is done via the following methods of any `IPlc` instance:

**Read a collection of _IPlcItem_'s from the plc.**
``` csharp
Task ReadItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
```

**Write a collection of _IPlcItem_'s to the plc.**
``` csharp
Task<bool> WriteItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
```

As to not pollute the `IPlc` interface with unnecessary methods, some extension methods for other common read or write operations are provided via the `PlcExtensions` class:

**Read a single _IPlcItem_ from the plc.**
``` csharp
Task<TValue> ReadItemAsync<TValue>(this IPlc plc, IPlcItem<TValue> plcItem, CancellationToken cancellationToken = default)
```
``` csharp
Task<BitCollection> ReadItemAsync(this IPlc plc, IPlcItem plcItem, CancellationToken cancellationToken = default)
```

**Write a single _IPlcItem_'s to the plc.**
``` csharp
Task<bool> WriteItemAsync(this IPlc plc, IPlcItem plcItem, CancellationToken cancellationToken = default)
```

**Write a single or multiple _IPlcItem_'s to the plc and validate the result.**
``` csharp
Task<bool> WriteItemWithValidationAsync(this IPlc plc, IPlcItem plcItem, CancellationToken cancellationToken = default)
```
``` csharp
Task<bool> WriteItemsWithValidationAsync(this IPlc plc, ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
```

## Read or write errors

Reading or writing can throw a `ReadOrWritePlcException` that should be handled by consuming code. This exception contains two collections identifying which items succeeded and which failed:
- ValidItems: This is a pure collection of `IPlcItems`that succeeded.
- FailedItems: This is a tuple containing the failed `IPlcItem`together with an error message.

:heavy_exclamation_mark: Trying to read or write anything to/from an `IPlc` inheriting from the `Plc` base class that is currently **not connected**, will not fail. Rather all `IPlcItems` that are affected will be put on hold until the connection has been established. This means that calling ```await plc.ReadItemsAsync(...)``` or ```await plc.WriteItemsAsync(...)``` won't return until the connection is up and running.

:heavy_exclamation_mark: Trying to read or write anything to/from an `IPlc` inheriting from the `Plc` base class that has been disposed will result in a `ReadOrWritePlcException`. The same goes for read or write operations whose `IPlcItems` have been put on hold.

## Typed plc items

### Data plc items

- BitPlcItem

	Has an extension method `SetValue` that allows to use a string for setting the value of the item. Every value except the following will be treated as **false** (comparison is case-insensitive): **true**, **yes**, **ok**, **1**.

- BitsPlcItem

- BytePlcItem

- BytesPlcItem

	Has an extension method `SetValue` that allows to use a string in **Hex** or **byte array** format for setting the value of the item. Acceptable string values are:

	- HEX
		- Can be prefixed with either **0x**, **#** or nothing.
		- The values can be chained together or separated by either **,** **;** **-** or **whitespace**.
	- Bytes
		- No prefix allowed here.
		- The values must be separated by either **,** **;** **-** or **whitespace**.

- DynamicBytesPlcItem (see [DynamicPlcItems](#DynamicPlcItems) for more information)

### Numeric plc items

- Int16PlcItem
- Int32PlcItem
- Int64PlcItem
- UInt16PlcItem
- UInt32PlcItem
- UInt64PlcItem
- WordPlcItem
- DWordPlcItem
- LWordPlcItem

### Text plc items

Those items typically inherit from `TextPlcItem` and provide different text encodings.

- Utf8PlcItem
- DynamicUtf8PlcItem (see [DynamicPlcItems](#DynamicPlcItems) for more information)

### Other plc items

- EnumPlcItem

## DynamicPlcItems

Those are special `PlcItems` that can be used for dynamic data where the length of the item is not known during design time but rather encoded within the first few bytes of the item itself. A typical usage scenario are strings of different sizes, where the actual string length is the first byte of the item itself.

Each `IDynamicPlcItem` internally consists of two separate `PlcItems`.

|  |  |
| :- | :- |
| `LengthPlcItem` | This is the item whose value is the actual length of the second item. The `LengthPlcItem` itself has a fixed size and must be an `INumericPlcItem`. |
| `FlexiblePlcItem` | This is the item whose length is dynamic. It can be any normal `IPlcItem` but actually only `BytesPlcItem` and `TextPlcItem` are currently implemented as dynamic items. |

Reading and writing an `IDynamicPlcItem` always consists of two steps. When reading such an item the `LengthPlcItem` will be read first to obtain the current length and afterwards the data of the `FlexiblePlcItem` is obtained. Writing is the opposite.

:grey_exclamation: Since reading and writing is done in two steps, it cannot be guaranteed, that the data of an `IDynamicPlcItem` is consistent.

Dynamic items additionally provide some special properties that may come in handy under certain conditions.

|  |  |
| :- | :- |
| `LengthFactor` | This factor will be applied to the length of a dynamic item. It should be used if the `LengthPlcItem` does not provide an absolute byte amount, but rather an amount of items. |
| `LengthLimit` | This is an optional limit that will be applied to the length being read or written. |

___

# PlcMonitor

Sometimes it may be necessary to monitor data within a plc and react if this data changes. This can be done with one of the specific [`IPlcMonitor` Implementations](#IPlcMonitor-Implementations). The `IPlcMonitor` can be used on its own, or wrapped together with an `IPlc` as an `IMonitorablePlc`. Later should be used, if monitoring data is done regularly. This way the two dependencies `IPlc` and `IPlcMonitor` can be replaced by just a single `IMonitorablePlc`.

___

# IPlcMonitor Implementations

## PollingPlcMonitor

| .NET Framework | .NET Standard | .NET |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 5.0 |

This monitor internally uses an `IPlc` and regularly polls monitored `IPlcItems` and checks if their data has changed. It provides the `PollingMonitorablePlc` wrapper class that combines both the plc and the monitor instance.

### Polling Frequency

The frequency at witch `IPlcItems` are polled has a default value of _300 milliseconds_. This is defined by `PlcItemMonitorConfigurations.DefaultPollingFrequency`. Although this interval should be sufficient enough for most cases, it can be changed to a different value.

:grey_exclamation: Changing the default monitoring frequency does not change the interval at which already monitored items are polled.  
:grey_exclamation: The minimum polling frequency is _50 milliseconds_ and cannot be undershot.

In case the normal frequency is good enough expect for some critical items, the `PollingPlcMonitor` accepts special `PlcItemMonitorConfigurations` that define different frequencies for named `IPlcItems`. A single `PlcItemMonitorConfiguration` for an `IPLcItem` simply consists of the `IPlcItem.Identifier` and a custom polling interval that will be applied to the it. The `PlcItemMonitorConfigurations` are implicitly convertible from a **Dictionary<string, uint>**, so it is possible to pass one such dictionary to the constructor of the `PollingPlcMonitor`. This is done so custom configuration can be stored externally (in some kind of settings file) without the storage provider needing to know anything about the special configuration classes.

### Initialization

**Create a new _PollingPlcMonitor_ instance.**
``` csharp
IPlc plc = new IPlc();
IPlcMonitor plcMonitor = new PollingPlcMonitor(plc);
```

**Create a new _PollingMonitorablePlc_ instance.**
``` csharp
IPlc plc = new IPlc();
IMonitorablePlc monitoredPlc = new PollingMonitorablePlc(plc);
```

**Create a new _PollingMonitorablePlc_ instance via fluent syntax.**
``` csharp
IMonitorablePlc monitoredPlc = new IPlc().MakeMonitorable();
```

**Create a new _PollingMonitorablePlc_ instance via fluent syntax and custom polling frequency configuration.**
``` csharp
IMonitorablePlc monitoredPlc = new MockPlc().MakeMonitorable
	(
		new Dictionary<string, uint>
		{
			{"FastItem", 200},
			{"LightspeedItem", 50},          
			{"SlowItem", 1000},
		}
	);
```
___

# Logging

The ***Phoenix.Data.Plc*** package provides its own small logging facility in form the `ILogger` interface and the static `LogManager` which is internally used to provide specific logger instances. Via the static property `LogManager.LoggerFactory` the kind of provided `ILogger` can be changed externally.

```csharp
// Create a custom ILogger instance and let the log manager use it.
Phoenix.Data.Plc.Logging.ILogger logger = new ...
Phoenix.Data.Plc.Logging.LogManager.LoggerFactory = () => logger;
```

***Phoenix.Data.Plc*** comes with the following `ILogger` implementations:

- `NullLogger`: An implementation that does nothing. This is the default.
- `TraceLogger`: An implementation that uses **System.Diagnostics.Trace** to output log messages.

The `LogManager` has another static property `LogAllReadAndWriteOperations` which instructs the `Plc` base class to log all read and write operations. This is disabled by default, as it could be a very costly operation depending on the amount of operations.

```csharp
// Conditionally log all read and write operations of the plc.
#if DEBUG
	Phoenix.Data.Plc.Logging.LogManager.LogAllReadAndWriteOperations = true;
#else
	Phoenix.Data.Plc.Logging.LogManager.LogAllReadAndWriteOperations = false;
#endif
```

___

# Helper

The ***Phoenix.Data.Plc.Mock*** package contains a static helper class `ByteArrayExtensions` that provides some extensions methods build to help manipulating data with byte arrays. Basically those methods allow for automatically converting basic data types into byte data and then writing this data to any byte array. For values that surpass the size of a single byte, the corresponding endianness has to be specified.

The following functions are available:

```cs
// Applies a boolean array.
var data = new byte[45];
var booleans = new bool[] { true, true, false, false };
data.ApplyValue(bytePosition: 10, booleans);
data.ApplyValue(bytePosition: 10, bitPosition: BitPosition.X3, booleans);
```
```cs
// Applies a byte.
var data = new byte[45];
var @byte = byte.MaxValue;
data.ApplyValue(bytePosition: 10, @byte);
```
```cs
// Applies another byte array.
var data = new byte[45];
var bytes = new byte[] {byte.MaxValue, byte.MaxValue};
data.ApplyValue(bytePosition: 10, bytes);
```
```cs
// Applies a short (Int16).
var data = new byte[45];
var @short = Int16.MinValue;
data.ApplyValue(bytePosition: 10, @short, DataConverter.Endianness.BigEndian);
```
```cs
// Applies an ushort (UInt16).
var data = new byte[45];
var @ushort = UInt16.MaxValue;
data.ApplyValue(bytePosition: 10, @ushort, DataConverter.Endianness.LittleEndian);
```
```cs
// Applies an int (Int32).
var data = new byte[45];
var @int = Int32.MinValue;
data.ApplyValue(bytePosition: 10, @int, DataConverter.Endianness.BigEndian);
```
```cs
// Applies an uint (UInt32).
var data = new byte[45];
var @uint = UInt32.MaxValue;
data.ApplyValue(bytePosition: 10, @uint, DataConverter.Endianness.LittleEndian);
```
```cs
// Applies a long (Int64).
var data = new byte[45];
var @long = Int64.MinValue;
data.ApplyValue(bytePosition: 10, @long, DataConverter.Endianness.BigEndian);
```
```cs
// Applies an ulong (UInt64).
var data = new byte[45];
var @ulong = UInt64.MaxValue;
data.ApplyValue(bytePosition: 10, @ulong, DataConverter.Endianness.LittleEndian);
```
```cs
// Applies a string.
var data = new byte[45];
var @string = "Foo";
data.ApplyValue(bytePosition: 10, @string, Encoding.ASCII);
```


Those methods directly operate on the original byte array and additionally also return it back to the caller which allows for chaining multiple commands.

```csharp
var data = new byte[45]
	.ApplyValue(bytePosition: 10, value: ushort.MaxValue, DataConverter.Endianness.BigEndian)
	.ApplyValue(bytePosition: 15, value: long.MaxValue, DataConverter.Endianness.LittleEndian)
	.ApplyValue(bytePosition: 30, value: "Bar", Encoding.ASCII)
	;
```

___

# Authors

* **Felix Leistner**: _v1.x_ - _v4.x_