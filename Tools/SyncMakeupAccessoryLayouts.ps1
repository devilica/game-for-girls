$makeupFolder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Makeup"
$accessoryFolder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Accessories"

function Get-LayoutFromAsset($assetPath) {
    if (-not (Test-Path $assetPath)) { return $null }
    $text = Get-Content $assetPath -Raw
    $offset = $null
    $scale = $null
    if ($text -match "layerOffset: (\{[^\}]+\})") { $offset = $Matches[1] }
    if ($text -match "layerScale: (\{[^\}]+\})") { $scale = $Matches[1] }
    if (-not $offset -or -not $scale) { return $null }
    return @{ Offset = $offset; Scale = $scale }
}

function Set-LayoutOnAsset($assetPath, $offsetLine, $scaleLine) {
    if (-not (Test-Path $assetPath)) { return $false }
    $content = Get-Content $assetPath -Raw
    if ($content -match "layerOffset:") {
        $content = [regex]::Replace($content, "  layerOffset: \{[^\}]+\}", "  layerOffset: $offsetLine")
        $content = [regex]::Replace($content, "  layerScale: \{[^\}]+\}", "  layerScale: $scaleLine")
    }
    else {
        $insert = "  layerOffset: $offsetLine`n  layerScale: $scaleLine`n"
        $content = [regex]::Replace($content, "(  isNoneOption: 0\r?\n)", "`$1$insert")
    }
    Set-Content $assetPath -Value $content -NoNewline -Encoding UTF8
    return $true
}

function Sync-AllInFolder($folder, $filter, $referencePath, [string[]]$excludeNames) {
    $layout = Get-LayoutFromAsset $referencePath
    if (-not $layout) {
        Write-Warning "Missing layout on reference: $referencePath"
        return 0
    }

    $count = 0
    Get-ChildItem $folder -Filter $filter | ForEach-Object {
        if ($excludeNames -contains $_.BaseName) { return }
        if (Set-LayoutOnAsset $_.FullName $layout.Offset $layout.Scale) { $count++ }
    }
    return $count
}

function Sync-ShapeGroup($folder, $filter, $shapePattern, $referenceNamePattern) {
    $count = 0
    $groups = Get-ChildItem $folder -Filter $filter | Group-Object {
        if ($_.BaseName -match $shapePattern) { return $Matches[1] }
        return $_.BaseName
    }

    foreach ($group in $groups) {
        $shape = $group.Name
        if ($shape -match "none$") { continue }

        $referenceFileName = ($referenceNamePattern -f $shape)
        if (-not $referenceFileName.EndsWith(".asset")) {
            $referenceFileName = "$referenceFileName.asset"
        }
        $referencePath = Join-Path $folder $referenceFileName
        if (-not (Test-Path $referencePath)) {
            $referencePath = Join-Path $folder "$shape.asset"
        }
        if (-not (Test-Path $referencePath)) {
            Write-Warning "No reference for shape '$shape' in $folder"
            continue
        }

        $layout = Get-LayoutFromAsset $referencePath
        if (-not $layout) {
            Write-Warning "Missing layout on reference: $referencePath"
            continue
        }

        foreach ($item in $group.Group) {
            if (Set-LayoutOnAsset $item.FullName $layout.Offset $layout.Scale) { $count++ }
        }
    }

    return $count
}

$updated = @{}

$updated["blush"] = Sync-AllInFolder $makeupFolder "blush_2_*.asset" (Join-Path $makeupFolder "blush_2_01.asset") @("blush_none")
$updated["eyeshadow"] = Sync-AllInFolder $makeupFolder "eyeshadow_2_*.asset" (Join-Path $makeupFolder "eyeshadow_2_01.asset") @("eyeshadow_none")
$updated["lipstick"] = Sync-ShapeGroup $makeupFolder "lipstick_*.asset" "^(lipstick_\d+)" "{0}_01"

$fallbackLipstick = Get-LayoutFromAsset (Join-Path $makeupFolder "lipstick_1_01.asset")
if ($fallbackLipstick) {
    foreach ($legacyShape in @("lipstick_3", "lipstick_4", "lipstick_5")) {
        $path = Join-Path $makeupFolder "$legacyShape.asset"
        if (Test-Path $path) {
            if (Set-LayoutOnAsset $path $fallbackLipstick.Offset $fallbackLipstick.Scale) {
                $updated["lipstick"]++
            }
        }
    }
}

$updated["crown"] = Sync-ShapeGroup $accessoryFolder "crown_*.asset" "^(crown_\d+)" "{0}_01"
$updated["glasses"] = Sync-ShapeGroup $accessoryFolder "glasses_*.asset" "^(glasses_\d+)" "{0}_01"

Write-Output ("Synced layer layouts - blush: {0}, eyeshadow: {1}, lipstick: {2}, crown: {3}, glasses: {4}" -f `
    $updated["blush"], $updated["eyeshadow"], $updated["lipstick"], $updated["crown"], $updated["glasses"])
