Add-Type -AssemblyName System.Drawing

$srcPath = Join-Path $PSScriptRoot '..\Assets\Sprites\Pet\Hairbows\bow_6.png' | Resolve-Path
$outDir = Split-Path $srcPath -Parent
$keyLum = (0.299 * 248 + 0.587 * 246 + 0.114 * 245) / 255.0
$minLum = 0.12
$firstIndex = 8

# kind: 0=solid, 1=rainbow, 2=glitter
$variants = @(
    @{ Kind = 0; R = 240; G = 160; B = 190 },
    @{ Kind = 0; R = 168; G = 42; B = 48 },
    @{ Kind = 0; R = 90; G = 160; B = 220 },
    @{ Kind = 0; R = 160; G = 120; B = 210 },
    @{ Kind = 0; R = 100; G = 190; B = 140 },
    @{ Kind = 0; R = 230; G = 120; B = 45 },
    @{ Kind = 0; R = 200; G = 150; B = 230 },
    @{ Kind = 0; R = 218; G = 175; B = 55 },
    @{ Kind = 0; R = 192; G = 192; B = 200 },
    @{ Kind = 0; R = 160; G = 220; B = 150 },
    @{ Kind = 0; R = 140; G = 210; B = 120 },
    @{ Kind = 1; R = 255; G = 255; B = 255 },
    @{ Kind = 2; R = 240; G = 180; B = 220 }
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

function Get-HsvRgb($h, $s, $v) {
    $h = (($h % 360) + 360) % 360
    $c = $v * $s
    $x = $c * (1 - [Math]::Abs(($h / 60.0) % 2 - 1))
    $m = $v - $c
    if ($h -lt 60) { $rf = $c; $gf = $x; $bf = 0 }
    elseif ($h -lt 120) { $rf = $x; $gf = $c; $bf = 0 }
    elseif ($h -lt 180) { $rf = 0; $gf = $c; $bf = $x }
    elseif ($h -lt 240) { $rf = 0; $gf = $x; $bf = $c }
    elseif ($h -lt 300) { $rf = $x; $gf = 0; $bf = $c }
    else { $rf = $c; $gf = 0; $bf = $x }
    return @(
        [byte][Math]::Round(($rf + $m) * 255),
        [byte][Math]::Round(($gf + $m) * 255),
        [byte][Math]::Round(($bf + $m) * 255)
    )
}

function Apply-Glitter($x, $y, [ref]$r, [ref]$g, [ref]$b) {
    $hash = [Math]::Abs((($x * 73856093) -bxor ($y * 19349663)) % 1000)
    if ($hash -lt 25) { $r.Value = 255; $g.Value = 250; $b.Value = 245; return }
    if ($hash -lt 35) { $r.Value = 255; $g.Value = 230; $b.Value = 120 }
}

$source = [System.Drawing.Bitmap]::FromFile($srcPath)
try {
    $w = $source.Width
    $h = $source.Height
    for ($vi = 0; $vi -lt $variants.Count; $vi++) {
        $v = $variants[$vi]
        $bowIndex = $firstIndex + $vi
        $out = New-Object System.Drawing.Bitmap $w, $h
        for ($y = 0; $y -lt $h; $y++) {
            for ($x = 0; $x -lt $w; $x++) {
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
                if ($v.Kind -eq 1) {
                    $t = if ($w -le 1) { 0 } else { $x / ($w - 1.0) }
                    $hue = $t * 360.0
                    $val = [Math]::Min(1.0, $lum * 1.15 + 0.08)
                    $rgb = Get-HsvRgb $hue 0.72 $val
                    $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($c.A, $rgb[0], $rgb[1], $rgb[2]))
                    continue
                }
                $scale = $lum / $keyLum
                $nr = Scale-Channel $v.R $scale
                $ng = Scale-Channel $v.G $scale
                $nb = Scale-Channel $v.B $scale
                if ($v.Kind -eq 2) {
                    $rr = [ref][byte]$nr; $gg = [ref][byte]$ng; $bb = [ref][byte]$nb
                    Apply-Glitter $x $y $rr $gg $bb
                    $nr = $rr.Value; $ng = $gg.Value; $nb = $bb.Value
                }
                $out.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($c.A, $nr, $ng, $nb))
            }
        }
        $outPath = Join-Path $outDir ("bow_{0}.png" -f $bowIndex)
        $out.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
        $out.Dispose()
        Write-Host "Wrote $outPath"
    }
    for ($bowIndex = $firstIndex + $variants.Count; $bowIndex -le 30; $bowIndex++) {
        $extra = Join-Path $outDir ("bow_{0}.png" -f $bowIndex)
        if (Test-Path $extra) { Remove-Item $extra -Force; Write-Host "Removed $extra" }
        $extraMeta = $extra + '.meta'
        if (Test-Path $extraMeta) { Remove-Item $extraMeta -Force }
    }
}
finally {
    $source.Dispose()
}
