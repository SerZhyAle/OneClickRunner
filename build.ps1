$projectDir = "$PSScriptRoot\OneClickRunner"
$publishDir = "$projectDir\bin\publish"
$destination = "C:\GD\tc\SZA\_APP"

Write-Host "Publishing OneClickRunner (framework-dependent, single-file)..."

# Clean previous output so a stale multi-file layout cannot leak into the deploy.
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

dotnet publish "$projectDir\OneClickRunner.csproj" -c Release -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed."
    exit $LASTEXITCODE
}

$exe = Get-Item "$publishDir\OneClickRunner.exe" -ErrorAction SilentlyContinue
if (-not $exe) {
    Write-Error "Executable not found after publish."
    exit 1
}

# Guard: single-file publish must not leave the app's managed dll beside the exe.
if (Test-Path "$publishDir\OneClickRunner.dll") {
    Write-Warning "OneClickRunner.dll is present next to the exe - single-file bundling did not take effect; the deployed exe alone will not run."
}

$sizeMb = [math]::Round($exe.Length / 1MB, 1)
Write-Host "Copying $($exe.FullName) ($sizeMb MB) -> $destination"
if (-not (Test-Path $destination)) {
    New-Item -ItemType Directory -Path $destination | Out-Null
}
Copy-Item $exe.FullName -Destination $destination -Force

Write-Host "Done. Deployed single-file exe to $destination\OneClickRunner.exe"
Write-Host "Note: the target PC must have the .NET 8 Desktop Runtime installed (framework-dependent build)."
