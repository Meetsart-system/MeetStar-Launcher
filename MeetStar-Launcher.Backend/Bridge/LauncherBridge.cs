using System.Runtime.InteropServices;
using System.Text.Json;
using MeetStar_Launcher.Backend.Models;
using MeetStar_Launcher.Backend.Services;

namespace MeetStar_Launcher.Backend.Bridge;

/// <summary>
/// JS ↔ C# 桥接层。
/// 前端通过 window.chrome.webview.hostObjects.launcher 调用这里的公开方法。
/// 方法签名与异常语义的契约定义见仓库根 contracts/README.md。
/// </summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class LauncherBridge
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly ConfigService _configService = new();
    private readonly AppLauncherService _launcherService = new();
    private readonly IWindowController _window;

    public LauncherBridge(IWindowController window)
    {
        _window = window;
    }

    /// <summary>获取应用列表（JSON 字符串，camelCase）。</summary>
    public string LoadAppsJson()
    {
        var apps = _configService.LoadApps();
        return JsonSerializer.Serialize(apps, JsonOptions);
    }

    /// <summary>保存应用列表（接收 camelCase JSON 字符串）。</summary>
    public void SaveAppsJson(string appsJson)
    {
        var apps = JsonSerializer.Deserialize<List<LauncherApp>>(appsJson, JsonOptions)
                   ?? throw new ArgumentException("无效的应用列表 JSON");
        _configService.SaveApps(apps);
    }

    /// <summary>按名称启动一个应用；目标不存在时抛出异常（前端 promise 将 reject）。</summary>
    public void LaunchApp(string name)
    {
        var app = _configService.LoadApps()
            .FirstOrDefault(a => a.Name == name)
            ?? throw new ArgumentException($"应用不存在: {name}");

        _launcherService.Launch(app);
    }

    /// <summary>打开配置文件所在目录（资源管理器）。</summary>
    public void OpenConfigFolder()
    {
        var path = _configService.ConfigFilePathForDisplay();
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\"");
    }

    /// <summary>最小化窗口。</summary>
    public void MinimizeWindow() => _window.MinimizeWindow();

    /// <summary>切换最大化/还原，返回切换后的状态（"maximized" 或 "normal"）。</summary>
    public string ToggleMaximizeWindow() => _window.ToggleMaximizeWindow();

    /// <summary>关闭窗口。</summary>
    public void CloseWindow() => _window.CloseWindow();
}
