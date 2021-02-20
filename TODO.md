# TODO

All planned changes to this project will be documented in this file.
___

## Functionality

- [ ] Make `MockPlc` configurable via passing an external `IPlcConfiguration` instance to it. This instance should define callback-driven behaviors that are applied before and after reading or writing.
- [x] ~~Find another way to provide **AGLink.license** (and the error file) as the current mechanism is no longer compatible with **.Net 5** single file publishing.~~
- [x] ~~Investigate if [**ArrayPools**](https://adamsitnik.com/Array-Pool/) could improve the performance of `BitCollections`.~~
→ Seems not to be the case. The implementation only creates two arrays itself. One when cloning and another when settings all bits to true or false. Those methods are probably not called that often to make a difference.
- [x] ~~`MockPlc` should accept a plc id as constructor parameter.~~
- [x] ~~Don't hard-code the **AGLink** license information into the code. Use a separate (and maybe AES encrypted) file instead.~~~
- [x] ~~Create a new `IPlc` implementation that logs all read / write operations. Use a decorator.~~  
→ Implementing logging as decorator will lead to only being able to write log entries at the start or at the end of any method. So the `PLC` base class uses its own logging facility.
- ~~[x] Find a way to not embed the **AGLink** assemblies into the assembly of the NuGet package but rather as a resource.~~  
→ The specific **AGLink** assemblies are outsourced into a separate project. 
- [x] ~~Try to make the package .NET Framework 4.5 compatible.~~
- [x] ~~Include the typed plc items in the main ***Plc*** assembly. Typical usage would otherwise require to always add two **NuGet** packages.~~
- [x] ~~Implement dynamic items for strings and bytes.~~
___

## Unit Tests

- [ ] Remove all dependencies of protected methods.
- [ ] When all typed `IPlcItem`s have their own unit tests, the read / write implementation tests should only need to cover basic bit and byte operations.
- [x] Create tests for all typed `IPlcItem`s.