# Package Images

This folder contains the required images for the MSIX package manifest.

## Required Images

Place the following images in this directory:

### Store Logos
- **StoreLogo.png** - 50x50 px
  - Used in Microsoft Store and Settings

### Square Tiles
- **Square44x44Logo.png** - 44x44 px
  - App list and Start menu (small)
  
- **Square71x71Logo.png** (SmallTile.png) - 71x71 px
  - Small tile on Start menu
  
- **Square150x150Logo.png** - 150x150 px
  - Medium tile on Start menu
  
- **Square310x310Logo.png** (LargeTile.png) - 310x310 px
  - Large tile on Start menu

### Wide Tile
- **Wide310x150Logo.png** - 310x150 px
  - Wide tile on Start menu

### Splash Screen
- **SplashScreen.png** - 620x300 px
  - Shown while app is loading

## Image Guidelines

### Format
- Use PNG format with transparency
- 32-bit color depth
- Transparent background recommended

### Design
- Follow Windows 11 design principles
- Use app primary color
- Keep design simple and recognizable
- Ensure legibility at all sizes

### Scaling
The system will automatically scale these base images, but you can provide scale variants:
- `Logo.scale-100.png` (base)
- `Logo.scale-125.png` (125%)
- `Logo.scale-150.png` (150%)
- `Logo.scale-200.png` (200%)
- `Logo.scale-400.png` (400%)

### Tools
Use these tools to create/edit images:
- **Adobe Illustrator** - Professional
- **Figma** - Free, web-based
- **Paint.NET** - Free, Windows
- **GIMP** - Free, cross-platform

### Templates
Download official templates:
https://docs.microsoft.com/windows/apps/design/style/app-icons-and-logos

## Quick Start

If you don't have custom images yet:

1. Create a simple 310x310 px image with your app icon/logo
2. Resize it to each required size
3. Save as PNG in this directory with the exact names above
4. Build the package

## Temporary Placeholders

For development/testing, you can use solid color placeholders:

```powershell
# PowerShell script to create placeholder images
Add-Type -AssemblyName System.Drawing

function New-PlaceholderImage {
    param($Width, $Height, $Path)
    $bitmap = New-Object System.Drawing.Bitmap($Width, $Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215)) # Windows blue
    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}

New-PlaceholderImage 50 50 "StoreLogo.png"
New-PlaceholderImage 44 44 "Square44x44Logo.png"
New-PlaceholderImage 71 71 "SmallTile.png"
New-PlaceholderImage 150 150 "Square150x150Logo.png"
New-PlaceholderImage 310 310 "LargeTile.png"
New-PlaceholderImage 310 150 "Wide310x150Logo.png"
New-PlaceholderImage 620 300 "SplashScreen.png"
```

Run this in this directory to create basic placeholders.

## Resources

- [App icon design guidelines](https://docs.microsoft.com/windows/apps/design/style/app-icons-and-logos)
- [Tile and icon assets](https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/app-assets)
- [Asset generator tool](https://github.com/shanselman/UWPAssetGenerator)
