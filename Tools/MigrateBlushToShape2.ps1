$root = Join-Path $PSScriptRoot ".."
$makeupFolder = Join-Path $root "Assets\Resources\Items\Makeup"
$spriteFolder = Join-Path $root "Assets\Sprites\Character\Makeup"
$libraryCs = Join-Path $root "Assets\Scripts\Data\BlushColorLibrary.cs"
$mainScene = Join-Path $root "Assets\Scenes\MainScene.unity"

$oldSpriteGuid = "9fac2047cfd0ebf4383871b6b93f6a79"
$newSpriteGuid = "ccba3f33c81d33047aff58c54fdd6471"

function Get-NewBlushId([int]$number) {
    if ($number -lt 10) {
        return "blush_2_0$number"
    }
    return "blush_2_$number"
}

function Update-AssetContent($assetPath, $newId) {
    $content = Get-Content $assetPath -Raw -Encoding UTF8
    $content = $content -replace "guid: $oldSpriteGuid", "guid: $newSpriteGuid"
    $content = [regex]::Replace($content, 'm_Name: blush_\d+', "m_Name: $newId")
    $content = [regex]::Replace($content, 'id: blush_\d+', "id: $newId")
    Set-Content $assetPath -Value $content.TrimEnd() -NoNewline -Encoding UTF8
}

function Rename-AssetPair($oldBaseName, $newBaseName) {
    $oldAsset = Join-Path $makeupFolder "$oldBaseName.asset"
    $newAsset = Join-Path $makeupFolder "$newBaseName.asset"
    $oldMeta = "$oldAsset.meta"
    $newMeta = "$newAsset.meta"

    if (-not (Test-Path $oldAsset)) {
        Write-Warning "Missing asset: $oldAsset"
        return
    }

    if (Test-Path $newAsset) {
        Write-Warning "Target already exists, skipping rename: $newAsset"
        return
    }

    Move-Item $oldAsset $newAsset
    if (Test-Path $oldMeta) {
        Move-Item $oldMeta $newMeta
    }

    Update-AssetContent $newAsset $newBaseName
}

Rename-AssetPair "blush_2" (Get-NewBlushId 2)
for ($n = 21; $n -ge 3; $n--) {
    Rename-AssetPair "blush_$n" (Get-NewBlushId $n)
}
Rename-AssetPair "blush_1" (Get-NewBlushId 1)

if (Test-Path $libraryCs) {
    $library = Get-Content $libraryCs -Raw -Encoding UTF8
    for ($n = 21; $n -ge 1; $n--) {
        $oldId = "blush_$n"
        $newId = Get-NewBlushId $n
        $library = $library.Replace("`"$oldId`"", "`"$newId`"")
    }
    Set-Content $libraryCs -Value $library.TrimEnd() -NoNewline -Encoding UTF8
}

if (Test-Path $mainScene) {
    $scene = Get-Content $mainScene -Raw -Encoding UTF8
    $scene = $scene.Replace("guid: $oldSpriteGuid", "guid: $newSpriteGuid")
    Set-Content $mainScene -Value $scene.TrimEnd() -NoNewline -Encoding UTF8
}

$oldSprite = Join-Path $spriteFolder "blush_1.png"
$oldSpriteMeta = "$oldSprite.meta"
if (Test-Path $oldSprite) { Remove-Item $oldSprite -Force }
if (Test-Path $oldSpriteMeta) { Remove-Item $oldSpriteMeta -Force }

Write-Output "Migrated 21 blush items to blush_2_XX using sprite guid $newSpriteGuid"
