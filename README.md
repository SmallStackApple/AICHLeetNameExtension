# AnimatedPlayerName

AliceInCradleHack 扩展：为联机多人显示提供**动态 leet（黑客风）拆字名称**动画。

## 功能

- 将指定名称逐帧拆分显示，支持 leet 替换（例如 `ASM` → `4SM`、`A5M` 等）
- 可配置每帧间隔（`FrameMs`）、完整名称停留时间（`HoldMs`）以及是否启用 leet 形态（`UseLeet`）
- 模块配置由 AliceInCradleHack 持久化

## 安装

1. 编译得到 `AnimatedPlayerName.dll`（见下方构建）
2. 将 `AnimatedPlayerName.dll` 放入 `<AliceInCradleHack目录>\Extensions\AnimatedPlayerName`
3. 启动游戏，扩展会自动加载

## 构建

项目为 .NET Framework 4.8.1（x64），使用 Visual Studio 打开 `AnimatedPlayerName.csproj` 直接生成即可。输出为单个 `AnimatedPlayerName.dll`。

## 项目结构

| 文件 | 说明 |
| --- | --- |
| `AnimatedPlayerNameExtension.cs` | 扩展入口点，注册模块 |
| `AnimatedPlayerNameModule.cs` | 核心动画模块 |

## 依赖

- **AliceInCradleHack**：扩展宿主框架，提供 `Extension` / `Module` 等扩展 API
