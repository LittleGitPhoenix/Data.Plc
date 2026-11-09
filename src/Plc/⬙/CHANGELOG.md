# Changelog

All notable changes to **Plc** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 6.3.0

📅 _2026-11-09_

### Changed

- `PlcItem.Value` is now a simple get-only auto property. The unnecessary lock around reading its (readonly) backing field was removed.

### Fixed

- `ReadItemsAsync` and `WriteItemsAsync` no longer change the collection that is passed by the caller. Previously dynamic items in a mutable collection (e.g. a `List<IPlcItem>`) were replaced by their internal parts (`LengthPlcItem` / `FlexiblePlcItem`) while reading or writing.
- Reading and writing dynamic items (`IDynamicPlcItem`) together with other items now reports failures correctly. Both operations work in two phases (read: length, then flexible part; write: flexible part, then length). Previously a failure aborted the operation and the `ReadPlcException` / `WritePlcException` only contained the items of the phase that failed. It also referred to the internal parts of dynamic items (`LengthPlcItem` / `FlexiblePlcItem`) instead of the dynamic item that was passed. Because of that `ReadOrWritePlcException.ValidItems` was incomplete (e.g. items that had been read successfully in the first phase were missing after a failure in the second phase), which made consumers like the polling monitor discard valid values.
  - Failures of both phases are now collected and thrown as a single exception, whose `ValidItems` and `FailedItems` only contain the items that were passed to `ReadItemsAsync` / `WriteItemsAsync` (or their single item counterparts).
  - A failure in the first phase of one dynamic item no longer prevents the second phase for the other dynamic items.
  - The length of a dynamic item whose flexible part could not be written is no longer written, so that the plc does not end up with a length that doesn't match its data.
___

## 6.2.0

📅 _2026-11-03_

### Add

- A new **`ConsoleLogger`** class was added that implements `ILogger` and logs messages via `System.Console`.

### References

🔵 System.Memory ~~4.5.5~~ → **4.6.3** (.NET Framework and .NET Standard only)  
___

## 6.1.0

📅 _2026-07-27_

### Changes

- Added an optional `communicationTimeout` constructor parameter to `Plc`. If specified, read/write operations that take longer than this duration are treated as unresponsive: the connection is marked as interrupted (triggering an automatic reconnect) and a `NotConnectedPlcException` is thrown, causing the affected items to be put on hold and automatically retried once reconnected.
- Added a new overridable `OnCommunicationTimeout` method that is invoked when the above communication timeout is exceeded, allowing subclasses to immediately abandon the underlying (possibly still busy) connection object.
___

## 6.0.0

📅 _2024-06-23_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

> [!Caution]
> ### Breaking Changes
> - The library now exclusively supports **.NET Standard 2.0**.
___

## 5.0.0

📅 _2023-03-02_

|                   .NET                    |     .NET Standard      |     .NET Framework     |
| :---------------------------------------: | :--------------------: | :--------------------: |
| :heavy_check_mark: 6 :heavy_check_mark: 7 | :heavy_check_mark: 2.0 | :heavy_check_mark: 4.8 |

### Added

- The `BitPlcItemConstructor` now allows for specifing the value of the new `BitPlcItem` directly via the `WithValue` method. This is usefull, if the value is only known during runtime.

### Changed

- If an attempt to `Connect` to a plc failed, the base `Plc` class now automatically invokes its `Disconnect` method. This is done, so that an implementing class can properly unload used resources.

### References

🔵 System.Memory ~~4.5.4~~ → **4.5.5** (.NET Framework and .NET Standard only)  
___

## 4.1.1

📅 _2021-06-23_

### Fixed

- Reading or writing `IPlcItems`caused a memory leak due to an undisposed `CancellationTokenSource` that was introduced with version **4.0.0**.
___

## 4.1.0

📅 _2021-01-30_

### Added

- New `IPlc` extension method `WriteItemsInOrderAsync` that writes items in the same order that the collection passed to the method enforces.
- New `BytesPlcItem` extension method `SetValue` that allows to use a string in **Hex** or **byte array** format for setting the value of the item.
- New `BitPlcItem` extension method `SetValue` that allows to use a string for setting the value of the item.

### Changed

- When disposing any `IPlc` instance inheriting from the `Plc` base class each read or write operation will result in a `ReadOrWritePlcException` being thrown.
___

