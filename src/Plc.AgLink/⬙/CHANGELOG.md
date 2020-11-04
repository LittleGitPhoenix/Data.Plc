# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
___

## 2.2.1 (2020-11-05)

### Fixed

- Updated ***AgLinkErrorMapping*** with the error codes from the new version of the **AGLink** assembly.
___

## 2.1.0 (2020-11-04)

### Added

- If the connection to the plc couldn't be established, then now a more detailed error will be logged.

### Updated

- AGLink ~~5.5.1~~ → **5.6.0**
___

## 2.0.0 (2020-09-13)

### Updated

- Phoenix.Data.Plc ~~1.6.0~~ → **2.0.0**
___

## 1.3.0 (2020-08-28)

### Changed

- The ***Id*** provided by ***IAgLinkPlcConnectionData*** is now passed down to the plc base class.
___

## 1.2.0 (2020-08-27)

### Changed

- Extracted the new ***IAgLinkPlcConnectionData*** interface from ***AgLinkPlcConnectionData*** so that consumers can use their existing classes without the need for an adapter.
___

## 1.1.0 (2020-04-29)

### Fixed

- A **System.AccessViolationException** was thrown by the **AGLink** assembly when establishing a connection to a plc if the device number was higher than 255.
___

## 1.0.1 (2020-04-15)

### Fixed

- Loading the embedded **AGLink** resources failed in cases where the default namespace differed from the assembly name. Resources are embedded during build via the **GenerateResourceTask**. This uses the default namespace as part of the final resource name. Sadly the default namespace is just metadata information the project holds and is not part of the created assembly.
___

## 1.0.0 (2020-03-31)

- Initial release.