using System.Runtime.InteropServices;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MeetStar_Launcher.Backend.Bridge;

namespace MeetStar_Launcher.Backend.Forms;

/// <summary>
/// 主窗体：无边框，标题栏由前端绘制（app-region: drag），
/// 通过 WM_NCCALCSIZE 剥离原生非客户区，同时保留 WS_THICKFRAME 获得系统贴靠与四边缩放。
/// Debug 模式导航到 Vite 开发服务器（热更新），Release 通过虚拟域名加载本地打包产物。
/// </summary>
public class MainForm : Form, IWindowController
{
    private const string DevServerUrl = "http://localhost:5173/";
    private const string VirtualHost = "app.local";
    private const int WM_NCCALCSIZE = 0x0083;

    private readonly WebView2 _webView = new();

    public MainForm()
    {
        Text = "MeetStar Launcher";
        Size = new Size(1000, 650);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        // 保留 Sizable（WS_THICKFRAME）：缩放边框与 Aero 贴靠由 DWM 提供，
        // 视觉上的标题栏/边框在 WM_NCCALCSIZE 中剥离，由前端绘制
        FormBorderStyle = FormBorderStyle.Sizable;

        _webView.Dock = DockStyle.Fill;
        Controls.Add(_webView);

        Load += async (_, _) => await InitializeWebViewAsync();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // 向 DWM 扩展 1px 客户区：恢复四边拖拽缩放的命中区域（视觉不可见）
        var margins = new MARGINS(1, 1, 1, 1);
        DwmExtendFrameIntoClientArea(Handle, ref margins);
    }

    protected override void WndProc(ref Message m)
    {
        // wParam 非 0 表示需要重新计算非客户区：返回 0 即剥离全部原生边框/标题栏
        if (m.Msg == WM_NCCALCSIZE && m.WParam != IntPtr.Zero)
        {
            return;
        }
        base.WndProc(ref m);
    }

    private async Task InitializeWebViewAsync()
    {
        // 显式指定用户数据目录，避免打包安装到 Program Files 后默认目录不可写
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MeetStar-Launcher", "WebView2");
        Directory.CreateDirectory(userDataFolder);

        var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

        try
        {
            await _webView.EnsureCoreWebView2Async(environment);
        }
        catch (WebView2RuntimeNotFoundException)
        {
            MessageBox.Show(
                "未检测到 WebView2 Runtime。\n\n请安装 Microsoft Edge WebView2 Runtime 后重试：\nhttps://developer.microsoft.com/microsoft-edge/webview2/",
                "MeetStar Launcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        var core = _webView.CoreWebView2;

#if DEBUG
        core.Settings.AreDevToolsEnabled = true;
#else
        core.Settings.AreDevToolsEnabled = false;
#endif

        // 开启非客户区支持：前端 CSS app-region: drag 区域成为可拖拽标题栏，
        // 双击最大化、系统菜单等行为由 WebView2 自动映射
        core.Settings.IsNonClientRegionSupportEnabled = true;

        // 暴露 C# 桥接对象：window.chrome.webview.hostObjects.launcher
        core.AddHostObjectToScript("launcher", new LauncherBridge(this));

#if DEBUG
        core.Navigate(DevServerUrl);
#else
        var wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        core.SetVirtualHostNameToFolderMapping(
            VirtualHost, wwwroot, CoreWebView2HostResourceAccessKind.AllowCorsAndCrossSiteAccess);
        core.Navigate($"https://{VirtualHost}/index.html");
#endif
    }

    public void MinimizeWindow() => WindowState = FormWindowState.Minimized;

    public string ToggleMaximizeWindow()
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
        return WindowState == FormWindowState.Maximized ? "maximized" : "normal";
    }

    public void CloseWindow() => Close();

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public MARGINS(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);
}
