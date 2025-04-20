using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace COM3D2.PropMyItem.Plugin
{
    // Token: 0x02000004 RID: 4
    public class Config
    {
        // Token: 0x04000006 RID: 6
        private static Config _config;

        // Token: 0x04000005 RID: 5

        // Token: 0x04000004 RID: 4

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000009 RID: 9 RVA: 0x0000249B File Offset: 0x0000069B
        // (set) Token: 0x0600000A RID: 10 RVA: 0x000024A3 File Offset: 0x000006A3
        public List<string> TargetBGList { get; set; } = new List<string>();

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600000B RID: 11 RVA: 0x000024AC File Offset: 0x000006AC
        // (set) Token: 0x0600000C RID: 12 RVA: 0x000024B4 File Offset: 0x000006B4
        public List<SMenuInfo> MenuItems { get; set; } = new List<SMenuInfo>();

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600000D RID: 13 RVA: 0x000024C0 File Offset: 0x000006C0
        public static Config Instance
        {
            get
            {
                if (_config == null)
                {
                    var text = Directory.GetCurrentDirectory() + "\\Sybaris\\UnityInjector\\Config\\PropMyItem.xml";
                    _config = new Config();
                    _config.SetDefault();
                    if (File.Exists(text))
                        _config.Load(text);
                    else
                        _config.Save();
                }

                return _config;
            }
        }

        // Token: 0x0600000E RID: 14 RVA: 0x0000251C File Offset: 0x0000071C
        public void Load(string filePath)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(Config));
                using (var streamReader = new StreamReader(filePath, new UTF8Encoding(false)))
                {
                    var config = (Config)xmlSerializer.Deserialize(streamReader);
                    TargetBGList.Clear();
                    TargetBGList.AddRange(config.TargetBGList.ToArray());
                    MenuItems = config.MenuItems;
                }
            }
            catch (Exception)
            {
                SetDefault();
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x000025B4 File Offset: 0x000007B4
        public void SetDefault()
        {
            try
            {
                string[] collection =
                {
                    "MyRoom",
                    "MyRoom_Night",
                    "Shukuhakubeya_BedRoom",
                    "Shukuhakubeya_BedRoom_Night",
                    "Soap",
                    "SMClub",
                    "HeroineRoom_A1",
                    "HeroineRoom_A1_night",
                    "HeroineRoom_B1",
                    "HeroineRoom_B1_night",
                    "HeroineRoom_C1",
                    "HeroineRoom_C1_night",
                    "HeroineRoom_A",
                    "HeroineRoom_A_night",
                    "HeroineRoom_B",
                    "HeroineRoom_B_night",
                    "HeroineRoom_C",
                    "HeroineRoom_C_night"
                };
                TargetBGList.AddRange(collection);
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002684 File Offset: 0x00000884
        public void Save()
        {
            var filePath = Directory.GetCurrentDirectory() + "\\Sybaris\\UnityInjector\\Config\\PropMyItem.xml";
            Save(filePath);
        }

        // Token: 0x06000011 RID: 17 RVA: 0x000026A8 File Offset: 0x000008A8
        public void Save(string filePath)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(Config));
                var streamWriter = new StreamWriter(filePath, false, new UTF8Encoding(false));
                xmlSerializer.Serialize(streamWriter, this);
                streamWriter.Close();
            }
            catch (Exception value)
            {
                PropMyItem.Log.LogMessage(value);
            }
        }
    }
}