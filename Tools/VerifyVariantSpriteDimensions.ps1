Add-Type -AssemblyName System.Drawing
$root = Join-Path (Split-Path $PSScriptRoot -Parent) 'Assets\Sprites\Pet'
$pairs = @(
    @('Hairbows\bow_6.png', 'Hairbows\bow_8.png'),
    @('Collar\collar_4.png', 'Collar\collar_7.png'),
    @('Collar\collar_2.png', 'Collar\collar_13.png')
)
foreach ($pair in $pairs) {
    $a = [System.Drawing.Image]::FromFile((Join-Path $root $pair[0]))
    $b = [System.Drawing.Image]::FromFile((Join-Path $root $pair[1]))
    $match = ($a.Width -eq $b.Width -and $a.Height -eq $b.Height)
    Write-Host ("{0} {1}x{2} vs {3} {4}x{5} match={6}" -f $pair[0], $a.Width, $a.Height, $pair[1], $b.Width, $b.Height, $match)
    $a.Dispose()
    $b.Dispose()
}
