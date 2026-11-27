using System.Diagnostics;
using MeetStar_Launcher.Backend.Models;

namespace MeetStar_Launcher.Backend.Services;

/// <summary>
/// 负责以独立进程启动应用条目。
/// </summary>
public class AppLauncherService
{
    public void Launch(LauncherApp app)
    {
        if (!File.Exists(app.ExecutablePath))
        {
            throw new FileNotFoundException(
                $"找不到可执行文件: {app.ExecutablePath}", app.ExecutablePath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = app.ExecutablePath,
            Arguments = app.Arguments ?? string.Empty,
            WorkingDirectory = app.WorkingDirectory
                ?? Path.GetDirectoryName(app.ExecutablePath) ?? string.Empty,
            UseShellExecute = true,
        };

        Process.Start(startInfo);
    }
}
