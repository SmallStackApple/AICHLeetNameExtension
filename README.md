# LeetName

AliceInCradleHack 扩展：为联机多人显示提供**动态 leet（黑客风）拆字名称**动画。

## 功能

- 将指定名称逐帧拆分显示，支持 leet 替换（例如 `ASM` → `4SM`、`A5M` 等）
- 可配置每帧间隔（`FrameMs`）、完整名称停留时间（`HoldMs`）以及是否启用 leet 形态（`UseLeet`）
- 所有配置均可通过游戏内控制台命令实时调整并持久化

## 安装

1. 编译得到 `LeetName.dll`（见下方构建）
2. 将 `LeetName.dll` 放入 `<AliceInCradleHack目录>\Extensions\LeetName`
3. 启动游戏，扩展会自动加载

## 构建

项目为 .NET Framework 4.8.1（x64），使用 Visual Studio 打开 `LeetName.csproj` 直接生成即可。输出为单个 `LeetName.dll`。

## 使用

在游戏控制台（由 AliceInCradleHack 提供）中可通过 `leetname` 命令控制：

```
leetname                查看当前设置
leetname ASM            设置基础名称（例如 ASM / asm）
leetname interval 500   每帧间隔（毫秒）
leetname hold 2000      完整名称停留时间（0 表示不停留）
leetname leet on        开启/关闭中间字符的 leet 形态
```

## 项目结构

| 文件 | 说明 |
| --- | --- |
| `LeetNameExtension.cs` | 扩展入口点，注册模块与命令 |
| `LeetNameModule.cs` | 核心动画模块 |
| `LeetNameCommand.cs` | 控制台命令 `leetname` |

## 依赖

- **AliceInCradleHack**：扩展宿主框架，提供 `Extension` / `Module` / `Command` 等扩展 API
