$root = Join-Path $PSScriptRoot '..'
$hairbowItems = Join-Path $root 'Assets\Resources\Items\Pet\Hairbows'
$sprites = Join-Path $root 'Assets\Sprites\Pet\Hairbows'
$bow6Path = Join-Path $hairbowItems 'bow_6.asset'
$templatePath = Join-Path $hairbowItems 'bow_7.asset'

function Get-SpriteGuid($index) {
    $meta = Join-Path $sprites ("bow_{0}.png.meta" -f $index)
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

$layoutsBlock = Get-PerPetLayoutsBlock $bow6Path
if (-not $layoutsBlock) {
    Write-Error 'Could not read perPetLayouts from bow_6.asset'
    exit 1
}

$template = Get-Content $templatePath -Raw

for ($index = 8; $index -le 20; $index++) {
    $spriteGuid = Get-SpriteGuid $index
    if (-not $spriteGuid) {
        Write-Warning "Skip bow_$index - missing PNG or meta"
        continue
    }

    $assetPath = Join-Path $hairbowItems ("bow_{0}.asset" -f $index)
    $metaPath = $assetPath + '.meta'

    $content = $template
    $content = $content.Replace('bow_7', "bow_$index")
    $content = $content.Replace('Bow 7', "Bow $index")
    $content = $content.Replace('guid: d1da32581be59164f9fbba11069cd115', "guid: $spriteGuid")
    $pattern = '(?ms)^\s*perPetLayouts:.*?(?=^\s*hairbowLayerAnchorOffset:)'
    $replacement = $layoutsBlock + [Environment]::NewLine
    $content = [regex]::Replace($content, $pattern, $replacement)

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

    Write-Host ('Updated bow_{0} asset with bow_6 layouts' -f $index)
}
