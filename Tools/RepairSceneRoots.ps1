$ErrorActionPreference = 'Stop'

function Repair-SceneRoots([string]$scenePath) {
    $text = [IO.File]::ReadAllText($scenePath)
    $blocks = [regex]::Split($text, '(?=--- !u!)')

    $validRootTransformIds = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($block in $blocks) {
        if ($block -notmatch '--- !u!4 &(\d+)') { continue }
        $transformId = [int]$Matches[1]
        if ($block -match 'm_Father: \{fileID: 0\}') {
            [void]$validRootTransformIds.Add($transformId)
        }
    }

    if ($text -notmatch '(?ms)(--- !u!1660057539 &\d+\r?\nSceneRoots:\r?\n  m_ObjectHideFlags: 0\r?\n  m_Roots:\r?\n)(.*?)(\r?\n(?=--- !u!|\z))') {
        Write-Host "No SceneRoots in $scenePath"
        return
    }

    $rootsHeader = $Matches[1]
    $rootsBody = $Matches[2]
    $rootsTail = $Matches[3]

    $kept = New-Object System.Collections.Generic.List[string]
    $removed = 0
    foreach ($line in ($rootsBody -split '\r?\n')) {
        if ($line -match '^\s*- \{fileID: (\d+)\}') {
            $id = [int]$Matches[1]
            if ($validRootTransformIds.Contains($id)) {
                $kept.Add($line)
            } else {
                $removed++
            }
        } elseif (-not [string]::IsNullOrWhiteSpace($line)) {
            $kept.Add($line)
        }
    }

    $newRootsBody = ($kept -join "`n")
    if ($kept.Count -gt 0) { $newRootsBody += "`n" }

    $newText = [regex]::Replace(
        $text,
        '(?ms)(--- !u!1660057539 &\d+\r?\nSceneRoots:\r?\n  m_ObjectHideFlags: 0\r?\n  m_Roots:\r?\n).*?(\r?\n(?=--- !u!|\z))',
        { param($m) $m.Groups[1].Value + $newRootsBody + $m.Groups[2].Value },
        1)

    [IO.File]::WriteAllText($scenePath, $newText)
    Write-Host "$([IO.Path]::GetFileName($scenePath)): kept $($kept.Count) root(s), removed $removed stale reference(s)"
}

$root = Split-Path $PSScriptRoot -Parent
Repair-SceneRoots (Join-Path $root 'Assets\Scenes\MainScene.unity')
Repair-SceneRoots (Join-Path $root 'Assets\Scenes\PetScene.unity')
