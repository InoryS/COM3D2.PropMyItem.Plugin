using System.IO;
using BepInEx;

namespace COM3D2.PropMyItem.Plugin
{
    /// <summary>
    ///     配置文件定位：统一放在 BepInEx 的配置目录下。
    ///     旧版本写在 Sybaris\UnityInjector\Config 里，但 COM3D2.5 只有 BepInEx、没有 Sybaris，
    ///     该目录不存在时写文件会抛 DirectoryNotFoundException（被 catch 吞掉），
    ///     结果是设置、收藏、颜色锁定全都无法持久化。
    /// </summary>
    internal static class ConfigPath
    {
        public static string Resolve(string fileName)
        {
            var dir = Paths.ConfigPath;
            if (string.IsNullOrEmpty(dir)) dir = Directory.GetCurrentDirectory();

            try
            {
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            }
            catch
            {
                // 建不出来就交给调用方的 try/catch 处理
            }

            return Path.Combine(dir, fileName);
        }
    }
}
