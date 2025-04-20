using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GearMenu;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COM3D2.PropMyItem.Plugin
{
    using static CommonUtil;

    [BepInPlugin(PluginInfo.PluginName, PluginInfo.PluginName, PluginInfo.PluginVersion)]
    public class PropMyItem : BaseUnityPlugin
    {
        public static ManualLogSource Log;

        // Token: 0x04000044 RID: 68
        private static bool _isForcedInit;

        // Token: 0x04000045 RID: 69
        //private bool _isStartUpLoadead;

        private static bool _isLoading;

        // Token: 0x0400002A RID: 42
        private readonly AutoShoesHide _autoShoesHide = new AutoShoesHide();

        // Token: 0x0400003C RID: 60
        private readonly Dictionary<string, MPN> _categoryMPNDic = new Dictionary<string, MPN>();

        // Token: 0x04000032 RID: 50
        private Vector2 _categoryScrollPosition;

        // Token: 0x04000034 RID: 52
        private Vector2 _colorItemScrollPosition;

        // Token: 0x04000035 RID: 53
        private readonly List<FolderMenu> _folders = new List<FolderMenu>();

        // Token: 0x04000040 RID: 64
        private bool _isFavFilter;

        // Token: 0x04000043 RID: 67
        private bool _isFreeColor;

        // Token: 0x0400002B RID: 43
        private bool _isMinimum;

        // Token: 0x0400002F RID: 47
        private bool _isShowFilterSetting;

        // Token: 0x0400002D RID: 45
        //private bool _isPluginKeyChange;

        // Token: 0x0400002E RID: 46
        private bool _isShowSetting;

        // Token: 0x0400002C RID: 44
        private bool _isVisible;

        private bool _menuFilesReady;

        // Token: 0x04000046 RID: 70
        //private Dictionary<MPN, string> presetRestoreDic_ = new Dictionary<MPN, string>();

        // Token: 0x04000047 RID: 71
        private readonly List<string> _menuList = new List<string>();

        // Token: 0x0400003D RID: 61
        private readonly Dictionary<MPN, string> _menuMPNCategoryDic = new Dictionary<MPN, string>();

        // Token: 0x04000041 RID: 65
        public Dictionary<MPN, List<MenuInfo>> _mpnMenuListDictionary = new Dictionary<MPN, List<MenuInfo>>();

        // Token: 0x0400004C RID: 76
        private readonly List<string> _myPatternList = new List<string>();

        // Token: 0x0400004B RID: 75
        private SavePreset _savepreset = new SavePreset();

        // Token: 0x04000028 RID: 40
        private int _sceneLevel;

        // Token: 0x04000031 RID: 49
        private Vector2 _scrollFilterPosition;

        // Token: 0x04000033 RID: 51
        private Vector2 _scrollPosition;

        // Token: 0x04000039 RID: 57
        private int _selectedCategory = -1;

        // Token: 0x04000042 RID: 66
        private int _selectedEyeClorType;

        // Token: 0x0400003F RID: 63
        private int _selectedFilter;

        // Token: 0x04000030 RID: 48
        private string _selectedFilterText = string.Empty;

        // Token: 0x04000037 RID: 55
        private int _selectedFolder;

        // Token: 0x0400003A RID: 58
        private MenuInfo _selectedItem;

        // Token: 0x04000036 RID: 54
        private int _selectedMaid;

        // Token: 0x04000038 RID: 56
        private MPN _selectedMPN;

        // Token: 0x0400003E RID: 62
        private List<CharacterMgr.Preset> _selectedPresetList = new List<CharacterMgr.Preset>();

        // Token: 0x0400004A RID: 74
        private CharacterMgr.PresetType _selectedPresetType = CharacterMgr.PresetType.All;

        // Token: 0x0400003B RID: 59
        private MenuInfo _selectedVariationItem;

        // Token: 0x04000029 RID: 41
        private Rect _windowRect;
        private bool isAllMaid;
        private Task task;

        public PropMyItem()
        {
            _folders.Add(new FolderMenu("頭", new[] // 얼굴
            {
                "顔",
                "眉",
                "目",
                "目ハイライト",
                "鞏膜",
                "上まつげ",
                "下まつげ",
                "二重まぶた",
                "ほくろ",
                "唇",
                "歯"
            }));
            _folders.Add(new FolderMenu("髪", new[] // 헤어
            {
                "前髪",
                "後髪",
                "横髪",
                "エクステ髪",
                "アホ毛"
            }));
            _folders.Add(new FolderMenu("身体", new[]
            {
                "肌",
                "乳首",
                "chikubicolor",
                "タトゥー",
                "アンダーヘア",
                "ボディ",
                "指甲",
                "moza"
            }));
            _folders.Add(new FolderMenu("服装", new[]
            {
                "帽子",
                "ヘッドドレス",
                "トップス",
                "ボトムス",
                "ワンピース",
                "水着",
                "ブラジャー",
                "パンツ",
                "靴下",
                "靴"
            }));
            _folders.Add(new FolderMenu("アクセサリ", new[]
            {
                " 前髪 ",
                "メガネ",
                "アイマスク",
                "鼻",
                "耳",
                "手袋",
                "ネックレス",
                "チョーカー",
                "リボン",
                "\u3000乳首\u3000",
                "腕",
                "へそ",
                "足首",
                "背中",
                "accanl",
                "accvag",
                "しっぽ",
                "前穴"
            }));
            _folders.Add(new FolderMenu("セット", new[]
            {
                "メイド服",
                "コスチューム",
                "下着",
                "\u3000身体\u3000"
            }));
            _folders.Add(new FolderMenu("プリセット", new[]
            {
                "服 / 身体",
                "服",
                "身体",
                "\u3000全て\u3000"
            }));
            _folders.Add(new FolderMenu("全て", new string[0]));
            _folders.Add(new FolderMenu("選択中", new string[0]));
            _categoryMPNDic.Add("顔", MPN.head);
            _categoryMPNDic.Add("眉", MPN.folder_mayu);
            _categoryMPNDic.Add("目", MPN.folder_eye);
            _categoryMPNDic.Add("目ハイライト", MPN.eye_hi);
            _categoryMPNDic.Add("ほくろ", MPN.hokuro);
            _categoryMPNDic.Add("唇", MPN.lip);
            _categoryMPNDic.Add("歯", MPN.accha);

            _categoryMPNDic.Add("上まつげ", MPN.folder_matsuge_up);
            _categoryMPNDic.Add("下まつげ", MPN.folder_matsuge_low);
            _categoryMPNDic.Add("二重まぶた", MPN.folder_futae);

            _categoryMPNDic.Add("前髪", MPN.hairf);
            _categoryMPNDic.Add("後髪", MPN.hairr);
            _categoryMPNDic.Add("横髪", MPN.hairs);
            _categoryMPNDic.Add("エクステ髪", MPN.hairt);
            _categoryMPNDic.Add("アホ毛", MPN.hairaho);
            _categoryMPNDic.Add("肌", MPN.folder_skin);
            _categoryMPNDic.Add("乳首", MPN.chikubi);
            _categoryMPNDic.Add("タトゥー", MPN.acctatoo);
            _categoryMPNDic.Add("アンダーヘア", MPN.folder_underhair);
            _categoryMPNDic.Add("ボディ", MPN.body);
            _categoryMPNDic.Add("帽子", MPN.acchat);
            _categoryMPNDic.Add("ヘッドドレス", MPN.headset);
            _categoryMPNDic.Add("トップス", MPN.wear);
            _categoryMPNDic.Add("ボトムス", MPN.skirt);
            _categoryMPNDic.Add("ワンピース", MPN.onepiece);
            _categoryMPNDic.Add("水着", MPN.mizugi);
            _categoryMPNDic.Add("ブラジャー", MPN.bra);
            _categoryMPNDic.Add("パンツ", MPN.panz);
            _categoryMPNDic.Add("靴下", MPN.stkg);
            _categoryMPNDic.Add("靴", MPN.shoes);
            _categoryMPNDic.Add(" 前髪 ", MPN.acckami);
            _categoryMPNDic.Add("メガネ", MPN.megane);
            _categoryMPNDic.Add("アイマスク", MPN.acchead);
            _categoryMPNDic.Add("鼻", MPN.acchana);
            _categoryMPNDic.Add("耳", MPN.accmimi);
            _categoryMPNDic.Add("手袋", MPN.glove);
            _categoryMPNDic.Add("ネックレス", MPN.acckubi);
            _categoryMPNDic.Add("チョーカー", MPN.acckubiwa);
            _categoryMPNDic.Add("リボン", MPN.acckamisub);
            _categoryMPNDic.Add("\u3000乳首\u3000", MPN.accnip);
            _categoryMPNDic.Add("腕", MPN.accude);
            _categoryMPNDic.Add("へそ", MPN.accheso);
            _categoryMPNDic.Add("足首", MPN.accashi);
            _categoryMPNDic.Add("背中", MPN.accsenaka);
            _categoryMPNDic.Add("しっぽ", MPN.accshippo);
            _categoryMPNDic.Add("前穴", MPN.accxxx);
            _categoryMPNDic.Add("メイド服", MPN.set_maidwear);
            _categoryMPNDic.Add("コスチューム", MPN.set_mywear);
            _categoryMPNDic.Add("下着", MPN.set_underwear);
            _categoryMPNDic.Add("\u3000身体\u3000", MPN.set_body);

            _categoryMPNDic.Add("鞏膜", MPN.folder_eyewhite);

            _categoryMPNDic.Add("accanl", MPN.accanl);
            _categoryMPNDic.Add("指甲", MPN.accnail);
            _categoryMPNDic.Add("accvag", MPN.accvag);
            _categoryMPNDic.Add("moza", MPN.moza);

            _categoryMPNDic.Add("chikubicolor", MPN.chikubicolor);

            foreach (var text in _categoryMPNDic.Keys) _menuMPNCategoryDic.Add(_categoryMPNDic[text], text);

            foreach (var obj in Enum.GetValues(typeof(MPN)))
            {
                var key = (MPN)obj;
                _mpnMenuListDictionary.Add(key, new List<MenuInfo>());
            }
        }

        private ConfigEntry<KeyboardShortcut> ShowCounter { get; set; }

        // Token: 0x0600002C RID: 44 RVA: 0x00003808 File Offset: 0x00001A08
        public void Awake()
        {
            Log = Logger;
            Log.LogMessage("[PropMyItem] Awake");

            ShowCounter = Config.Bind("Hotkeys", "Show GUI", new KeyboardShortcut(KeyCode.I));
        }

        public void Start()
        {
            Log.LogMessage("[PropMyItem] Start");
            GameMain.Instance.StartCoroutine(CheckMenuDatabase());
            SceneManager.sceneLoaded += OnSceneLoaded;

            // add button to GearMenu
            Buttons.Add(
                "PropMyItem",
                "Toggle COM3D2.PropMyItem.Plugin GUI",
                Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAAXNSR0IB2cksfwAAAAlwSFlzAAAWJQAAFiUBSVIk8AAAAcJQTFRF////+fn57u7u3d3dzc3NxsbGxcXFzMzMpqamo6Ojn5+f2dnZqKioxMTEtbW1vLy8ubm55+fnqampvr6+j4+P4uLikZGR1NTUi4uL8fHx6urqkpKS4eHhr6+v5ubmCwsLdHR0mpqaJiYm/v7+Dg4OWVlZ3t7eKCgomZmZe3t7PT09wcHBFxcXl5eXJSUly8vLNDQ0hYWFkJCQ+/v7ERERg4ODQUFBz8/Pv7+/QEBAeXl5nJycysrKTExMfHx8IyMjeHh4nZ2dWlpaWFhYiIiIEhIS9PT0U1NTq6ur+Pj4V1dX9vb2Hx8fHBwc9/f3RkZGt7e3yMjIQkJCVFRUR0dHc3NzUlJSrq6uLCwsS0tLRUVFenp6urq6Y2NjZWVl19fXgYGB6Ojo1tbWpKSk1dXV39/fHh4eKSkpPz8/9fX1f39/dXV1VVVVampqwMDApaWlXV1doKCgXl5e5OTkGxsbdnZ2p6en/Pz8EBAQLi4u09PTtra2X19fw8PDW1tbd3d3ycnJQ0ND2trauLi4ZmZm8vLyk5OTcHBwgICA6+vr+vr6x8fH29vbZ2dnKysr8/Pz4ODghoaG7e3tjY2Nu7u7YGBgHfa81gAAAbdJREFUeJxjZCAAGIlQwAgCGCr/M/wHAZA4EytIASYASv/+B1LAzfj9L1bTmTn/fwUp4P3/BYf9PIyfQQpYOT/hUMD3/TdIAT/jB0GQO18ziIHd+/39fxnGz2wcjxgE/n8EKRBg+MAiDZR4L8TI+FICpOSaNuNfFsYr/4AyUAX6jBcYDEFq7ooonmYwA7KOM1gxHkUosGE8zGDNfNjuIIMD434Gp3PMBnsZjIT2ICngZNzlzsj49BIDp+M2b8ZLX60YN/kzbkAoCATZvNr7+j0GZXFmScaV/J5A/lIGhAJWhdso/othXAyiEArYbPahKBDwW4SqAEdAkaAgnnGBmvV8PAokPedZac7FoyDlDyhNrA25/VN3w7PsecmM80TNJCYhe9Nur4P+RIaCtZrajH0h7w3Omrw4xXEJSQGv6b5Sxq5yxk8MF+zboxTaqhlbGNKfb0IoqGVsqp/8ztj1AAODR0MjY10zYw1DhG41QoG264Q2xo3CfPpAl5SahZUE2BYz9DEWQhUAE8yE3seSFTc19rgeY7TKKZz4r2HKG4apW7ZDEwzBJEcw0RJM9gQzDsGshx8QVAAAyd+zIah/5QEAAAAASUVORK5CYII="),
                go => ToggleGUI()
            );
        }

        // Token: 0x0600002E RID: 46 RVA: 0x0000384C File Offset: 0x00001A4C
        public void Update()
        {
            try
            {
                _autoShoesHide.Update();
                ShowCounterHandler();
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00003A88 File Offset: 0x00001C88
        public void OnGUI()
        {
            if (!_isVisible) return;

            // Calculate the new window size
            float newWidth = 800;
            float newHeight = 600;

            if (_isShowSetting)
            {
                // Update the windowRect size
                _windowRect.width = newWidth;
                _windowRect.height = newHeight;

                // Ensure that the window doesn't go off the screen
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - newWidth);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - newHeight);

                _windowRect = GUI.Window(PluginInfo.WindowID, _windowRect,
                    GuiSettingFunc, "PropMyItem " + PluginInfo.PluginVersion,
                    GuiStyles.WindowStyle);
                return;
            }

            if (_isShowFilterSetting)
            {
                // Update the windowRect size
                _windowRect.width = newWidth;
                _windowRect.height = newHeight;

                // Ensure that the window doesn't go off the screen
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - newWidth);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - newHeight);

                _windowRect = GUI.Window(PluginInfo.WindowID, _windowRect,
                    GuiFilterSettingFunc, "PropMyItem " + PluginInfo.PluginVersion,
                    GuiStyles.WindowStyle);
                return;
            }

            // Update the windowRect size
            _windowRect.width = newWidth;
            _windowRect.height = newHeight;

            // Ensure that the window doesn't go off the screen
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - newWidth);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - newHeight);

            _windowRect = GUI.Window(PluginInfo.WindowID, _windowRect, GuiFunc,
                "PropMyItem " + PluginInfo.PluginVersion, GuiStyles.WindowStyle);
        }

        public void OnApplicationQuit()
        {
            //MyLog.LogMessage("[PropMyItem] OnApplicationQuit");
            //isTaskStop = true;
        }

        private IEnumerator CheckMenuDatabase()
        {
            if (_menuFilesReady) yield break;
            while (!GameMain.Instance.MenuDataBase.JobFinished()) yield return null;
            _menuFilesReady = true;
            _isLoading = true;
            task = Task.Factory.StartNew(() => LoadMenuFiles());
            Log.LogMessage("[PropMyItem] Menu files are ready");
        }

        // Token: 0x0600002D RID: 45 RVA: 0x0000383C File Offset: 0x00001A3C
        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            _sceneLevel = scene.buildIndex;
        }


        private void ShowCounterHandler()
        {
            if (ShowCounter.Value.IsUp())
            {
                ToggleGUI();
            }
            else
            {
                if (_isVisible && _windowRect.Contains(new Vector2(Input.mousePosition.x,
                        Screen.height - Input.mousePosition.y)))
                {
                    GameMain.Instance.MainCamera.SetControl(false);
                    if (Event.current.type != EventType.KeyDown && Event.current.type != EventType.KeyUp)
                        Input.ResetInputAxes();
                }
                else if (!GameMain.Instance.MainCamera.GetControl())
                {
                    GameMain.Instance.MainCamera.SetControl(true);
                }
            }
        }


        public void ToggleGUI()
        {
            if (_menuFilesReady)
            {
                _isVisible = !_isVisible;
                _isMinimum = !_isVisible;
            }
            else
            {
                Log.LogMessage("[PropMyItem] Menu files are not ready yet");
            }
        }


        // Token: 0x06000030 RID: 48 RVA: 0x00003B40 File Offset: 0x00001D40
        private void GuiSettingFunc(int windowID)
        {
            _windowRect.width = 300f;
            var num = GuiStyles.ControlHeight + GuiStyles.Margin;
            var margin = GuiStyles.Margin;
            var width = _windowRect.width - GuiStyles.Margin * 2f;
            /*
            string text = "キー入力待機中...";
            if (!this._isPluginKeyChange)
            {
                List<string> list = new List<string>();
                if (UserConfig.Instance.IsControlKey)
                {
                    list.Add("Ctrl");
                }
                if (UserConfig.Instance.IsShiftKey)
                {
                    list.Add("Shift");
                }
                if (UserConfig.Instance.IsAltKey)
                {
                    list.Add("Alt");
                }
                list.Add(UserConfig.Instance.GuiVisibleKey.ToUpper());
                text = "表示キー : ";
                for (int i = 0; i < list.Count; i++)
                {
                    text += list[i];
                    if (i != list.Count - 1)
                    {
                        text += " + ";
                    }
                }
            }
            GUI.enabled = !this._isPluginKeyChange;
            if (GUI.Button(new Rect(margin, num, width, GuiStyles.ControlHeight), text, GuiStyles.ButtonStyle))
            {
                this._isPluginKeyChange = !this._isPluginKeyChange;
            }
            */
            GUI.enabled = true;
            num += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
            var flag = UserConfig.Instance.IsAutoShoesHide;
            flag = GUI.Toggle(new Rect(margin, num, width, GuiStyles.ControlHeight), flag, "室内で自動的に靴を脱ぐ ",
                GuiStyles.ToggleStyle);
            if (flag != UserConfig.Instance.IsAutoShoesHide)
            {
                UserConfig.Instance.IsAutoShoesHide = flag;
                UserConfig.Instance.Save();
            }

            num += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
            var flag2 = UserConfig.Instance.IsOutputInfoLog;
            flag2 = GUI.Toggle(new Rect(margin, num, width, GuiStyles.ControlHeight), flag2, "アイテム変更時のログ出力",
                GuiStyles.ToggleStyle);
            if (flag2 != UserConfig.Instance.IsOutputInfoLog)
            {
                UserConfig.Instance.IsOutputInfoLog = flag2;
                UserConfig.Instance.Save();
            }

            num += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
            if (GUI.Button(new Rect(margin, num, width, GuiStyles.ControlHeight), "Menu/Mod 再読み込み",
                    GuiStyles.ButtonStyle))
            {
                _isShowSetting = false;
                //this._isPluginKeyChange = false;
                _isForcedInit = true;
                if (_isLoading)
                {
                    Log.LogMessage("[PropMyItem] _isLoading...");
                    //return;
                }
                else
                {
                    _isLoading = true;
                    task = Task.Factory.StartNew(() => LoadMenuFiles(_isForcedInit));
                }
            }

            num += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
            if (GUI.Button(new Rect(margin, num, width, GuiStyles.ControlHeight), "戻る", GuiStyles.ButtonStyle))
                _isShowSetting = false;
            //this._isPluginKeyChange = false;
            num += GuiStyles.ControlHeight + GuiStyles.Margin;
            _windowRect.height = num;
            GUI.DragWindow();
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00003DF0 File Offset: 0x00001FF0
        private void GuiFilterSettingFunc(int windowID)
        {
            var num = _windowRect.width - GuiStyles.Margin * 2f;
            var num2 = _windowRect.height - GuiStyles.Margin * 2f;
            var num3 = GuiStyles.ControlHeight + GuiStyles.Margin;
            var num4 = GuiStyles.Margin;
            var text = "フィルタ文字列：";
            GUI.Label(new Rect(num4, num3, GuiStyles.FontSize * text.Length, GuiStyles.ControlHeight), text,
                GuiStyles.LabelStyle);
            num4 += GuiStyles.FontSize * text.Length + GuiStyles.Margin;
            var num5 = num - num4 - GuiStyles.Margin - GuiStyles.FontSize * 4;
            _selectedFilterText = GUI.TextField(new Rect(num4, num3, num5, GuiStyles.ControlHeight),
                _selectedFilterText, GuiStyles.TextFieldStyle);
            num4 += num5 + GuiStyles.Margin;
            GUI.enabled = !string.IsNullOrEmpty(_selectedFilterText);
            if (GUI.Button(new Rect(num4, num3, GuiStyles.FontSize * 4, GuiStyles.ControlHeight), "登録",
                    GuiStyles.ButtonStyle))
            {
                UserConfig.Instance.FilterTextList.Add(_selectedFilterText);
                UserConfig.Instance.FilterTextList.Sort();
                UserConfig.Instance.Save();
                _isShowFilterSetting = false;
            }

            GUI.enabled = true;
            num4 = GuiStyles.Margin;
            num3 += GuiStyles.ControlHeight + GuiStyles.Margin;
            var filterTextList = UserConfig.Instance.FilterTextList;
            var count = filterTextList.Count;
            var position = new Rect(num4, num3, num, num2 - num3 - GuiStyles.ControlHeight - GuiStyles.Margin * 2f);
            var viewRect = new Rect(0f, 0f, num - GuiStyles.ScrollWidth,
                count * (GuiStyles.ControlHeight + GuiStyles.Margin));
            _scrollFilterPosition = GUI.BeginScrollView(position, _scrollFilterPosition, viewRect);
            var position2 = new Rect(GuiStyles.Margin, 0f, viewRect.width - GuiStyles.Margin - 50f,
                GuiStyles.ControlHeight);
            var position3 = new Rect(position2.x + position2.width + GuiStyles.Margin, 0f, 50f,
                GuiStyles.ControlHeight);
            for (var i = 0; i < filterTextList.Count; i++)
            {
                position2.y = i * (GuiStyles.ControlHeight + GuiStyles.Margin);
                position3.y = position2.y;
                if (GUI.Button(position2, filterTextList[i], GuiStyles.ButtonStyle))
                {
                    _selectedFilterText = filterTextList[i];
                    _isShowFilterSetting = false;
                }

                if (GUI.Button(position3, "x", GuiStyles.ButtonStyle))
                {
                    UserConfig.Instance.FilterTextList.RemoveAt(i);
                    UserConfig.Instance.Save();
                }
            }

            GUI.EndScrollView();
            num3 = num2 - GuiStyles.ControlHeight - GuiStyles.Margin * 2f;
            if (GUI.Button(new Rect(num4, num3, num, GuiStyles.ControlHeight), "戻る", GuiStyles.ButtonStyle))
            {
                _isShowSetting = false;
                //this._isPluginKeyChange = false;
                _isShowFilterSetting = false;
            }

            num3 += GuiStyles.ControlHeight + GuiStyles.Margin;
            GUI.DragWindow();
        }

        // Token: 0x06000032 RID: 50 RVA: 0x0000410C File Offset: 0x0000230C
        private void GuiFunc(int windowID)
        {
            try
            {
                // Add "X" button for close window in the top right corner
                if (GUI.Button(new Rect(5, 30, 20, 20), "X"))
                    // Set visibility to false to close window
                    _isVisible = false;

                var text = _isMinimum ? "" : "最小化";
                var position = new Rect(GuiStyles.Margin, 0f, GuiStyles.FontSize * (text.Length + 2),
                    GuiStyles.ControlHeight);
                _isMinimum = GUI.Toggle(position, _isMinimum, text, GuiStyles.ToggleStyle);
                if (_isMinimum)
                {
                    _windowRect.width = 100f;
                    _windowRect.height = GuiStyles.ControlHeight;
                }
                else
                {
                    _windowRect.height = 570f;
                    var yPos = GuiStyles.ControlHeight + GuiStyles.Margin;
                    var margin = GuiStyles.Margin;
                    guiSelectedMaid(ref margin, ref yPos);
                    guiSelectedCategoryFolder(ref margin, yPos, _windowRect.height);
                    guiSelectedCategory(ref margin, yPos, _windowRect.height);
                    if (_folders[_selectedFolder].Name == "プリセット") //프리셋
                    {
                        guiSelectedPreset(ref margin, yPos, _windowRect.height);
                    }
                    else if (_folders[_selectedFolder].Name == "Debug")
                    {
                        guiDebug(ref margin, yPos, _windowRect.height);
                    }
                    else
                    {
                        if (_selectedMPN != MPN.null_mpn || _folders[_selectedFolder].Name == "全て" ||
                            _folders[_selectedFolder].Name == "選択中")
                        {
                            guiSelectedItemFilter(margin, yPos);
                            guiSelectedItem(ref margin, yPos, _windowRect.height);
                        }

                        guiSelectedColorSet(ref margin, ref yPos);
                        guiSelectedMugenColor(ref margin, ref yPos);
                    }

                    _windowRect.width = margin;
                }
            }
            catch (Exception ex)
            {
                Log(ex.StackTrace);
            }
            finally
            {
                GUI.DragWindow();
            }
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00004330 File Offset: 0x00002530
        private void guiSelectedMaid(ref float xPos, ref float yPos)
        {
            try
            {
                var visibleMaidList = GetVisibleMaidList();
                if (visibleMaidList.Count < 1)
                {
                    _selectedMaid = -1;
                }
                else
                {
                    if (_selectedMaid >= visibleMaidList.Count || _selectedMaid < 0) _selectedMaid = 0;

                    var text = visibleMaidList[_selectedMaid].status.isFirstNameCall
                        ? visibleMaidList[_selectedMaid].status.firstName
                        : visibleMaidList[_selectedMaid].status.lastName;
                    var position = new Rect(xPos + 30f, 8f, 50f, 75f);
                    var position2 = new Rect(xPos + 85f, yPos, 10 * GuiStyles.FontSize,
                        GuiStyles.ControlHeight);
                    var position3 = new Rect(xPos, yPos + 24f, 2 * GuiStyles.FontSize,
                        GuiStyles.ControlHeight);
                    var position4 = new Rect(xPos + 85f, yPos + 24f, 2 * GuiStyles.FontSize,
                        GuiStyles.ControlHeight);
                    var position5 = new Rect(position4.x + position4.width + 5f, position4.y, 75f, position4.height);

                    GUI.Label(position, visibleMaidList[_selectedMaid].GetThumIcon(), GuiStyles.LabelStyle);
                    GuiStyles.LabelStyle.alignment = TextAnchor.MiddleLeft;
                    GUI.Label(position2, text, GuiStyles.LabelStyle);
                    GuiStyles.LabelStyle.alignment = TextAnchor.MiddleCenter;
                    if (GUI.Button(position3, "<", GuiStyles.ButtonStyle))
                        _selectedMaid = _selectedMaid == 0
                            ? visibleMaidList.Count - 1
                            : _selectedMaid - 1;

                    if (GUI.Button(position4, ">", GuiStyles.ButtonStyle))
                        _selectedMaid = _selectedMaid == visibleMaidList.Count - 1
                            ? 0
                            : _selectedMaid + 1;

                    isAllMaid = GUI.Toggle(position5, isAllMaid, "All Maid", GuiStyles.ToggleStyle);
                }
            }
            finally
            {
                yPos += 50f;
            }
        }

        // Token: 0x06000034 RID: 52 RVA: 0x00004508 File Offset: 0x00002708
        private void updateMaidEyePosY(Maid maid, float value)
        {
            var num = 5000f;
            var num2 = maid.body0.trsEyeL.localPosition.y * num;
            if (value < 0f) value = 0f;

            var localPosition = maid.body0.trsEyeL.localPosition;
            var localPosition2 = maid.body0.trsEyeR.localPosition;
            maid.body0.trsEyeL.localPosition = new Vector3(localPosition.x, Math.Max((num2 + value) / num, 0f),
                localPosition.z);
            maid.body0.trsEyeR.localPosition = new Vector3(localPosition.x, Math.Min((num2 - value) / num, 0f),
                localPosition.z);
        }

        // Token: 0x06000035 RID: 53 RVA: 0x000045C4 File Offset: 0x000027C4
        private void guiSelectedCategoryFolder(ref float xPos, float yPos, float windowHeight)
        {
            float num = GuiStyles.FontSize * 6;
            var num2 = (float)(GuiStyles.ControlHeight * 1.5);
            for (var i = 0; i < _folders.Count; i++)
            {
                if (_folders[i].Name == "全て") //모든
                    yPos += GuiStyles.Margin * 2f;

                var position = new Rect(xPos, yPos + (num2 + GuiStyles.Margin) * i, num, num2);
                GUI.enabled = _selectedFolder != i;
                if (GUI.Button(position, _folders[i].Name, GuiStyles.ButtonStyle))
                {
                    _selectedFolder = i;
                    _selectedMPN = MPN.null_mpn;
                    _selectedCategory = -1;
                    _selectedItem = null;
                    _selectedVariationItem = null;
                }

                GUI.enabled = true;
            }

            if (GUI.Button(new Rect(xPos, windowHeight - num2 - GuiStyles.Margin, num, num2), "設定",
                    GuiStyles.ButtonStyle)) //설정
                _isShowSetting = true;

            xPos += num + GuiStyles.Margin;
        }

        // Token: 0x06000036 RID: 54 RVA: 0x000046D8 File Offset: 0x000028D8
        private void nextPattern(Maid maid, MPN mpn, string basename, string filename, bool isPrev = false)
        {
            List<MenuInfo> list = null;
            if (!_mpnMenuListDictionary.TryGetValue(mpn, out list)) return;

            foreach (var menuInfo in list)
                if (!(menuInfo.FileName.ToLower() != basename))
                {
                    if (menuInfo.IsColorLock) break;

                    var variationMenuList = menuInfo.VariationMenuList;
                    if (variationMenuList == null) break;

                    if (variationMenuList.Count < 2) break;

                    for (var i = 0; i < variationMenuList.Count; i++)
                        if (variationMenuList[i].FileName.ToLower() == filename)
                        {
                            var fileName = variationMenuList[0].FileName;
                            if (!isPrev)
                            {
                                if (i < variationMenuList.Count - 1) fileName = variationMenuList[i + 1].FileName;
                            }
                            else if (i == 0)
                            {
                                fileName = variationMenuList[variationMenuList.Count - 1].FileName;
                            }
                            else
                            {
                                fileName = variationMenuList[i - 1].FileName;
                            }

                            maid.SetProp(mpn, fileName, fileName.GetHashCode());
                            maid.AllProcProp();
                            if (UserConfig.Instance.IsOutputInfoLog)
                                Log.LogMessage($"[PropMyItem] change item = {mpn} , {fileName}");

                            return;
                        }
                }
        }

        // Token: 0x06000037 RID: 55 RVA: 0x00004850 File Offset: 0x00002A50
        private void guiSelectedCategory(ref float xPos, float yPos, float windowHeight)
        {
            var num = _folders[_selectedFolder].Categories.Length;
            if (num > 0)
            {
                float width = 7 * GuiStyles.FontSize;
                var num2 = GuiStyles.ControlHeight * 1.5f;
                var viewRect = new Rect(0f, 0f, width, (num2 + GuiStyles.Margin) * num);
                var position = new Rect(xPos, yPos, viewRect.width + GuiStyles.ScrollWidth,
                    windowHeight - yPos - GuiStyles.Margin);
                _categoryScrollPosition = GUI.BeginScrollView(position, _categoryScrollPosition, viewRect);
                for (var i = 0; i < num; i++)
                {
                    var position2 = new Rect(0f, (num2 + GuiStyles.Margin) * i, width, num2);
                    GUI.enabled = _selectedCategory != i;
                    if (GUI.Button(position2, _folders[_selectedFolder].Categories[i], GuiStyles.ButtonStyle))
                    {
                        _selectedPresetList.Clear();
                        _selectedItem = null;
                        _selectedVariationItem = null;
                        _scrollPosition.y = 0f;
                        var selectedMPN = MPN.head;
                        if (_categoryMPNDic.TryGetValue(_folders[_selectedFolder].Categories[i],
                                out selectedMPN))
                        {
                            _selectedMPN = selectedMPN;
                        }
                        else
                        {
                            if (_folders[_selectedFolder].Name == "プリセット")
                            {
                                if (_folders[_selectedFolder].Categories[i] == "\u3000全て\u3000")
                                {
                                    _selectedPresetList = GameMain.Instance.CharacterMgr.PresetListLoad();
                                }
                                else
                                {
                                    var list = GameMain.Instance.CharacterMgr.PresetListLoad();
                                    _selectedPresetType = CharacterMgr.PresetType.All;
                                    if (_folders[_selectedFolder].Categories[i] == "服")
                                        _selectedPresetType = CharacterMgr.PresetType.Wear;
                                    else if (_folders[_selectedFolder].Categories[i] == "身体")
                                        _selectedPresetType = CharacterMgr.PresetType.Body;

                                    foreach (var preset in list)
                                        if (preset.ePreType == _selectedPresetType)
                                            _selectedPresetList.Add(preset);
                                }
                            }

                            _selectedMPN = MPN.null_mpn;
                        }

                        _selectedCategory = i;
                    }

                    GUI.enabled = true;
                }

                GUI.EndScrollView();
                xPos += position.width + GuiStyles.Margin;
            }
        }

        // Token: 0x06000038 RID: 56 RVA: 0x00004AC4 File Offset: 0x00002CC4
        private static int CompareCreateTime(string fileX, string fileY)
        {
            return DateTime.Compare(File.GetLastWriteTime(fileX), File.GetCreationTime(fileY));
        }

        // Token: 0x06000039 RID: 57 RVA: 0x00004AD8 File Offset: 0x00002CD8
        private void guiDebug(ref float xPos, float yPos, float windowHeight)
        {
            var num = 200f;
            if (GUI.Button(new Rect(xPos, yPos, num, GuiStyles.ControlHeight), "プリセット保存", GuiStyles.ButtonStyle))
            {
                var visibleMaidList = GetVisibleMaidList();
                if (_selectedMaid >= 0 && visibleMaidList.Count - 1 >= _selectedMaid)
                {
                    var f_maid = visibleMaidList[_selectedMaid];
                    GameMain.Instance.CharacterMgr.PresetSave(f_maid, CharacterMgr.PresetType.All);
                }
            }

            yPos += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
            xPos += num + GuiStyles.Margin;
        }

        // Token: 0x0600003A RID: 58 RVA: 0x00004B6C File Offset: 0x00002D6C
        private void guiSelectedPreset(ref float xPos, float yPos, float windowHeight)
        {
            var num = 300f;
            var num2 = 4f;
            var num3 = num / num2;
            var num4 = (float)(num3 * 1.5);
            if (_selectedPresetList.Count > 0)
            {
                var count = _selectedPresetList.Count;
                var num5 = count % num2 == 0f ? 0 : 1;
                var viewRect = new Rect(0f, 0f, num, ((int)(count / num2) + num5) * num4);
                var position = new Rect(xPos, yPos, viewRect.width + GuiStyles.ScrollWidth,
                    windowHeight - yPos - GuiStyles.FontSize);
                _scrollPosition = GUI.BeginScrollView(position, _scrollPosition, viewRect);
                new List<int>();
                new Rect(0f, 0f, num3, num4);
                for (var i = 0; i < count; i++)
                {
                    var position2 = new Rect(num3 * (i % num2), num4 * (int)(i / num2), num3,
                        num4);
                    new Rect(num3 * (i % num2), num4 * (int)(i / num2), 20f, 20f);
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (GUI.Button(position2, _selectedPresetList[i].texThum))
                        {
                            var visibleMaidList = GetVisibleMaidList();
                            if (_selectedMaid >= 0 && visibleMaidList.Count - 1 >= _selectedMaid)
                            {
                                var maid = visibleMaidList[_selectedMaid];
                                /*
                                MPN[] array = new MPN[]
                                {
                                    MPN.megane,
                                    MPN.acckami,
                                    MPN.acckamisub,
                                    MPN.hairt,
                                    MPN.headset,
                                    MPN.acchat
                                };
                                this.presetRestoreDic_.Clear();
                                foreach (MPN mpn in array)
                                {
                                    MaidProp prop = maid.GetProp(mpn);
                                    this.presetRestoreDic_.Add(mpn, prop.strFileName);
                                }
                                */
                                if (isAllMaid)
                                    foreach (var item in visibleMaidList)
                                        GameMain.Instance.CharacterMgr.PresetSet(item, _selectedPresetList[i]);
                                else
                                    GameMain.Instance.CharacterMgr.PresetSet(maid, _selectedPresetList[i]);
                            }
                        }
                    }
                    else if (GUI.Button(position2, _selectedPresetList[i].texThum))
                    {
                        var visibleMaidList2 = GetVisibleMaidList();
                        if (_selectedMaid >= 0 && visibleMaidList2.Count - 1 >= _selectedMaid)
                        {
                            var maid2 = visibleMaidList2[_selectedMaid];
                            /*
                            MPN[] array3 = new MPN[]
                            {
                                MPN.megane,
                                MPN.acckami,
                                MPN.acckamisub,
                                MPN.hairt,
                                MPN.headset,
                                MPN.acchat
                            };
                            this.presetRestoreDic_.Clear();
                            foreach (MPN mpn2 in array3)
                            {
                                MaidProp prop2 = maid2.GetProp(mpn2);
                                this.presetRestoreDic_.Add(mpn2, prop2.strFileName);
                            }
                            */
                            if (isAllMaid)
                                foreach (var item in visibleMaidList2)
                                    GameMain.Instance.CharacterMgr.PresetSet(item, _selectedPresetList[i]);
                            else
                                GameMain.Instance.CharacterMgr.PresetSet(maid2, _selectedPresetList[i]);
                        }
                    }
                }

                GUI.EndScrollView();
                xPos += num + GuiStyles.ScrollWidth + GuiStyles.Margin;
            }
        }

        // Token: 0x0600003B RID: 59 RVA: 0x00004E68 File Offset: 0x00003068
        private void guiSelectedItemFilter(float xPos, float yPos)
        {
            var num = xPos;
            var num2 = yPos - GuiStyles.ControlHeight - GuiStyles.Margin;
            string[] array =
            {
                "全て",
                "製品",
                "MOD"
            };
            var num3 = 112;
            if (_folders[_selectedFolder].Name == "全て" || _folders[_selectedFolder].Name == "選択中")
                num += 40f;
            else
                num3 += 40;

            GUI.Label(new Rect(num, num2, GuiStyles.FontSize * 6, GuiStyles.ControlHeight), "フィルタ：",
                GuiStyles.LabelStyle);
            num += GuiStyles.FontSize * 5 + GuiStyles.Margin;
            var position = new Rect(num, num2 - GuiStyles.ControlHeight, GuiStyles.FontSize * 7,
                GuiStyles.ControlHeight);
            _isFavFilter = GUI.Toggle(position, _isFavFilter, "お気に入り", GuiStyles.ToggleStyle);
            var position2 = new Rect(num, num2, GuiStyles.FontSize * 4, GuiStyles.ControlHeight);
            if (GUI.Button(position2, array[_selectedFilter], GuiStyles.ButtonStyle))
            {
                _selectedFilter = _selectedFilter == 2 ? 0 : _selectedFilter + 1;
                _scrollPosition.y = 0f;
            }

            num += GuiStyles.FontSize * 4 + GuiStyles.Margin;
            var position3 = new Rect(num, num2, num3, GuiStyles.ControlHeight);
            _selectedFilterText = GUI.TextField(position3, _selectedFilterText, GuiStyles.TextFieldStyle);
            num += num3 + 4;
            position2.Set(num, num2, GuiStyles.FontSize * 2, GuiStyles.ControlHeight);
            if (GUI.Button(position2, "▽", GuiStyles.ButtonStyle)) _isShowFilterSetting = true;

            num += GuiStyles.FontSize * 2 + GuiStyles.Margin;
            position2.Set(num, num2, GuiStyles.FontSize * 2, GuiStyles.ControlHeight);
            if (GUI.Button(position2, "x", GuiStyles.ButtonStyle)) _selectedFilterText = string.Empty;
        }

        // Token: 0x0600003C RID: 60 RVA: 0x00005074 File Offset: 0x00003274
        private List<MenuInfo> getItemList()
        {
            var list = new List<MenuInfo>();
            if (_folders[_selectedFolder].Name == "全て")
                using (var enumerator =
                       _categoryMPNDic.Values.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var key = enumerator.Current;
                        List<MenuInfo> collection = null;
                        if (_mpnMenuListDictionary.TryGetValue(key, out collection)) list.AddRange(collection);
                    }

                    return list;
                }

            if (_folders[_selectedFolder].Name == "選択中")
            {
                var visibleMaidList = GetVisibleMaidList();
                if (_selectedMaid < 0 || _selectedMaid > visibleMaidList.Count - 1) return list;

                var maid = visibleMaidList[_selectedMaid];
                using (var enumerator2 =
                       _categoryMPNDic.Values.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        var mpn = enumerator2.Current;
                        if (mpn != MPN.set_maidwear && mpn != MPN.set_mywear && mpn != MPN.set_underwear &&
                            mpn != MPN.set_body)
                        {
                            List<MenuInfo> list2 = null;
                            if (_mpnMenuListDictionary.TryGetValue(mpn, out list2))
                            {
                                var selectedMenuFileName = GetSelectedMenuFileName(mpn, maid);
                                var flag = false;
                                foreach (var menuInfo in list2)
                                    if (menuInfo != null)
                                    {
                                        foreach (var menuInfo2 in menuInfo.VariationMenuList)
                                            if (menuInfo2.FileName.IndexOf(selectedMenuFileName,
                                                    StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                if (menuInfo2.IconName != "_I_del.tex") list.Add(menuInfo);

                                                flag = true;
                                                break;
                                            }

                                        if (flag) break;
                                    }
                            }
                        }
                    }

                    return list;
                }
            }

            _mpnMenuListDictionary.TryGetValue(_selectedMPN, out list);
            return list;
        }

        // Token: 0x0600003D RID: 61 RVA: 0x000052C0 File Offset: 0x000034C0
        private void guiSelectedItem(ref float xPos, float yPos, float windowHeight)
        {
            var itemList = getItemList();
            if (itemList == null || itemList.Count == 0) return;

            var num = 8f;
            var num2 = 20f;
            var num3 = 8f;
            var num4 = 3f;
            var num5 = 0f;
            var num6 = 0f;
            var num7 = num6;
            var num8 = itemList.Count;
            var num9 = 0;
            if (_selectedMPN == MPN.set_maidwear || _selectedMPN == MPN.set_mywear ||
                _selectedMPN == MPN.set_underwear || _selectedMPN == MPN.set_body)
            {
                num6 = 75f;
                num7 = num6 * 1.44f;
                num4 = 4f;
                num = 16f;
                num2 = 16f;
                num3 = 10f;
            }
            else
            {
                num6 = 60f;
                num7 = 60f;
                num4 = 5f;
                num = 18f;
                num2 = 24f;
                num3 = 6f;
            }

            num5 = num6 * num4;
            var havePartsItems = GameMain.Instance.CharacterMgr.status.havePartsItems;
            for (var i = 0; i < num8; i++)
            {
                var menuInfo = itemList[i];
                menuInfo.IsHave = true;
                if (menuInfo.IsShopTarget && havePartsItems.ContainsKey(menuInfo.FileName) &&
                    !havePartsItems[menuInfo.FileName])
                {
                    num9++;
                    menuInfo.IsHave = false;
                }
                else
                {
                    if (_selectedFilter == 1)
                    {
                        if (menuInfo.IsMod)
                        {
                            num9++;
                            menuInfo.IsHave = false;
                            goto IL_1DC;
                        }
                    }
                    else if (_selectedFilter == 2 && !menuInfo.IsMod)
                    {
                        num9++;
                        menuInfo.IsHave = false;
                        goto IL_1DC;
                    }

                    if (_isFavFilter && !menuInfo.IsFavorite)
                    {
                        num9++;
                        menuInfo.IsHave = false;
                    }
                    else if (!string.IsNullOrEmpty(_selectedFilterText) &&
                             menuInfo.ItemName.IndexOf(_selectedFilterText, StringComparison.OrdinalIgnoreCase) ==
                             -1 && menuInfo.FilePath.IndexOf(_selectedFilterText,
                                 StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        num9++;
                        menuInfo.IsHave = false;
                    }
                }

                IL_1DC: ;
            }

            num8 -= num9;
            var num10 = num8 % num4 == 0f ? 0 : 1;
            var num11 = (int)(windowHeight - yPos - GuiStyles.FontSize - GuiStyles.ControlHeight);
            var viewRect = new Rect(0f, 0f, num6 * num4, ((int)(num8 / num4) + num10) * num7);
            if (_folders[_selectedFolder].Name == "選択中")
                viewRect = new Rect(0f, 0f, num6 * num4, ((int)((num8 + 5) / num4) + num10) * num7);

            var position = new Rect(xPos, yPos + GuiStyles.ControlHeight, viewRect.width + GuiStyles.ScrollWidth,
                num11);
            _scrollPosition = GUI.BeginScrollView(position, _scrollPosition, viewRect);
            new List<int>();
            new Rect(0f, 0f, num6, num7);
            var guistyle = new GUIStyle();
            guistyle.alignment = TextAnchor.UpperRight;
            guistyle.fontSize = 10;
            guistyle.normal = new GUIStyleState
            {
                textColor = Color.black
            };
            var guistyle2 = new GUIStyle("button");
            guistyle2.fontSize = 14;
            guistyle2.alignment = TextAnchor.UpperRight;
            guistyle2.normal.textColor = Color.white;
            guistyle2.hover.textColor = Color.white;
            guistyle2.padding = new RectOffset(0, 3, 1, 0);
            var guistyle3 = new GUIStyle("button");
            guistyle3.fontSize = 14;
            guistyle3.alignment = TextAnchor.UpperRight;
            guistyle3.normal.textColor = Color.yellow;
            guistyle3.hover.textColor = Color.yellow;
            guistyle3.padding = new RectOffset(0, 3, 1, 0);
            var guistyle4 = new GUIStyle("button");
            guistyle4.fontSize = 14;
            guistyle4.alignment = TextAnchor.UpperRight;
            guistyle4.normal.textColor = Color.white;
            guistyle4.hover.textColor = Color.white;
            guistyle4.padding = new RectOffset(0, 6, 1, 0);
            var guistyle5 = new GUIStyle("button");
            guistyle5.fontSize = 14;
            guistyle5.alignment = TextAnchor.UpperRight;
            guistyle5.normal.textColor = Color.red;
            guistyle5.hover.textColor = Color.red;
            guistyle5.padding = new RectOffset(0, 6, 1, 0);
            Maid maid = null;
            var text = string.Empty;
            var visibleMaidList = GetVisibleMaidList();
            if (_selectedMaid >= 0 && visibleMaidList.Count - 1 >= _selectedMaid)
            {
                maid = visibleMaidList[_selectedMaid];
                text = GetSelectedMenuFileName(_selectedMPN, maid);
            }

            var y = _scrollPosition.y;
            var num12 = _scrollPosition.y + num11;
            var num13 = 0;
            for (var j = 0; j < itemList.Count; j++)
            {
                var menuInfo2 = itemList[j];
                if (menuInfo2.IsHave)
                {
                    var enabled = true;
                    if (!string.IsNullOrEmpty(text))
                    {
                        if (menuInfo2.FileName.IndexOf(text, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            enabled = false;
                            _selectedItem = menuInfo2;
                            _selectedVariationItem = menuInfo2.VariationMenuList[0];
                        }
                        else if (menuInfo2.VariationMenuList.Count > 1)
                        {
                            foreach (var menuInfo3 in menuInfo2.VariationMenuList)
                                if (menuInfo3.FileName.IndexOf(text, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    _selectedItem = menuInfo2;
                                    _selectedVariationItem = menuInfo3;
                                    enabled = false;
                                    break;
                                }
                        }
                    }

                    var num14 = num7 * (int)(num13 / num4);
                    var num15 = num14 + num7;
                    if ((y <= num14 && num15 <= num12) || (num14 <= y && y <= num15) || (y <= num14 && num14 <= num12))
                    {
                        if (menuInfo2.Icon == null && !string.IsNullOrEmpty(menuInfo2.IconName) && !menuInfo2.IsError)
                        {
                            if (!menuInfo2.IsOfficialMOD)
                                try
                                {
                                    menuInfo2.Icon = ImportCM.CreateTexture(menuInfo2.IconName);
                                    goto IL_632;
                                }
                                catch (Exception e)
                                {
                                    //menuInfo2.IsError = true;
                                    //goto IL_D64;
                                    Log.LogMessage(e.ToString());
                                }

                            var menuInfo4 = MenuModParser.parseMod(menuInfo2.FilePath);
                            menuInfo2.Icon = menuInfo4.Icon;
                        }

                        IL_632:
                        var tooltip = menuInfo2.ItemName;
                        if (_folders[_selectedFolder].Name == "全て" ||
                            _folders[_selectedFolder].Name == "選択中")
                            tooltip = menuInfo2.CategoryName + "：" + menuInfo2.ItemName;

                        var position2 = new Rect(num6 * (num13 % num4),
                            num7 * (int)(num13 / num4), num6, num7);
                        var position3 = new Rect(position2.x, position2.y + position2.height - 20f, 20f, 20f);
                        var position4 = new Rect(position2.x, position2.y, 20f, 20f);
                        var position5 = new Rect(position2.x + position2.width - 20f,
                            position2.y + position2.height - 20f, 20f, 20f);
                        if (Event.current.type == EventType.Repaint)
                        {
                            GUI.enabled = enabled;


                            if (GUI.Button(position2, new GUIContent(menuInfo2.Icon, tooltip)))
                            {
                                _selectedItem = menuInfo2;
                                _selectedVariationItem = menuInfo2.VariationMenuList[0];
                                var visibleMaidList2 = GetVisibleMaidList();
                                if (_selectedMaid >= 0 && visibleMaidList2.Count - 1 >= _selectedMaid)
                                {
                                    if (UserConfig.Instance.IsOutputInfoLog)
                                        Log.LogMessage("[PropMyItem] change item = " + menuInfo2.FileName);

                                    if (isAllMaid)
                                        foreach (var item in visibleMaidList2)
                                            SetItem(menuInfo2, item);
                                    else
                                        SetItem(menuInfo2, visibleMaidList2[_selectedMaid]);
                                }
                            }


                            GUI.enabled = true;
                            var style = guistyle2;
                            if (menuInfo2.IsFavorite) style = guistyle3;

                            if (GUI.Button(position3, new GUIContent("★", tooltip), style))
                            {
                                if (menuInfo2.IsFavorite)
                                {
                                    menuInfo2.IsFavorite = false;
                                    UserConfig.Instance.FavList.Remove(menuInfo2.FileName.ToLower());
                                }
                                else
                                {
                                    menuInfo2.IsFavorite = true;
                                    UserConfig.Instance.FavList.Add(menuInfo2.FileName.ToLower());
                                }

                                UserConfig.Instance.Save();
                            }

                            if (_folders[_selectedFolder].Name == "選択中")
                            {
                                if (GUI.Button(position4, new GUIContent("×", tooltip)))
                                {
                                    if (isAllMaid)
                                    {
                                        //List<Maid> visibleMaidList = CommonUtil.GetVisibleMaidList();
                                        foreach (var item in visibleMaidList)
                                        {
                                            item.DelProp(menuInfo2.MPN);
                                            item.AllProcProp();
                                        }
                                    }
                                    else
                                    {
                                        maid.DelProp(menuInfo2.MPN);
                                        maid.AllProcProp();
                                    }
                                }

                                var style2 = guistyle4;
                                if (menuInfo2.IsColorLock) style2 = guistyle5;

                                if (GUI.Button(position5, new GUIContent("■", tooltip), style2))
                                {
                                    if (menuInfo2.IsColorLock)
                                    {
                                        menuInfo2.IsColorLock = false;
                                        UserConfig.Instance.ColorLockList.Remove(menuInfo2.FileName.ToLower());
                                    }
                                    else
                                    {
                                        menuInfo2.IsColorLock = true;
                                        UserConfig.Instance.ColorLockList.Add(menuInfo2.FileName.ToLower());
                                    }

                                    UserConfig.Instance.Save();
                                }
                            }
                        }
                        else
                        {
                            if (_folders[_selectedFolder].Name == "選択中")
                            {
                                if (GUI.Button(position4, new GUIContent("×", tooltip)))
                                {
                                    if (isAllMaid)
                                    {
                                        foreach (var item in visibleMaidList)
                                        {
                                            item.DelProp(menuInfo2.MPN);
                                            item.AllProcProp();
                                        }
                                    }
                                    else
                                    {
                                        maid.DelProp(menuInfo2.MPN);
                                        maid.AllProcProp();
                                    }
                                }

                                var style3 = guistyle4;
                                if (menuInfo2.IsColorLock) style3 = guistyle5;

                                if (GUI.Button(position5, new GUIContent("■", tooltip), style3))
                                {
                                    if (menuInfo2.IsColorLock)
                                    {
                                        menuInfo2.IsColorLock = false;
                                        UserConfig.Instance.ColorLockList.Remove(menuInfo2.FileName.ToLower());
                                    }
                                    else
                                    {
                                        menuInfo2.IsColorLock = true;
                                        UserConfig.Instance.ColorLockList.Add(menuInfo2.FileName.ToLower());
                                    }

                                    UserConfig.Instance.Save();
                                }
                            }

                            var style4 = guistyle2;
                            if (menuInfo2.IsFavorite) style4 = guistyle3;

                            if (GUI.Button(position3, new GUIContent("★", tooltip), style4))
                            {
                                if (menuInfo2.IsFavorite)
                                {
                                    menuInfo2.IsFavorite = false;
                                    UserConfig.Instance.FavList.Remove(menuInfo2.FileName.ToLower());
                                }
                                else
                                {
                                    menuInfo2.IsFavorite = true;
                                    UserConfig.Instance.FavList.Add(menuInfo2.FileName.ToLower());
                                }

                                UserConfig.Instance.Save();
                            }

                            GUI.enabled = enabled;
                            if (GUI.Button(position2, new GUIContent(menuInfo2.Icon, tooltip)))
                            {
                                _selectedItem = menuInfo2;
                                _selectedVariationItem = menuInfo2.VariationMenuList[0];
                                var visibleMaidList3 = GetVisibleMaidList();
                                if (_selectedMaid >= 0 && visibleMaidList3.Count - 1 >= _selectedMaid)
                                {
                                    if (UserConfig.Instance.IsOutputInfoLog)
                                        Log.LogMessage("[PropMyItem] change item = " + menuInfo2.FileName);

                                    if (isAllMaid)
                                        foreach (var maid1 in visibleMaidList3)
                                            maid1.SetProp(menuInfo2.MPN, menuInfo2.FileName,
                                                Path.GetFileName(menuInfo2.FileName).GetHashCode());
                                    else
                                        visibleMaidList3[_selectedMaid].SetProp(menuInfo2.MPN, menuInfo2.FileName,
                                            Path.GetFileName(menuInfo2.FileName).GetHashCode());


                                    if ((menuInfo2.MPN == MPN.folder_futae || menuInfo2.MPN == MPN.folder_matsuge_low ||
                                         menuInfo2.MPN == MPN.folder_matsuge_up || menuInfo2.MPN == MPN.folder_eye ||
                                         menuInfo2.MPN == MPN.folder_mayu || menuInfo2.MPN == MPN.folder_skin ||
                                         menuInfo2.MPN == MPN.folder_underhair || menuInfo2.MPN == MPN.chikubi) &&
                                        menuInfo2.ColorSetMenuList.Count > 0)
                                    {
                                        var menuInfo6 = _selectedVariationItem.ColorSetMenuList[0];
                                        if (isAllMaid)
                                            foreach (var maid1 in visibleMaidList)
                                                maid1.SetProp(menuInfo2.ColorSetMPN, menuInfo6.FileName,
                                                    Path.GetFileName(menuInfo6.FileName).GetHashCode());
                                        else
                                            visibleMaidList3[_selectedMaid].SetProp(menuInfo2.ColorSetMPN,
                                                menuInfo6.FileName, Path.GetFileName(menuInfo6.FileName).GetHashCode());
                                    }

                                    if (isAllMaid)
                                        foreach (var maid1 in visibleMaidList)
                                            maid1.AllProcProp();
                                    else
                                        visibleMaidList3[_selectedMaid].AllProcProp();
                                }
                            }

                            GUI.enabled = true;
                        }

                        var count = menuInfo2.VariationMenuList.Count;
                        if (count > 1)
                        {
                            var position6 = new Rect(position2.x + position2.width - num, position2.y + num3, 10f,
                                10f);
                            if (menuInfo2.MPN == MPN.set_maidwear || menuInfo2.MPN == MPN.set_mywear ||
                                menuInfo2.MPN == MPN.set_underwear || menuInfo2.MPN == MPN.set_body)
                                position6 = new Rect(position2.x + position2.width - num2, position2.y + num3, 10f,
                                    10f);

                            GUI.Label(position6, count.ToString(), guistyle);
                        }
                    }

                    num13++;
                }
                //IL_D64:;
            }

            if (_folders[_selectedFolder].Name == "選択中")
            {
                var num16 = num7 * (int)(num13 / num4);
                num16 = num13 % num4 == 0f ? num16 : num16 + num7;
                var position7 = new Rect(0f, num16, num6 * 2f, num7);
                var position8 = new Rect(num6 * 3f, num16, num6 * 2f, num7);
                MPN[] array =
                {
                    MPN.acchat,
                    MPN.headset,
                    MPN.wear,
                    MPN.skirt,
                    MPN.onepiece,
                    MPN.mizugi,
                    MPN.bra,
                    MPN.panz,
                    MPN.stkg,
                    MPN.shoes,
                    MPN.acckami,
                    MPN.megane,
                    MPN.acchead,
                    MPN.acchana,
                    MPN.accmimi,
                    MPN.glove,
                    MPN.acckubi,
                    MPN.acckubiwa,
                    MPN.acckamisub,
                    MPN.accnip,
                    MPN.accude,
                    MPN.accheso,
                    MPN.accashi,
                    MPN.accsenaka,
                    MPN.accshippo,
                    MPN.accxxx
                };
                if (GUI.Button(position7, "カラバリ変更(前)", GuiStyles.ButtonStyle) && maid != null)
                    foreach (var mpn in array)
                    {
                        var prop = maid.GetProp(mpn);
                        var text2 = prop.strFileName.ToLower();
                        var basename = prop.strFileName.ToLower();
                        if (Regex.IsMatch(text2, "_z\\d{1,4}"))
                        {
                            var match = Regex.Match(text2, "_z\\d{1,4}");
                            basename = text2.Replace(match.Value, "");
                        }

                        nextPattern(maid, mpn, basename, text2, true);
                    }

                if (GUI.Button(position8, "カラバリ変更(後)", GuiStyles.ButtonStyle) && maid != null)
                    foreach (var mpn2 in array)
                        try
                        {
                            var prop2 = maid.GetProp(mpn2);
                            var text3 = prop2.strFileName.ToLower();
                            var basename2 = prop2.strFileName.ToLower();
                            if (Regex.IsMatch(text3, "_z\\d{1,4}"))
                            {
                                var match2 = Regex.Match(text3, "_z\\d{1,4}");
                                basename2 = text3.Replace(match2.Value, "");
                            }

                            nextPattern(maid, mpn2, basename2, text3);
                        }
                        catch (Exception ex)
                        {
                            Log.LogMessage("" + ex);
                        }
            }

            GUI.EndScrollView();
            GuiStyles.LabelStyle.alignment = TextAnchor.UpperLeft;
            var position9 = new Rect(xPos, yPos, num6 * num4 + GuiStyles.ScrollWidth, GuiStyles.ControlHeight);
            xPos += num5 + GuiStyles.ScrollWidth + 8f;
            GUI.Label(position9, GUI.tooltip, GuiStyles.LabelStyle);
            GuiStyles.LabelStyle.alignment = TextAnchor.MiddleCenter;
            if (_selectedItem != null && _selectedItem.VariationMenuList.Count > 1)
                guiSelectedVariation(ref xPos, yPos, _selectedItem, num6, num7, windowHeight, text);
        }

        private void SetItem(MenuInfo menuInfo2, Maid visibleMaidList2)
        {
            visibleMaidList2.SetProp(menuInfo2.MPN, menuInfo2.FileName,
                Path.GetFileName(menuInfo2.FileName).GetHashCode());
            if ((menuInfo2.MPN == MPN.folder_futae || menuInfo2.MPN == MPN.folder_matsuge_low ||
                 menuInfo2.MPN == MPN.folder_matsuge_up || menuInfo2.MPN == MPN.folder_eye ||
                 menuInfo2.MPN == MPN.folder_mayu || menuInfo2.MPN == MPN.folder_skin ||
                 menuInfo2.MPN == MPN.folder_underhair || menuInfo2.MPN == MPN.chikubi) &&
                menuInfo2.ColorSetMenuList.Count > 0)
            {
                var menuInfo5 = _selectedVariationItem.ColorSetMenuList[0];
                visibleMaidList2.SetProp(menuInfo2.ColorSetMPN, menuInfo5.FileName,
                    Path.GetFileName(menuInfo5.FileName).GetHashCode());
            }

            visibleMaidList2.AllProcProp();
        }

        // Token: 0x0600003E RID: 62 RVA: 0x00006364 File Offset: 0x00004564
        private void guiSelectedVariation(ref float posX, float posY, MenuInfo itemMenuInfo, float iconWidth,
            float iconHeight, float windowHeight, string selectedFileName)
        {
            var count = itemMenuInfo.VariationMenuList.Count;
            var viewRect = new Rect(0f, 0f, iconWidth, count * (iconWidth + 4f));
            var position = new Rect(posX, posY + GuiStyles.ControlHeight, viewRect.width + GuiStyles.ScrollWidth,
                windowHeight - posY - GuiStyles.FontSize - GuiStyles.ControlHeight);
            _colorItemScrollPosition = GUI.BeginScrollView(position, _colorItemScrollPosition, viewRect);
            new Rect(0f, 0f, iconWidth, iconWidth);
            var i = 0;
            while (i < count)
            {
                var menuInfo = itemMenuInfo.VariationMenuList[i];
                if (menuInfo.Icon == null && !string.IsNullOrEmpty(menuInfo.IconName) && !menuInfo.IsError)
                {
                    if (!menuInfo.IsOfficialMOD)
                        try
                        {
                            menuInfo.Icon = ImportCM.CreateTexture(menuInfo.IconName);
                            goto IL_10A;
                        }
                        catch (Exception e)
                        {
                            Log.LogMessage("" + e);
                        }

                    var menuInfo2 = MenuModParser.parseMod(menuInfo.FilePath);
                    menuInfo.Icon = menuInfo2.Icon;
                }

                goto IL_10A;
                IL_101:
                i++;
                continue;
                IL_10A:
                var tooltip = menuInfo.ItemName;
                if (_folders[_selectedFolder].Name == "全て" ||
                    _folders[_selectedFolder].Name == "選択中")
                    tooltip = menuInfo.CategoryName + "：" + menuInfo.ItemName;

                if (!string.IsNullOrEmpty(selectedFileName) &&
                    menuInfo.FileName.IndexOf(selectedFileName, StringComparison.OrdinalIgnoreCase) == 0)
                    GUI.enabled = false;

                if (GUI.Button(new Rect(0f, (iconWidth + 4f) * i, iconWidth, iconWidth),
                        new GUIContent(menuInfo.Icon, tooltip)))
                {
                    _selectedVariationItem = menuInfo;
                    var visibleMaidList = GetVisibleMaidList();
                    if (_selectedMaid >= 0 && visibleMaidList.Count - 1 >= _selectedMaid)
                    {
                        if (isAllMaid)
                        {
                            foreach (var maid1 in visibleMaidList)
                            {
                                maid1.SetProp(menuInfo.MPN, menuInfo.FileName,
                                    Path.GetFileName(menuInfo.FileName).GetHashCode());
                                maid1.AllProcProp();
                            }
                        }
                        else
                        {
                            visibleMaidList[_selectedMaid].SetProp(menuInfo.MPN, menuInfo.FileName,
                                Path.GetFileName(menuInfo.FileName).GetHashCode());
                            visibleMaidList[_selectedMaid].AllProcProp();
                        }


                        if (UserConfig.Instance.IsOutputInfoLog)
                            Log.LogMessage("[PropMyItem] change item = " + menuInfo.FileName);
                    }
                }

                // Display item name label next to the button
                GUILayout.BeginVertical();
                GUILayout.Label(menuInfo.ItemName, GuiStyles.LabelStyle);
                GUILayout.EndVertical();


                GUI.enabled = true;
                goto IL_101;
            }

            GUI.EndScrollView();
            posX += iconWidth + GuiStyles.ScrollWidth + 8f;
        }

        // Token: 0x0600003F RID: 63 RVA: 0x00006600 File Offset: 0x00004800
        private void guiSelectedColorSet(ref float posX, ref float posY)
        {
            if (_selectedVariationItem != null && _selectedVariationItem.ColorSetMenuList.Count > 0)
            {
                var value = string.Empty;
                var visibleMaidList = GetVisibleMaidList();
                if (_selectedMaid >= 0 && visibleMaidList.Count - 1 >= _selectedMaid)
                {
                    var maid = visibleMaidList[_selectedMaid];
                    value = GetSelectedMenuFileName(_selectedVariationItem.ColorSetMPN, maid);
                }

                float num = 420 / _selectedVariationItem.ColorSetMenuList.Count;
                num = num > 28f ? 28f : num;
                var i = 0;
                while (i < _selectedVariationItem.ColorSetMenuList.Count)
                {
                    var menuInfo = _selectedVariationItem.ColorSetMenuList[i];
                    if (menuInfo.Icon == null && !string.IsNullOrEmpty(menuInfo.IconName) && !menuInfo.IsError)
                    {
                        if (!menuInfo.IsOfficialMOD)
                            try
                            {
                                menuInfo.Icon = ImportCM.CreateTexture(menuInfo.IconName);
                                goto IL_125;
                            }
                            catch (Exception e)
                            {
                                Log.LogMessage("" + e);
                            }

                        var menuInfo2 = MenuModParser.parseMod(menuInfo.FilePath);
                        menuInfo.Icon = menuInfo2.Icon;
                    }

                    goto IL_125;
                    IL_11C:
                    i++;
                    continue;
                    IL_125:
                    if (!string.IsNullOrEmpty(value) &&
                        menuInfo.FileName.IndexOf(value, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (menuInfo.FileName.IndexOf("mugen", StringComparison.OrdinalIgnoreCase) != -1)
                            _isFreeColor = true;
                        else
                            _isFreeColor = false;

                        GUI.enabled = false;
                    }

                    if (GUI.Button(new Rect(posX, posY + 10f + (num + 1f) * i, num, num), menuInfo.Icon))
                    {
                        if (menuInfo.FileName.IndexOf("mugen", StringComparison.OrdinalIgnoreCase) != -1)
                            _isFreeColor = true;
                        else
                            _isFreeColor = false;

                        var visibleMaidList2 = GetVisibleMaidList();
                        if (_selectedMaid >= 0 && visibleMaidList2.Count - 1 >= _selectedMaid)
                        {
                            visibleMaidList2[_selectedMaid].SetProp(menuInfo.MPN, menuInfo.FileName,
                                Path.GetFileName(menuInfo.FileName).GetHashCode());
                            visibleMaidList2[_selectedMaid].AllProcProp();
                        }
                    }


                    // Display item name label next to the button
                    GUILayout.BeginVertical();
                    GUILayout.Label(menuInfo.ItemName, GuiStyles.LabelStyle);
                    GUILayout.EndVertical();


                    GUI.enabled = true;
                    goto IL_11C;
                }

                posX += num + 8f;
                return;
            }

            _isFreeColor = false;
        }

        // Token: 0x06000040 RID: 64 RVA: 0x00006874 File Offset: 0x00004A74
        private void guiSelectedMugenColor(ref float posX, ref float posY)
        {
            if (_isFreeColor)
            {
                var fontSize = GuiStyles.FontSize;
                var controlHeight = GuiStyles.ControlHeight;
                var buttonStyle = GuiStyles.ButtonStyle;
                var labelStyle = GuiStyles.LabelStyle;
                var visibleMaidList = GetVisibleMaidList();
                if (_selectedMaid >= 0 && visibleMaidList.Count - 1 >= _selectedMaid)
                {
                    var num = posY;
                    var maid = visibleMaidList[_selectedMaid];
                    var parts_COLOR = MaidParts.PARTS_COLOR.SKIN;
                    var mpn = _selectedItem.MPN;
                    switch (mpn)
                    {
                        case MPN.folder_matsuge_up:
                            parts_COLOR = MaidParts.PARTS_COLOR.MATSUGE_UP;
                            goto IL_189;

                        case MPN.folder_matsuge_low:
                            parts_COLOR = MaidParts.PARTS_COLOR.MATSUGE_LOW;
                            goto IL_189;

                        case MPN.folder_futae:
                            parts_COLOR = MaidParts.PARTS_COLOR.FUTAE;
                            goto IL_189;

                        case MPN.hairf:
                        case MPN.hairr:
                        case MPN.hairt:
                        case MPN.hairs:
                        case MPN.hairaho:
                        case MPN.haircolor:
                            parts_COLOR = MaidParts.PARTS_COLOR.HAIR;
                            goto IL_189;
                        case MPN.skin:
                            break;
                        case MPN.acctatoo:
                        case MPN.accnail:
                        case MPN.hokuro:
                        case MPN.lip:
                        case MPN.eye_hi:
                        case MPN.eye_hi_r:
                            goto IL_189;
                        case MPN.underhair:
                            goto IL_F8;
                        case MPN.mayu:
                            goto IL_100;
                        case MPN.eye:
                            goto IL_108;
                        case MPN.chikubi:
                        case MPN.chikubicolor:
                            parts_COLOR = MaidParts.PARTS_COLOR.NIPPLE;
                            goto IL_189;
                        default:
                            switch (mpn)
                            {
                                case MPN.folder_eye:
                                    goto IL_108;
                                case MPN.folder_mayu:
                                    goto IL_100;
                                case MPN.folder_underhair:
                                    goto IL_F8;
                                case MPN.folder_skin:
                                    break;
                                default:
                                    goto IL_189;
                            }

                            break;
                    }

                    parts_COLOR = MaidParts.PARTS_COLOR.SKIN;
                    goto IL_189;
                    IL_F8:
                    parts_COLOR = MaidParts.PARTS_COLOR.UNDER_HAIR;
                    goto IL_189;
                    IL_100:
                    parts_COLOR = MaidParts.PARTS_COLOR.EYE_BROW;
                    goto IL_189;
                    IL_108:
                    var text = string.Empty;
                    if (_selectedEyeClorType == 0)
                    {
                        parts_COLOR = MaidParts.PARTS_COLOR.EYE_L;
                        text = "両目";
                    }
                    else if (_selectedEyeClorType == 1)
                    {
                        parts_COLOR = MaidParts.PARTS_COLOR.EYE_L;
                        text = "左目";
                    }
                    else if (_selectedEyeClorType == 2)
                    {
                        parts_COLOR = MaidParts.PARTS_COLOR.EYE_R;
                        text = "右目";
                    }

                    if (GUI.Button(new Rect(posX, posY, fontSize * 8, controlHeight), text, buttonStyle))
                        _selectedEyeClorType =
                            _selectedEyeClorType == 2 ? 0 : _selectedEyeClorType + 1;

                    num = controlHeight + 8f + posY;
                    IL_189:
                    var partsColor = maid.Parts.GetPartsColor(parts_COLOR);
                    string[] array =
                    {
                        "色相\u3000",
                        "彩度\u3000",
                        "明度\u3000",
                        "対称\u3000",
                        "影率\u3000",
                        "影色 色相",
                        "影色 彩度",
                        "影色 明度",
                        "影色 対称"
                    };
                    int[] array2 =
                    {
                        partsColor.m_nMainHue,
                        partsColor.m_nMainChroma,
                        partsColor.m_nMainBrightness,
                        partsColor.m_nMainContrast,
                        partsColor.m_nShadowRate,
                        partsColor.m_nShadowHue,
                        partsColor.m_nShadowChroma,
                        partsColor.m_nShadowBrightness,
                        partsColor.m_nShadowContrast
                    };
                    int[] array3 =
                    {
                        255,
                        255,
                        510,
                        200,
                        255,
                        255,
                        255,
                        510,
                        200
                    };
                    var num2 = controlHeight * 0.8f;
                    for (var i = 0; i < array.Length; i++)
                    {
                        var num3 = num + i * (num2 * 2f + 8f);
                        var position = new Rect(posX, num3, fontSize * array[i].Length, num2);
                        var position2 = new Rect(posX + (fontSize * array[i].Length + 4), num3,
                            fontSize * 4, num2);
                        var position3 = new Rect(posX, num3 + num2, fontSize * 2, num2);
                        var position4 = new Rect(posX + fontSize * 2 + 4f,
                            num3 + num2 + (float)(num2 * 0.25), 80f, num2);
                        var position5 = new Rect(posX + 80f + fontSize * 2 + 8f, num3 + num2,
                            fontSize * 2, num2);
                        GUI.Label(position, array[i], labelStyle);
                        GUI.Label(position2, array2[i].ToString(), labelStyle);
                        float num4 =
                            (int)GUI.HorizontalSlider(position4, array2[i], 0f, array3[i]);
                        if (num4 != array2[i])
                        {
                            array2[i] = (int)num4;
                            changeColor(partsColor, parts_COLOR, array2, maid);
                        }

                        if (GUI.Button(position3, "-", buttonStyle))
                        {
                            var num5 = array2[i] - 1;
                            num5 = num5 < 0 ? 0 : num5;
                            array2[i] = num5;
                            changeColor(partsColor, parts_COLOR, array2, maid);
                        }

                        if (GUI.Button(position5, "+", buttonStyle))
                        {
                            var num6 = array2[i] + 1;
                            num6 = num6 > array3[i] ? array3[i] : num6;
                            array2[i] = num6;
                            changeColor(partsColor, parts_COLOR, array2, maid);
                        }
                    }

                    posX += 80 + fontSize * 2 + 8 + fontSize * 2 + 8;
                }
            }
        }

        // Token: 0x06000041 RID: 65 RVA: 0x00006CA4 File Offset: 0x00004EA4
        private void changeColor(MaidParts.PartsColor partsColor, MaidParts.PARTS_COLOR partsColorType, int[] values,
            Maid maid)
        {
            partsColor.m_nMainHue = values[0];
            partsColor.m_nMainChroma = values[1];
            partsColor.m_nMainBrightness = values[2];
            partsColor.m_nMainContrast = values[3];
            partsColor.m_nShadowRate = values[4];
            partsColor.m_nShadowHue = values[5];
            partsColor.m_nShadowChroma = values[6];
            partsColor.m_nShadowBrightness = values[7];
            partsColor.m_nShadowContrast = values[8];
            maid.Parts.SetPartsColor(partsColorType, partsColor);
            if (partsColorType == MaidParts.PARTS_COLOR.EYE_L && _selectedEyeClorType == 0)
            {
                partsColor.m_nMainHue = values[0];
                partsColor.m_nMainChroma = values[1];
                partsColor.m_nMainBrightness = values[2];
                partsColor.m_nMainContrast = values[3];
                partsColor.m_nShadowRate = values[4];
                partsColor.m_nShadowHue = values[5];
                partsColor.m_nShadowChroma = values[6];
                partsColor.m_nShadowBrightness = values[7];
                partsColor.m_nShadowContrast = values[8];
                maid.Parts.SetPartsColor(MaidParts.PARTS_COLOR.EYE_R, partsColor);
            }
        }

        // Token: 0x06000042 RID: 66 RVA: 0x00006D8C File Offset: 0x00004F8C
        public void LoadMenuFiles(bool isInit = false)
        {
            Log.LogMessage("[PropMyItem] LoadMenuFiles...st");
            try
            {
                var
                    menuItems =
                        new List<SMenuInfo>(); //COM3D2.PropMyItem.Plugin.Config.Instance.MenuItems;//new List<SMenuInfo>();
                var dictionary = new Dictionary<string, MenuInfo>();
                if (!isInit)
                {
                    using (var enumerator =
                           Plugin.Config.Instance.MenuItems.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var smenuInfo = enumerator.Current;
                            if (!dictionary.ContainsKey(smenuInfo.FileName))
                                dictionary.Add(smenuInfo.FileName, new MenuInfo(smenuInfo));
                        }
                        //	goto IL_CA;
                    }
                }
                else
                {
                    _mpnMenuListDictionary = new Dictionary<MPN, List<MenuInfo>>();
                    foreach (var obj in Enum.GetValues(typeof(MPN)))
                    {
                        var key = (MPN)obj;
                        _mpnMenuListDictionary.Add(key, new List<MenuInfo>());
                    }
                }

                //IL_CA:
                if (dictionary.Count == 0) Log.LogMessage("[PropMyItem] 準備中...");

                var dictionary2 = new Dictionary<string, string>();
                foreach (var text in UserConfig.Instance.FavList)
                    if (!dictionary2.ContainsKey(text))
                        dictionary2.Add(text.ToLower(), text);

                var dictionary3 = new Dictionary<string, string>();
                foreach (var text2 in UserConfig.Instance.ColorLockList)
                    if (!dictionary3.ContainsKey(text2))
                        dictionary3.Add(text2.ToLower(), text2);

                var list = new List<MenuInfo>();
                Log.LogMessage("[PropMyItem] 完了1 " + menuItems.Count);
                GetMainMenuFiles(ref list, dictionary, dictionary2, dictionary3, ref menuItems);
                Log.LogMessage("[PropMyItem] 完了2 " + menuItems.Count);
                GetModFiles(ref list, dictionary, dictionary2, dictionary3, ref menuItems); // 여기서 에러남
                Log.LogMessage("[PropMyItem] 完了3 " + menuItems.Count);
                SetVariationMenu(dictionary2, dictionary3, ref list);
                sort(false, true);
                setColorSet();
                Plugin.Config.Instance.MenuItems = menuItems;
                Plugin.Config.Instance.Save();
                if (dictionary.Count == 0) Log.LogMessage("[PropMyItem] 完了");

                _selectedFolder = 0;
                _selectedMPN = MPN.null_mpn;
                _selectedCategory = -1;
                _selectedItem = null;
                _selectedVariationItem = null;
                _selectedPresetList.Clear();
                _selectedItem = null;
                _selectedVariationItem = null;
                _scrollPosition.y = 0f;
                var selectedMPN = MPN.head;
                if (_categoryMPNDic.TryGetValue(_folders[_selectedFolder].Categories[0],
                        out selectedMPN))
                    _selectedMPN = selectedMPN;

                _selectedCategory = 0;
            }
            catch (Exception value)
            {
                Log.LogFatal(value);
            }

            _isLoading = false;
            _isForcedInit = false;
            Log.LogMessage("[PropMyItem] LoadMenuFiles...ed " +
                           Plugin.Config.Instance.MenuItems.Count);
            Log.LogMessage("[PropMyItem] LoadMenuFiles...ed " + _mpnMenuListDictionary.Count);
        }

        // Token: 0x06000043 RID: 67 RVA: 0x00007088 File Offset: 0x00005288
        private void sort(bool isFilePath, bool isColorNumber)
        {
            Comparison<MenuInfo> comparator = (a, b) =>
            {
                if (isFilePath)
                {
                    if (a.IsMod && !b.IsMod) return 1;

                    if (!a.IsMod && b.IsMod) return -1;

                    if (a.IsMod && b.IsMod) return string.Compare(a.FilePath, b.FilePath);
                }

                if ((int)a.Priority != (int)b.Priority) return (int)a.Priority - (int)b.Priority;

                return string.Compare(a.ItemName, b.ItemName);
            };
            foreach (var key in _mpnMenuListDictionary.Keys)
            {
                _mpnMenuListDictionary[key].Sort(comparator);
                if (isColorNumber)
                    foreach (var menuInfo in _mpnMenuListDictionary[key])
                        if (menuInfo.VariationMenuList.Count > 1)
                            menuInfo.VariationMenuList.Sort(delegate(MenuInfo a, MenuInfo b)
                            {
                                if (a.ColorNumber != b.ColorNumber) return a.ColorNumber - b.ColorNumber;

                                return string.Compare(a.FileName, b.FileName);
                            });
            }
        }

        // Token: 0x06000044 RID: 68 RVA: 0x000071A0 File Offset: 0x000053A0
        private void GetMainMenuFiles(ref List<MenuInfo> variationMenuList, Dictionary<string, MenuInfo> loadItems,
            Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic, ref List<SMenuInfo> saveItems)
        {
            _menuList.Clear();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.menu", SearchOption.AllDirectories);
            var dictionary = new Dictionary<string, string>();
            foreach (var text in files)
            {
                var key = Path.GetFileName(text).ToLower();
                if (!dictionary.ContainsKey(key)) dictionary.Add(key, text);
            }

            var list = new List<string>(); //saveItems.Select(x=>x.FileName); //new List<string>();

            var menuDataBase = GameMain.Instance.MenuDataBase;

            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles1 " + saveItems.Count);
            // foreach (string text2 in menuFiles)
            for (var j = 0; j < menuDataBase.GetDataSize(); j++)
            {
                menuDataBase.SetIndex(j);
                var menuFileName = menuDataBase.GetMenuFileName();
                ParseMainMenuFile(menuFileName, list, ref variationMenuList, loadItems, dictionary, favDic,
                    colorLockDic, ref saveItems);
            }

            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles2 " + saveItems.Count);
            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles2 " +
                           saveItems[saveItems.Count - 1].FileName);
            foreach (var menuFile in GameUty.ModOnlysMenuFiles)
                ParseMainMenuFile(menuFile, list, ref variationMenuList, loadItems, dictionary, favDic, colorLockDic,
                    ref saveItems);

            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles3 " + saveItems.Count);
            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles3 " +
                           saveItems[saveItems.Count - 1].FileName);
        }

        // lmao
        private void ParseMainMenuFile(string menuFile, List<string> list, ref List<MenuInfo> variationMenuList,
            Dictionary<string, MenuInfo> loadItems, Dictionary<string, string> dictionary,
            Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic, ref List<SMenuInfo> saveItems)
        {
            var havePartsItems = GameMain.Instance.CharacterMgr.status.havePartsItems;
            try
            {
                if (menuFile.IndexOf("_i_man_") != 0 && menuFile.IndexOf("mbody") != 0 &&
                    menuFile.IndexOf("mhead") != 0 && !(Path.GetExtension(menuFile) != ".menu"))
                {
                    var fileName = Path.GetFileName(menuFile);
                    _menuList.Add(fileName.ToLower());
                    if (fileName.Contains("cv_pattern")) _myPatternList.Add(fileName.ToLower());

                    if (!list.Contains(fileName))
                    {
                        MenuInfo menuInfo = null;
                        if (!loadItems.TryGetValue(fileName, out menuInfo))
                            menuInfo = MenuModParser.ParseMenu(menuFile);

                        if (menuInfo != null && menuInfo.MPN != MPN.null_mpn)
                        {
                            menuInfo.FileName = fileName;
                            if (havePartsItems.ContainsKey(fileName))
                                menuInfo.IsShopTarget = true;
                            else
                                menuInfo.IsShopTarget = false;

                            var filePath = menuFile;
                            if (dictionary.TryGetValue(fileName, out filePath))
                            {
                                menuInfo.IsMod = true;
                                menuInfo.FilePath = filePath;
                            }
                            else
                            {
                                menuInfo.IsMod = false;
                                menuInfo.FilePath = fileName;
                            }

                            var empty = string.Empty;
                            if (_menuMPNCategoryDic.TryGetValue(menuInfo.MPN, out empty)) menuInfo.CategoryName = empty;

                            list.Add(fileName);
                            if (!string.IsNullOrEmpty(menuInfo.IconName))
                            {
                                if (Regex.IsMatch(menuFile, "_z\\d{1,4}") || menuFile.Contains("_zurashi") ||
                                    menuFile.Contains("_mekure") || menuFile.Contains("_porori") ||
                                    menuFile.Contains("_back"))
                                {
                                    variationMenuList.Add(menuInfo);
                                }
                                else
                                {
                                    if (favDic.ContainsKey(menuInfo.FileName)) menuInfo.IsFavorite = true;

                                    if (colorLockDic.ContainsKey(menuInfo.FileName)) menuInfo.IsColorLock = true;

                                    menuInfo.ColorNumber = 0;
                                    menuInfo.VariationMenuList.Add(menuInfo);
                                    _mpnMenuListDictionary[menuInfo.MPN].Add(menuInfo);
                                }
                            }

                            saveItems.Add(new SMenuInfo(menuInfo));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x06000045 RID: 69 RVA: 0x00007484 File Offset: 0x00005684
        private void GetModFiles(ref List<MenuInfo> variationMenuList, Dictionary<string, MenuInfo> loadItems,
            Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic, ref List<SMenuInfo> saveItems)
        {
            var list = new List<string>();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mod", SearchOption.AllDirectories);
            var dictionary = new Dictionary<string, string>();
            foreach (var text in files)
            {
                var fileName = Path.GetFileName(text);
                if (!dictionary.ContainsKey(fileName))
                {
                    list.Add(text);
                    dictionary.Add(fileName, text);
                }
            }

            foreach (var text2 in list)
                try
                {
                    if (Path.GetExtension(text2) == ".mod")
                    {
                        MenuInfo menuInfo = null;
                        var fileName2 = Path.GetFileName(text2);
                        if (!loadItems.TryGetValue(fileName2, out menuInfo))
                            menuInfo = MenuModParser.parseMod(text2); // 여기서 오류남

                        menuInfo.FileName = fileName2;
                        menuInfo.IsShopTarget = false;
                        menuInfo.IsMod = true;
                        menuInfo.IsOfficialMOD = true;
                        menuInfo.FilePath = text2;
                        var empty = string.Empty;
                        if (_menuMPNCategoryDic.TryGetValue(menuInfo.MPN, out empty)) menuInfo.CategoryName = empty;

                        var text3 = fileName2.ToLower();
                        if (Regex.IsMatch(text3, "_z\\d{1,4}") || text3.Contains("_porori") ||
                            text3.Contains("_zurashi") || text3.Contains("_mekure") || text3.Contains("_back"))
                        {
                            variationMenuList.Add(menuInfo);
                        }
                        else
                        {
                            if (favDic.ContainsKey(menuInfo.FileName.ToLower())) menuInfo.IsFavorite = true;

                            if (colorLockDic.ContainsKey(menuInfo.FileName.ToLower())) menuInfo.IsColorLock = true;

                            menuInfo.ColorNumber = 0;
                            menuInfo.VariationMenuList.Add(menuInfo);
                            _mpnMenuListDictionary[menuInfo.MPN].Add(menuInfo);
                        }

                        saveItems.Add(new SMenuInfo(menuInfo));
                    }
                }
                catch (Exception ex)
                {
                    Log.LogMessage("" + ex.StackTrace);
                }
        }

        // Token: 0x06000046 RID: 70 RVA: 0x000076B4 File Offset: 0x000058B4
        private void SetVariationMenu(Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic,
            ref List<MenuInfo> variationMenuList)
        {
            var list = new List<MenuInfo>();
            var list2 = new List<MenuInfo>();
            foreach (var menuInfo in variationMenuList)
            {
                var fileName = Path.GetFileName(menuInfo.FileName.ToLower());

                var colorNumber = 0;
                try
                {
                    var array = Regex.Split(fileName, "_z\\d{1,4}");
                    if (array.Length > 1) int.TryParse(array[1].Remove(0, 3).Split('.', '_')[0], out colorNumber);
                }
                catch (Exception e)
                {
                    Log.LogMessage("" + fileName);
                    Log.LogMessage("" + e);
                }

                menuInfo.ColorNumber = colorNumber;

                var text = Regex.Replace(fileName, "_z\\d{1,4}", "");
                text = Regex.Replace(text, "_zurashi\\d{0,4}", "");
                text = Regex.Replace(text, "_mekure\\d{0,4}", "");
                text = Regex.Replace(text, "_porori\\d{0,4}", "");
                text = Regex.Replace(text, "_back\\d{0,4}", "");
                text = text.Replace("_i.", "_i_.");
                if (_mpnMenuListDictionary.TryGetValue(menuInfo.MPN, out list))
                {
                    var flag = false;
                    foreach (var menuInfo2 in list)
                        if (menuInfo2.FileName.IndexOf(text, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            flag = true;
                            menuInfo2.VariationMenuList.Add(menuInfo);
                            break;
                        }

                    if (!flag)
                    {
                        if (favDic.ContainsKey(menuInfo.FileName.ToLower())) menuInfo.IsFavorite = true;

                        if (colorLockDic.ContainsKey(menuInfo.FileName.ToLower())) menuInfo.IsColorLock = true;

                        menuInfo.ColorNumber = 0;
                        menuInfo.VariationMenuList.Add(menuInfo);
                        list2.Add(menuInfo);
                    }
                }
            }

            foreach (var menuInfo3 in list2) _mpnMenuListDictionary[menuInfo3.MPN].Add(menuInfo3);
        }

        // Token: 0x06000047 RID: 71 RVA: 0x00007918 File Offset: 0x00005B18
        private void setColorSet()
        {
            foreach (var key in _mpnMenuListDictionary.Keys)
            foreach (var menuInfo in _mpnMenuListDictionary[key])
            {
                var list = new List<MenuInfo>();
                list.AddRange(menuInfo.VariationMenuList);
                foreach (var menuInfo2 in list)
                    if (!string.IsNullOrEmpty(menuInfo2.ColorSetMenuName))
                    {
                        var list2 = new List<MenuInfo>();
                        var list3 = new List<MenuInfo>();
                        if (_mpnMenuListDictionary.TryGetValue(menuInfo2.ColorSetMPN, out list3))
                        {
                            var pattern = Regex.Replace(menuInfo2.ColorSetMenuName, ".",
                                WildCardMatchEvaluator);
                            foreach (var menuInfo3 in list3)
                                if (Regex.IsMatch(menuInfo3.FileName, pattern, RegexOptions.IgnoreCase))
                                    list2.Add(menuInfo3);
                        }

                        menuInfo2.ColorSetMenuList.AddRange(list2);
                    }
            }
        }

        // Token: 0x02000010 RID: 16
        private class FolderMenu
        {
            // Token: 0x04000071 RID: 113
            public readonly string[] Categories;

            // Token: 0x04000070 RID: 112
            public readonly string Name = string.Empty;

            // Token: 0x0600006B RID: 107 RVA: 0x000085F9 File Offset: 0x000067F9
            public FolderMenu(string name, string[] categories)
            {
                Name = name;
                Categories = categories;
            }
        }
    }
}