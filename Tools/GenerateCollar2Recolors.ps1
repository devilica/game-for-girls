Add-Type -AssemblyName System.Drawing

$srcPath = Join-Path $PSScriptRoot '..\Assets\Sprites\Pet\Collar\collar_2.png' | Resolve-Path
$outDir = Split-Path $srcPath -Parent
$keyLum = (0.299 * 89 + 0.587 * 191 + 0.114 * 253) / 255.0
$minLum = 0.12
$firstIndex = 13

$targets = @(
    @(168, 42, 48),
    @(240, 100, 150),
    @(55, 150, 75),
    @(140, 70, 180),
    @(230, 110, 45)
)

function Get-Luminance($r, $g, $b) {
    return (0.299 * $r + 0.587 * $g + 0.114 * $b) / 255.0
}

function Scale-Channel($channel, $scale) {
    $v = [int][Math]::Round($channel * $scale)
    if ($v -lt 0) { return 0 }
    if ($v -gt 255) { return 255 }
    return [byte]$v
}

$source = [System.Drawing.Bitmap]::FromFile($srcPath)
try {
    for ($i = 0; $i -lt $targets.Count; $i++) {
        $collarIndex = $firstIndex + $i
        $tr = $targets[$i][0]
        $tg = $targets[$i][1]
        $tb = $targets[$i][2]
        $out = New-Object System.Drawing.Bitmap $source.Width, $source.Height
        for ($y = 0; $y -lt $source.Height; $y++) {
            for ($x = 0; $x -lt $source.Width; $x++) {
                $c = $source.GetPixel($x, $y)
                if ($c.A -lt 8) {
                    $out.SetPixel($x, $y, $c)
                    continue
                }
                $lum = Get-Luminance $c.R $c.G $c.B
                if ($lum -lt $minLum) {
                    $out.SetPixel($x, $y, $c)
                    continue
                }
                $scale = $lum / $keyLum
                $nr = Scale-Channel $tr $scale
                $ng = Scale-Channel $tg $scale
                $nb = Scale-Channel $tb $scale
                $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($c.A, $nr, $ng, $nb))
            }
        }
        $outPath = Join-Path $outDir ("collar_{0}.png" -f $collarIndex)
        $out.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $out.Dispose()
        Write-Host "Wrote $outPath"
    }
}
finally {
    $source.Dispose()
}
