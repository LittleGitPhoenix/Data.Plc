# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 6.3.0

📅 _2026-11-09_

### Added

- The polling frequency can now be specified per monitor instance. New constructor overloads of `PollingPlcMonitor` and `PollingMonitorablePlc` and new `MakeMonitorable` overloads accept a default polling frequency (`TimeSpan`) that is used for all items that are not listed in the `PlcItemMonitorConfigurations`. The static `PlcItemMonitorConfigurations.DefaultPollingFrequency` is still used as fallback, so the existing API keeps working.
- Added `PlcItemMonitorConfigurations.GetPollingFrequencyForPlcItem(IPlcItem, TimeSpan)` that returns the passed default polling frequency for items that are not listed.

### Changed

- All monitored items that share the same polling frequency are now read together with a single `ReadItemsAsync` call per polling cycle instead of one call per distinct item and cycle. Items that are equal to one another (same address and length) are still read only once. If a batched read fails for single items, the successfully read items are still updated.

### Fixed

- `UnMonitorItem` now actually stops monitoring an item. Previously the item was looked up with a different key than it was stored with, so it kept being polled and kept raising `ValueChanged`.
- A handler is now stopped and removed once its last item has been unmonitored. Previously it kept reading the plc forever.
- The polling of an item no longer stops for good if an exception occurs. Failing reads (e.g. `ReadPlcException`) and exceptions thrown by subscribers of `ValueChanged` are now logged via the `LogManager` and polling continues with the next cycle. Only cancellation ends polling.
- An exception thrown by a `ValueChanged` subscriber no longer affects the other monitored items.
- Items that are monitored before `Start` was called or after `Stop` was called are no longer polled. The monitor now keeps a running state and only starts new handlers while it is running.
___

## 6.2.0

📅 _2026-11-03_

### References

🔵 Phoenix.Data.Plc.Monitor ~~6.1.0~~ → [**6.2.0**](..\..\Plc.Monitor\⬙\CHANGELOG.md#6.2.0)  
___

## 6.1.0

📅 _2026-07-27_

### References

🔵 Phoenix.Data.Plc.Monitor ~~6.0.0~~ → [**6.1.0**](..\..\Plc.Monitor\⬙\CHANGELOG.md#6.1.0)  
___

## 6.0.0

📅 _2024-06-23_

| .NET | .NET Standard | .NET Framework |
| :-: | :-: | :-: |
| :heavy_minus_sign: | :heavy_check_mark: 2.0 | :heavy_minus_sign: |

> [!Caution]
> ### Breaking Changes
> - The library now exclusively supports **.NET Standard 2.0**.

### References

🔵 Phoenix.Data.Plc.Monitor ~~5.0.0~~ → [**6.0.0**](..\..\Plc.Monitor\⬙\CHANGELOG.md#6.0.0)  
___

## 5.0.0

📅 _2023-03-02_

|                   .NET                    |     .NET Standard      |     .NET Framework     |
| :---------------------------------------: | :--------------------: | :--------------------: |
| :heavy_check_mark: 6 :heavy_check_mark: 7 | :heavy_check_mark: 2.0 | :heavy_check_mark: 4.8 |
___

## 4.0.0

📅 _2021-01-10_

This is just a version bump. No breaking changes where implemented into this project.

### References

🔵 Phoenix.Data.Plc ~~3.1.0~~ → [**4.0.0**](..\..\Plc\⬙\CHANGELOG.md)  
🔵 Phoenix.Data.Plc.Monitor ~~3.1.0~~ → [**4.0.0**](..\..\Plc.Monitor\⬙\CHANGELOG.md)  
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

## 2.1.0

📅 _2020-11-15_

### Added

- Now also targeting **.NET5.0**.
___

## 2.0.0

📅 _2020-09-13_

### References

🔵 Phoenix.Data.Plc ~~1.6.0~~ → [**2.0.0**](..\..\Plc\⬙\CHANGELOG.md)  
🔵 Phoenix.Data.Plc.Monitor ~~1.0.0~~ → [**2.0.0**](..\..\Plc.Monitor\⬙\CHANGELOG.md)  
___

## 1.1.0

📅 _2020-08-28_

### Added

- Implemented new `IPlc.Id` property in `PollingMonitorablePlc`.
___

## 1.0.0

📅 _2020-03-31_

- Initial release.