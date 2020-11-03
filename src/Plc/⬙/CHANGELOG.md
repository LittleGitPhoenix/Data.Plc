# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.1.0 (2020-11-04)

### Added

- New ***TraceLogger*** class has been added to get log output to **System.Diagnostics.Trace**. The default logger is still ***NullLogger***.
___

## 2.0.0 (2020-09-13)

### Changed

- Due to immense performance drawbacks caused by the way change tracking was handled within a ***BitCollection*** the whole behavior has been altered. Prior a collection of ***BitChange*** objects was created, that, depending on the amount of changes, could take a considerable amount of time. To circumvent this the new class ***BitChanges*** was introduced. It is a **Dictionary** that uses **ValueTuples** to represent all changes.
___

## 1.6.0 (2020-08-28)

### Added

- The ***IPlc*** interface now contains an additional property ***Id*** that can be used for identification purposes.
- Since both ***Id*** and ***Name*** of an plc are only for identification purposes, new constructors have been added so that they can be omitted. Their default values will be **-1** for the id and an empty string as the name.
___

## 1.5.0 (2020-08-27)

### Changed

- Added a new constructor overload for the ***Plc*** class that accepts any ***IPlcInformation***.
___

## 1.4.0 (2020-04-20)

### Changed

- Log messages are now a little bit more detailed.
- The classes ***Plc***, ***PlcItem***, ***BitCollection*** implement the **IFormattable** interface as to format their string representation for output purposes.

### Fixed

- Handling ***IPlcItems*** with zero length does not throw an exception anymore, but rather doesn't handle them at all.
___

## 1.3.0 (2020-04-07)

### Added

- The length of any ***IDynamicPlcItem*** can now be multiplied with a new constructor parameter ***lengthFactor***. When using an ***IPlcItemBuilder*** to create items, the new option ***WithLengthFactor*** can be used to specify this limit.

### Fixed

- The value of an ***IDynamicPlcItem*** with a defined ***LengthLimit*** was not constraint if the value was changed directly.
- ***INumericPlcItems*** used in ***IDynamicPlcItems*** where always treaded as if they have little endian byte encoding.
___

## 1.2.1 (2020-04-06)

### Fixed

- ***BitCollection*** didn't raise its ***BitsChanged*** if the underlying data source was either expanded or truncated. This bug was introduced because of some performance improvements made in version **1.1.0**.
___

## 1.2.0 (2020-04-05)

### Added

- The length of any ***IDynamicPlcItem*** can now be limited via a new constructor parameter ***lengthLimit***. When using an ***IPlcItemBuilder*** to create items, the new option ***WithLengthLimit*** can be used to specify this limit.
___

## 1.1.0 (2020-04-05)

### Changed

- Data conversion of numeric types is now handled by the static ***DataConverter*** class that uses **System.Buffers.Binary.BinaryPrimitives**.
___

## 1.0.0 (2020-03-31)

- Initial release.