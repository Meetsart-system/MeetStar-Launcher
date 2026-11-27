using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MeetStar_Launcher.Backend.Bridge;

namespace MeetStar_Launcher.Backend.Forms;

/// <summary>
/// 主窗体：承载 WebView2，加载 Vue 前端并注入 C# 桥接对象。
/// Debug 模式导航到 Vite 开发服务器（热更新），Release 通过虚拟域名加载本地打包产物。
/// </summary>
public class MainForm : Form
{
    private const string DevServerUrl = "http://localhost:5173/";
    private const string VirtualHost = "app.local";

    private readonly WebView2 _webView = new();

    public MainForm()
    {
        Text = "MeetStar Launcher";
        Size = new Size(1000, 650);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        _webView.Dock = DockStyle.Fill;
        Controls.Add(_webView);

        Load += async (_, _) => await InitializeWebViewAsync();
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

        // 暴露 C# 桥接对象：window.chrome.webview.hostObjects.launcher
        core.AddHostObjectToScript("launcher", new LauncherBridge());

#if DEBUG
        core.Navigate(DevServerUrl);
#else
        var wwwroot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        core.SetVirtualHostNameToFolderMapping(
            VirtualHost, wwwroot, CoreWebView2HostResourceAccessKind.AllowCorsAndCrossSiteAccess);
        core.Navigate($"https://{VirtualHost}/index.html");
#endif
    }
}
