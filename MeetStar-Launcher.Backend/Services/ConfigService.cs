using System.Text.Json;
using MeetStar_Launcher.Backend.Models;

namespace MeetStar_Launcher.Backend.Services;

/// <summary>
/// 负责读写启动器配置（应用列表等），保存为 JSON。
/// 配置位置：%LOCALAPPDATA%\MeetStar-Launcher\config.json
/// </summary>
public class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private static string ConfigDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MeetStar-Launcher");

    private static string ConfigFilePath => Path.Combine(ConfigDirectory, "config.json");

    /// <summary>加载应用列表；配置不存在时创建默认空配置。</summary>
    public List<LauncherApp> LoadApps()
    {
        if (!File.Exists(ConfigFilePath))
        {
            SaveApps([]);
            return [];
        }

        var json = File.ReadAllText(ConfigFilePath);
        return JsonSerializer.Deserialize<List<LauncherApp>>(json, JsonOptions) ?? [];
    }

    /// <summary>保存应用列表。</summary>
    public void SaveApps(IEnumerable<LauncherApp> apps)
    {
        Directory.CreateDirectory(ConfigDirectory);
        File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(apps, JsonOptions));
    }

    /// <summary>配置文件完整路径（供前端展示用）。</summary>
    public string ConfigFilePathForDisplay() => ConfigFilePath;
}
