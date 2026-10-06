# Sets spritePixelsToUnits to match reference bow_6 / collar_4 / collar_2 (100) on variant PNG .meta files.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$sprites = Join-Path $root 'Assets\Sprites\Pet'

function Set-PpuFromReference {
    param(
        [string]$ReferenceMeta,
        [int[]]$Indices,
        [string]$SubFolder,
        [string]$Prefix
    )
    $refPpu = 100
    if (Test-Path $ReferenceMeta) {
        $refContent = Get-Content $ReferenceMeta -Raw
        if ($refContent -match 'spritePixelsToUnits:\s*(\d+)') {
            $refPpu = [int]$Matches[1]
        }
    }

    foreach ($index in $Indices) {
        $metaPath = Join-Path $sprites "$SubFolder\${Prefix}_$index.png.meta"
        if (-not (Test-Path $metaPath)) {
            Write-Warning "Missing $metaPath"
            continue
        }
        $content = Get-Content $metaPath -Raw
        $newContent = [regex]::Replace(
            $content,
            'spritePixelsToUnits:\s*\d+',
            "spritePixelsToUnits: $refPpu")
        if ($newContent -ne $content) {
            [System.IO.File]::WriteAllText($metaPath, $newContent)
            Write-Host "Updated ${Prefix}_$index PPU -> $refPpu"
        }
    }
}

Set-PpuFromReference -ReferenceMeta (Join-Path $sprites 'Hairbows\bow_6.png.meta') -Indices (8..20) -SubFolder 'Hairbows' -Prefix 'bow'
Set-PpuFromReference -ReferenceMeta (Join-Path $sprites 'Collar\collar_4.png.meta') -Indices (7..12) -SubFolder 'Collar' -Prefix 'collar'
Set-PpuFromReference -ReferenceMeta (Join-Path $sprites 'Collar\collar_2.png.meta') -Indices (13..17) -SubFolder 'Collar' -Prefix 'collar'

Write-Host 'Variant sprite PPU meta sync complete.'
