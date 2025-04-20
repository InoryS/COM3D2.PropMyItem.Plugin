using System;
using System.IO;
using System.Text;

namespace COM3D2.PropMyItem.Plugin
{
    // Token: 0x02000008 RID: 8
    public class MenuModParser
    {
        // Token: 0x06000027 RID: 39 RVA: 0x00002C6C File Offset: 0x00000E6C
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

        // Token: 0x06000028 RID: 40 RVA: 0x00002CC4 File Offset: 0x00000EC4
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
                var mpn = MPN.null_mpn;
                if (!EnumUtil.TryParse(value, true, out mpn)) return null;

                menuInfo.MPN = mpn;
                binaryReader.ReadString();
                if (!EnumUtil.TryParse(binaryReader.ReadString(), true, out mpn)) return null;

                if (mpn != MPN.null_mpn) binaryReader.ReadString();

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

        // Token: 0x06000029 RID: 41 RVA: 0x00002F28 File Offset: 0x00001128
        public static MenuInfo ParseMenu(string filePath)
        {
            var menuInfo = new MenuInfo();
            MenuInfo result;
            using (var memoryStream =
                   new MemoryStream(LoadMenuInternal(Path.GetFileName(filePath)), false))
            {
                using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
                {
                    if (binaryReader.ReadString() != "CM3D2_MENU")
                    {
                        result = null;
                    }
                    else
                    {
                        binaryReader.ReadInt32();
                        binaryReader.ReadString();
                        menuInfo.ItemName = binaryReader.ReadString();
                        binaryReader.ReadString();
                        binaryReader.ReadString().Replace("《改行》", "\n");
                        binaryReader.ReadInt32();
                        for (;;)
                        {
                            int num = binaryReader.ReadByte();
                            if (num == 0) goto IL_15F;

                            var a = binaryReader.ReadString();
                            var array = new string[num - 1];
                            for (var i = 0; i < num - 1; i++) array[i] = binaryReader.ReadString();

                            if (!(a == "category"))
                            {
                                if (!(a == "priority"))
                                {
                                    if (!(a == "icon") && !(a == "icons"))
                                    {
                                        if (a == "color_set")
                                        {
                                            if (!EnumUtil.TryParse(array[0], true, out menuInfo.ColorSetMPN))
                                                goto IL_167;

                                            menuInfo.ColorSetMenuName = array[1];
                                        }
                                    }
                                    else
                                    {
                                        menuInfo.IconName = array[0];
                                    }
                                }
                                else
                                {
                                    menuInfo.Priority = float.Parse(array[0]);
                                }
                            }
                            else if (!EnumUtil.TryParse(array[0], true, out menuInfo.MPN))
                            {
                                break;
                            }
                        }

                        return null;
                        IL_15F:
                        return menuInfo;
                        IL_167:
                        result = null;
                    }
                }
            }

            return result;
        }
    }
}