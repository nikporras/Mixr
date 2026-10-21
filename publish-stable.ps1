# Builds Mixr Release x86 and installs it as the daily use version in C:\Apps\Mixr,
# started at login from a shortcut in the Startup folder.
# Settings for this copy live in HKCU\Software\Mixr (not the packaged dev settings).

$ErrorActionPreference = 'Stop'

$repo = $PSScriptRoot
$msbuild = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
$buildOutput = Join-Path $repo 'Build\Release'
$installDir = 'C:\Apps\Mixr'
$exe = Join-Path $installDir 'Mixr.exe'
$shortcutPath = Join-Path ([Environment]::GetFolderPath('Startup')) 'Mixr.lnk'

# 1. Close any running Mixr (stable or dev), so the files aren't locked
$running = Get-Process -Name 'Mixr' -ErrorAction SilentlyContinue
if ($running) {
    Write-Host 'Closing running Mixr...'
    $running | Stop-Process -Force
    $running | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue
}

# 2. Build Release x86. Only the app project, so the package version isn't bumped.
Write-Host 'Building Release x86...'
& $msbuild (Join-Path $repo 'Mixr\Mixr.csproj') -restore -p:RestorePackagesConfig=true `
    -p:Configuration=Release -p:Platform=x86 -v:minimal -nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit code $LASTEXITCODE)" }

# 3. Copy the output to C:\Apps\Mixr (mirror, so removed files go too)
Write-Host "Copying to $installDir..."
robocopy $buildOutput $installDir /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "Copy failed (robocopy exit code $LASTEXITCODE)" }

# 4. Startup folder shortcut so it starts at login
Write-Host "Creating startup shortcut $shortcutPath..."
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exe
$shortcut.WorkingDirectory = $installDir
$shortcut.Description = 'Mixr'
$shortcut.Save()

# Start it now so it's running without logging out
Start-Process -FilePath $exe -WorkingDirectory $installDir
Write-Host 'Done. Mixr stable is installed and running.'
