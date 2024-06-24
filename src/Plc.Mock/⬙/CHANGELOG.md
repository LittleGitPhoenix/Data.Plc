# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 6.0.0

:calendar: _2024-06-23_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

> [!Caution]
> ### Breaking Changes
> - The library now exclusively supports **.NET Standard 2.0**.

### References

:large_blue_circle: Phoenix.Data.Plc ~~5.0.0~~ → [**6.0.0**](..\..\Plc\⬙\CHANGELOG.md#6.0.0)
___

## 5.0.0

:calendar: _2023-03-02_

|                   .NET                    |     .NET Standard      |     .NET Framework     |
| :---------------------------------------: | :--------------------: | :--------------------: |
| :heavy_check_mark: 6 :heavy_check_mark: 7 | :heavy_check_mark: 2.0 | :heavy_check_mark: 4.8 |

### Changed

- Newly create data blocks for the `MockPlc` are now just emtpy byte arrays. Previously those where byte arrays with a length of **256**.

### References

:large_blue_circle: Phoenix.Data.Plc ~~4.1.1~~ → [**5.0.0**](..\..\Plc\⬙\CHANGELOG.md#5.0.0)
___

## 4.0.1

:calendar: _2021-06-23_

### Fixed

- Reading or writing `IPlcItems`caused a memory leak due to an undisposed `CancellationTokenSource` that was introduced with **Phoenix.Data.Plc 4.0.0**.

### References

:large_blue_circle: Phoenix.Data.Plc ~~4.0.0~~ → [**4.1.1**](..\..\Plc\⬙\CHANGELOG.md)
___

## 4.0.0

:calendar: _2021-01-10_

This is just a version bump. No breaking changes where implemented into this project.

### Added

- New constructors that accept a predefined plc id.

### References

:large_blue_circle: Phoenix.Data.Plc ~~3.1.0~~ → [**4.0.0**](..\..\Plc\⬙\CHANGELOG.md)
___

## 3.1.0

:calendar: _2020-12-09_

### Changed

- License is now limited to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html) only and no longer any later version.
___

## 3.0.0

:calendar: _2020-11-18_

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).
___

## 2.1.0

:calendar: _2020-11-15_

### Added

- Now also targeting **.NET5.0**.
___

## 2.0.0

:calendar: _2020-09-13_

### References

:large_blue_circle: Phoenix.Data.Plc ~~1.6.0~~ → [**2.0.0**](..\..\Plc\⬙\CHANGELOG.md)
___

## 1.1.0

:calendar: _2020-09-04_

### Added

- Extension methods for byte array, that help setting different types of values within a given array.
___

## 1.0.0

:calendar: _2020-03-31_

- Initial release.