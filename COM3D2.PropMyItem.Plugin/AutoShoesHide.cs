using System;
using System.Linq;

namespace COM3D2.PropMyItem.Plugin;

public class AutoShoesHide
{
    private readonly MPN[] checkMPNs;
    private string _lastBGName = string.Empty;

    private int _lastMaidCount;

    public AutoShoesHide()
    {
        checkMPNs = new[]
        {
            Mpn.acchat,
            Mpn.headset,
            Mpn.wear,
            Mpn.skirt,
            Mpn.onepiece,
            Mpn.mizugi,
            Mpn.bra,
            Mpn.panz,
            Mpn.stkg
        };
    }

    public void Update()
    {
        try
        {
            if (UserConfig.Instance.IsAutoShoesHide)
            {
                var visibleMaidList = CommonUtil.GetVisibleMaidList();
                if (visibleMaidList.Count != 0)
                {
                    var bgname = GameMain.Instance.BgMgr.GetBGName();
                    if (!(_lastBGName == bgname) || _lastMaidCount != visibleMaidList.Count)
                    {
                        // 目标背景（室内）里自动脱鞋；其他背景则把临时脱掉的鞋穿回去
                        if (PluginConfig.Instance.TargetBGList.Contains(bgname,
                                StringComparer.OrdinalIgnoreCase))
                        {
                            foreach (var maid in visibleMaidList)
                            {
                                if (maid.IsAllProcPropBusy) return;

                                if (maid.GetProp(Mpn.shoes).strTempFileName !=
                                    "_i_shoes_del.menu")
                                {
                                    Menu.SetMaidItemTemp(maid, "_i_shoes_del.menu", true);
                                    maid.AllProcProp();
                                }
                            }
                        }
                        else
                        {
                            foreach (var maid2 in visibleMaidList)
                            {
                                if (maid2.IsAllProcPropBusy) return;

                                var wearingNothingChanged = true;
                                foreach (var mpn in checkMPNs)
                                {
                                    var prop = maid2.GetProp(mpn);
                                    if (prop.strFileName == prop.strTempFileName)
                                    {
                                        wearingNothingChanged = false;
                                        break;
                                    }
                                }

                                if (wearingNothingChanged && maid2.GetProp(Mpn.shoes).strTempFileName ==
                                    "_i_shoes_del.menu")
                                {
                                    maid2.ResetProp(Mpn.shoes);
                                    maid2.AllProcProp();
                                }
                            }
                        }

                        _lastBGName = bgname;
                        _lastMaidCount = visibleMaidList.Count;
                    }
                }
            }
        }
        catch (Exception)
        {
        }
    }

    public class PropFileName
    {
        public string FileName;

        public int FileNameRID;

        public string TempFileName;

        public PropFileName(int rid, string filename, string tmpFileName)
        {
            FileNameRID = rid;
            FileName = filename;
            TempFileName = tmpFileName;
        }
    }
}