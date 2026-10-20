namespace COM3D2.PropMyItem.Plugin;

public class SMenuInfo
{
    public MPN ColorSetMPN = Mpn.head;
    public string ColorSetMenuName = string.Empty;

    public string FileName = string.Empty;

    public string IconName = string.Empty;

    public string ItemName = string.Empty;

    public MPN MPN = Mpn.head;

    public float Priority;

    public SMenuInfo()
    {
    }

    public SMenuInfo(MenuInfo menuInfo)
    {
        ItemName = menuInfo.ItemName;
        FileName = menuInfo.FileName;
        IconName = menuInfo.IconName;
        Priority = menuInfo.Priority;
        ColorSetMPN = menuInfo.ColorSetMPN;
        ColorSetMenuName = menuInfo.ColorSetMenuName;
        MPN = menuInfo.MPN;
    }
}