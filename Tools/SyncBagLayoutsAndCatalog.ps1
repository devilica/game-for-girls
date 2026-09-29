$accessoryFolder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Accessories"
$catalogPath = Join-Path $PSScriptRoot "..\Assets\Resources\GameCatalog.asset"

function Get-GuidFromMeta($assetPath) {
    $metaPath = "$assetPath.meta"
    if (-not (Test-Path $metaPath)) { return $null }
    $line = (Get-Content $metaPath | Select-String "^guid:").Line
    if ($line -match "guid: ([0-9a-f]+)") { return $Matches[1] }
    return $null
}

function Set-BagLayout($assetPath, $offsetLine, $scaleLine) {
    if (-not (Test-Path $assetPath)) { return }
    $content = Get-Content $assetPath -Raw
    $content = [regex]::Replace($content, "  layerOffset: \{[^\}]+\}", "  layerOffset: $offsetLine")
    $content = [regex]::Replace($content, "  layerScale: \{[^\}]+\}", "  layerScale: $scaleLine")
    Set-Content $assetPath -Value $content -NoNewline -Encoding UTF8
}

$layouts = @{}
foreach ($shape in 1..3) {
    $basePath = Join-Path $accessoryFolder "bag_$shape.asset"
    if (-not (Test-Path $basePath)) { continue }
    $text = Get-Content $basePath -Raw
    if ($text -match "layerOffset: (\{[^\}]+\})") { $offset = $Matches[1] }
    if ($text -match "layerScale: (\{[^\}]+\})") { $scale = $Matches[1] }
    $layouts[$shape] = @{ Offset = $offset; Scale = $scale }
}

foreach ($shape in $layouts.Keys) {
    $layout = $layouts[$shape]
    for ($color = 1; $color -le 10; $color++) {
        $variantPath = Join-Path $accessoryFolder ("bag_{0}_{1:D2}.asset" -f $shape, $color)
        Set-BagLayout $variantPath $layout.Offset $layout.Scale
    }
}

$orderedBagGuids = New-Object System.Collections.Generic.List[string]
$noneGuid = Get-GuidFromMeta (Join-Path $accessoryFolder "bag_none.asset")
if ($noneGuid) { [void]$orderedBagGuids.Add($noneGuid) }

foreach ($shape in 1..3) {
    $baseGuid = Get-GuidFromMeta (Join-Path $accessoryFolder "bag_$shape.asset")
    if ($baseGuid) { [void]$orderedBagGuids.Add($baseGuid) }
}

foreach ($shape in 1..3) {
    for ($color = 1; $color -le 10; $color++) {
        $variantGuid = Get-GuidFromMeta (Join-Path $accessoryFolder ("bag_{0}_{1:D2}.asset" -f $shape, $color))
        if ($variantGuid) { [void]$orderedBagGuids.Add($variantGuid) }
    }
}

$bagGuidSet = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($g in $orderedBagGuids) { [void]$bagGuidSet.Add($g) }

$catalog = Get-Content $catalogPath -Raw
$accessoryBlockMatch = [regex]::Match($catalog, "(?ms)(  accessoryItems:\r?\n)(.*?)(\r?\n  [a-zA-Z]|\z)")
if (-not $accessoryBlockMatch.Success) {
    Write-Error "Could not parse accessoryItems in GameCatalog.asset"
    exit 1
}

$entryPattern = "  - \{fileID: 11400000, guid: ([0-9a-f]+), type: 2\}"
$existingEntries = [regex]::Matches($accessoryBlockMatch.Groups[2].Value, $entryPattern)
$nonBagGuids = New-Object System.Collections.Generic.List[string]
foreach ($entry in $existingEntries) {
    $guid = $entry.Groups[1].Value
    if (-not $bagGuidSet.Contains($guid)) {
        [void]$nonBagGuids.Add($guid)
    }
}

$lines = New-Object System.Collections.Generic.List[string]
foreach ($guid in $nonBagGuids) {
    [void]$lines.Add("  - {fileID: 11400000, guid: $guid, type: 2}")
}
foreach ($guid in $orderedBagGuids) {
    [void]$lines.Add("  - {fileID: 11400000, guid: $guid, type: 2}")
}

$newAccessoryBlock = ($lines -join "`n") + "`n"
$tailStart = $accessoryBlockMatch.Groups[3].Index
if ($accessoryBlockMatch.Groups[3].Value -eq "") {
    $tailStart = $catalog.Length
}
$newCatalog = $catalog.Substring(0, $accessoryBlockMatch.Groups[1].Index + $accessoryBlockMatch.Groups[1].Length) +
    $newAccessoryBlock +
    $catalog.Substring($tailStart)
Set-Content $catalogPath -Value $newCatalog -NoNewline -Encoding UTF8

Write-Output "Updated bag color layouts and catalog order (None, bag_1..3, color variants). Bag entries: $($orderedBagGuids.Count)"
