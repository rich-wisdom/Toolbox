namespace WindowsToolbox.Core.Models;

public enum ToolCategoryType
{
    Video,
    Audio,
    Image,
    Document,
    TextAndSpeech,
    File,
    System,
    Developer
}

public class ToolCategory
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Icon { get; init; }
    public required ToolCategoryType Type { get; init; }
    public int ToolCount { get; set; }

    public static readonly ToolCategory Video = new()
    {
        Id = "video",
        Name = "Video Tools",
        Description = "Download, compress, convert, and edit videos",
        Icon = "Video",
        Type = ToolCategoryType.Video
    };

    public static readonly ToolCategory Audio = new()
    {
        Id = "audio",
        Name = "Audio Tools",
        Description = "Clean, trim, convert, and process audio",
        Icon = "Audio",
        Type = ToolCategoryType.Audio
    };

    public static readonly ToolCategory Image = new()
    {
        Id = "image",
        Name = "Image Tools",
        Description = "Capture, edit, and optimize images",
        Icon = "Image",
        Type = ToolCategoryType.Image
    };

    public static readonly ToolCategory Document = new()
    {
        Id = "document",
        Name = "Document Tools",
        Description = "Scan, merge, sign, and convert documents",
        Icon = "Document",
        Type = ToolCategoryType.Document
    };

    public static readonly ToolCategory TextAndSpeech = new()
    {
        Id = "text-speech",
        Name = "Text & Speech Tools",
        Description = "Transcribe, translate, and format text",
        Icon = "TextDocument",
        Type = ToolCategoryType.TextAndSpeech
    };

    public static readonly ToolCategory File = new()
    {
        Id = "file",
        Name = "File Tools",
        Description = "Rename, organize, and manage files",
        Icon = "Folder",
        Type = ToolCategoryType.File
    };

    public static readonly ToolCategory System = new()
    {
        Id = "system",
        Name = "System Tools",
        Description = "Monitor and optimize your Windows system",
        Icon = "System",
        Type = ToolCategoryType.System
    };

    public static readonly ToolCategory Developer = new()
    {
        Id = "developer",
        Name = "Developer Tools",
        Description = "Format, convert, and debug code and data",
        Icon = "Code",
        Type = ToolCategoryType.Developer
    };

    public static IReadOnlyList<ToolCategory> All { get; } = new List<ToolCategory>
    {
        Video, Audio, Image, Document, TextAndSpeech, File, System, Developer
    };
}
