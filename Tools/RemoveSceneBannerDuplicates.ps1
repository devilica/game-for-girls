$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$scenes = @(
    (Join-Path $root 'Assets\Scenes\MainScene.unity'),
    (Join-Path $root 'Assets\Scenes\PetScene.unity')
)
$targetName = 'BottomBannerAdController'

function Remove-BannerDuplicatesFromScene([string]$path) {
    $totalRemoved = 0
    do {
        $text = [IO.File]::ReadAllText($path)
        $blocks = [regex]::Split($text, '(?=--- !u!)')
        $removeIds = New-Object 'System.Collections.Generic.HashSet[string]'

        foreach ($block in $blocks) {
            if ($block -notmatch '--- !u!1 &(\d+)') { continue }
            $goId = $Matches[1]
            if ($block -notmatch "m_Name: $targetName\s*") { continue }
            [void]$removeIds.Add($goId)
            foreach ($m in [regex]::Matches($block, 'component: \{fileID: (\d+)\}')) {
                [void]$removeIds.Add($m.Groups[1].Value)
            }
        }

        if ($removeIds.Count -eq 0) { break }

        $kept = New-Object System.Collections.Generic.List[string]
        $removed = 0
        foreach ($block in $blocks) {
            if ([string]::IsNullOrWhiteSpace($block)) { continue }
            $drop = $false
            if ($block -match '--- !u!\d+ &(\d+)') {
                if ($removeIds.Contains($Matches[1])) { $drop = $true }
            }
            if (-not $drop -and $block -match 'm_GameObject: \{fileID: (\d+)\}') {
                if ($removeIds.Contains($Matches[1])) { $drop = $true }
            }
            if (-not $drop -and $block -match 'm_EditorClassIdentifier: Assembly-CSharp::DressUpGame\.UI\.BottomBannerAdController') {
                $drop = $true
            }
            if ($drop) {
                $removed++
            } else {
                [void]$kept.Add($block)
            }
        }

        [IO.File]::WriteAllText($path, ($kept -join ''))
        $totalRemoved += $removed
    } while ($removed -gt 0)

    Write-Host "$([IO.Path]::GetFileName($path)): removed $totalRemoved block(s)"
}

foreach ($scene in $scenes) {
    if (Test-Path $scene) {
        Remove-BannerDuplicatesFromScene $scene
    }
}

$repairScript = Join-Path $PSScriptRoot 'RepairSceneRoots.ps1'
if (Test-Path $repairScript) {
    & $repairScript
}
