# Rebuild PetCatalog.asset item GUID list from PetItem assets (matches GeneratePetItemsTool sort).
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$itemsRoot = Join-Path $projectRoot 'Assets\Resources\Items\Pet'
$catalogPath = Join-Path $projectRoot 'Assets\Resources\PetCatalog.asset'

$colorSuffixes = @('dark_yellow', 'dark_orange', 'grey', 'black', 'brown', 'red', 'green')

function Get-PetSortKey {
    param([string]$petId)
    if ($petId -eq 'pet_none') { return @{ Index = -1; Rank = 0 } }
    if ($petId -match '^pet_(\d+)$') { return @{ Index = [int]$Matches[1]; Rank = 0 } }
    if ($petId -match '^pet_(\d+)_(.+)$') {
        $idx = [int]$Matches[1]
        $suffix = $Matches[2].ToLowerInvariant()
        for ($i = 0; $i -lt $colorSuffixes.Count; $i++) {
            if ($colorSuffixes[$i] -eq $suffix) {
                return @{ Index = $idx; Rank = $i + 1 }
            }
        }
        return @{ Index = $idx; Rank = 999 }
    }
    return @{ Index = 999; Rank = 0 }
}

$items = @()
Get-ChildItem -Path $itemsRoot -Recurse -Filter '*.asset' | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -notmatch '(?m)^  id:\s*(\S+)') { return }
    $id = $Matches[1]
    if ($content -notmatch '(?m)^  category:\s*(\d+)') { return }
    $cat = [int]$Matches[1]
    $none = $false
    if ($content -match '(?m)^  isNoneOption:\s*1') { $none = $true }
    $metaPath = $_.FullName + '.meta'
    if (-not (Test-Path $metaPath)) { return }
    $meta = Get-Content $metaPath -Raw
    if ($meta -notmatch '(?m)^guid:\s*(\S+)') { return }
    $guid = $Matches[1]
    $items += [pscustomobject]@{ Id = $id; Category = $cat; None = $none; Guid = $guid }
}

$sorted = $items | Sort-Object `
    @{ Expression = { $_.Category } }, `
    @{ Expression = { if ($_.Category -ne 0) { if ($_.None) { 0 } else { 1 } } else { 0 } } }, `
    @{ Expression = {
        if ($_.Category -eq 0) {
            $k = Get-PetSortKey $_.Id
            return ('{0:D4}-{1:D4}-{2}' -f $k.Index, $k.Rank, $_.Id)
        }
        return $_.Id
    } }

$lines = @(
    '%YAML 1.1',
    '%TAG !u! tag:unity3d.com,2011:',
    '--- !u!114 &11400000',
    'MonoBehaviour:',
    '  m_ObjectHideFlags: 0',
    '  m_CorrespondingSourceObject: {fileID: 0}',
    '  m_PrefabInstance: {fileID: 0}',
    '  m_PrefabAsset: {fileID: 0}',
    '  m_GameObject: {fileID: 0}',
    '  m_Enabled: 1',
    '  m_EditorHideFlags: 0',
    '  m_Script: {fileID: 11500000, guid: b9e9254e947941e479c9bbd8128de3fd, type: 3}',
    '  m_Name: PetCatalog',
    '  m_EditorClassIdentifier: Assembly-CSharp::DressUpGame.Pet.PetCatalog',
    '  items:'
)
foreach ($item in $sorted) {
    $lines += "  - {fileID: 11400000, guid: $($item.Guid), type: 2}"
}

$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllLines($catalogPath, $lines, $utf8NoBom)
Write-Host "PetCatalog updated with $($sorted.Count) item(s)."
