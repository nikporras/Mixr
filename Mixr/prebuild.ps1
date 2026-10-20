param($version)

$PackageManifestPath = "$PSScriptRoot\..\Mixr.Package\Package.appxmanifest"

$xml = [xml](Get-Content "$PackageManifestPath")
$xml.Package.Identity.Version = "$version"
$xml.Save("$PackageManifestPath")
