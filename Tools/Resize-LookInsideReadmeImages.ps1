# README thumbnails for docs/look-inside/ (max width 400px, JPEG quality 85).
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$projectRoot = Split-Path $PSScriptRoot -Parent
$outDir = Join-Path $projectRoot 'docs\look-inside'
$landingExamples = Join-Path (Split-Path $projectRoot -Parent) 'dress-up-make-up-girls\examples'
$localDir = $outDir

$MaxWidth = 400
$JpegQuality = 85L

$jobs = @(
    @{ Out = 'makeup.jpg'; Landing = 'Pink Princess Dress-Up Game.png'; Local = 'makeup.png' },
    @{ Out = 'dresses.jpg'; Landing = 'Kawaii Princess Dress-Up Game.png'; Local = 'dresses.png' },
    @{ Out = 'hairstyles.jpg'; Landing = 'Dreamy Pink Dollhouse Dress-Up Game.png'; Local = 'hairstyles.png' },
    @{ Out = 'pet-salon.jpg'; Landing = 'Sparkling Puppy Dress-Up Game.png'; Local = 'pet-salon.png' },
    @{ Out = 'memory-match.jpg'; Landing = 'Kawaii Memory Game Dreamscape.png'; Local = 'memory-match.png' },
    @{ Out = 'mystery-gifts.jpg'; Landing = 'Mystery Box Princess Room.png'; Local = 'mystery-gifts.png' }
)

function Resolve-SourcePath {
    param(
        [string]$LandingName,
        [string]$LocalName
    )

    $landingPath = Join-Path $landingExamples $LandingName
    if (Test-Path -LiteralPath $landingPath) {
        return $landingPath
    }

    $localPath = Join-Path $localDir $LocalName
    if (Test-Path -LiteralPath $localPath) {
        return $localPath
    }

    return $null
}

function Save-ResizedJpeg {
    param(
        [string]$SourcePath,
        [string]$DestPath,
        [int]$MaxW,
        [long]$Quality
    )

    $src = [System.Drawing.Image]::FromFile($SourcePath)
    try {
        $scale = [math]::Min(1.0, $MaxW / [double]$src.Width)
        $newW = [int][math]::Round($src.Width * $scale)
        $newH = [int][math]::Round($src.Height * $scale)

        $bmp = New-Object System.Drawing.Bitmap $newW, $newH
        try {
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            try {
                $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $g.DrawImage($src, 0, 0, $newW, $newH)
            }
            finally {
                $g.Dispose()
            }

            $encoder = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() |
                Where-Object { $_.MimeType -eq 'image/jpeg' } |
                Select-Object -First 1
            if (-not $encoder) {
                throw 'JPEG encoder not found.'
            }

            $encParams = New-Object System.Drawing.Imaging.EncoderParameters(1)
            $encParams.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter(
                [System.Drawing.Imaging.Encoder]::Quality,
                $Quality)

            $dir = Split-Path $DestPath -Parent
            if (-not (Test-Path $dir)) {
                New-Item -ItemType Directory -Path $dir -Force | Out-Null
            }

            if (Test-Path -LiteralPath $DestPath) {
                Remove-Item -LiteralPath $DestPath -Force
            }

            $bmp.Save($DestPath, $encoder, $encParams)
        }
        finally {
            $bmp.Dispose()
        }
    }
    finally {
        $src.Dispose()
    }
}

if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

foreach ($job in $jobs) {
    $source = Resolve-SourcePath -LandingName $job.Landing -LocalName $job.Local
    if (-not $source) {
        Write-Warning "Skip $($job.Out): source not found (landing or docs/look-inside/$($job.Local))."
        continue
    }

    $dest = Join-Path $outDir $job.Out
    Save-ResizedJpeg -SourcePath $source -DestPath $dest -MaxW $MaxWidth -Quality $JpegQuality
    $bytes = (Get-Item -LiteralPath $dest).Length
    Write-Host "Wrote $dest ($bytes bytes) from $source"
}

Get-ChildItem -Path $outDir -Filter '*.png' -ErrorAction SilentlyContinue | ForEach-Object {
    Remove-Item -LiteralPath $_.FullName -Force
    Write-Host "Removed large PNG $($_.Name)"
}

Write-Host 'Done. Update README if new output names were added.'