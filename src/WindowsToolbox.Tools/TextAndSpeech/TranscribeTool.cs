using System.Text;
using FFMpegCore;
using Whisper.net;
using Whisper.net.Ggml;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class TranscribeTool : BaseTool
{
    public override string Id => "transcribe";
    public override string Name => "Transcribe";
    public override string Description => "Convert speech to text from audio or video";
    public override string Category => "text-speech";
    public override string Icon => "Microphone";

    private static readonly string ModelPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WindowsToolbox", "Models", "Whisper");

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var language = parameters.Get<string>("language") ?? "en";
            var outputFormat = parameters.Get<string>("outputFormat") ?? "text";
            var modelSize = parameters.Get<string>("modelSize") ?? "base";

            if (string.IsNullOrWhiteSpace(inputFile))
                return Failure("Input file is required");

            if (!File.Exists(inputFile))
                return Failure("Input file does not exist");

            ReportProgress(progress, 5, "Preparing transcription...");

            var tempAudioFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");

            try
            {
                ReportProgress(progress, 10, "Converting to WAV format...");

                await FFMpegArguments
                    .FromFileInput(inputFile)
                    .OutputToFile(tempAudioFile, true, options => options
                        .WithAudioCodec("pcm_s16le")
                        .WithAudioSamplingRate(16000)
                        .WithCustomArgument("-ac 1"))
                    .CancellableThrough(cancellationToken)
                    .ProcessAsynchronously();

                ReportProgress(progress, 30, "Downloading model (if needed)...");

                Directory.CreateDirectory(ModelPath);
                var modelFile = Path.Combine(ModelPath, $"ggml-{modelSize}.bin");

                if (!File.Exists(modelFile))
                {
                    var modelType = modelSize.ToLower() switch
                    {
                        "tiny" => GgmlType.Tiny,
                        "base" => GgmlType.Base,
                        "small" => GgmlType.Small,
                        "medium" => GgmlType.Medium,
                        _ => GgmlType.Base
                    };

                    await WhisperGgmlDownloader.GetGgmlModelAsync(modelType, modelFile);
                }

                ReportProgress(progress, 50, "Transcribing audio...");

                using var whisperFactory = WhisperFactory.FromPath(modelFile);
                using var processor = whisperFactory.CreateBuilder()
                    .WithLanguage(language)
                    .Build();

                var segments = new List<SegmentData>();
                var fullText = new StringBuilder();

                await foreach (var segment in processor.ProcessAsync(tempAudioFile, cancellationToken))
                {
                    segments.Add(segment);
                    fullText.AppendLine(segment.Text);
                    ReportProgress(progress, 50 + (int)(segments.Count * 0.5), "Transcribing...");
                }

                ReportProgress(progress, 90, "Formatting output...");

                var outputText = FormatOutput(segments, outputFormat, fullText.ToString());
                var outputFile = Path.ChangeExtension(inputFile, outputFormat == "srt" || outputFormat == "vtt" ? outputFormat : "txt");

                await File.WriteAllTextAsync(outputFile, outputText, cancellationToken);

                ReportProgress(progress, 100, "Transcription complete");

                return Success("Transcription completed successfully", new Dictionary<string, object>
                {
                    ["outputFile"] = outputFile,
                    ["text"] = fullText.ToString(),
                    ["segmentCount"] = segments.Count
                });
            }
            finally
            {
                if (File.Exists(tempAudioFile))
                    File.Delete(tempAudioFile);
            }
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to transcribe: {ex.Message}", ex);
        }
    }

    private string FormatOutput(List<SegmentData> segments, string format, string fullText)
    {
        return format.ToLower() switch
        {
            "srt" => FormatAsSrt(segments),
            "vtt" => FormatAsVtt(segments),
            "json" => FormatAsJson(segments),
            _ => fullText
        };
    }

    private string FormatAsSrt(List<SegmentData> segments)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            sb.AppendLine($"{i + 1}");
            sb.AppendLine($"{FormatTime(segment.Start)} --> {FormatTime(segment.End)}");
            sb.AppendLine(segment.Text.Trim());
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string FormatAsVtt(List<SegmentData> segments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();
        
        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            sb.AppendLine($"{FormatTime(segment.Start)} --> {FormatTime(segment.End)}");
            sb.AppendLine(segment.Text.Trim());
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string FormatAsJson(List<SegmentData> segments)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(segments.Select(s => new
        {
            start = s.Start.TotalSeconds,
            end = s.End.TotalSeconds,
            text = s.Text.Trim()
        }), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        return json;
    }

    private string FormatTime(TimeSpan time)
    {
        return $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00},{time.Milliseconds:000}";
    }
}

