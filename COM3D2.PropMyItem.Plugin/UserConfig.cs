using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace COM3D2.PropMyItem.Plugin;

public class UserConfig
{
    private static UserConfig _config;

    /*
    private const string DefaultGUIKey = "i";

    private string _guiVisible = "i";

    private bool _isControlKey;

    private bool _isAltKey;

    private bool _isShift;
    */

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
    public bool IsAutoShoesHide { get; set; }

    public bool IsOutputInfoLog { get; set; }

    public List<string> FavList { get; set; } = new();

    public List<string> ColorLockList { get; set; } = new();

    public List<string> FilterTextList { get; set; } = new();

    public static UserConfig Instance
    {
        get
        {
            if (_config == null)
            {
                var text = ConfigPath.Resolve("PropMyItemUser.xml");
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

    public void Save()
    {
        var filePath = ConfigPath.Resolve("PropMyItemUser.xml");
        Save(filePath);
    }

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