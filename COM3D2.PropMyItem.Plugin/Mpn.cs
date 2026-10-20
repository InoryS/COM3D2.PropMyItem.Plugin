using System;

namespace COM3D2.PropMyItem.Plugin;

/// <summary>
///     运行时解析出来的 MPN 常量。
///
///     插件编译时引用的是 GameLibs（3.46.1），和实际运行的游戏版本不一定一致：
///     3.46.1 里 MPN.wear = 173，3.49.4 里 MPN.wear = 214 —— 枚举中间插入新成员会让后面
///     所有成员的数值整体偏移，编译期常量拿去当字典键或做比较就会整体错位。
///     （实际表现：分类表里「帽子」指向了 eye_hi，「トップス」指向了 EyePosZ，界面全空。）
///
///     所以统一在启动时按名字解析一次，插件内部一律用这里的字段，不再直接写 MPN.xxx。
///     某个成员在当前游戏版本里不存在时退回 null_mpn。
/// </summary>
internal static class Mpn
{
    public static readonly MPN accanl = Resolve("accanl");
    public static readonly MPN accashi = Resolve("accashi");
    public static readonly MPN accha = Resolve("accha");
    public static readonly MPN acchana = Resolve("acchana");
    public static readonly MPN acchat = Resolve("acchat");
    public static readonly MPN acchead = Resolve("acchead");
    public static readonly MPN accheso = Resolve("accheso");
    public static readonly MPN acckami = Resolve("acckami");
    public static readonly MPN acckamisub = Resolve("acckamisub");
    public static readonly MPN acckubi = Resolve("acckubi");
    public static readonly MPN acckubiwa = Resolve("acckubiwa");
    public static readonly MPN accmimi = Resolve("accmimi");
    public static readonly MPN accnail = Resolve("accnail");
    public static readonly MPN accnip = Resolve("accnip");
    public static readonly MPN accsenaka = Resolve("accsenaka");
    public static readonly MPN accshippo = Resolve("accshippo");
    public static readonly MPN acctatoo = Resolve("acctatoo");
    public static readonly MPN accude = Resolve("accude");
    public static readonly MPN accvag = Resolve("accvag");
    public static readonly MPN accxxx = Resolve("accxxx");
    public static readonly MPN body = Resolve("body");
    public static readonly MPN bra = Resolve("bra");
    public static readonly MPN chikubi = Resolve("chikubi");
    public static readonly MPN chikubicolor = Resolve("chikubicolor");
    public static readonly MPN eye = Resolve("eye");
    public static readonly MPN eye_hi = Resolve("eye_hi");
    public static readonly MPN eye_hi_r = Resolve("eye_hi_r");
    public static readonly MPN folder_eye = Resolve("folder_eye");
    public static readonly MPN folder_eyewhite = Resolve("folder_eyewhite");
    public static readonly MPN folder_futae = Resolve("folder_futae");
    public static readonly MPN folder_matsuge_low = Resolve("folder_matsuge_low");
    public static readonly MPN folder_matsuge_up = Resolve("folder_matsuge_up");
    public static readonly MPN folder_mayu = Resolve("folder_mayu");
    public static readonly MPN folder_skin = Resolve("folder_skin");
    public static readonly MPN folder_underhair = Resolve("folder_underhair");
    public static readonly MPN glove = Resolve("glove");
    public static readonly MPN hairaho = Resolve("hairaho");
    public static readonly MPN haircolor = Resolve("haircolor");
    public static readonly MPN hairf = Resolve("hairf");
    public static readonly MPN hairr = Resolve("hairr");
    public static readonly MPN hairs = Resolve("hairs");
    public static readonly MPN hairt = Resolve("hairt");
    public static readonly MPN head = Resolve("head");
    public static readonly MPN headset = Resolve("headset");
    public static readonly MPN hokuro = Resolve("hokuro");
    public static readonly MPN lip = Resolve("lip");
    public static readonly MPN mayu = Resolve("mayu");
    public static readonly MPN megane = Resolve("megane");
    public static readonly MPN mizugi = Resolve("mizugi");
    public static readonly MPN moza = Resolve("moza");
    public static readonly MPN null_mpn = Resolve("null_mpn");
    public static readonly MPN onepiece = Resolve("onepiece");
    public static readonly MPN panz = Resolve("panz");
    public static readonly MPN set_body = Resolve("set_body");
    public static readonly MPN set_maidwear = Resolve("set_maidwear");
    public static readonly MPN set_mywear = Resolve("set_mywear");
    public static readonly MPN set_underwear = Resolve("set_underwear");
    public static readonly MPN shoes = Resolve("shoes");
    public static readonly MPN skin = Resolve("skin");
    public static readonly MPN skirt = Resolve("skirt");
    public static readonly MPN stkg = Resolve("stkg");
    public static readonly MPN underhair = Resolve("underhair");
    public static readonly MPN wear = Resolve("wear");

    private static MPN Resolve(string name)
    {
        MPN value;
        return EnumUtil.TryParse(name, true, out value) ? value : MPN.null_mpn;
    }
}
