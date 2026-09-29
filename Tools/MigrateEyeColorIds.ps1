$folder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Makeup"
$scriptGuid = "574b7066a73413d48b048ab522b6ed09"
$spriteGuid = "bb14fb5c17c5ade488a7ce25ddd47f9f"
$noneGuid = "e82840d6e5174e36bbd114e4281425fb"

# Shift eyes_1_01..60 -> eyes_1_02..61 using temp files (include old 60 -> 61).
for ($i = 61; $i -ge 2; $i--) {
    $source = Join-Path $folder ("eyes_1_{0:D2}.asset" -f ($i - 1))
    $temp = Join-Path $folder ("eyes_1_temp_{0:D2}.asset" -f $i)
    $sourceMeta = "$source.meta"
    $tempMeta = "$temp.meta"
    if (Test-Path $source) {
        Move-Item $source $temp -Force
    }
    if (Test-Path $sourceMeta) {
        Move-Item $sourceMeta $tempMeta -Force
    }
}

for ($i = 2; $i -le 61; $i++) {
    $temp = Join-Path $folder ("eyes_1_temp_{0:D2}.asset" -f $i)
    $target = Join-Path $folder ("eyes_1_{0:D2}.asset" -f $i)
    $tempMeta = "$temp.meta"
    $targetMeta = "$target.meta"
    if (Test-Path $temp) {
        $content = Get-Content $temp -Raw
        $oldId = "eyes_1_{0:D2}" -f ($i - 1)
        $newId = "eyes_1_{0:D2}" -f $i
        $content = $content.Replace("m_Name: $oldId", "m_Name: $newId")
        $content = $content.Replace("id: $oldId", "id: $newId")
        Set-Content $target -Value $content -NoNewline -Encoding UTF8
        Remove-Item $temp -Force
    }
    if (Test-Path $tempMeta) {
        Move-Item $tempMeta $targetMeta -Force
    }
}

# Write Original eyes_1_01
$originalGuid = [guid]::NewGuid().ToString("N")
$originalLines = @(
    "%YAML 1.1",
    "%TAG !u! tag:unity3d.com,2011:",
    "--- !u!114 &11400000",
    "MonoBehaviour:",
    "  m_ObjectHideFlags: 0",
    "  m_CorrespondingSourceObject: {fileID: 0}",
    "  m_PrefabInstance: {fileID: 0}",
    "  m_PrefabAsset: {fileID: 0}",
    "  m_GameObject: {fileID: 0}",
    "  m_Enabled: 1",
    "  m_EditorHideFlags: 0",
    "  m_Script: {fileID: 11500000, guid: $scriptGuid, type: 3}",
    "  m_Name: eyes_1_01",
    "  m_EditorClassIdentifier: Assembly-CSharp::DressUpGame.Data.MakeupItem",
    "  id: eyes_1_01",
    "  displayName: Original",
    "  sprite: {fileID: 21300000, guid: $spriteGuid, type: 3}",
    "  makeupType: 3",
    "  tintColor: {r: 1, g: 1, b: 1, a: 1}",
    "  isNoneOption: 0",
    "  layerOffset: {x: 0, y: 2.01, z: 0}",
    "  layerScale: {x: 0.13, y: 0.13, z: 0.13}"
)
Set-Content (Join-Path $folder "eyes_1_01.asset") -Value ($originalLines -join "`n") -NoNewline -Encoding UTF8
Set-Content (Join-Path $folder "eyes_1_01.asset.meta") -Value @(
    "fileFormatVersion: 2",
    "guid: $originalGuid",
    "NativeFormatImporter:",
    "  externalObjects: {}",
    "  mainObjectFileID: 11400000",
    "  userData: ",
    "  assetBundleName: ",
    "  assetBundleVariant: "
) -Encoding UTF8

# Build catalog entries: none + original + 02..61
$catalogGuids = New-Object System.Collections.Generic.List[string]
[void]$catalogGuids.Add("  - {fileID: 11400000, guid: $noneGuid, type: 2}")
[void]$catalogGuids.Add("  - {fileID: 11400000, guid: $originalGuid, type: 2}")

for ($i = 2; $i -le 61; $i++) {
    $metaPath = Join-Path $folder ("eyes_1_{0:D2}.asset.meta" -f $i)
    if (-not (Test-Path $metaPath)) { continue }
    $guidLine = (Get-Content $metaPath | Select-String "^guid:").Line
    if ($guidLine -match "guid: ([0-9a-f]+)") {
        [void]$catalogGuids.Add("  - {fileID: 11400000, guid: $($Matches[1]), type: 2}")
    }
}

$catalogPath = Join-Path $PSScriptRoot "..\Assets\Resources\GameCatalog.asset"
$catalog = Get-Content $catalogPath -Raw
$eyeBlock = ($catalogGuids -join "`n")
$catalog = [regex]::Replace(
    $catalog,
    "  - \{fileID: 11400000, guid: $noneGuid, type: 2\}[\s\S]*?(?=  - \{fileID: 11400000, guid: 291dbbbdc870d184d971e868ae65467c, type: 2\})",
    "$eyeBlock`n")
Set-Content $catalogPath -Value $catalog -NoNewline -Encoding UTF8

Write-Output "Migrated eye items. Original guid: $originalGuid"
