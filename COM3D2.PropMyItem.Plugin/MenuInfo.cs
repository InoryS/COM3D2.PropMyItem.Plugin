using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.PropMyItem.Plugin;

public class MenuInfo
{
    public string CategoryName = string.Empty;

    public int ColorNumber;

    public MPN ColorSetMPN = Mpn.head;

    public List<MenuInfo> ColorSetMenuList = new();

    public string ColorSetMenuName = string.Empty;

    public string FileName = string.Empty;

    public string FilePath = string.Empty;

    public string IconName = string.Empty;

    public bool IsColorLock;

    public bool IsError;

    public bool IsFavorite;

    public bool IsHave = true;

    public bool IsMod;

    public bool IsOfficialMOD;

    public bool IsShopTarget;

    public string ItemName = string.Empty;

    public MPN MPN = Mpn.head;

    public float Priority;

    public List<MenuInfo> VariationMenuList = new();

    //menuInfo.Icon = new Texture2D(1, 1, TextureFormat.RGBA32, false);// 여기서 문제됨
    //menuInfo.Icon.LoadImage(data);

    public byte[] data;

    private Texture2D icon;

    public MenuInfo()
    {
    }

    public MenuInfo(SMenuInfo menuInfo)
    {
        ItemName = menuInfo.ItemName;
        FileName = menuInfo.FileName;
        IconName = menuInfo.IconName;
        Priority = menuInfo.Priority;
        ColorSetMPN = menuInfo.ColorSetMPN;
        ColorSetMenuName = menuInfo.ColorSetMenuName;
        MPN = menuInfo.MPN;
    }

    public Texture2D Icon
    {
        get
        {
            if (data != null)
            {
                icon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                icon.LoadImage(data);
                data = null;
            }

            return icon;
        }
        set => icon = value;
    }
}