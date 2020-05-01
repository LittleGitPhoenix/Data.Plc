# Phoenix.Data.Plc

Contains assemblies for communicating with a plc.
___

# Usage

To get data from or write data to a plc, two things are needed.

- An instance of a concrete [***IPlc***](#IPlc-Implementations).
- At least one [***IPlcItem***](#PlcItems).

The first is responsible for establishing the connection to the plc and how to read or write.  
The later defines what kind of data and where to read it from or write it to the plc.

___

# IPlc Implementations

The following concrete implementations for accessing a plc are currently available as separate NuGet packages:

## Plc.Mock

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This provides a mocked plc, that stores its data in-memory. It can be used for test and simulation purposes.

### Initialization

When creating an instance of the **_MockPlc_** class, initially available datablocks can be specified optionally. This implementation automatically creates or expands datablocks as they are accessed. 

``` csharp
var initialDataBlocks = new Dictionary<ushort, byte[]>()
      {
        {65, new byte[] {0,1,2,3, 255} },
        {1245, new byte[] {0,1,2,3, 255} },
      };
IPlc plc = new MockPlc(initialDataBlocks);
```

:grey_exclamation: Like all other ***IPlc*** implementations this one has to be disposed once it is not used anymore.

## Plc.AgLink

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This implementation utilizes the proprietary **AGLink** owned by **Delta Logic** for communicating with the plc via **S7 TCP/IP**.

:grey_exclamation: **AGLink** is a commercial product owned by **Delta Logic**. Using the ***Plc.AgLink*** package requires their software, so make sure you are allowed to.

The ***Plc.AgLink*** assembly itself only references the .Net wrapper assembly **AGL4DotNET.4.dll** from the trial version of **AGLink** in order to properly compile. This wrapper assembly will not be copied to the output folder during compilation, nor is it part of the created **NuGet** package. **AGLink** additionally requires some native assemblies on top of the wrapper assembly in order to work properly. Those are also not part of the ***Plc.AgLink*** project and must be provided by [other means](#Providing-AGLink-requisites).

### Providing AGLink requisites

The ***Plc.AgLink.Demo*** project can be used as a template for providing all additional assemblies and other files that **AGLink** requires. It has a folder _Resources\AgLink_ that contains all files from the trial version. Those could be replaced by registered versions of the same assemblies and additional files. The license code of the commercial version can be saved in the _AGLink.license_ file, that ***Plc.AgLink*** will consume upon startup. 

So basically ***Plc.AgLink.Demo*** is just a container for all the required **AGLink** files and doesn't have any executable code itself. This project will output a separate **NuGet** package that can be easily referenced and will copy all files into the output folder of the referencing project.

### Initialization

To create an instance of the ***AGLinkPlc*** class, the ***AgLinkPlcConnectionData*** has to be supplied. It basically contains the ip address and some additional information about the plc.

``` csharp
var connectionData = new AgLinkPlcConnectionData(deviceNumber: 0, ip: "127.0.0.2", rack: 0, slot: 0);
IPlc plc = new AgLinkPlc(connectionData);
```

:grey_exclamation: Like all other ***IPlc*** implementations this one has to be disposed once it is not used anymore.
___

# PlcItems

An **_IPlcItem_** contains all data needed to read or write to the plc.

| Property | Data Type | Description |
| :- | :- | :- |
| Type | Enum ***PlcItemType*** | Input, Output, Flags, Data |
| DataBlock | **UInt16** | The datablock of the item. This is 0 for all types except ***PlcItemType.Data***. |
| Position | **UInt16** | The zero-based byte-position. |
| BitPosition | Enum ***BitPosition*** | X0, X1, ... , X7 |
| Value | **_BitCollection_** | Specialized class that holds the bits and bytes of the item. |


To make working with plc items easier, specialized items for the most common data types exist in the namespace **_PhoenixPlc.Items.Typed_**. Those items automatically convert the underlying **_BitCollection_** into more concrete types:

- BitPlcItem
- BytesPlcItem
- EnumPlcItem
- Int16PlcItem
- Utf8PlcItem
- WordPlcItem
- [DynamicPlcItems](#DynamicPlcItems)
- ...

## Initialization

Creating a new ***IPlcItem*** can be done either by using the constructor of any concrete item or more guided via builder pattern.

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

Reading or writing is done via the following methods of any **_IPlc_** instance:

**Read a collection of _IPlcItem_'s from the plc.**
``` csharp
Task ReadItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
```

**Write a collection of _IPlcItem_'s to the plc.**
``` csharp
Task<bool> WriteItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
```

As to not pollute the **_IPlc_** interface with unnecessary methods, some extension methods for other common read or write operations are provided via the **_PlcExtensions_** class:

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

## DynamicPlcItems

Those are special ***PlcItems*** that can be used for dynamic data where the length of the item is not known during design time but rather encoded within the first few bytes of the item itself. A typical usage scenario are strings of different sizes, where the actual string length is the first byte of the item itself.

Each ***IDynamicPlcItem*** internally consists of two separate ***PlcItems***.

|  |  |
| :- | :- |
| ***LengthPlcItem*** | This is the item whose value is the actual length of the second item. The ***LengthPlcItem*** itself has a fixed size and must be an ***INumericPlcItem***. |
| ***FlexiblePlcItem*** | This is the item whose length is dynamic. It can be any normal ***IPlcItem*** but actually only ***BytesPlcItem*** and ***TextPlcItem*** are currently implemented as dynamic items. |

Reading and writing an ***IDynamicPlcItem*** always consists of two steps. When reading such an item the ***LengthPlcItem*** will be read first to obtain the current length and afterwards the data of the ***FlexiblePlcItem*** is obtained. Writing is the opposite.

:grey_exclamation: Since reading and writing is done in two steps, it cannot be guaranteed, that the data of an ***IDynamicPlcItem*** is consistent.

Dynamic items additionally provide some special properties that may come in handy under certain conditions.

|  |  |
| :- | :- |
| ***LengthFactor*** | This factor will be applied to the length of a dynamic item. It should be used if the ***LengthPlcItem*** does not provide an absolute byte amount, but rather an amount of items. |
| ***LengthLimit*** | This is an optional limit that will be applied to the length being read or written. |
___

# PlcMonitor

Sometimes it may be necessary to monitor data within a plc and react if this data changes. This can be done with one of the concrete [***IPlcMonitor*** Implementations](#IPlcMonitor-Implementations). The ***IPlcMonitor*** can be used on its own, or wrapped together with an ***IPlc*** as an ***IMonitorablePlc***. Later should be used, if monitoring data is done regularly. This way the two dependencies ***IPlc*** nad ***IPlcMonitor*** can be replaced by just a single ***IMonitorablePlc***.

___

# IPlcMonitor Implementations

## PollingPlcMonitor

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This monitor internally uses an ***IPlc*** and regularly polls monitored ***IPlcItems*** and checks if their data has changed. It provides the ***PollingMonitorablePlc*** wrapper class that combines both the plc and the monitor instance.

### Polling Frequency

The frequency at witch ***IPlcItems*** are polled has a default value of _300 milliseconds_. This is defined by ***PlcItemMonitorConfigurations.DefaultPollingFrequency***. Although this interval should be sufficient enough for most cases, it can be changed to a different value.

:grey_exclamation: Changing the default monitoring frequency does not change the interval at which already monitored items are polled.  
:grey_exclamation: The minimum polling frequency is _50 milliseconds_ and cannot be undershot.

In case the normal frequency is good enough expect for some critical items, the ***PollingPlcMonitor*** accepts special ***PlcItemMonitorConfigurations*** that define different frequencies for named ***IPlcItems***. A single ***PlcItemMonitorConfiguration*** for an ***IPLcItem*** simply consists of the ***IPlcItem.Identifier*** and a custom polling interval that will be applied to the it. The ***PlcItemMonitorConfigurations*** are implicitly convertible from a **Dictionary<string, uint>**, so it is possible to pass one such dictionary to the constructor of the ***PollingPlcMonitor***. This is done so custom configuration can be stored externally (in some kind of settings file) without the storage provider needing to know anything about the special configuration classes.

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

The ***Phoenix.Data.Plc*** package provides its own small logging facility in form the ***ILogger*** interface and the static ***LogManager*** which is internally used to provide concrete logger instances. Via the static property ***LogManager.LoggerFactory*** the kind of provided ***ILogger*** can be changed externally. The default implementation of the logger factory will return a simple null object. For forwarding the log messages a simple adapter implementing the ***ILogger*** interface can be created and then supplied to the factory property.

The ***LogManager*** has another static property ***LogAllReadAndWriteOperations*** which instructs the ***Plc*** base class to log all read and write operations. This is disabled by default, as it could be a very costly operation depending on the amount of operations.
___

# Authors

* **Felix Leistner** - _Initial release_