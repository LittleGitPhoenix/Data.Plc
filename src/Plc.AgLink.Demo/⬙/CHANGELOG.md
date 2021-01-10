# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 4.0.0 (2021-01-10)

### Changed

- Because of the new way **.Net 5** handles execution of applications published as single file, the previous mechanism of providing **AGLink** requirements like the license key or the unmanaged assembly via a custom build target is no longer applicable. Instead this project no has a new `DemoAgLinkPlc` class that inherits from the now abstract base class `AgLinkPlc`. `DemoAgLinkPlc` provides all requirements that are bundled as embedded resources via its static constructor.

### Updated

- Phoenix.Data.Plc.AgLink ~~3.1.0~~ → [**4.0.0**](..\..\Plc.AgLink\⬙\CHANGELOG.md)
___

## 3.1.0 (2020-12-09)

### Changed

- License is now limited to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html) only and no longer any later version.

### Fixed

- The required **AGL4DotNET.4.dll** was not copied to the output folder of **.Net 5.0** projects and even manually adding this file didn't help, as it was not added to the ***.deps.json** file too. Therefore the project has been restructured to make it clearer, which changes must be made, when adding new build targets.
___

## 3.0.0 (2020-11-18)

### Changed

- Changed license to [**LGPL-3.0**](https://www.gnu.org/licenses/lgpl-3.0.html).
___

## 2.2.0 (2020-11-15)

### Added

- Now also targeting **.NET5.0**.

### Removed

- All **AGlink** files have been removed from the repository. Those have to be added manually from now on.
___

## 2.1.1 (2020-11-05)

### Fixed

- Exchanged **AGLink40_Error.txt** with the english version.
___

## 2.1.0 (2020-11-04)

### Updated

- AGLink ~~5.5.1~~ → **5.6.0**
___

## 2.0.0 (2020-09-13)

### Updated

- Phoenix.Data.Plc ~~1.6.0~~ → [**2.0.0**](..\..\Plc\⬙\CHANGELOG.md)
___

## 1.0.0 (2020-03-31)

- Initial release.