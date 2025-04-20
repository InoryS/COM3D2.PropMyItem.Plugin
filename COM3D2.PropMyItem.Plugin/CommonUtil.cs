using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace COM3D2.PropMyItem.Plugin
{
    // Token: 0x02000003 RID: 3
    public class CommonUtil
    {
        public enum ModKey
        {
            Control,
            Alt,
            Shift
        }

        // Token: 0x06000003 RID: 3 RVA: 0x000022A0 File Offset: 0x000004A0
        public static void Log(string text)
        {
            try
            {
                PropMyItem.Log.LogMessage(string.Format("{0}({1}) : {2}", "PropMyItem", "2.3.0.0", text));
            }
            catch
            {
            }
        }

        // Token: 0x06000004 RID: 4 RVA: 0x000022DC File Offset: 0x000004DC
        public static string WildCardMatchEvaluator(Match match)
        {
            var value = match.Value;
            if (value.Equals("?")) return ".";

            if (value.Equals("*")) return ".*";

            return Regex.Escape(value);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x0000231C File Offset: 0x0000051C
        public static Maid GetVisibleMaid(int index)
        {
            Maid result = null;
            try
            {
                var visibleMaidList = GetVisibleMaidList();
                if (visibleMaidList.Count > index) result = visibleMaidList[index];
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return result;
        }

        // Token: 0x06000006 RID: 6 RVA: 0x00002364 File Offset: 0x00000564
        public static List<Maid> GetVisibleMaidList()
        {
            var list = new List<Maid>();
            var characterMgr = GameMain.Instance.CharacterMgr;
            var maidCount = characterMgr.GetMaidCount();
            for (var i = 0; i < maidCount; i++)
            {
                var maid = characterMgr.GetMaid(i);
                if (maid != null && maid.isActiveAndEnabled && maid.Visible) list.Add(maid);
            }

            var stockMaidCount = characterMgr.GetStockMaidCount();
            for (var j = 0; j < stockMaidCount; j++)
            {
                var stockMaid = characterMgr.GetStockMaid(j);
                if (stockMaid != null && stockMaid.isActiveAndEnabled && stockMaid.Visible && !list.Contains(stockMaid))
                    list.Add(stockMaid);
            }

            return list;
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002418 File Offset: 0x00000618
        public static string GetSelectedMenuFileName(MPN? mpn, Maid maid)
        {
            var result = string.Empty;
            if (mpn != null)
                if (mpn != MPN.set_maidwear && mpn != MPN.set_mywear && mpn != MPN.set_underwear && mpn != MPN.set_body)
                    result = maid.GetProp(mpn.Value).strFileName;

            return result;
        }

        public static bool IsModKey(ModKey key)
        {
            switch (key)
            {
                case ModKey.Control: return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                case ModKey.Alt: return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
                case ModKey.Shift: return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                default: return false;
            }
        }
    }
}