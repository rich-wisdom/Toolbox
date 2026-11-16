#Requires -Version 5.1
<#
.SYNOPSIS
    Creates placeholder images for MSIX package during development.

.DESCRIPTION
    This script generates simple colored placeholder images for the MSIX package manifest.
    Use this for development and testing. Replace with proper branding images for production.

.PARAMETER OutputPath
    Directory to save the images. Default: ../src/WindowsToolbox.Package/Images

.PARAMETER Color
    Hex color code for the placeholder (e.g., "#0078D7" for Windows blue)
    Default: #0078D7 (Windows accent blue)

.EXAMPLE
    .\Create-Placeholder-Images.ps1
    Creates blue placeholder images in the package Images folder

.EXAMPLE
    .\Create-Placeholder-Images.ps1 -Color "#FF6B6B" -OutputPath "C:\MyImages"
    Creates red placeholder images in a custom folder
#>

param(
    [Parameter()]
    [string]$OutputPath,
    
    [Parameter()]
    [string]$Color = "#0078D7"
)

$ErrorActionPreference = 'Stop'

# Resolve output path
if (-not $OutputPath) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $OutputPath = Join-Path (Split-Path -Parent $scriptDir) "src\WindowsToolbox.Package\Images"
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "MSIX Package Image Generator" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Creating placeholder images..." -ForegroundColor Yellow
Write-Host "Output: $OutputPath" -ForegroundColor Gray
Write-Host "Color: $Color" -ForegroundColor Gray
Write-Host ""

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# Add System.Drawing
Add-Type -AssemblyName System.Drawing

# Parse hex color
$Color = $Color.TrimStart('#')
$r = [Convert]::ToInt32($Color.Substring(0, 2), 16)
$g = [Convert]::ToInt32($Color.Substring(2, 2), 16)
$b = [Convert]::ToInt32($Color.Substring(4, 2), 16)
$fillColor = [System.Drawing.Color]::FromArgb(255, $r, $g, $b)

function New-PlaceholderImage {
    param(
        [int]$Width,
        [int]$Height,
        [string]$Filename,
        [System.Drawing.Color]$Color
    )
    
    $path = Join-Path $OutputPath $Filename
    
    try {
        $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        
        # Fill background
        $graphics.Clear($Color)
        
        # Add "WT" text in the center
        $text = "WT"
        $font = New-Object System.Drawing.Font("Segoe UI", [Math]::Min($Width, $Height) / 3, [System.Drawing.FontStyle]::Bold)
        $textBrush = [System.Drawing.Brushes]::White
        
        $textSize = $graphics.MeasureString($text, $font)
        $x = ($Width - $textSize.Width) / 2
        $y = ($Height - $textSize.Height) / 2
        
        $graphics.DrawString($text, $font, $textBrush, $x, $y)
        
        # Save
        $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        
        Write-Host "  ✓ Created: $Filename ($Width x $Height)" -ForegroundColor Green
        
        # Cleanup
        $graphics.Dispose()
        $font.Dispose()
        $bitmap.Dispose()
    }
    catch {
        Write-Host "  ✗ Failed: $Filename - $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Create all required images
$images = @(
    @{ Width = 50;  Height = 50;  Name = "StoreLogo.png" },
    @{ Width = 44;  Height = 44;  Name = "Square44x44Logo.png" },
    @{ Width = 71;  Height = 71;  Name = "SmallTile.png" },
    @{ Width = 150; Height = 150; Name = "Square150x150Logo.png" },
    @{ Width = 310; Height = 310; Name = "LargeTile.png" },
    @{ Width = 310; Height = 150; Name = "Wide310x150Logo.png" },
    @{ Width = 620; Height = 300; Name = "SplashScreen.png" }
)

foreach ($img in $images) {
    New-PlaceholderImage -Width $img.Width -Height $img.Height -Filename $img.Name -Color $fillColor
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "✓ All images created successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠ These are placeholder images for development only." -ForegroundColor Yellow
Write-Host "Replace with professional branding images for production release." -ForegroundColor Yellow
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review the images in: $OutputPath"
Write-Host "  2. Replace with your branded images"
Write-Host "  3. Run: .\Build-Installer.ps1"
Write-Host ""
