<#
.SYNOPSIS
    RELEASE OneClickRunner to a GitHub Release (the only script that creates a v* tag).

    Build/release wall: build.ps1 is BUILD (local deploy, never tags); release.ps1 is RELEASE.
    Publishes a framework-dependent single-file win-x64 exe, zips it, writes a .sha256, tags
    v<version>, pushes the tag, and creates the GitHub Release with the zip + sha256 attached.

.PARAMETER Version
    The version stamp (YY.MMdd.HHmm). Defaults to now. It becomes the tag (v<Version>), the zip
    name, and the stamped exe version, so all three always match.

.PARAMETER SelfContained
    Publish a self-contained build (no .NET runtime needed on the target). Default: framework-dependent.

.PARAMETER DryRun
    Do everything except tag/push/create the GitHub Release. Leaves the zip + sha256 in temp/release.
#>
[CmdletBinding()]
param(
    [string]$Version = (Get-Date -Format 'yy.MMdd.HHmm'),
    [switch]$SelfContained,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$exitCode = 0
try {
    $repo       = $PSScriptRoot
    $projectDir = Join-Path $repo 'OneClickRunner'
    $csproj     = Join-Path $projectDir 'OneClickRunner.csproj'
    $outDir     = Join-Path $repo 'temp\release'
    $publishDir = Join-Path $outDir "OneClickRunner-$Version"
    $tag        = "v$Version"
    $zipName    = "OneClickRunner-$Version-win-x64.zip"
    $zipPath    = Join-Path $outDir $zipName

    Write-Host "RELEASE OneClickRunner $Version (tag $tag)$( if ($DryRun) { ' [DRY RUN]' } )"

    # Guard: the tag must be new (a version stamp is unique per minute, but never overwrite a release).
    if (git tag --list $tag) { throw "Tag $tag already exists - pick a new minute or delete the tag." }

    # Clean staging so a stale layout cannot leak into the zip.
    if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null

    Write-Host "Publishing ($( if ($SelfContained) { 'self-contained' } else { 'framework-dependent' } ), single-file, win-x64).."
    dotnet publish $csproj -c Release -r win-x64 `
        --self-contained $($SelfContained.IsPresent) `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:DebugType=none `
        -p:DebugSymbols=false `
        -p:ProductVersion=$Version `
        -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "Publish failed (exit $LASTEXITCODE)." }

    $exe = Get-Item (Join-Path $publishDir 'OneClickRunner.exe') -ErrorAction SilentlyContinue
    if (-not $exe) { throw "Executable not found after publish." }
    if (Test-Path (Join-Path $publishDir 'OneClickRunner.dll')) {
        throw "OneClickRunner.dll is present next to the exe - single-file bundling did not take effect."
    }

    Write-Host "Zipping -> $zipName"
    Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -Force

    $sha = (Get-FileHash $zipPath -Algorithm SHA256).Hash.ToLower()
    "$sha  $zipName" | Set-Content -Path "$zipPath.sha256" -Encoding ascii -NoNewline
    Write-Host "SHA256: $sha"

    # Release notes = the CHANGELOG section for this version (fall back to a one-liner).
    $notesFile = Join-Path $outDir 'notes.md'
    $changelog = Get-Content (Join-Path $repo 'CHANGELOG.md') -Raw
    $m = [regex]::Match($changelog, "(?ms)^## \[$([regex]::Escape($Version))\].*?(?=^## \[|\z)")
    if ($m.Success) { $m.Value.Trim() | Set-Content $notesFile -Encoding utf8 }
    else { "OneClickRunner $Version" | Set-Content $notesFile -Encoding utf8 }

    if ($DryRun) {
        Write-Host "[DRY RUN] Artifacts ready in $outDir. Skipping tag/push/gh release."
    } else {
        Write-Host "Tagging $tag and pushing.."
        git tag $tag
        if ($LASTEXITCODE -ne 0) { throw "git tag failed (exit $LASTEXITCODE)." }
        git push origin $tag
        if ($LASTEXITCODE -ne 0) { throw "git push tag failed (exit $LASTEXITCODE)." }

        Write-Host "Creating GitHub Release.."
        gh release create $tag $zipPath "$zipPath.sha256" --title "OneClickRunner $Version" --notes-file $notesFile
        if ($LASTEXITCODE -ne 0) { throw "gh release create failed (exit $LASTEXITCODE)." }
        Write-Host "Done. Release $tag published."
    }
}
catch {
    Write-Error $_
    $exitCode = 1
}
exit $exitCode
