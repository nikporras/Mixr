# Compiling Mixr

## Requirements

- Visual Studio 2022 with the `.NET desktop development` and `Windows application development` workloads
- .NET Framework 4.6.2 Developer Pack
- Windows 11 SDK (10.0.26100.0, or edit the SDK version in `Mixr/Mixr.csproj` and `Mixr.Package/Mixr.Package.wapproj`)
- Developer Mode enabled (Settings > System > For developers)
- Git for Windows

## Steps

1. Clone the repository: `git clone https://github.com/nikporras/Mixr.git`
2. Open `Mixr.sln` in Visual Studio 2022.
3. Set the platform to `x86`.
4. Set `Mixr.Package` as the startup project.
5. In Build > Configuration Manager, make sure `Deploy` is ticked for `Mixr.Package`.
6. Press F5.
