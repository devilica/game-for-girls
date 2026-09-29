$folder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Makeup"
$scriptGuid = "574b7066a73413d48b048ab522b6ed09"
$spriteGuid = "f783c7fc01056c742901d1b2008ae688"
$noneGuid = "e82840d6e5174e36bbd114e4281425fb"

$pairs = @(
    @{ n = "Sky Blue"; lr = 0.62; lg = 0.82; lb = 0.98; dr = 0.15; dg = 0.35; db = 0.72 },
    @{ n = "Ocean Blue"; lr = 0.45; lg = 0.68; lb = 0.92; dr = 0.08; dg = 0.22; db = 0.55 },
    @{ n = "Emerald"; lr = 0.55; lg = 0.88; lb = 0.65; dr = 0.12; dg = 0.48; db = 0.28 },
    @{ n = "Forest Green"; lr = 0.48; lg = 0.72; lb = 0.48; dr = 0.10; dg = 0.32; db = 0.18 },
    @{ n = "Hazel Gold"; lr = 0.82; lg = 0.72; lb = 0.45; dr = 0.52; dg = 0.38; db = 0.18 },
    @{ n = "Honey Amber"; lr = 0.92; lg = 0.78; lb = 0.42; dr = 0.65; dg = 0.42; db = 0.12 },
    @{ n = "Chocolate Brown"; lr = 0.72; lg = 0.55; lb = 0.42; dr = 0.32; dg = 0.18; db = 0.12 },
    @{ n = "Warm Brown"; lr = 0.68; lg = 0.48; lb = 0.35; dr = 0.28; dg = 0.15; db = 0.10 },
    @{ n = "Soft Grey"; lr = 0.75; lg = 0.78; lb = 0.82; dr = 0.35; dg = 0.38; db = 0.42 },
    @{ n = "Storm Grey"; lr = 0.62; lg = 0.65; lb = 0.70; dr = 0.22; dg = 0.24; db = 0.28 },
    @{ n = "Violet"; lr = 0.78; lg = 0.62; lb = 0.92; dr = 0.42; dg = 0.18; db = 0.58 },
    @{ n = "Lavender"; lr = 0.85; lg = 0.72; lb = 0.95; dr = 0.48; dg = 0.28; db = 0.62 },
    @{ n = "Rose Pink"; lr = 0.95; lg = 0.65; lb = 0.75; dr = 0.62; dg = 0.22; db = 0.38 },
    @{ n = "Cherry"; lr = 0.92; lg = 0.48; lb = 0.52; dr = 0.55; dg = 0.12; db = 0.22 },
    @{ n = "Teal"; lr = 0.52; lg = 0.88; lb = 0.85; dr = 0.15; dg = 0.45; db = 0.42 },
    @{ n = "Turquoise"; lr = 0.45; lg = 0.85; lb = 0.88; dr = 0.12; dg = 0.48; db = 0.52 },
    @{ n = "Mint"; lr = 0.72; lg = 0.95; lb = 0.82; dr = 0.28; dg = 0.58; db = 0.38 },
    @{ n = "Olive"; lr = 0.72; lg = 0.78; lb = 0.48; dr = 0.35; dg = 0.42; db = 0.18 },
    @{ n = "Navy"; lr = 0.48; lg = 0.58; lb = 0.82; dr = 0.08; dg = 0.12; db = 0.35 },
    @{ n = "Ice Blue"; lr = 0.82; lg = 0.92; lb = 0.98; dr = 0.42; dg = 0.58; db = 0.72 },
    @{ n = "Plum"; lr = 0.78; lg = 0.52; lb = 0.72; dr = 0.42; dg = 0.15; db = 0.35 },
    @{ n = "Copper"; lr = 0.92; lg = 0.62; lb = 0.45; dr = 0.58; dg = 0.28; db = 0.15 },
    @{ n = "Mahogany"; lr = 0.72; lg = 0.38; lb = 0.32; dr = 0.38; dg = 0.12; db = 0.10 },
    @{ n = "Caramel"; lr = 0.88; lg = 0.68; lb = 0.45; dr = 0.55; dg = 0.32; db = 0.15 },
    @{ n = "Peach"; lr = 0.98; lg = 0.78; lb = 0.68; dr = 0.72; dg = 0.42; db = 0.32 },
    @{ n = "Silver"; lr = 0.88; lg = 0.90; lb = 0.92; dr = 0.55; dg = 0.58; db = 0.62 },
    @{ n = "Charcoal"; lr = 0.58; lg = 0.58; lb = 0.62; dr = 0.18; dg = 0.18; db = 0.22 },
    @{ n = "Golden"; lr = 0.95; lg = 0.85; lb = 0.45; dr = 0.72; dg = 0.55; db = 0.15 },
    @{ n = "Ruby"; lr = 0.88; lg = 0.35; lb = 0.42; dr = 0.52; dg = 0.08; db = 0.18 },
    @{ n = "Onyx"; lr = 0.42; lg = 0.40; lb = 0.45; dr = 0.08; dg = 0.06; db = 0.10 }
)

$catalogGuids = New-Object System.Collections.Generic.List[string]
$suffix = 1

foreach ($pair in $pairs) {
    foreach ($variant in @(
            @{ prefix = "Light"; r = $pair.lr; g = $pair.lg; b = $pair.lb },
            @{ prefix = "Dark"; r = $pair.dr; g = $pair.dg; b = $pair.db }
        )) {
        $id = "eyes_2_{0:D2}" -f $suffix
        $displayName = "{0} {1}" -f $variant.prefix, $pair.n
        $assetGuid = [guid]::NewGuid().ToString("N")
        [void]$catalogGuids.Add($assetGuid)

        $assetPath = Join-Path $folder "$id.asset"
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
            "  m_EditorClassIdentifier: Assembly-CSharp::DressUpGame.Data.MakeupItem",
            "  id: $id",
            "  displayName: $displayName",
            "  sprite: {fileID: 21300000, guid: $spriteGuid, type: 3}",
            "  makeupType: 3",
            "  tintColor: {r: $($variant.r), g: $($variant.g), b: $($variant.b), a: 1}",
            "  isNoneOption: 0",
            "  layerOffset: {x: 0, y: 2.01, z: 0}",
            "  layerScale: {x: 0.13, y: 0.13, z: 0.13}"
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
        Set-Content -Path "$assetPath.meta" -Value ($metaLines -join "`n") -Encoding UTF8
        $suffix++
    }
}

$catalogLines = New-Object System.Collections.Generic.List[string]
[void]$catalogLines.Add("  - {fileID: 11400000, guid: $noneGuid, type: 2}")
foreach ($g in $catalogGuids) {
    [void]$catalogLines.Add("  - {fileID: 11400000, guid: $g, type: 2}")
}
$outFile = Join-Path $PSScriptRoot "..\_eye_catalog_entries.txt"
Set-Content -Path $outFile -Value ($catalogLines -join "`n") -Encoding UTF8
Write-Output "Created $($catalogGuids.Count) eye color assets"
