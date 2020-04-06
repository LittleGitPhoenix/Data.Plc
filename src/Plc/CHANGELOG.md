# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 1.2.0 (2020-04-05)
___

### Added

- The length of any ***IDynamicPlcItem*** can now be limited via a new constructor parameter ***lengthLimit***. When using an ***IPlcItemBuilder*** to create items there should be a new option ***WithLengthLimit*** to specify this limit.

## 1.1.0 (2020-04-05)
___

### Changed

- Data conversion of numeric types is now handled by the static ***DataConverter*** class that uses **System.Buffers.Binary.BinaryPrimitives**.

## 1.0.0 (2020-03-31)
___

### Added

- Initial release.