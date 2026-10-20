#!/usr/bin/env python3
# -*- coding: utf-8 -*-
# Function: 组装单个目标（COM3D2 / COM3D25）的发布 zip / Assemble the release zip for one target
"""组装发布 zip。

解压到游戏根目录即可，包内布局与插件运行时读取的路径一致：

    BepInEx/plugins/COM3D2.PropMyItem.Plugin.dll     （COM3D2 版）
    BepInEx/plugins/System.Threading.dll             （仅 COM3D2 / net35，见下）
    BepInEx/plugins/COM3D25.PropMyItem.Plugin.dll    （COM3D2.5 版）
    README.md

两个版本的 dll 名字不同（COM3D25 前缀），zip 名直接取 dll 名，所以一眼能看出该下哪个。

COM3D2（Unity 5.6.4 / net35）没有 System.Threading.Tasks，插件用 Task 做后台菜单扫描，
而游戏 Managed 目录和 BepInEx 都不带 System.Threading.dll，所以必须随包分发；
COM3D2.5（net48）的 Task 在 mscorlib 里，不需要。

The zip mirrors the layout the plugin expects after extracting into the game root.
The two builds have different dll names (COM3D25 prefix) and the zip name follows the dll name.
COM3D2 (net35) has no System.Threading.Tasks, and neither the game nor BepInEx ships
System.Threading.dll, so it has to travel with the plugin. COM3D2.5 (net48) does not need it.
"""

import argparse
import os
import sys
import zipfile

# 目标定义 / Target definitions
TARGETS = {
    "COM3D2": {
        "build_dir": os.path.join("COM3D2", "net35"),
        "plugin_dll": "COM3D2.PropMyItem.Plugin.dll",
    },
    "COM3D25": {
        "build_dir": os.path.join("COM3D25", "net48"),
        "plugin_dll": "COM3D25.PropMyItem.Plugin.dll",
    },
}

PLUGIN_DIR_IN_ZIP = "BepInEx/plugins"

# 固定时间戳，保证同样的输入产出同样的 zip / Fixed timestamp so identical inputs give identical zips
FIXED_DATE_TIME = (2020, 1, 1, 0, 0, 0)


def add_dir(zf, name):
    info = zipfile.ZipInfo(name.rstrip("/") + "/", date_time=FIXED_DATE_TIME)
    info.external_attr = (0o40755 << 16) | 0x10
    info.compress_type = zipfile.ZIP_STORED
    zf.writestr(info, b"")


def add_file(zf, src, arcname):
    info = zipfile.ZipInfo(arcname, date_time=FIXED_DATE_TIME)
    info.external_attr = 0o100644 << 16
    info.compress_type = zipfile.ZIP_DEFLATED
    with open(src, "rb") as f:
        zf.writestr(info, f.read(), compresslevel=9)


def main():
    parser = argparse.ArgumentParser(description="Assemble a release zip")
    parser.add_argument("--target", required=True, choices=sorted(TARGETS))
    parser.add_argument("--version", required=True, help="例如 2.5.0.0 / e.g. 2.5.0.0")
    parser.add_argument("--build-root", required=True,
                        help="dotnet build 的输出根目录（bin/Release）/ dotnet build output root")
    parser.add_argument("--repo-root", default=".", help="仓库根目录 / repository root")
    parser.add_argument("--output-dir", default="dist", help="zip 输出目录 / output directory")
    args = parser.parse_args()

    target = TARGETS[args.target]
    plugin_dll = target["plugin_dll"]
    repo_root = os.path.abspath(args.repo_root)
    build_dir = os.path.join(os.path.abspath(args.build_root), target["build_dir"])
    readme = os.path.join(repo_root, "README.md")

    if not os.path.isdir(build_dir):
        sys.exit(f"::error::build output not found: {build_dir}")
    if not os.path.isfile(readme):
        sys.exit(f"::error::missing file: {readme}")

    # pdb 不进包；其余 dll 都是运行期需要的东西（插件本体 + 可能的依赖）
    # No pdb in the archive; every other dll is needed at runtime
    dlls = sorted(n for n in os.listdir(build_dir) if n.lower().endswith(".dll"))
    if plugin_dll not in dlls:
        sys.exit(f"::error::{plugin_dll} not found in {build_dir}, got {dlls}")

    # zip 名直接取 dll 名：COM3D2.PropMyItem.Plugin-<ver>.zip / COM3D25.PropMyItem.Plugin-<ver>.zip
    zip_prefix = os.path.splitext(plugin_dll)[0]
    os.makedirs(os.path.join(repo_root, args.output_dir), exist_ok=True)
    zip_path = os.path.join(repo_root, args.output_dir, f"{zip_prefix}-{args.version}.zip")

    with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED, compresslevel=9) as zf:
        add_dir(zf, "BepInEx")
        add_dir(zf, PLUGIN_DIR_IN_ZIP)

        for name in dlls:
            add_file(zf, os.path.join(build_dir, name),
                     f"{PLUGIN_DIR_IN_ZIP}/{name}")

        add_file(zf, readme, "README.md")

    # 自检：不允许 pdb，且必须带上插件本体 / Sanity check: no pdb, plugin dll present
    with zipfile.ZipFile(zip_path) as zf:
        names = zf.namelist()
    bad = [n for n in names if n.lower().endswith(".pdb")]
    if bad:
        sys.exit(f"::error::unexpected entries in {zip_path}: {bad}")
    if f"{PLUGIN_DIR_IN_ZIP}/{plugin_dll}" not in names:
        sys.exit(f"::error::{plugin_dll} missing from {zip_path}")

    size = os.path.getsize(zip_path)
    print(f"built {zip_path} ({len(names)} entries, {size} bytes)")
    print("  dlls: " + ", ".join(dlls))


if __name__ == "__main__":
    main()
