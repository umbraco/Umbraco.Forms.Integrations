#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates a CycloneDX SBOM per package with umbraco-sbom, checks license policy, and
    optionally uploads each one to Dependency-Track.

.DESCRIPTION
    Ported from Umbraco.Cms.Integrations (.azure-pipelines/scripts/sbom/generate-sbom.ps1),
    itself ported from Umbraco.AI, and adapted to this repo's layout: each package is
    src/<package>/ holding <package>.csproj, which sets the package version in <Version>, and
    its backoffice client, if it has one, is src/<package>/Client with its own package-lock.json.

    For each package:
    1. Restores the package's .csproj and reads its Version and PackageId from it. With
       -ReleaseTag, fails if the version doesn't match the tag's.
    2. Scans the .csproj. Only the shipped project is scanned, so the test projects and test
       sites aren't in the SBOM or subject to the license policy. The .csproj is passed rather
       than the folder: the folder also holds Client/, which umbraco-sbom would reject as a
       mixed-ecosystem path (exit 41).
    3. Scans the client from a slice of its lockfile holding only its runtime dependencies
       (see prune-npm-lockfile.js), then merges everything into one SBOM.
    4. Fails the package on a license policy violation (umbraco-sbom exit 50).
    5. With -Upload, posts the SBOM to Dependency-Track as one project per package major
       (e.g. "Umbraco.Forms.Integrations.Crm.Hubspot v9") with the full version (e.g. 9.0.0).
       isLatest is set when the version is stable and at least as high as the newest stable
       version of the same major on NuGet.org, so each major's project marks its newest stable
       release. An upload failure is a warning, not a failure: it must not block a release.

.PARAMETER Products
    Comma-separated package folder names under src/ to generate SBOMs for.

.PARAMETER ReleaseTag
    A release tag (release/<area>-<name>-<version>, e.g. refs/tags/release/crm-hubspot-9.0.0)
    to generate the SBOM for, in place of -Products. The package is the src/ folder named
    Umbraco.Forms.Integrations.<Area>.<Name>, matched case-insensitively.

.PARAMETER OutputDir
    Folder the <package>-bom.xml files are written to.

.PARAMETER Upload
    Upload each SBOM to Dependency-Track. Reads DT_BASE_URL and DT_API_KEY from the environment.

.PARAMETER PolicyAllowFile
    Per-package license exceptions passed to umbraco-sbom --policy-allow-file, if the file exists.

.EXAMPLE
    ./generate-sbom.ps1 -Products "Umbraco.Forms.Integrations.Crm.Hubspot" -OutputDir ./sbom

.EXAMPLE
    ./generate-sbom.ps1 -ReleaseTag "refs/tags/release/crm-hubspot-9.0.0" -OutputDir ./sbom -Upload
#>

