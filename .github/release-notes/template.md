# Note

This release provides two builds of the same plugin. **Download the one matching your game:**

- `COM3D2.PropMyItem.Plugin-COM3D2-{{VERSION}}.zip` — for COM3D2 (2.x, Unity 5.6.4)
- `COM3D2.PropMyItem.Plugin-COM3D25-{{VERSION}}.zip` — for COM3D2.5 (3.x, Unity 2022.3)

Both builds produce the same `COM3D2.PropMyItem.Plugin.dll`, so install only one of them.

# Install

Extract the zip into your game root, so that the files end up at:

```
COM3D2/BepInEx/plugins/COM3D2.PropMyItem.Plugin.dll
COM3D2/BepInEx/plugins/System.Threading.dll      (COM3D2 build only, required)
```

The COM3D2 build needs `System.Threading.dll` because .NET 3.5 has no `Task`; it is included in the zip.
Do not delete it, and keep both files together.

Requirements: BepInEx 5.x. If you still have the old `Sybaris\UnityInjector` version installed, remove it first.

Settings are written to `BepInEx/config/PropMyItem.xml` and `BepInEx/config/PropMyItemUser.xml`.

Default hotkey: `I` toggles the GUI. There is also a button in the gear menu.

# Changes

{{CHANGELOG}}

<br>
<br>
<br>

# 说明

本次发布提供同一插件的两个版本，**请下载与你的游戏对应的那个**：

- `COM3D2.PropMyItem.Plugin-COM3D2-{{VERSION}}.zip` — 对应 COM3D2（2.x，Unity 5.6.4）
- `COM3D2.PropMyItem.Plugin-COM3D25-{{VERSION}}.zip` — 对应 COM3D2.5（3.x，Unity 2022.3）

两个版本产出的 dll 文件名相同，只需安装其中一个。

# 安装

把 zip 解压到游戏根目录，最终文件位置应为：

```
COM3D2/BepInEx/plugins/COM3D2.PropMyItem.Plugin.dll
COM3D2/BepInEx/plugins/System.Threading.dll      （仅 COM3D2 版需要，必需）
```

COM3D2 版需要 `System.Threading.dll`，因为 .NET 3.5 没有 `Task`；该文件已包含在压缩包内，
请勿删除，并保持两个文件在同一目录。

前置条件：BepInEx 5.x。如果之前装过 `Sybaris\UnityInjector` 版本，请先删掉。

配置文件写入 `BepInEx/config/PropMyItem.xml` 与 `BepInEx/config/PropMyItemUser.xml`。

默认快捷键：`I` 开关界面；齿轮菜单里也有一个按钮。

# 更改

{{CHANGELOG_ZH}}

**Full Changelog**: {{FULL_CHANGELOG_URL}}