## 4.0.0

📅 _2021-01-10_

### Changed

- Rewrote error handling. The old `PlcException` with its `PlcExceptionType` has been replaced in favor of only two main exception types:
	- `NotConnectedPlcException`	
	    This is used internally when a `Plc` instance is not connected and `IPlcItems` should be read or written. This exception is not thrown, but rather leads to the affected items being put on hold until the connection has been established.
	
	- `ReadOrWritePlcException`
	    This exception is thrown when reading or writing failed. This is probably the exception that consumer code should catch.

### Fixed

- If reading or writing `IPlcItems` failed, then this will now raise an `ReadOrWritePlcException`. Previously this didn't raise an exception, because the error code returned by **AGLink** when reading or writing may have indicated, that everything was okay. Whether an **AGLink** item failed was stored inside the `Result` property of the item.
- When creating either a `WordPlcItem`, `DWordPlcItem` or `LWordPlcItem`with an initial value, the underlying `BitCollection` was created with wrong endianness. Reason was, that the initial value was converted into byte data before the constructor could apply the correct endianness.

### Removed

- Renamed ~~`WithOutInitialValue`~~ to the proper capitalized `WithoutInitialValue` in the `WordPlcItemConstructor`.
___

## 3.1.0

📅 _2020-12-09_

### Changed

- License is now limited to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html) only and no longer any later version.
___

## 3.0.0

📅 _2020-11-18_

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).
___

## 2.2.0

📅 _2020-11-15_

### Added

- Now also targeting **.NET5.0**.
___

## 2.1.0

📅 _2020-11-04_

### Added

- New `TraceLogger` class has been added to get log output to **System.Diagnostics.Trace**. The default logger is still `NullLogger`.
___

## 2.0.0

📅 _2020-09-13_

### Changed

- Due to immense performance drawbacks caused by the way change tracking was handled within a `BitCollection` the whole behavior has been altered. Prior a collection of `BitChange` objects was created, that, depending on the amount of changes, could take a considerable amount of time. To circumvent this, the new class `BitChanges` was introduced. It is a **Dictionary** that uses **ValueTuples** to represent all changes.
___

## 1.6.0

📅 _2020-08-28_

### Added

- The `IPlc` interface now contains an additional property `Id` that can be used for identification purposes.
- Since both `Id` and `Name` of an plc are only for identification purposes, new constructors have been added so that they can be omitted. Their default values will be **-1** for the id and an empty string as the name.
___

## 1.5.0

📅 _2020-08-27_

### Changed

- Added a new constructor overload for the `Plc` class that accepts any `IPlcInformation`.
___

## 1.4.0

📅 _2020-04-20_

### Changed

- Log messages are now a little bit more detailed.
- The classes `Plc`, `PlcItem`, `BitCollection` implement the **IFormattable** interface as to format their string representation for output purposes.

### Fixed

- Handling `IPlcItems` with zero length does not throw an exception anymore, but rather doesn't handle them at all.
___

## 1.3.0

📅 _2020-04-07_

### Added

- The length of any `IDynamicPlcItem` can now be multiplied with a new constructor parameter `lengthFactor`. When using an `IPlcItemBuilder` to create items, the new option `WithLengthFactor` can be used to specify this limit.

### Fixed

- The value of an `IDynamicPlcItem` with a defined `LengthLimit` was not constraint if the value was changed directly.
- `INumericPlcItems` used in `IDynamicPlcItems` where always treaded as if they have little endian byte encoding.
___

## 1.2.1

📅 _2020-04-06_

### Fixed

- `BitCollection` didn't raise its `BitsChanged` if the underlying data source was either expanded or truncated. This bug was introduced because of some performance improvements made in version **1.1.0**.
___

## 1.2.0

📅 _2020-04-05_

### Added

- The length of any `IDynamicPlcItem` can now be limited via a new constructor parameter `lengthLimit`. When using an `IPlcItemBuilder` to create items, the new option `WithLengthLimit` can be used to specify this limit.
___

## 1.1.0

📅 _2020-04-05_

### Changed

- Data conversion of numeric types is now handled by the static `DataConverter` class that uses **System.Buffers.Binary.BinaryPrimitives**.
___

## 1.0.0

📅 _2020-03-31_

- Initial release.