param(
    [string]$Products = "",

    [string]$ReleaseTag = "",

    [Parameter(Mandatory)]
    [string]$OutputDir,

    [switch]$Upload,

    [string]$PolicyAllowFile = ".sbom-policy.json",

    [string]$RootPath = (Get-Location).Path
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$PolicyViolationExitCode = 50
$UnknownLicensesExitCode = 10
$PackagePrefix = "Umbraco.Forms.Integrations."

$workDir = Join-Path ([System.IO.Path]::GetTempPath()) "sbom-$([guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Force -Path $workDir, $OutputDir | Out-Null
$OutputDir = (Resolve-Path $OutputDir).Path

$policyArgs = @()
$policyFilePath = Join-Path $RootPath $PolicyAllowFile
if (Test-Path $policyFilePath) {
    Write-Host "Using license policy exceptions from $PolicyAllowFile"
    $policyArgs = @("--policy-allow-file", $policyFilePath)
}

function Invoke-WithRetry {
    param([scriptblock]$Action, [string]$Description, [int]$MaxAttempts = 3)

    $delay = 15
    for ($attempt = 1; ; $attempt++) {
        try {
            return & $Action
        }
        catch {
            if ($attempt -ge $MaxAttempts) { throw }
            Write-Host "##[warning]$Description failed (attempt $attempt/$MaxAttempts): $($_.Exception.Message). Retrying in ${delay}s..."
            Start-Sleep -Seconds $delay
            $delay *= 2
        }
    }
}

# Resolves release/<area>-<name>-<version> to the package folder and the version it tags.
# The older release-<name>-<version> tags predate this pipeline and aren't supported.
function Resolve-ReleaseTag {
    param([string]$Tag)

    $name = $Tag -replace '^refs/tags/', ''
    if ($name -notmatch '^release/(?<slug>[a-z0-9]+(-[a-z0-9]+)*)-(?<version>\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?)$') {
        throw "'$name' is not a release tag (release/<area>-<name>-<version>)"
    }
    $slug = $Matches.slug
    $version = $Matches.version

    $folderName = $PackagePrefix + ($slug -replace '-', '.')
    # -eq is case-insensitive: crm-activecampaign matches Crm.ActiveCampaign.
    $folder = @(Get-ChildItem (Join-Path $RootPath "src") -Directory | Where-Object { $_.Name -eq $folderName })
    if ($folder.Count -ne 1) { throw "No src/ folder matches $folderName for tag '$name'" }

    return @{ Product = $folder[0].Name; Version = $version }
}

function Get-ProjectProperty {
    param([string]$Project, [string]$Property)

    $value = dotnet msbuild $Project -getProperty:$Property
    if ($LASTEXITCODE -ne 0 -or -not $value) { throw "Could not read $Property from $Project" }
    return $value.Trim()
}

# Runs umbraco-sbom and returns $true on success. Unknown licenses (exit 10) still write a
# valid SBOM and pass the default policy, so they count as success.
function Invoke-UmbracoSbom {
    param([string]$InputPath, [string]$OutputFile)

    umbraco-sbom $InputPath --output-file $OutputFile @policyArgs | Out-Host
    switch ($LASTEXITCODE) {
        0 { return $true }
        $UnknownLicensesExitCode { return $true }
        $PolicyViolationExitCode {
            Write-Host "##[error]License policy violation in $InputPath - see the violated components above"
            return $false
        }
        default {
            Write-Host "##[error]umbraco-sbom failed for $InputPath with exit code $LASTEXITCODE"
            return $false
        }
    }
}

function New-NpmSbom {
    param([string]$Product, [string]$ProductPath, [string]$OutputFile)

    $lockfile = Join-Path (Join-Path $ProductPath "Client") "package-lock.json"
    if (-not (Test-Path $lockfile)) { return $null }

    $sliceDir = Join-Path $workDir "$Product-npm"
    node (Join-Path $PSScriptRoot "prune-npm-lockfile.js") $lockfile "." $sliceDir | Out-Host
    if ($LASTEXITCODE -ne 0) {
        Write-Host "##[error]Could not build the npm lockfile for $Product"
        return $false
    }

    if (-not (Invoke-UmbracoSbom -InputPath $sliceDir -OutputFile $OutputFile)) { return $false }
    return $OutputFile
}

# Merges the component lists of several CycloneDX XML files into the first, skipping any
# component already present (same bom-ref), and names the BOM after the package.
# umbraco-sbom output has no dependency graph, so components are all there is to merge.
function Merge-Sboms {
    param([string[]]$Files, [string]$Product, [string]$Version, [string]$OutputFile)

    [xml]$merged = Get-Content $Files[0] -Raw
    $bomNs = $merged.DocumentElement.NamespaceURI
    $ns = New-Object System.Xml.XmlNamespaceManager($merged.NameTable)
    $ns.AddNamespace("bom", $bomNs)

    $target = $merged.SelectSingleNode("/bom:bom/bom:components", $ns)
    if ($null -eq $target) {
        $target = $merged.DocumentElement.AppendChild($merged.CreateElement("components", $bomNs))
    }
    $seen = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($component in $target.SelectNodes("bom:component", $ns)) {
        [void]$seen.Add($component.GetAttribute("bom-ref"))
    }

    foreach ($file in ($Files | Select-Object -Skip 1)) {
        [xml]$other = Get-Content $file -Raw
        foreach ($component in $other.SelectNodes("/bom:bom/bom:components/bom:component", $ns)) {
            if ($seen.Add($component.GetAttribute("bom-ref"))) {
                [void]$target.AppendChild($merged.ImportNode($component, $true))
            }
        }
    }

    # The BOM describes the package, not the project or lockfile slice scanned.
    $metadata = $merged.SelectSingleNode("/bom:bom/bom:metadata", $ns)
    if ($null -ne $metadata -and $null -eq $metadata.SelectSingleNode("bom:component", $ns)) {
        $root = $merged.CreateElement("component", $bomNs)
        $root.SetAttribute("type", "application")
        $root.SetAttribute("bom-ref", "$Product@$Version")
        foreach ($field in @(@("name", $Product), @("version", $Version))) {
            $node = $merged.CreateElement($field[0], $bomNs)
            $node.InnerText = $field[1]
            [void]$root.AppendChild($node)
        }
        # Schema order puts component before properties.
        $properties = $metadata.SelectSingleNode("bom:properties", $ns)
        if ($null -ne $properties) {
            [void]$metadata.InsertBefore($root, $properties)
        }
        else {
            [void]$metadata.AppendChild($root)
        }
    }

    $merged.Save($OutputFile)
}

# Newest stable release of its major: stable, and not below any stable release of the same major
# on NuGet.org. Each major is its own Dependency-Track project, so each has its own latest.
function Test-IsLatest {
    param([string]$PackageId, [string]$Version)

    $current = [System.Management.Automation.SemanticVersion]$Version
    if ($current.PreReleaseLabel) { return $false }

    $uri = "https://api.nuget.org/v3-flatcontainer/$($PackageId.ToLowerInvariant())/index.json"
    $index = Invoke-WithRetry -Description "NuGet lookup for $PackageId" -Action {
        $response = Invoke-RestMethod -Uri $uri -SkipHttpErrorCheck -StatusCodeVariable status
        if ($status -eq 404) { return $null } # never published
        if ($status -ne 200) { throw "NuGet.org returned HTTP $status" }
        return $response
    }
    if ($null -eq $index) { return $true } # first stable release of a new package or major

    $published = @($index.versions |
        ForEach-Object {
            $parsed = $null
            if ([System.Management.Automation.SemanticVersion]::TryParse($_, [ref]$parsed)) { $parsed }
        } |
        Where-Object { -not $_.PreReleaseLabel -and $_.Major -eq $current.Major } |
        Sort-Object -Descending)

    return $published.Count -eq 0 -or $current -ge $published[0]
}

function Get-DependencyTrackProjectName {
    param([string]$PackageId, [string]$Version)

    $major = ([System.Management.Automation.SemanticVersion]$Version).Major
    return "$PackageId v$major"
}

function Send-Sbom {
    param([string]$PackageId, [string]$Version, [bool]$IsLatest, [string]$SbomFile)

    $baseUrl = $env:DT_BASE_URL
    if (-not $baseUrl -or -not $env:DT_API_KEY) {
        Write-Host "##[warning]DT_BASE_URL or DT_API_KEY not set - skipping upload of $PackageId"
        return $false
    }

    $form = @{
        autoCreate     = "true"
        projectName    = Get-DependencyTrackProjectName -PackageId $PackageId -Version $Version
        projectVersion = $Version
        isLatest       = $IsLatest.ToString().ToLowerInvariant()
        bom            = Get-Item $SbomFile
    }

    Invoke-WithRetry -Description "Dependency-Track upload of $PackageId" -Action {
        Invoke-RestMethod -Method Post -Uri "$($baseUrl.TrimEnd('/'))/api/v1/bom" `
            -Headers @{ "X-Api-Key" = $env:DT_API_KEY } -Form $form | Out-Null
    }
    return $true
}

$expectedVersion = $null
if ($ReleaseTag) {
    $release = Resolve-ReleaseTag -Tag $ReleaseTag
    $productList = @($release.Product)
    $expectedVersion = $release.Version
}
else {
    $productList = @($Products -split "," | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}
if ($productList.Count -eq 0) {
    Write-Host "No packages to generate SBOMs for"
    exit 0
}

$failed = @()
$uploadWarnings = @()

foreach ($product in $productList) {
    Write-Host ""
    Write-Host "##[section]$product"

    try {
        $productPath = Join-Path (Join-Path $RootPath "src") $product
        $project = Join-Path $productPath "$product.csproj"
        if (-not (Test-Path $project)) { throw "$product.csproj not found in $productPath" }

        Invoke-WithRetry -Description "dotnet restore $product" -Action {
            dotnet restore $project | Out-Host
            if ($LASTEXITCODE -ne 0) { throw "dotnet restore exited with $LASTEXITCODE" }
        }

        $version = Get-ProjectProperty -Project $project -Property "Version"
        $packageId = Get-ProjectProperty -Project $project -Property "PackageId"
        Write-Host "Package: $packageId $version"
        if ($expectedVersion -and $version -ne $expectedVersion) {
            throw "$product.csproj is at version $version but the tag is for $expectedVersion"
        }

        $prefix = Join-Path $workDir $product
        $nuget = "$prefix-nuget.xml"
        if (-not (Invoke-UmbracoSbom -InputPath $project -OutputFile $nuget)) { $nuget = $false }
        $npm = New-NpmSbom -Product $product -ProductPath $productPath -OutputFile "$prefix-npm.xml"
        if ($nuget -eq $false -or $npm -eq $false) {
            $failed += $product
            continue
        }

        $files = @(@($nuget) + @($npm) | Where-Object { $_ })
        $sbomFile = Join-Path $OutputDir "$product-bom.xml"
        Merge-Sboms -Files $files -Product $packageId -Version $version -OutputFile $sbomFile
        Write-Host "SBOM written: $sbomFile"

        if ($Upload) {
            try {
                $isLatest = Test-IsLatest -PackageId $packageId -Version $version
                $projectName = Get-DependencyTrackProjectName -PackageId $packageId -Version $version
                Write-Host "Uploading to Dependency-Track as $projectName $version (isLatest: $isLatest)"
                if (-not (Send-Sbom -PackageId $packageId -Version $version -IsLatest $isLatest -SbomFile $sbomFile)) {
                    $uploadWarnings += $product
                }
            }
            catch {
                Write-Host "##[warning]Upload of $product failed: $($_.Exception.Message)"
                $uploadWarnings += $product
            }
        }
    }
    catch {
        Write-Host "##[error]$product failed: $($_.Exception.Message)"
        $failed += $product
    }
}

Remove-Item -Recurse -Force $workDir -ErrorAction SilentlyContinue

Write-Host ""
if ($uploadWarnings.Count -gt 0) {
    Write-Host "##vso[task.logissue type=warning]Dependency-Track upload failed for: $($uploadWarnings -join ', ')"
    Write-Host "##vso[task.complete result=SucceededWithIssues;]"
}
if ($failed.Count -gt 0) {
    Write-Host "##vso[task.logissue type=error]SBOM generation failed for: $($failed -join ', ')"
    exit 1
}
Write-Host "SBOMs generated for: $($productList -join ', ')"

# Exit explicitly: the PowerShell task otherwise reports the last native command's exit code,
# and umbraco-sbom exits 10 (unknown licenses) on success.
exit 0
