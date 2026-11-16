using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class AddPasswordTool : BaseTool
{
    public override string Id => "add-password";
    public override string Name => "Add Password";
    public override string Description => "Protect PDF with password";
    public override string Category => "document";
    public override string Icon => "Lock";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var userPassword = parameters.Get<string>("userPassword");
            var ownerPassword = parameters.Get<string>("ownerPassword") ?? userPassword;
            var allowPrinting = parameters.Get<bool?>("allowPrinting") ?? true;
            var allowCopy = parameters.Get<bool?>("allowCopy") ?? false;
            var allowModify = parameters.Get<bool?>("allowModify") ?? false;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            if (string.IsNullOrEmpty(userPassword))
                return Failure("Password is required");

            PdfHelpers.ValidatePassword(userPassword);
            ReportProgress(progress, 0, "Encrypting PDF...");

            var userBytes = System.Text.Encoding.UTF8.GetBytes(userPassword);
            var ownerBytes = System.Text.Encoding.UTF8.GetBytes(ownerPassword);

            var permissions = 0;
            if (allowPrinting) permissions |= EncryptionConstants.ALLOW_PRINTING;
            if (allowCopy) permissions |= EncryptionConstants.ALLOW_COPY;
            if (allowModify) permissions |= EncryptionConstants.ALLOW_MODIFY_CONTENTS;

            using var reader = new PdfReader(inputFile);
            var writerProperties = new WriterProperties()
                .SetStandardEncryption(
                    userBytes,
                    ownerBytes,
                    permissions,
                    EncryptionConstants.ENCRYPTION_AES_256
                );

            using var writer = new PdfWriter(outputPath, writerProperties);
            using var sourcePdf = new PdfDocument(reader);
            using var outputPdf = new PdfDocument(writer);

            sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), outputPdf);

            ReportProgress(progress, 100, "Password added");
            return await Task.FromResult(Success("Successfully added password protection", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to add password: {ex.Message}", ex);
        }
    }
}
