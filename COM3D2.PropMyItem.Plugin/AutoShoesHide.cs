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
            MPN.acchat,
            MPN.headset,
            MPN.wear,
            MPN.skirt,
            MPN.onepiece,
            MPN.mizugi,
            MPN.bra,
            MPN.panz,
            MPN.stkg
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
                        if (PluginConfig.Instance.TargetBGList.Contains(bgname,
                                StringComparer.OrdinalIgnoreCase))
                            using (var enumerator = visibleMaidList.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var maid = enumerator.Current;
                                    if (maid.IsAllProcPropBusy) return;

                                    if (maid.GetProp(MPN.shoes).strTempFileName !=
                                        "_i_shoes_del.menu")
                                    {
                                        Menu.SetMaidItemTemp(maid, "_i_shoes_del.menu", true);
                                        maid.AllProcProp();
                                    }
                                }

                                goto IL_184;
                            }

                        foreach (var maid2 in visibleMaidList)
                        {
                            if (maid2.IsAllProcPropBusy) return;

                            var flag = true;
                            foreach (var mpn in checkMPNs)
                            {
                                var prop = maid2.GetProp(mpn);
                                if (prop.strFileName == prop.strTempFileName)
                                {
                                    flag = false;
                                    break;
                                }
                            }

                            if (flag && maid2.GetProp(MPN.shoes).strTempFileName ==
                                "_i_shoes_del.menu")
                            {
                                maid2.ResetProp(MPN.shoes);
                                maid2.AllProcProp();
                            }
                        }

                        IL_184:
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