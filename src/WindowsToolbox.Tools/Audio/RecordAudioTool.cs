using NAudio.Wave;
using NAudio.Lame;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class RecordAudioTool : BaseTool
{
    public override string Id => "record-audio";
    public override string Name => "Record Audio";
    public override string Description => "Record audio from microphone or system";
    public override string Category => "audio";
    public override string Icon => "Microphone";

    private WaveInEvent? _waveIn;
    private WasapiLoopbackCapture? _loopbackCapture;
    private WaveFileWriter? _writer;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var outputFile = parameters.Get<string>("outputFile");
            var recordMicrophone = parameters.Get<bool>("recordMicrophone") ?? true;
            var recordSystem = parameters.Get<bool>("recordSystem") ?? false;
            var sampleRate = parameters.Get<int>("sampleRate") ?? 44100;
            var bitDepth = parameters.Get<int>("bitDepth") ?? 16;

            if (string.IsNullOrWhiteSpace(outputFile))
                return Failure("Output file is required");

            ReportProgress(progress, 10, "Starting recording...");

            var waveFormat = new WaveFormat(sampleRate, bitDepth, recordMicrophone && recordSystem ? 2 : 1);

            if (recordMicrophone)
            {
                _waveIn = new WaveInEvent
                {
                    WaveFormat = waveFormat
                };

                _writer = new WaveFileWriter(outputFile, waveFormat);

                _waveIn.DataAvailable += (sender, e) =>
                {
                    _writer?.Write(e.Buffer, 0, e.BytesRecorded);
                };

                _waveIn.RecordingStopped += (sender, e) =>
                {
                    _writer?.Dispose();
                    _waveIn?.Dispose();
                };

                _waveIn.StartRecording();
            }
            else if (recordSystem)
            {
                _loopbackCapture = new WasapiLoopbackCapture();
                _writer = new WaveFileWriter(outputFile, _loopbackCapture.WaveFormat);

                _loopbackCapture.DataAvailable += (sender, e) =>
                {
                    _writer?.Write(e.Buffer, 0, e.BytesRecorded);
                };

                _loopbackCapture.RecordingStopped += (sender, e) =>
                {
                    _writer?.Dispose();
                    _loopbackCapture?.Dispose();
                };

                _loopbackCapture.StartRecording();
            }

            ReportProgress(progress, 50, "Recording...");

            cancellationToken.Register(() =>
            {
                _waveIn?.StopRecording();
                _loopbackCapture?.StopRecording();
            });

            await Task.Delay(Timeout.Infinite, cancellationToken);

            return Success("Recording completed", new Dictionary<string, object>
            {
                ["outputFile"] = outputFile
            });
        }
        catch (OperationCanceledException)
        {
            _waveIn?.StopRecording();
            _loopbackCapture?.StopRecording();
            return Success("Recording stopped by user");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to record audio: {ex.Message}", ex);
        }
    }
}
