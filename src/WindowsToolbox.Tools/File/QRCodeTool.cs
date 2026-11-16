using QRCoder;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.File;

public class QRCodeTool : BaseTool
{
    public override string Id => "qr-code";
    public override string Name => "QR Code Generator";
    public override string Description => "Generate QR codes from text or URLs";
    public override string Category => "file";
    public override string Icon => "QRCode";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var text = parameters.Get<string>("text");
            if (string.IsNullOrWhiteSpace(text))
                return Failure("Text or URL is required");

            var outputPath = parameters.Get<string>("outputPath");
            if (string.IsNullOrWhiteSpace(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 30, "Generating QR code...");

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            
            ReportProgress(progress, 70, "Saving QR code...");

            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            await System.IO.File.WriteAllBytesAsync(outputPath, qrCodeImage, cancellationToken);

            ReportProgress(progress, 100, "Complete");

            return Success("QR code generated successfully", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to generate QR code: {ex.Message}", ex);
        }
    }
}
