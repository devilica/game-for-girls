$folder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Accessories"
$scriptGuid = "08102772734f7b94c976e28427cca838"
$spriteGuids = @{
    1 = "eac6109085ec4104fa984888107dfc5e"
    2 = "182ed35b2519f0b48b9d748239bf134e"
    3 = "7463c866dd7a8a04681cbdac0b57020f"
}
$colors = @(
    @{ s = "01"; n = "Pink"; r = 0.95; g = 0.45; b = 0.65 },
    @{ s = "02"; n = "Red"; r = 0.85; g = 0.15; b = 0.20 },
    @{ s = "03"; n = "Purple"; r = 0.55; g = 0.25; b = 0.75 },
    @{ s = "04"; n = "Blue"; r = 0.25; g = 0.45; b = 0.85 },
    @{ s = "05"; n = "Mint"; r = 0.45; g = 0.85; b = 0.75 },
    @{ s = "06"; n = "Yellow"; r = 0.95; g = 0.85; b = 0.25 },
    @{ s = "07"; n = "Black"; r = 0.15; g = 0.15; b = 0.18 },
    @{ s = "08"; n = "Cream"; r = 0.96; g = 0.92; b = 0.82 },
    @{ s = "09"; n = "Coral"; r = 0.95; g = 0.50; b = 0.42 },
    @{ s = "10"; n = "Lavender"; r = 0.72; g = 0.58; b = 0.88 }
)

$catalogGuids = New-Object System.Collections.Generic.List[string]

foreach ($shape in 1..3) {
    foreach ($color in $colors) {
        $id = "bag_${shape}_$($color.s)"
        $assetGuid = [guid]::NewGuid().ToString("N")
        [void]$catalogGuids.Add($assetGuid)

        $assetPath = Join-Path $folder "$id.asset"
        $metaPath = "$assetPath.meta"

        $lines = @(
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
            "  m_Name: $id",
            "  m_EditorClassIdentifier: Assembly-CSharp::DressUpGame.Data.AccessoryItem",
            "  id: $id",
            "  displayName: Bag $shape $($color.n)",
            "  sprite: {fileID: 21300000, guid: $($spriteGuids[$shape]), type: 3}",
            "  tintColor: {r: $($color.r), g: $($color.g), b: $($color.b), a: 1}",
            "  category: 3",
            "  isNoneOption: 0",
            "  layerOffset: {x: 0.45, y: 0.55, z: 0}",
            "  layerScale: {x: 0.12, y: 0.12, z: 0.12}"
        )
        Set-Content -Path $assetPath -Value ($lines -join "`n") -Encoding UTF8

        $metaLines = @(
            "fileFormatVersion: 2",
            "guid: $assetGuid",
            "NativeFormatImporter:",
            "  externalObjects: {}",
            "  mainObjectFileID: 11400000",
            "  userData: ",
            "  assetBundleName: ",
            "  assetBundleVariant: "
        )
        Set-Content -Path $metaPath -Value ($metaLines -join "`n") -Encoding UTF8
    }
}

$noneGuid = "ef3f4505f0194dd4825204ae9531f26b"
$catalogLines = New-Object System.Collections.Generic.List[string]
[void]$catalogLines.Add("  - {fileID: 11400000, guid: $noneGuid, type: 2}")
foreach ($g in $catalogGuids) {
    [void]$catalogLines.Add("  - {fileID: 11400000, guid: $g, type: 2}")
}

$outFile = Join-Path $PSScriptRoot "..\_bag_catalog_entries.txt"
Set-Content -Path $outFile -Value ($catalogLines -join "`n") -Encoding UTF8
Write-Output "Created $($catalogGuids.Count) bag color assets"
