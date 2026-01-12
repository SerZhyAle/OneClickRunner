# PowerShell script to generate OneClickRunner icon
Add-Type -AssemblyName System.Drawing

# Create bitmap
$bmp = New-Object System.Drawing.Bitmap(256, 256)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Clear with transparent background
$g.Clear([System.Drawing.Color]::Transparent)

# Create gradient brush (blue gradient)
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    [System.Drawing.Point]::new(0, 0),
    [System.Drawing.Point]::new(256, 256),
    [System.Drawing.Color]::FromArgb(255, 41, 128, 185),
    [System.Drawing.Color]::FromArgb(255, 52, 152, 219)
)

# Draw main circle
$g.FillEllipse($brush, 20, 20, 216, 216)

# Draw checkmark/play symbol in white
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 14)
$pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round

# Draw checkmark representing "one click" action
$g.DrawLine($pen, 80, 128, 120, 168)
$g.DrawLine($pen, 120, 168, 190, 88)

# Cleanup
$g.Dispose()

# Save as PNG
$iconPath = Join-Path $PSScriptRoot "icon.png"
$bmp.Save($iconPath, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()

Write-Host "Icon generated at: $iconPath"
Write-Host "Now convert to ICO using online tool or magick: magick icon.png -define icon:auto-resize=256,128,64,48,32,16 icon.ico"
