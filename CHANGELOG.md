## CHANGE LOG

Migrated from README to CHANGELOG on 2025-03-29

- 1.1.0 2016-01 First release
- 1.2.0 2016-01 Clean up, add unit tests, published on NuGet
- 1.3.0 2016-01-18 Major revision of the interface
- 1.4.0 2016-08-10 Upgrade to .NETCore 1.0, switch tests to Xunit
- 1.4.1 2016-09-16 Bug fix
- 1.4.2 2016-09-16 Fix build issue
- 2.0.0 2016-11-19 Corrected byte-order placement, upgraded to .NETCore 1.1, downgraded to .NET 4.5.1. Switched from static classes to instance-based, allowing me to create a singleton with injected options for ASP.NET Core.
- 2.1.0 2016-11-20 Simplified API and corrected/tested byte order for PostgreSql, more README rewrites, git commit issue
- 2.2.0 2017-03-28 Fixed namespace for ICombProvider, adjusted the interface to allow overriding how the default timestamp and Guid are obtained. Created TimestampProvider implementation that forces unique, increasing timestamps (for its instance) as a solution for #5.
- 2.2.1 2017-04-02 Converted to `.csproj`. Now targeting .NET Standard 1.2. Not packaged for NuGet.
- 2.3.0 2017-07-09 Simplify interface w/static class, remove `TimestampProvider` and `GuidProvider` from the interface and make them immutable in the concrete implementations.
- 2.3.1 2018-06-10 Migrated demo and test apps to netstandard 2.1, downgraded library to netstandard 1.0 for maximum compatibility.
- 2.4.0 2020-04-23 Bumped to netstandard 2.0. (#16)
- 2.5.0 2020-12-13 Test package bumped to .NET 5.0. Added .NET Core DI package (#18, thanks @joaopgrassi!).
- 3.0.0 2021-10-27 Zero-alloc and nullable support (thanks @skarllot!); Switch to production build.
- 4.0.0 2022-12-11 Drop .NET 5 and .NET Framework support. Please continue to use v3.0 if you are still supporting these. Minor updates to bump versions and take advantage of newer C# features.
- 4.0.1 2023-02-11 Move back to .NET 6 version of DI for now for better compatibility. (#26)
- 4.0.2 2025-03-11
  - Drop .NET 6, bump the dependencies.
  - Move SQL code to separate files (thanks for the idea, @Reikooters! #29)
  - Add Binary variant for Oracle and SqlLite (#23, thanks @fubar-coder!)
  - Move all ancilliary projects to .NET 9
- 4.0.3 2025-03-29 Built with `dotnet build -c Release`, will do that in the future as well. (#31)
