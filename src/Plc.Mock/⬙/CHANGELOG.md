﻿# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 4.0.1 (2021-06-23)

### Fixed

- Reading or writing `IPlcItems`caused a memory leak due to an undisposed `CancellationTokenSource` that was introduced with **Phoenix.Data.Plc 4.0.0**.

### Updated

- Phoenix.Data.Plc ~~4.0.0~~ → [**4.1.1**](..\..\Plc\⬙\CHANGELOG.md)
___

## 4.0.0 (2021-01-10)

This is just a version bump. No breaking changes where implemented into this project.

### Added

- New constructors that accept a predefined plc id.

### Updated

- Phoenix.Data.Plc ~~3.1.0~~ → [**4.0.0**](..\..\Plc\⬙\CHANGELOG.md)
___

## 3.1.0 (2020-12-09)

### Changed

- License is now limited to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html) only and no longer any later version.
___

## 3.0.0 (2020-11-18)

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).
___

## 2.1.0 (2020-11-15)

### Added

- Now also targeting **.NET5.0**.
___

## 2.0.0 (2020-09-13)

### Updated

- Phoenix.Data.Plc ~~1.6.0~~ → [**2.0.0**](..\..\Plc\⬙\CHANGELOG.md)
___

## 1.1.0 (2020-09-04)

### Added

- Extension methods for byte array, that help setting different types of values within a given array.
___

## 1.0.0 (2020-03-31)

- Initial release.