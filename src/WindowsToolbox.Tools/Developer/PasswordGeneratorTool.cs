using System.Security.Cryptography;
using System.Text;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class PasswordGeneratorTool : BaseTool
{
    public override string Id => "password-generator";
    public override string Name => "Password Generator";
    public override string Description => "Generate secure random passwords";
    public override string Category => "developer";
    public override string Icon => "PasswordField";

    private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Digits = "0123456789";
    private const string Symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    private const string Ambiguous = "0O1lI";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var length = parameters.Get<int>("length");
            if (length <= 0) length = 16;
            if (length > 256) length = 256;

            var count = parameters.Get<int>("count");
            if (count <= 0) count = 1;
            if (count > 100) count = 100;

            var includeLowercase = parameters.Get<bool>("includeLowercase");
            var includeUppercase = parameters.Get<bool>("includeUppercase");
            var includeDigits = parameters.Get<bool>("includeDigits");
            var includeSymbols = parameters.Get<bool>("includeSymbols");
            var excludeAmbiguous = parameters.Get<bool>("excludeAmbiguous");

            if (!parameters.Values.ContainsKey("includeLowercase")) includeLowercase = true;
            if (!parameters.Values.ContainsKey("includeUppercase")) includeUppercase = true;
            if (!parameters.Values.ContainsKey("includeDigits")) includeDigits = true;

            if (!includeLowercase && !includeUppercase && !includeDigits && !includeSymbols)
                return Failure("At least one character type must be selected");

            ReportProgress(progress, 30, $"Generating {count} password(s)...");

            var charset = new StringBuilder();
            if (includeLowercase) charset.Append(Lowercase);
            if (includeUppercase) charset.Append(Uppercase);
            if (includeDigits) charset.Append(Digits);
            if (includeSymbols) charset.Append(Symbols);

            if (excludeAmbiguous)
            {
                foreach (var c in Ambiguous)
                    charset.Replace(c.ToString(), "");
            }

            var passwords = new List<Dictionary<string, object>>();
            for (int i = 0; i < count; i++)
            {
                var password = GeneratePassword(length, charset.ToString());
                var strength = CalculateStrength(password);

                passwords.Add(new Dictionary<string, object>
                {
                    ["password"] = password,
                    ["strength"] = strength
                });

                if (i % 10 == 0)
                    ReportProgress(progress, 30 + (int)((i / (double)count) * 60), $"Generated {i + 1}/{count}...");
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Generated {count} password(s)", new Dictionary<string, object>
            {
                ["passwords"] = passwords,
                ["count"] = count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to generate passwords: {ex.Message}", ex);
        }
    }

    private string GeneratePassword(int length, string charset)
    {
        var password = new char[length];
        var randomBytes = new byte[length * 4];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (int i = 0; i < length; i++)
        {
            var randomValue = BitConverter.ToUInt32(randomBytes, i * 4);
            password[i] = charset[(int)(randomValue % (uint)charset.Length)];
        }

        return new string(password);
    }

    private string CalculateStrength(string password)
    {
        var score = 0;

        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;
        if (password.Any(char.IsLower)) score++;
        if (password.Any(char.IsUpper)) score++;
        if (password.Any(char.IsDigit)) score++;
        if (password.Any(c => Symbols.Contains(c))) score++;

        return score switch
        {
            >= 6 => "Strong",
            >= 4 => "Medium",
            >= 2 => "Weak",
            _ => "Very Weak"
        };
    }
}
