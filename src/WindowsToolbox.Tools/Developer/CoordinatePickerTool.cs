using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class CoordinatePickerTool : BaseTool
{
    public override string Id => "coordinate-picker";
    public override string Name => "Coordinate Picker";
    public override string Description => "Convert and format geographic coordinates";
    public override string Category => "developer";
    public override string Icon => "MapPin";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var latitude = parameters.Get<double>("latitude");
            var longitude = parameters.Get<double>("longitude");

            if (latitude < -90 || latitude > 90)
                return Failure("Latitude must be between -90 and 90");

            if (longitude < -180 || longitude > 180)
                return Failure("Longitude must be between -180 and 180");

            ReportProgress(progress, 50, "Converting coordinates...");

            var formats = new Dictionary<string, object>
            {
                ["dd"] = $"{latitude:F6}, {longitude:F6}",
                ["dms"] = $"{ConvertToDMS(latitude, true)}, {ConvertToDMS(longitude, false)}",
                ["ddm"] = $"{ConvertToDDM(latitude, true)}, {ConvertToDDM(longitude, false)}",
                ["latitude"] = latitude,
                ["longitude"] = longitude
            };

            ReportProgress(progress, 100, "Complete");

            return Success("Coordinates formatted successfully", formats);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to format coordinates: {ex.Message}", ex);
        }
    }

    private string ConvertToDMS(double coordinate, bool isLatitude)
    {
        var abs = Math.Abs(coordinate);
        var degrees = (int)abs;
        var minutesDecimal = (abs - degrees) * 60;
        var minutes = (int)minutesDecimal;
        var seconds = (minutesDecimal - minutes) * 60;

        var direction = coordinate >= 0 
            ? (isLatitude ? "N" : "E") 
            : (isLatitude ? "S" : "W");

        return $"{degrees}°{minutes}'{seconds:F2}\"{direction}";
    }

    private string ConvertToDDM(double coordinate, bool isLatitude)
    {
        var abs = Math.Abs(coordinate);
        var degrees = (int)abs;
        var minutes = (abs - degrees) * 60;

        var direction = coordinate >= 0 
            ? (isLatitude ? "N" : "E") 
            : (isLatitude ? "S" : "W");

        return $"{degrees}°{minutes:F4}'{direction}";
    }
}
