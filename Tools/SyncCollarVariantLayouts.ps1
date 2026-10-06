$root = Join-Path $PSScriptRoot '..'
$collarItems = Join-Path $root 'Assets\Resources\Items\Pet\Collar'
$sprites = Join-Path $root 'Assets\Sprites\Pet\Collar'
$templatePath = Join-Path $collarItems 'collar_6.asset'
$collar4Path = Join-Path $collarItems 'collar_4.asset'
$collar2Path = Join-Path $collarItems 'collar_2.asset'

function Get-SpriteGuid($index) {
    $meta = Join-Path $sprites ("collar_{0}.png.meta" -f $index)
    if (-not (Test-Path $meta)) { return $null }
    foreach ($line in Get-Content $meta) {
        if ($line -match '^guid:\s*(\S+)') { return $Matches[1] }
    }
    return $null
}

function Get-PerPetLayoutsBlock($assetPath) {
    $lines = Get-Content $assetPath
    $startLine = ($lines | Select-String -Pattern '^\s*perPetLayouts:' | Select-Object -First 1).LineNumber
    if (-not $startLine) { return $null }
    $block = New-Object System.Collections.Generic.List[string]
    for ($i = $startLine - 1; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($i -gt $startLine - 1 -and $line -match '^\s*hairbowLayerAnchorOffset:') { break }
        $block.Add($line)
    }
    return ($block -join [Environment]::NewLine)
}

function Update-CollarAsset($index, $layoutsBlock, $templateGuid) {
    $spriteGuid = Get-SpriteGuid $index
    if (-not $spriteGuid) {
        Write-Warning "Skip collar_$index - missing PNG or meta"
        return
    }

    $assetPath = Join-Path $collarItems ("collar_{0}.asset" -f $index)
    $metaPath = $assetPath + '.meta'
    $template = Get-Content $templatePath -Raw

    $content = $template
    $content = $content.Replace('collar_6', "collar_$index")
    $content = $content.Replace('Collar 6', "Collar $index")
    $content = $content.Replace("guid: $templateGuid", "guid: $spriteGuid")
    $pattern = '(?ms)^\s*perPetLayouts:.*?(?=^\s*hairbowLayerAnchorOffset:)'
    $content = [regex]::Replace($content, $pattern, ($layoutsBlock + [Environment]::NewLine))

    [System.IO.File]::WriteAllText($assetPath, $content)

    if (-not (Test-Path $metaPath)) {
        $assetGuid = [guid]::NewGuid().ToString('N')
        $metaText = @"
fileFormatVersion: 2
guid: $assetGuid
NativeFormatImporter:
  externalObjects: {}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 

"@
        [System.IO.File]::WriteAllText($metaPath, $metaText)
    }

    Write-Host ('Updated collar_{0}.asset' -f $index)
}

$layouts4 = Get-PerPetLayoutsBlock $collar4Path
$layouts2 = Get-PerPetLayoutsBlock $collar2Path
$templateGuid = 'f40d9b827b6e4e84c89fd7eafaba039c'

if (-not $layouts4 -or -not $layouts2) {
    Write-Error 'Missing perPetLayouts on collar_4 or collar_2'
    exit 1
}

for ($index = 7; $index -le 12; $index++) {
    Update-CollarAsset $index $layouts4 $templateGuid
}

for ($index = 13; $index -le 17; $index++) {
    Update-CollarAsset $index $layouts2 $templateGuid
}
