namespace MeetStar_Launcher.Backend.Bridge;

/// <summary>
/// 窗口控制能力，由主窗体实现，桥接层调用。
/// </summary>
public interface IWindowController
{
    void MinimizeWindow();

    /// <summary>切换最大化/还原，返回切换后的状态（"maximized" 或 "normal"）。</summary>
    string ToggleMaximizeWindow();

    void CloseWindow();
}
