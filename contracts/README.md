# Launcher Bridge Contract (v1)

C# 宿主与 Vue 前端之间的通信契约。**修改任何签名必须先更新本文件，再同步两侧实现。**

- C# 侧实现：`MeetStar-Launcher.Backend/Bridge/LauncherBridge.cs`
- TS 侧封装：`frontend/src/api/bridge.ts`

## 通道

WebView2 `AddHostObjectToScript("launcher", ...)`，
前端通过 `window.chrome.webview.hostObjects.launcher` 访问。

## 数据模型 `LauncherApp`

JSON 一律 **camelCase**（C# 侧 `JsonNamingPolicy.CamelCase` 保证）。

| 字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `name` | string | ✅ | 显示名称，作为唯一键 |
| `executablePath` | string | ✅ | 可执行文件绝对路径 |
| `arguments` | string? | — | 启动参数 |
| `workingDirectory` | string? | — | 默认取可执行文件所在目录 |
| `iconPath` | string? | — | 图标路径 |

## Bridge 方法（v1）

| 方法 | 参数 | 返回 | 异常（前端 promise reject） |
|---|---|---|---|
| `LoadAppsJson()` | — | `string`：`LauncherApp[]` 的 JSON | 配置文件损坏时抛异常 |
| `SaveAppsJson(json)` | `LauncherApp[]` 的 JSON 字符串 | void | JSON 无效抛 `ArgumentException` |
| `LaunchApp(name)` | 应用名 | void | 应用不存在 / 可执行文件缺失抛异常 |
| `OpenConfigFolder()` | — | void | — |

## 配置存储

- 路径：`%LOCALAPPDATA%\MeetStar-Launcher\config.json`
- 格式：`LauncherApp[]` 的 JSON 数组，首次运行自动创建空数组

## 宿主环境约定

| 模式 | 页面来源 | DevTools |
|---|---|---|
| Debug | `http://localhost:5173/`（Vite 开发服务器，热更新） | 开 |
| Release | `https://app.local/`（虚拟域名映射宿主输出目录 `wwwroot/`，由 `frontend/dist` 构建复制） | 关 |

## 变更流程

1. 修改本文件中的契约表
2. 同步 `LauncherBridge.cs` 与 `bridge.ts`
3. 两仓各自提交；涉及协议版本变化时在标题标注 `(vN)`
