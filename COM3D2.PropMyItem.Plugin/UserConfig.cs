using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace COM3D2.PropMyItem.Plugin
{
    // Token: 0x0200000D RID: 13
    public class UserConfig
    {
        // Token: 0x0400006B RID: 107
        private static UserConfig _config;

        // Token: 0x04000069 RID: 105

        // Token: 0x04000068 RID: 104

        // Token: 0x0400006A RID: 106

        /*
        // Token: 0x04000061 RID: 97
        private const string DefaultGUIKey = "i";

        // Token: 0x04000062 RID: 98
        private string _guiVisible = "i";

        // Token: 0x04000063 RID: 99
        private bool _isControlKey;

        // Token: 0x04000064 RID: 100
        private bool _isAltKey;

        // Token: 0x04000065 RID: 101
        private bool _isShift;
        */
        // Token: 0x04000066 RID: 102

        // Token: 0x04000067 RID: 103

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x06000053 RID: 83 RVA: 0x00008307 File Offset: 0x00006507
        // (set) Token: 0x06000054 RID: 84 RVA: 0x0000830F File Offset: 0x0000650F
        /*
        public string GuiVisibleKey
        {
            get
            {
                return this._guiVisible;
            }
            set
            {
                this._guiVisible = value;
            }
        }

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x06000055 RID: 85 RVA: 0x00008318 File Offset: 0x00006518
        // (set) Token: 0x06000056 RID: 86 RVA: 0x00008320 File Offset: 0x00006520
        public bool IsControlKey
        {
            get
            {
                return this._isControlKey;
            }
            set
            {
                this._isControlKey = value;
            }
        }

        // Token: 0x17000012 RID: 18
        // (get) Token: 0x06000057 RID: 87 RVA: 0x00008329 File Offset: 0x00006529
        // (set) Token: 0x06000058 RID: 88 RVA: 0x00008331 File Offset: 0x00006531
        public bool IsAltKey
        {
            get
            {
                return this._isAltKey;
            }
            set
            {
                this._isAltKey = value;
            }
        }

        // Token: 0x17000013 RID: 19
        // (get) Token: 0x06000059 RID: 89 RVA: 0x0000833A File Offset: 0x0000653A
        // (set) Token: 0x0600005A RID: 90 RVA: 0x00008342 File Offset: 0x00006542
        public bool IsShiftKey
        {
            get
            {
                return this._isShift;
            }
            set
            {
                this._isShift = value;
            }
        }
        */
        // Token: 0x17000014 RID: 20
        // (get) Token: 0x0600005B RID: 91 RVA: 0x0000834B File Offset: 0x0000654B
        // (set) Token: 0x0600005C RID: 92 RVA: 0x00008353 File Offset: 0x00006553
        public bool IsAutoShoesHide { get; set; }

        // Token: 0x17000015 RID: 21
        // (get) Token: 0x0600005D RID: 93 RVA: 0x0000835C File Offset: 0x0000655C
        // (set) Token: 0x0600005E RID: 94 RVA: 0x00008364 File Offset: 0x00006564
        public bool IsOutputInfoLog { get; set; }

        // Token: 0x17000016 RID: 22
        // (get) Token: 0x0600005F RID: 95 RVA: 0x0000836D File Offset: 0x0000656D
        // (set) Token: 0x06000060 RID: 96 RVA: 0x00008375 File Offset: 0x00006575
        public List<string> FavList { get; set; } = new List<string>();

        // Token: 0x17000017 RID: 23
        // (get) Token: 0x06000061 RID: 97 RVA: 0x0000837E File Offset: 0x0000657E
        // (set) Token: 0x06000062 RID: 98 RVA: 0x00008386 File Offset: 0x00006586
        public List<string> ColorLockList { get; set; } = new List<string>();

        // Token: 0x17000018 RID: 24
        // (get) Token: 0x06000063 RID: 99 RVA: 0x0000838F File Offset: 0x0000658F
        // (set) Token: 0x06000064 RID: 100 RVA: 0x00008397 File Offset: 0x00006597
        public List<string> FilterTextList { get; set; } = new List<string>();

        // Token: 0x17000019 RID: 25
        // (get) Token: 0x06000065 RID: 101 RVA: 0x000083A0 File Offset: 0x000065A0
        public static UserConfig Instance
        {
            get
            {
                if (_config == null)
                {
                    var text = Directory.GetCurrentDirectory() +
                               "\\Sybaris\\UnityInjector\\Config\\PropMyItemUser.xml";
                    _config = new UserConfig();
                    if (File.Exists(text))
                        _config.Load(text);
                    else
                        //UserConfig._config.IsShiftKey = true;
                        //UserConfig._config.GuiVisibleKey = "i";
                        _config.Save();
                }

                return _config;
            }
        }

        // Token: 0x06000066 RID: 102 RVA: 0x0000840C File Offset: 0x0000660C
        public void Load(string filePath)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(UserConfig));
                using (var streamReader = new StreamReader(filePath, new UTF8Encoding(false)))
                {
                    var userConfig = (UserConfig)xmlSerializer.Deserialize(streamReader);
                    //this.GuiVisibleKey = userConfig.GuiVisibleKey.ToLower();
                    IsAutoShoesHide = userConfig.IsAutoShoesHide;
                    IsOutputInfoLog = userConfig.IsOutputInfoLog;
                    FavList = userConfig.FavList;
                    ColorLockList = userConfig.ColorLockList;
                    FilterTextList = userConfig.FilterTextList;
                    //this.IsControlKey = userConfig.IsControlKey;
                    //this.IsAltKey = userConfig.IsAltKey;
                    //this.IsShiftKey = userConfig.IsShiftKey;
                }
            }
            catch (Exception)
            {
            }

            //if (string.IsNullOrEmpty(this.GuiVisibleKey))
            //{
            //    this.GuiVisibleKey = "i";
            //}
            if (FavList == null) FavList = new List<string>();

            if (FilterTextList == null) FilterTextList = new List<string>();

            if (ColorLockList == null) ColorLockList = new List<string>();
        }

        // Token: 0x06000067 RID: 103 RVA: 0x00008534 File Offset: 0x00006734
        public void Save()
        {
            var filePath = Directory.GetCurrentDirectory() + "\\Sybaris\\UnityInjector\\Config\\PropMyItemUser.xml";
            Save(filePath);
        }

        // Token: 0x06000068 RID: 104 RVA: 0x00008558 File Offset: 0x00006758
        public void Save(string filePath)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(typeof(UserConfig));
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