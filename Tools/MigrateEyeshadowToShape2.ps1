$root = Join-Path $PSScriptRoot ".."
$makeupFolder = Join-Path $root "Assets\Resources\Items\Makeup"
$spriteFolder = Join-Path $root "Assets\Sprites\Character\Makeup"
$libraryCs = Join-Path $root "Assets\Scripts\Data\EyeshadowColorLibrary.cs"
$mainScene = Join-Path $root "Assets\Scenes\MainScene.unity"

$oldSpriteGuid = "443ad80f136578a43b5529618b79fbfa"
$newSpriteGuid = "c7e4a8912b3d4f5e8a6c7d9e0f1a2b3c"
$newSpriteRef = "{fileID: 21300000, guid: $newSpriteGuid, type: 3}"

function Get-NewEyeshadowId([int]$number) {
    if ($number -lt 10) {
        return "eyeshadow_2_0$number"
    }
    return "eyeshadow_2_$number"
}

function Update-AssetContent($assetPath, $newId) {
    $content = Get-Content $assetPath -Raw -Encoding UTF8
    $content = $content -replace 'guid: 443ad80f136578a43b5529618b79fbfa', "guid: $newSpriteGuid"
    $content = [regex]::Replace($content, 'm_Name: eyeshadow_\d+', "m_Name: $newId")
    $content = [regex]::Replace($content, 'id: eyeshadow_\d+', "id: $newId")
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

# Rename in safe order: eyeshadow_2 first, then 90..3, then 1.
Rename-AssetPair "eyeshadow_2" (Get-NewEyeshadowId 2)
for ($n = 90; $n -ge 3; $n--) {
    Rename-AssetPair "eyeshadow_$n" (Get-NewEyeshadowId $n)
}
Rename-AssetPair "eyeshadow_1" (Get-NewEyeshadowId 1)

# Update EyeshadowColorLibrary.cs IDs.
if (Test-Path $libraryCs) {
    $library = Get-Content $libraryCs -Raw -Encoding UTF8
    for ($n = 90; $n -ge 1; $n--) {
        $oldId = "eyeshadow_$n"
        $newId = Get-NewEyeshadowId $n
        $library = $library.Replace("`"$oldId`"", "`"$newId`"")
    }
    Set-Content $libraryCs -Value $library.TrimEnd() -NoNewline -Encoding UTF8
}

# Update MainScene Eyeshadow layer sprite.
if (Test-Path $mainScene) {
    $scene = Get-Content $mainScene -Raw -Encoding UTF8
    $scene = $scene.Replace("guid: $oldSpriteGuid", "guid: $newSpriteGuid")
    Set-Content $mainScene -Value $scene.TrimEnd() -NoNewline -Encoding UTF8
}

# Remove old sprite.
$oldSprite = Join-Path $spriteFolder "eyeshadow_1.png"
$oldSpriteMeta = "$oldSprite.meta"
if (Test-Path $oldSprite) { Remove-Item $oldSprite -Force }
if (Test-Path $oldSpriteMeta) { Remove-Item $oldSpriteMeta -Force }

Write-Output "Migrated 90 eyeshadow items to eyeshadow_2_XX using sprite guid $newSpriteGuid"
