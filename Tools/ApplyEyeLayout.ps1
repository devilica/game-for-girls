$folder = Join-Path $PSScriptRoot "..\Assets\Resources\Items\Makeup"
$count = 0

Get-ChildItem $folder -Filter "eyes_2_*.asset" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace "layerOffset: \{x: [^}]+\}", "layerOffset: {x: 0, y: 2.01, z: 0}"
    $content = $content -replace "layerScale: \{x: [^}]+\}", "layerScale: {x: 0.13, y: 0.13, z: 0.13}"
    Set-Content $_.FullName -Value $content -NoNewline -Encoding UTF8
    $count++
}

Write-Output "Updated $count eye assets"
