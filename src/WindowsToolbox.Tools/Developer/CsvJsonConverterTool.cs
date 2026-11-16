using System.Text;
using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class CsvJsonConverterTool : BaseTool
{
    public override string Id => "csv-json-converter";
    public override string Name => "CSV ↔ JSON Converter";
    public override string Description => "Convert between CSV and JSON formats";
    public override string Category => "developer";
    public override string Icon => "Table";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = parameters.Get<string>("input");
            if (string.IsNullOrWhiteSpace(input))
                return Failure("Input is required");

            var mode = parameters.Get<string>("mode") ?? "csvToJson"; // csvToJson or jsonToCsv
            var delimiter = parameters.Get<string>("delimiter") ?? ",";
            var hasHeaders = parameters.Get<bool>("hasHeaders");
            if (!parameters.Values.ContainsKey("hasHeaders"))
                hasHeaders = true;

            ReportProgress(progress, 30, "Converting...");

            string output;
            if (mode == "csvToJson")
            {
                output = ConvertCsvToJson(input, delimiter[0], hasHeaders);
            }
            else if (mode == "jsonToCsv")
            {
                output = ConvertJsonToCsv(input, delimiter[0], hasHeaders);
            }
            else
            {
                return Failure($"Invalid mode: {mode}");
            }

            ReportProgress(progress, 100, "Complete");

            return Success("Conversion completed successfully", new Dictionary<string, object>
            {
                ["output"] = output
            });
        }
        catch (JsonException ex)
        {
            return Failure($"Invalid JSON: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert: {ex.Message}", ex);
        }
    }

    private string ConvertCsvToJson(string csv, char delimiter, bool hasHeaders)
    {
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                       .Select(l => l.Trim())
                       .Where(l => !string.IsNullOrEmpty(l))
                       .ToArray();

        if (lines.Length == 0)
            throw new Exception("CSV is empty");

        var headers = ParseCsvLine(lines[0], delimiter);
        var startIndex = hasHeaders ? 1 : 0;

        if (!hasHeaders)
        {
            headers = Enumerable.Range(0, headers.Length)
                                .Select(i => $"column{i}")
                                .ToArray();
        }

        var records = new List<Dictionary<string, object>>();

        for (int i = startIndex; i < lines.Length; i++)
        {
            var values = ParseCsvLine(lines[i], delimiter);
            var record = new Dictionary<string, object>();

            for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
            {
                record[headers[j]] = values[j];
            }

            records.Add(record);
        }

        return JsonSerializer.Serialize(records, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private string ConvertJsonToCsv(string json, char delimiter, bool includeHeaders)
    {
        var records = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);
        if (records == null || records.Count == 0)
            throw new Exception("JSON is empty or invalid");

        var headers = records[0].Keys.ToArray();
        var sb = new StringBuilder();

        if (includeHeaders)
        {
            sb.AppendLine(string.Join(delimiter, headers.Select(EscapeCsvValue)));
        }

        foreach (var record in records)
        {
            var values = headers.Select(h =>
            {
                if (record.TryGetValue(h, out var element))
                {
                    return element.ValueKind switch
                    {
                        JsonValueKind.String => element.GetString() ?? "",
                        JsonValueKind.Number => element.GetRawText(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => "",
                        _ => element.GetRawText()
                    };
                }
                return "";
            });

            sb.AppendLine(string.Join(delimiter, values.Select(EscapeCsvValue)));
        }

        return sb.ToString();
    }

    private string[] ParseCsvLine(string line, char delimiter)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString());
        return values.ToArray();
    }

    private string EscapeCsvValue(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
