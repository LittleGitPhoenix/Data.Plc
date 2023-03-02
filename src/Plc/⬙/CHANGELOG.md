# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 4.2.0 (2021-11-??)

### Added

- The `BitPlcItemConstructor` now allows for specifing the value of the new `BitPlcItem` directly via the `WithValue` method. This is usefull, if the value is only known during runtime.

### Changed

- If an attempt to `Connect` to a plc failed, the base `Plc` class now automatically invokes its `Disconnect` method. This is done, so that an implementing class can properly unload used resources.

___

## 4.1.1 (2021-06-23)

### Fixed

- Reading or writing `IPlcItems`caused a memory leak due to an undisposed `CancellationTokenSource` that was introduced with version **4.0.0**.
___

## 4.1.0 (2021-01-30)

### Added

- New `IPlc` extension method `WriteItemsInOrderAsync` that writes items in the same order that the collection passed to the method enforces.
- New `BytesPlcItem` extension method `SetValue` that allows to use a string in **Hex** or **byte array** format for setting the value of the item.
- New `BitPlcItem` extension method `SetValue` that allows to use a string for setting the value of the item.

### Changed

- When disposing any `IPlc` instance inheriting from the `Plc` base class each read or write operation will result in a `ReadOrWritePlcException` being thrown.
___

## 4.0.0 (2021-01-10)

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

## 3.1.0 (2020-12-09)

### Changed

- License is now limited to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html) only and no longer any later version.
___

## 3.0.0 (2020-11-18)

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).
___

## 2.2.0 (2020-11-15)

### Added

- Now also targeting **.NET5.0**.
___

## 2.1.0 (2020-11-04)

### Added

- New `TraceLogger` class has been added to get log output to **System.Diagnostics.Trace**. The default logger is still `NullLogger`.
___

## 2.0.0 (2020-09-13)

### Changed

- Due to immense performance drawbacks caused by the way change tracking was handled within a `BitCollection` the whole behavior has been altered. Prior a collection of `BitChange` objects was created, that, depending on the amount of changes, could take a considerable amount of time. To circumvent this, the new class `BitChanges` was introduced. It is a **Dictionary** that uses **ValueTuples** to represent all changes.
___

## 1.6.0 (2020-08-28)

### Added

- The `IPlc` interface now contains an additional property `Id` that can be used for identification purposes.
- Since both `Id` and `Name` of an plc are only for identification purposes, new constructors have been added so that they can be omitted. Their default values will be **-1** for the id and an empty string as the name.
___

## 1.5.0 (2020-08-27)

### Changed

- Added a new constructor overload for the `Plc` class that accepts any `IPlcInformation`.
___

## 1.4.0 (2020-04-20)

### Changed

- Log messages are now a little bit more detailed.
- The classes `Plc`, `PlcItem`, `BitCollection` implement the **IFormattable** interface as to format their string representation for output purposes.

### Fixed

- Handling `IPlcItems` with zero length does not throw an exception anymore, but rather doesn't handle them at all.
___

## 1.3.0 (2020-04-07)

### Added

- The length of any `IDynamicPlcItem` can now be multiplied with a new constructor parameter `lengthFactor`. When using an `IPlcItemBuilder` to create items, the new option `WithLengthFactor` can be used to specify this limit.

### Fixed

- The value of an `IDynamicPlcItem` with a defined `LengthLimit` was not constraint if the value was changed directly.
- `INumericPlcItems` used in `IDynamicPlcItems` where always treaded as if they have little endian byte encoding.
___

## 1.2.1 (2020-04-06)

### Fixed

- `BitCollection` didn't raise its `BitsChanged` if the underlying data source was either expanded or truncated. This bug was introduced because of some performance improvements made in version **1.1.0**.
___

## 1.2.0 (2020-04-05)

### Added

- The length of any `IDynamicPlcItem` can now be limited via a new constructor parameter `lengthLimit`. When using an `IPlcItemBuilder` to create items, the new option `WithLengthLimit` can be used to specify this limit.
___

## 1.1.0 (2020-04-05)

### Changed

- Data conversion of numeric types is now handled by the static `DataConverter` class that uses **System.Buffers.Binary.BinaryPrimitives**.
___

## 1.0.0 (2020-03-31)

- Initial release.