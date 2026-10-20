using System;
using System.IO;
using System.Text;

namespace COM3D2.PropMyItem.Plugin;

public class MenuModParser
{
    public static byte[] LoadMenuInternal(string filename)
    {
        byte[] result;
        try
        {
            using (var afileBase = GameUty.FileOpen(filename))
            {
                if (!afileBase.IsValid()) throw new Exception();

                result = afileBase.ReadAll();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return result;
    }

    public static MenuInfo parseMod(string filePath)
    {
        var menuInfo = new MenuInfo();
        menuInfo.Priority = 1000f;
        byte[] array = null;
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                if (fileStream == null) return null;

                array = new byte[fileStream.Length];
                fileStream.Read(array, 0, (int)fileStream.Length);
            }
        }
        catch (Exception)
        {
        }

        if (array == null) return null;

        using (var binaryReader = new BinaryReader(new MemoryStream(array), Encoding.UTF8))
        {
            if (binaryReader.ReadString() != "CM3D2_MOD") return null;

            binaryReader.ReadInt32();
            menuInfo.IconName = binaryReader.ReadString();
            binaryReader.ReadString();
            menuInfo.ItemName = binaryReader.ReadString();
            var value = binaryReader.ReadString();
            var mpn = Mpn.null_mpn;
            if (!EnumUtil.TryParse(value, true, out mpn)) return null;

            menuInfo.MPN = mpn;
            binaryReader.ReadString();
            if (!EnumUtil.TryParse(binaryReader.ReadString(), true, out mpn)) return null;

            if (mpn != Mpn.null_mpn) binaryReader.ReadString();

            var text = binaryReader.ReadString();
            if (text.Contains("色セット"))
            {
                var array2 = text.Replace("\r\n", "\n").Split('\n');
                for (var i = 0; i < array2.Length; i++)
                {
                    var array3 = array2[i].Split(new[]
                    {
                        '\t'
                    }, StringSplitOptions.RemoveEmptyEntries);
                    if (array3.Length > 2 && array3[0] == "色セット" &&
                        EnumUtil.TryParse(array3[1], true, out menuInfo.ColorSetMPN))
                    {
                        menuInfo.ColorSetMenuName = array3[2];
                        break;
                    }
                }
            }

            var num = binaryReader.ReadInt32();
            for (var j = 0; j < num; j++)
            {
                var strA = binaryReader.ReadString();
                var count = binaryReader.ReadInt32();
                var data = binaryReader.ReadBytes(count);
                if (string.Compare(strA, menuInfo.IconName, true) == 0) menuInfo.data = data;
                //menuInfo.Icon = new Texture2D(1, 1, TextureFormat.RGBA32, false);// 여기서 문제됨
                //menuInfo.Icon.LoadImage(data);
            }
        }

        return menuInfo;
    }

    public static MenuInfo ParseMenu(string filePath)
    {
        var menuInfo = new MenuInfo();

        using (var memoryStream =
               new MemoryStream(LoadMenuInternal(Path.GetFileName(filePath)), false))
        using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
        {
            if (binaryReader.ReadString() != "CM3D2_MENU") return null;

            // 头部：版本、资源路径、名称、分类、说明、长度
            binaryReader.ReadInt32();
            binaryReader.ReadString();
            menuInfo.ItemName = binaryReader.ReadString();
            binaryReader.ReadString();
            binaryReader.ReadString();
            binaryReader.ReadInt32();

            while (true)
            {
                var count = binaryReader.ReadByte();
                if (count == 0) return menuInfo; // 0 表示命令列表结束

                var command = binaryReader.ReadString();
                var args = new string[count - 1];
                for (var i = 0; i < args.Length; i++) args[i] = binaryReader.ReadString();

                if (args.Length == 0) continue;

                if (command == "category")
                {
                    if (!EnumUtil.TryParse(args[0], true, out menuInfo.MPN)) return null;
                }
                else if (command == "priority")
                {
                    menuInfo.Priority = float.Parse(args[0]);
                }
                else if (command == "icon" || command == "icons")
                {
                    menuInfo.IconName = args[0];
                }
                else if (command == "color_set")
                {
                    if (!EnumUtil.TryParse(args[0], true, out menuInfo.ColorSetMPN)) return null;

                    if (args.Length > 1) menuInfo.ColorSetMenuName = args[1];
                }
            }
        }
    }
}