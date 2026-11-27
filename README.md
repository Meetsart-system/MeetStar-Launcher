# MeetStar-Launcher

MeetStar 桌面启动器 —— WinForms + WebView2 混合架构：C# 壳提供原生能力（启动进程、读写配置），UI 由 Vite + Vue 3 + TypeScript 构建。

## 技术栈

| 层 | 技术 |
|---|---|
| 宿主壳 | C# / .NET 10.0-windows / WinForms |
| WebView | Microsoft.Web.WebView2 |
| 前端 | Vue 3 + TypeScript + Vite + miuix-vue（独立仓库，以 git 子模块接入 `frontend/`） |
| 通信 | `AddHostObjectToScript`（C# ↔ JS 桥接） |

## 仓库结构

```
MeetStar-Launcher/                  ← 主仓（Backend 直管）
├─ MeetStar-Launcher.slnx           解决方案（根目录）
├─ MeetStar-Launcher.Backend/       WinForms 宿主
│  ├─ Forms/                        窗体（MainForm 承载 WebView2）
│  ├─ Bridge/                       JS↔C# 桥接
│  ├─ Services/                     业务逻辑
│  ├─ Models/                       数据模型
│  └─ Resources/                    原生资源
├─ contracts/                       桥接协议契约（规划中）
├─ frontend/                        子模块：MeetStar-UI（待接入）
├─ scripts/                         跨仓构建/发布（规划中）
├─ Directory.Build.props            共享分析器/编译设置
└─ .editorconfig                    代码风格规范
```

## 开发

```bash
git clone --recursive https://github.com/MeetStar-System/MeetStar-Launcher.git
dotnet build MeetStar-Launcher.slnx

# 前端开发（包管理器：pnpm）
cd frontend && pnpm install && pnpm dev   # http://localhost:5173
```

### 接入前端子模块（创建 MeetStar-UI 仓库后）

```bash
git submodule add https://github.com/Meetsart-system/MeetStar-UI.git frontend
```

### 跨端改动流程

1. 前端改动 → 在 `frontend/` 内开发、提 PR、合入 MeetStar-UI
2. 回主仓：`git submodule update --remote frontend`
3. 主仓提交指针变更，触发整包构建

## 构建与发布

```bash
# 一键发布：前端构建 → dotnet publish → out/publish/MeetStar-Launcher.Backend.exe
powershell -File scripts/build.ps1
powershell -File scripts/build.ps1 -SelfContained   # 自包含（内置 .NET 运行时）

# 开发：一键起 Vite 开发服务器 + Debug 宿主
powershell -File scripts/dev.ps1
```

## 窗口形态

无边框窗口：原生标题栏由宿主剥离（`WM_NCCALCSIZE`），标题栏由前端绘制，
拖拽区域通过 WebView2 非客户区支持（CSS `app-region: drag`）实现，
窗口缩放与系统贴靠仍由 DWM 提供。窗口控制按钮经桥接调用宿主。

## 约定

- 提交信息遵循 Conventional Commits（`feat:` / `fix:` / `chore:` / `docs:` …）
- C# 代码风格由 `.editorconfig` 约束，构建时强制执行（`EnforceCodeStyleInBuild`）
- CI 会在每次构建前校验 frontend 子模块指针，防止主仓锁定过期前端

## License

MIT
