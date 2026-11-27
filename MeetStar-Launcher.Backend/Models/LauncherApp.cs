namespace MeetStar_Launcher.Backend.Models;

/// <summary>
/// 启动器中的一个可启动应用条目。
/// JSON 序列化为 camelCase，与 frontend/src/types.ts 中的 LauncherApp 契约对应，
/// 契约定义见仓库根 contracts/README.md。
/// </summary>
public class LauncherApp
{
    /// <summary>显示名称（唯一键）。</summary>
    public required string Name { get; set; }

    /// <summary>可执行文件路径。</summary>
    public required string ExecutablePath { get; set; }

    /// <summary>启动参数（可选）。</summary>
    public string? Arguments { get; set; }

    /// <summary>工作目录（可选，默认取可执行文件所在目录）。</summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>图标路径（可选）。</summary>
    public string? IconPath { get; set; }
}
