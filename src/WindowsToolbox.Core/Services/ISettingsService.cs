namespace WindowsToolbox.Core.Services;

public interface ISettingsService
{
    T? GetValue<T>(string key, T? defaultValue = default);
    void SetValue<T>(string key, T value);
    void Save();
    void Load();
}

public class SettingsService : ISettingsService
{
    private readonly Dictionary<string, object> _settings = new();
    private readonly string _settingsFilePath;

    public SettingsService(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
        Load();
    }

    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        if (_settings.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }

    public void SetValue<T>(string key, T value)
    {
        if (value is not null)
            _settings[key] = value;
        else
            _settings.Remove(key);
    }

    public void Save()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = System.Text.Json.JsonSerializer.Serialize(_settings, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (settings != null)
                {
                    _settings.Clear();
                    foreach (var kvp in settings)
                    {
                        _settings[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load settings: {ex.Message}");
        }
    }
}
