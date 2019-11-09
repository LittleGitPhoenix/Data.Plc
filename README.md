# Phoenix.Data.Plc

Contains assemblies for communicating with a plc.
___

# Usage

## Plc

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

The **Phoenix.Data.Plc** assembly provides an interface to establish a connection to a plc. It also has asynchronous methods for reading and writing to the plc via so called [**_IPlcItem_**'s](#PlcItem).

The following concrete implementations are currently available as separate NuGet packages:

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

// Don't forget to call dispose.
plc.Dispose();
```

## Plc.AgLink

| .NET Framework | .NET Standard | .NET Core |
| :-: | :-: | :-: |
| :heavy_check_mark: 4.5.0 | :heavy_check_mark: 2.0 | :heavy_check_mark: 2.0 |

This implementation utilizes **AGLink** assemblies for communicating with the plc.

### Initialization

To create an instance of the **_AGLinkPlc_** class, the **_AgLinkPlcConnectionData_** has to be supplied. It basically contains the ip address and some additional information about the plc.

``` csharp
var connectionData = new AgLinkPlcConnectionData(name: "AGLink@1518", ip: "172.20.4.241", rack: 0, slot: 0);
IPlc plc = new AgLinkPlc(connectionData);

// Don't forget to call dispose.
plc.Dispose();
```

## PlcItem

An **_IPlcItem_** wraps all data needed to read or write to the plc.
- Type: **_PlcItemType_** (Input | Output | Flags | Data)
- DataBlock
- Position
- BitPosition
- Value: **_BitCollection_** (a specialized class that holds the real bits and bytes of the item)

To make working with those plc items easier, specialized items for the most common data types exist in the namespace **_PhoenixPlc.Items.Typed_**, that automatically convert the underlying **_BitCollection_** into more concrete types:
- BitPlcItem
- BytesPlcItem
- EnumPlcItem
- Int16PlcItem
- Utf8PlcItem
- WordPlcItem
- ...

### Initialization

Creating new instance can be done either by using the constructor of any concrete **PlcItem** or more guided via builder pattern.

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

### Reading / Writing plc items

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

## PlcMonitor

TODO...

# Authors

* **Felix Leistner** - _Initial release_