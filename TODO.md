# TODO

All planned changes to this project will be documented in this file.
___

## Functionality
___

- [ ] Don't hard-code the **AGLink** license information into the code. Use a separate (and maybe AES encrypted) file instead.
- [x] ~~Create a new ***IPlc*** implementation that logs all read / write operations. Use a decorator.~~  
→ Implementing logging as decorator limits log entries being either written at the start or at the end of some method. So the ***PLC*** base class uses its own logging facility directly.
- [x] Find a way to not embed the **AGLink** assemblies into the assembly of the NuGet package but rather as a resource.  
→ The concrete **AGLink** assemblies are outsourced into a separate project. 
- [x] ~~Try to make the package .NET Framework 4.5 compatible.~~
- [x] ~~Include the typed plc items in the main ***Plc*** assembly. Typical usage would otherwise require to always add two **NuGet** packages.~~
- [x] ~Implement dynamic items for strings and bytes.~ 

## Unit Tests
___

- [ ] Remove all dependencies of protected methods.
- Create tests for all typed plc items. Afterwards the read/write implementation test should only need to include basic bit and byte operations.
  - [x] ~~BitPlcItem~~
  - [x] ~~BitsPlcItem~~
  - [ ] BytePlcItem
  - [ ] BytesPlcItem
  - [ ] DWordPlcItem
  - [x] ~~DynamicBytesPlcItem~~
  - [x] ~~DynamicUtf8PlcItem~~
  - [x] ~~EnumPlcItem~~
  - [ ] Int16PlcItem
  - [ ] Int32PlcItem
  - [ ] Int64PlcItem
  - [ ] LWordPlcItem
  - [ ] UInt16PlcItem
  - [ ] UInt32PlcItem
  - [ ] UInt64PlcItem
  - [x] ~~Utf8PlcItem~~
  - [ ] WordPlcItem

