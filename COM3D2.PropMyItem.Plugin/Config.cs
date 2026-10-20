using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace COM3D2.PropMyItem.Plugin;

// 名字不能叫 Config：PropMyItem 继承的 BaseUnityPlugin 有一个 ConfigFile Config 属性，
// 在 PropMyItem 内部写 Config.Instance 会绑定到那个属性而不是本类型，编译不过。
// XmlRoot 保持根元素名不变，老的 PropMyItem.xml 仍然能读。
[XmlRoot("Config")]
public class PluginConfig
{
    private static PluginConfig _config;

    public List<string> TargetBGList { get; set; } = new();

    public List<SMenuInfo> MenuItems { get; set; } = new();

    public static PluginConfig Instance
    {
        get
        {
            if (_config == null)
            {
                var text = ConfigPath.Resolve("PropMyItem.xml");
                _config = new PluginConfig();
                _config.SetDefault();
                if (File.Exists(text))
                    _config.Load(text);
                else
                    _config.Save();
            }

            return _config;
        }
    }

    public void Load(string filePath)
    {
        try
        {
            var xmlSerializer = new XmlSerializer(typeof(PluginConfig));
            using (var streamReader = new StreamReader(filePath, new UTF8Encoding(false)))
            {
                var config = (PluginConfig)xmlSerializer.Deserialize(streamReader);
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

    public void Save()
    {
        var filePath = ConfigPath.Resolve("PropMyItem.xml");
        Save(filePath);
    }

    public void Save(string filePath)
    {
        try
        {
            var xmlSerializer = new XmlSerializer(typeof(PluginConfig));
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