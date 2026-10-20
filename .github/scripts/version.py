#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# Function: 读取插件版本号 / Print the plugin version
"""打印插件版本号。

唯一来源是 PluginInfo.cs 的 PluginVersion 常量 —— 游戏里显示的插件版本、
程序集版本（csproj 通过正则从该常量推导）都取它。发版时用它和 tag 比对，
避免漏改版本号。
The single source is the PluginVersion constant in PluginInfo.cs. The assembly version
is derived from it by the csproj, and the game shows it. A release compares it against
the tag so a forgotten bump fails the build instead of shipping a wrong version.
"""

import os
import re
import sys

PLUGIN_INFO = os.path.join("COM3D2.PropMyItem.Plugin", "PluginInfo.cs")

PLUGIN_INFO_PATTERN = re.compile(
    r'public\s+const\s+string\s+PluginVersion\s*=\s*"([^"]+)"')


def main():
    if not os.path.isfile(PLUGIN_INFO):
        sys.exit(f"::error::file not found: {PLUGIN_INFO}")

    with open(PLUGIN_INFO, encoding="utf-8-sig") as f:
        match = PLUGIN_INFO_PATTERN.search(f.read())

    if not match:
        sys.exit(f"::error::PluginVersion constant not found in {PLUGIN_INFO}")

    print(match.group(1))


if __name__ == "__main__":
    main()
