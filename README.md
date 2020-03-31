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

This implementation utilizes **AGLink** assemblies for communicating with the plc via **S7 TCP/IP**. Since those assemblies are proprietary and owned by **Delta Logic**, the **_Plc.AgLink_** assembly itself is just a wrapper and cannot be used on its own. Fortunately **Delta Logic** provides a trial version of their communication assembly. Those trial assemblies are implemented in the separate **_Plc.AgLink.Demo_** package.

### Initialization

To create an instance of any concrete **_AGLinkPlc_** class (like **_DemoAgLinkPlc_**), the **_AgLinkPlcConnectionData_** has to be supplied. It basically contains the ip address and some additional information about the plc.

``` csharp
var connectionData = new AgLinkPlcConnectionData(name: "AGLink@PLC", ip: "127.0.0.2", rack: 0, slot: 0);
IPlc plc = new DemoAgLinkPlc(connectionData);
```

:grey_exclamation: Like all other ***IPlc*** implementations this one has to be disposed once it is not used anymore.
___

# PlcItems

An **_IPlcItem_** wraps all data needed to read or write to the plc.
- Type: **_PlcItemType_** (Input | Output | Flags | Data)
- DataBlock
- Position
- BitPosition
- Value: **_BitCollection_** (a specialized class that holds the real bits and bytes of the item)

To make working with those plc items easier, specialized items for the most common data types exist in the namespace **_PhoenixPlc.Items.Typed_**. Those items automatically convert the underlying **_BitCollection_** into more concrete types:

- BitPlcItem
- BytesPlcItem
- EnumPlcItem
- Int16PlcItem
- Utf8PlcItem
- WordPlcItem
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