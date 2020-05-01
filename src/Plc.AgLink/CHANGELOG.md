# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
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

### Added

- Initial release.