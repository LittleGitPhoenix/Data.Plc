# Phoenix.Data.Plc

Contains assemblies for communicating with a plc.
___

# Table of content

[toc]
___

# Usage

To get data from or write data to a plc, two things are needed.

- An instance of a specific [`IPlc`](#Plc).
- At least one [`IPlcItem`](#PlcItems).

The first is responsible for establishing the connection to the plc and how to read or write. The later defines what kind of data and where to read it from or write it to the plc.
___

# Plc

The following specific implementations for accessing a plc are currently available as separate NuGet packages:



## Plc.Mock

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

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

> [!WARNING]
>
> Like all other `IPlc` implementations this one has to be disposed once it is not used anymore.



## Plc.AgLink

> [!Caution]
> **AGLink** as a product is no longer available because **Delta Logic** was bought by **Softing Industrial** which bought rebranded it under the name **siAccess**. Use the new package [**Phoenix.Data.Plc.SiAccess**](#Plc.SiAccess) instead.



## Plc.SiAccess

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

This implementation utilizes the proprietary **siAccess** library owned by **Softing Industrial** for communicating with the plc via **S7 TCP/IP**.

> [!Caution]
> **siAccess** is a commercial product owned by **Softing Industrial**. Using the **Plc.SiAccess** package requires their software, so make sure you are allowed to.

### Build

> [!Caution]
> This repository does not contain the necessary **siAccess** libraries and auxiliary files needed to build the projects. Those files must be provided individually.



#### Plc.SiAccess

To get the project **Plc.SiAccess** to build, at least the .Net wrapper assembly `siAccess_DotNET.dll` must be put into the `⬙\siAccess` folder of the project. Optionally the `siAccess_DotNET.xml` documentation file can be added too. Afterwards this project should be compilable. Bear in mind that the wrapper assembly will not be copied to the output folder during compilation, nor will it be part of the created **NuGet** package. Its only purpose within the **Plc.SiAccess** project is to get it to properly build. Supplying all needed **siAccess** libraries and files must be done via other means. One way is described [below](#Plc.SiAccess.Demo).



#### Plc.SiAccess.Demo

**siAccess** requires some additional files to properly run. The below table lists all those files.

|         File          | Required |      Origin      |                       Description                        |
| :-------------------: | :------: | :--------------: | :------------------------------------------------------: |
| `siAccess_DotNET.dll` |    ☑     | siAccess package |                  .Net wrapper assembly                   |
| `siAccess_DotNET.xml` |    ➖     | siAccess package |         .Net documentation file (can be omitted)         |
|  `siAccess_x86.dll`   |    ☑     | siAccess package |    Native connection assembly for 32 bit architecture    |
|  `siAccess_x64.dll`   |    ☑     | siAccess package |    Native connection assembly for 64 bit architecture    |
| `siAccess_Error.txt`  |    ➖     | siAccess package | Contains error code to message mapping (can be omitted)  |
|  `siAccess.license`   |    ❕     |      custom      | Contains the license key (can, but shouldn't be omitted) |

The abstract `SiAccessPlc` base class from **Phoenix.Data.Plc.SiAccess** has to be inherited by a custom class in implementing code whose sole purpose should be, to provide the necessary files for **siAccess** to run. The proposed way to provide those files is creating a separate project that contains a custom class inheriting from `SiAccessPlc` as well as all required files as an embedded resource of that assembly. The main Project ***Phoenix.Data.Plc.SiAccess*** has two helper classes that have been created to help in this case.

The first is **`SiAccessRequirementsHelper`** which has the following static methods that should be called for providing the requirements:

- Copy the unmanaged assembly to the working directory. This method also sets a callback so that the unmanaged assembly file will be automatically deleted once the process exits.
	
	````csharp
	static void CopyUnmanagedAssemblies(FileInfo assemblyFile, Stream assemblyContent)
	````
	
- 	Provide the license key for activation. This can be omitted, in which case **siAccess** would operate in demo mode.
	
	```csharp
	static bool Activate(string? licenseKey)
	```
	
- 	Override the default null-object error mapping. This can be omitted, in which case error codes from **siAccess** cannot be translated into readable messages.
	
	````csharp
	static void OverrideErrorMapping(SiAccessErrorMapping errorMapping)
	````

The second one **`SiAccessPlcEmbeddedRequirementsHelper`** is for dealing with locating and extracting embedded resources from an assembly. It has the following static, but protected properties.

> Those functions are protected so **IntelliSense** is not  cluttered with too much functions. Therefore, `SiAccessPlcEmbeddedRequirementsHelper` must be inherited.

- Gets a list of all embedded resources of an assembly.
	
	````csharp
	static ICollection<string> GetAlResourceNames(Assembly? assembly = null, bool writeNamesToDebug = false)
	````
	
- Loads the internal resource as a string. Can be used for the license key and the error messages.
	
	````csharp
	static string? LoadInternalResourceAsString(string? resourceName, Assembly? assembly = null)
	````
	
- Loads the internal resource as a stream. Can be used for the unmanaged assembly.
	
	````csharp
	static System.IO.Stream? LoadInternalResourceAsStream(string? resourceName, Assembly? assembly = null)
	````

The **Phoenix.Plc.SiAccess.Demo** project contains the [**`DemoSiAccessPlc`**](./src/Plc.SiAccess.Demo/DemoSiAccessPlc.cs) example class that demonstrates how to setup such a custom implementation where the required files are an embedded resources of the project. In its static constructor all requirements are provided with the help of the above described `SiAccessRequirementsHelper` and a custom implementation of `SiAccessPlcEmbeddedRequirementsHelper` named [**`DemoSiAccessPlcResourceHelper`**](./src/Plc.SiAccess.Demo/DemoSiAccessPlcResourceHelper.cs). Below is the code of both classes.



### Initialization

To create an instance of any `SiAccessPlc` class, special `ISiAccessPlcConnectionData` has to be supplied. It basically contains the IP address and some additional information about the plc.

``` csharp
var connectionData = new SiAccessPlcConnectionData
(
	deviceNumber: 0,
	ip: "127.0.0.2",
	rack: 0,
	slot: 0
);
IPlc plc = new DemoSiAccessPlc(connectionData);
```

> [!Important]
> Like all other `IPlc` implementations this one has to be disposed once it is not used anymore.
___

# PlcItems

An `IPlcItem` contains all data needed to read or write to the plc.

| Property | Data Type | Description |
| :- | :- | :- |
| `Type` | Enum `PlcItemType` | `Input`, `Output`, `Flags`, `Data` |
| `DataBlock` | `UInt16` | The data block of the item. This is 0 for all types except `PlcItemType.Data`. |
| `Position` | `UInt16` | The zero-based byte-position. |
| `BitPosition` | Enum `BitPosition` | `X0`, `X1`, ... , `X7` |
| `Value` | `BitCollection` | Specialized class that holds the bits and bytes of the item. |


To make working with plc items easier, specialized items for the most common data types exist in the namespace `Phoenix.Data.Plc.Items.Typed`. Those items automatically convert the underlying `BitCollection` into more specific types. A list of all available items can be found [here](#Typed-plc-items).



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



To not pollute the `IPlc` interface with unnecessary methods, extension methods for other common read or write operations are provided via the `PlcExtensions` class:

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
- `ValidItems`: This is a pure collection of `IPlcItems`that succeeded.
- `FailedItems`: This is a tuple containing the failed `IPlcItem`together with an error message.

> [!Caution]
> Trying to read or write anything to/from an `IPlc` inheriting from the `Plc` base class that is currently **not connected** to a plc, will not fail. Rather all `IPlcItems` that are affected will be put on hold until the connection has been established. This means that calling `await plc.ReadItemsAsync(...)` or `await plc.WriteItemsAsync(...)` won't return until the connection is up and running.

> [!Caution]
> Trying to read or write anything to/from an `IPlc` inheriting from the `Plc` base class that has been disposed will result in a `ReadOrWritePlcException`. The same holds true for pending read or write operations (e.g. from `IPlcItems` that have been put on hold).



## Typed plc items

### Data plc items

- **`BitPlcItem`**

	Has an extension method `SetValue` that allows to use a string for setting the value of the item. Every value except the following will be treated as **false** (comparison is case-insensitive): **true**, **yes**, **ok**, **1**.

- **`BitsPlcItem`**

- **`BytePlcItem`**

- **`BytesPlcItem`**

	Has an extension method `SetValue` that allows to use a string in **Hex** or **byte array** format for setting the value of the item. Acceptable string values are:

	- **HEX**
		- Can be prefixed with either **0x**, **#** or nothing.
		- The values can be chained together or separated by either **,** **;** **-** or **whitespace**.
	- **Bytes**
		- No prefix allowed here.
		- The values must be separated by either **,** **;** **-** or **whitespace**.

- **`DynamicBytesPlcItem`** (see [DynamicPlcItems](#DynamicPlcItems) for more information)

### Numeric plc items

- **`Int16PlcItem`**
- **`Int32PlcItem`**
- **`Int64PlcItem`**
- **`UInt16PlcItem`**
- **`UInt32PlcItem`**
- **`UInt64PlcItem`**
- **`WordPlcItem`**
- **`DWordPlcItem`**
- **`LWordPlcItem`**

### Text plc items

Those items typically inherit from `TextPlcItem` and provide different text encodings.

- **`Utf8PlcItem`**
- **`DynamicUtf8PlcItem`** (see [DynamicPlcItems](#DynamicPlcItems) for more information)

### Other plc items

- **`EnumPlcItem`**



## DynamicPlcItems

Those are special `PlcItems` that can be used for dynamic data where the length of the item is not known during design time but rather encoded within the first few bytes of the item itself. A typical usage scenario are strings of different sizes, where the actual string length is the first byte of the item itself.

Each `IDynamicPlcItem` internally consists of two separate `PlcItems`.

|  | Description |
| :- | :- |
| `LengthPlcItem` | This is the item whose value is the actual length of the second item. The `LengthPlcItem` itself has a fixed size and must be an `INumericPlcItem`. |
| `FlexiblePlcItem` | This is the item whose length is dynamic. It can be any normal `IPlcItem` but actually only `BytesPlcItem` and `TextPlcItem` are currently implemented as dynamic items. |

Reading and writing an `IDynamicPlcItem` always consists of two steps. When reading such an item the `LengthPlcItem` will be read first to obtain the current length. Afterwards the data of the `FlexiblePlcItem` is obtained. Writing is the opposite.

> [!Important]
> Since reading and writing is done in two steps, it cannot be guaranteed, that the data of an `IDynamicPlcItem` is consistent.

Dynamic items additionally provide some special properties that may come in handy under certain conditions.

|  | Description |
| :- | :- |
| `LengthFactor` | This factor will be applied to the length of a dynamic item. It should be used if the `LengthPlcItem` does not provide an absolute byte amount, but rather an amount of items. |
| `LengthLimit` | This is an optional limit that will be applied to the length being read or written. |

___

# PlcMonitor

Sometimes it may be necessary to monitor data within a plc and react if this data changes. This can be done with one of the specific [`IPlcMonitor` Implementations](#Implementations). The `IPlcMonitor` can be used on its own, or wrapped together with an `IPlc` as an `IMonitorablePlc`. Later should be used, if monitoring data is done regularly. This way the two dependencies `IPlc` and `IPlcMonitor` can be replaced by just a single `IMonitorablePlc`.



## Implementations



### PollingPlcMonitor

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

This monitor internally uses an `IPlc` and regularly polls monitored `IPlcItems` and checks if their data has changed. It provides the `PollingMonitorablePlc` wrapper class that combines both the plc and the monitor instance.

#### Polling Frequency

The frequency at witch `IPlcItems` are polled has a default value of _300 milliseconds_. This is defined by `PlcItemMonitorConfigurations.DefaultPollingFrequency`. Although this interval should be sufficient enough for most cases, it can be changed to a different value.

> [!CAUTION]
> Changing the default monitoring frequency does not change the interval at which already monitored items are polled.

> [!CAUTION]
> The minimum polling frequency is _50 milliseconds_ and cannot be undershot.

In case the normal frequency is good enough expect for some critical items, the `PollingPlcMonitor` accepts special `PlcItemMonitorConfigurations` that define different frequencies for named `IPlcItems`. A single `PlcItemMonitorConfiguration` for an `IPlcItem` simply consists of the `IPlcItem.Identifier` and a custom polling interval that will be applied to it. The `PlcItemMonitorConfigurations` are implicitly convertible from a `Dictionary<string, uint>`, so it is possible to pass one such dictionary to the constructor of the `PollingPlcMonitor`. This is done so custom configuration can be stored externally (in some kind of settings file) without the storage provider needing to know anything about the special configuration classes.

#### Initialization

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

The ***Phoenix.Data.Plc*** package provides its own small logging facility in form the `ILogger` interface and the static `LogManager` which is internally used to provide specific logger instances. Via the static property `LogManager.LoggerFactory` the logger can be changed externally.

```csharp
// Create a custom ILogger instance and let the log manager use it.
Phoenix.Data.Plc.Logging.ILogger logger = new ...
Phoenix.Data.Plc.Logging.LogManager.LoggerFactory = () => logger;
```

***Phoenix.Data.Plc*** comes with the following `ILogger` implementations:

- `NullLogger`: An implementation that does nothing. This is the default.
- `DebugLogger`: An implementation that uses **System.Console** to output log messages.
- `TraceLogger`: An implementation that uses **System.Diagnostics.Trace** to output log messages.

The `LogManager` has another static property `LogAllReadAndWriteOperations` which instructs the `Plc` base class to log all read and write operations. This is disabled by default, as it could be a very costly operation depending on the amount of operations. In user code a conditional like the following could be used:

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

The ***Phoenix.Data.Plc.Mock*** package contains a static helper class `ByteArrayExtensions` providing extension methods intended to help manipulating data with byte arrays. Those methods allow to automatically convert basic data types into byte data and then writing this data to any byte array. For values that surpass the size of a single byte, the corresponding endianness has to be specified.

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

* **Felix Leistner**: _v1.x_ - _v7.x_