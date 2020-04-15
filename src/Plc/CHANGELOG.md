# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
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

### Added

- Initial release.