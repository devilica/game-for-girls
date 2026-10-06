Add-Type -AssemblyName System.Drawing

$srcPath = Join-Path $PSScriptRoot '..\Assets\Sprites\Pet\Pets\pet_1.png' | Resolve-Path
$outDir = Split-Path $srcPath -Parent
$key = [byte[]]@(178, 104, 51)
$maxDist = 58.0

$variants = @(
    @{ Suffix = 'dark_yellow'; Target = @(154, 126, 22) },
    @{ Suffix = 'dark_orange'; Target = @(168, 88, 32) },
    @{ Suffix = 'grey'; Target = @(110, 110, 110) },
    @{ Suffix = 'black'; Target = @(32, 32, 32) },
    @{ Suffix = 'brown'; Target = @(74, 52, 37) }
)

function Get-Strength($r, $g, $b) {
    $dr = $r - $key[0]
    $dg = $g - $key[1]
    $db = $b - $key[2]
    $dist = [Math]::Sqrt($dr * $dr + $dg * $dg + $db * $db)
    if ($dist -gt $maxDist) { return -1.0 }
    return 1.0 - [Math]::Min(1.0, $dist / $maxDist)
}

$source = [System.Drawing.Bitmap]::FromFile($srcPath)
try {
    foreach ($variant in $variants) {
        $out = New-Object System.Drawing.Bitmap $source.Width, $source.Height
        $tr = $variant.Target[0]
        $tg = $variant.Target[1]
        $tb = $variant.Target[2]
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $c = $source.GetPixel($x, $y)
                if ($c.A -lt 8) {
                    $out.SetPixel($x, $y, $c)
                    continue
                }
                $strength = Get-Strength $c.R $c.G $c.B
                if ($strength -lt 0) {
                    $out.SetPixel($x, $y, $c)
                    continue
                }
                $nr = [byte][Math]::Round($c.R + ($tr - $c.R) * $strength)
                $ng = [byte][Math]::Round($c.G + ($tg - $c.G) * $strength)
                $nb = [byte][Math]::Round($c.B + ($tb - $c.B) * $strength)
                $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($c.A, $nr, $ng, $nb))
            }
        }
        $outPath = Join-Path $outDir ("pet_1_{0}.png" -f $variant.Suffix)
        $out.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $out.Dispose()
        Write-Host "Wrote $outPath"
    }
}
finally {
    $source.Dispose()
}
