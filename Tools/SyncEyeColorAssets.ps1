# Syncs EyeColorLibrary.asset and eyes_2_XX makeup tint colors from C# defaults.
$root = Join-Path $PSScriptRoot ".."
$libraryAsset = Join-Path $root "Assets\Resources\EyeColorLibrary.asset"
$makeupFolder = Join-Path $root "Assets\Resources\Items\Makeup"

$pairs = @(
    @{ n = "Sky Blue"; light = "#8FD4F0"; dark = "#2596BE" },
    @{ n = "Ocean Blue"; light = "#58AEE8"; dark = "#1568A0" },
    @{ n = "Emerald"; light = "#58E088"; dark = "#1A9850" },
    @{ n = "Forest Green"; light = "#48C860"; dark = "#157030" },
    @{ n = "Light Green"; light = "#7EE878"; dark = "#38A838" },
    @{ n = "Spring Green"; light = "#98F088"; dark = "#48C040" },
    @{ n = "Orange"; light = "#FFA040"; dark = "#E06818" },
    @{ n = "Tangerine"; light = "#FF8830"; dark = "#D04810" },
    @{ n = "Red"; light = "#F04848"; dark = "#C01828" },
    @{ n = "Cherry"; light = "#F05868"; dark = "#B01830" },
    @{ n = "Hazel Gold"; light = "#E8C868"; dark = "#A88030" },
    @{ n = "Honey Amber"; light = "#F0B848"; dark = "#C88018" },
    @{ n = "Warm Brown"; light = "#D0A060"; dark = "#906028" },
    @{ n = "Chocolate"; light = "#B87848"; dark = "#704018" },
    @{ n = "Soft Grey"; light = "#B8C0C8"; dark = "#687078" },
    @{ n = "Storm Grey"; light = "#9098A0"; dark = "#404850" },
    @{ n = "Violet"; light = "#C098F0"; dark = "#7848B0" },
    @{ n = "Lavender"; light = "#D0B0F8"; dark = "#9068C0" },
    @{ n = "Rose Pink"; light = "#F088A8"; dark = "#C04068" },
    @{ n = "Magenta"; light = "#F068C0"; dark = "#B02888" },
    @{ n = "Teal"; light = "#40D0C8"; dark = "#188880" },
    @{ n = "Turquoise"; light = "#38D8E8"; dark = "#10A0B0" },
    @{ n = "Mint"; light = "#78F0A8"; dark = "#30B068" },
    @{ n = "Seafoam"; light = "#90F8C8"; dark = "#40B888" },
    @{ n = "Navy"; light = "#5878B8"; dark = "#183058" },
    @{ n = "Ice Blue"; light = "#C8ECFF"; dark = "#88C0E0" },
    @{ n = "Plum"; light = "#D078C0"; dark = "#903868" },
    @{ n = "Copper"; light = "#F09850"; dark = "#C05820" },
    @{ n = "Golden"; light = "#FFE048"; dark = "#E0A818" },
    @{ n = "Ruby"; light = "#E83848"; dark = "#B01020" }
)

function Convert-HexToRgb($hex) {
    $hex = $hex.TrimStart('#')
    $r = [Convert]::ToInt32($hex.Substring(0, 2), 16) / 255.0
    $g = [Convert]::ToInt32($hex.Substring(2, 2), 16) / 255.0
    $b = [Convert]::ToInt32($hex.Substring(4, 2), 16) / 255.0
    return @{ r = $r; g = $g; b = $b }
}

function Update-TintColor($assetPath, $rgb) {
    if (-not (Test-Path $assetPath)) { return }
    $content = Get-Content $assetPath -Raw -Encoding UTF8
    $replacement = "tintColor: {r: $($rgb.r), g: $($rgb.g), b: $($rgb.b), a: 1}"
    $content = [regex]::Replace($content, 'tintColor: \{r: [^}]+\}', $replacement)
    Set-Content $assetPath -Value $content.TrimEnd() -NoNewline -Encoding UTF8
}

$libraryLines = @(
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
    "  m_Script: {fileID: 11500000, guid: a7c3e8914f2d4b6e9d1a8c5b3e7f0421, type: 3}",
    "  m_Name: EyeColorLibrary",
    "  m_EditorClassIdentifier: Assembly-CSharp::DressUpGame.Data.EyeColorLibrary",
    "  colors:"
)

$familyIndex = 1
foreach ($pair in $pairs) {
    foreach ($variant in @(
            @{ prefix = "Light"; hex = $pair.light },
            @{ prefix = "Dark"; hex = $pair.dark }
        )) {
        $suffix = $familyIndex * 2
        if ($variant.prefix -eq "Dark") { $suffix = $familyIndex * 2 + 1 }
        $id = "eyes_2_{0:D2}" -f $suffix
        $displayName = "{0} {1}" -f $variant.prefix, $pair.n
        $rgb = Convert-HexToRgb $variant.hex

        $libraryLines += "  - id: $id"
        $libraryLines += "    color: {r: $($rgb.r), g: $($rgb.g), b: $($rgb.b), a: 1}"
        $libraryLines += "    displayName: $displayName"

        $assetPath = Join-Path $makeupFolder "$id.asset"
        Update-TintColor $assetPath $rgb
        $content = Get-Content $assetPath -Raw -Encoding UTF8
        $content = $content -replace 'displayName: .*', "displayName: $displayName"
        Set-Content $assetPath -Value $content.TrimEnd() -NoNewline -Encoding UTF8
    }
    $familyIndex++
}

Set-Content $libraryAsset -Value ($libraryLines -join "`n") -NoNewline -Encoding UTF8
Write-Host "Synced 60 eye color tints and EyeColorLibrary.asset"
