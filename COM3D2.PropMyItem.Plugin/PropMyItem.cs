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

namespace COM3D2.PropMyItem.Plugin;

using static CommonUtil;

[BepInPlugin(PluginInfo.PluginName, PluginInfo.PluginName, PluginInfo.PluginVersion)]
public class PropMyItem : BaseUnityPlugin
{
    public static ManualLogSource Log;

    private static bool _isForcedInit;

    //private bool _isStartUpLoadead;

    private static bool _isLoading;

    private readonly AutoShoesHide _autoShoesHide = new();

    private readonly Dictionary<string, MPN> _categoryMPNDic = new();

    private readonly List<FolderMenu> _folders = new();

    //private Dictionary<MPN, string> presetRestoreDic_ = new Dictionary<MPN, string>();

    private readonly List<string> _menuList = new();

    private readonly Dictionary<MPN, string> _menuMPNCategoryDic = new();

    private readonly List<string> _myPatternList = new();

    private Vector2 _categoryScrollPosition;

    private Vector2 _colorItemScrollPosition;

    private bool _isFavFilter;

    private bool _isFreeColor;

    private bool _isMinimum;

    private bool _isShowFilterSetting;

    //private bool _isPluginKeyChange;

    private bool _isShowSetting;

    private bool _isVisible;

    private bool _menuFilesReady;

    public Dictionary<MPN, List<MenuInfo>> _mpnMenuListDictionary = new();

    private SavePreset _savepreset = new();

    private int _sceneLevel;

    private Vector2 _scrollFilterPosition;

    private Vector2 _scrollPosition;

    private int _selectedCategory = -1;

    private int _selectedEyeClorType;

    private int _selectedFilter;

    private string _selectedFilterText = string.Empty;

    private int _selectedFolder;

    private MenuInfo _selectedItem;

    private MPN _selectedMPN;

    private int _selectedMaid;

    private List<CharacterMgr.Preset> _selectedPresetList = new();

    private CharacterMgr.PresetType _selectedPresetType = CharacterMgr.PresetType.All;

    private MenuInfo _selectedVariationItem;

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
        // COM3D2.5 把服装拆成了叠穿的多层（インナー / アウター / ベスト …），这些 MPN 在 2.x 的
        // 枚举里不存在，连编译用的 GameLibs 3.46.1 里都还没有 outerwear —— 所以按名字解析，
        // 当前游戏版本没有的就整体跳过。
        var wearCategories = new List<string>
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
        };
        var accessoryCategories = new List<string>
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
            "乳首",
            "腕",
            "へそ",
            "足首",
            "背中",
            "accanl",
            "accvag",
            "しっぽ",
            "前穴"
        };
        var extraCategories = new Dictionary<string, MPN>();
        AppendResolvableCategories(ExtraWearCategories, wearCategories, extraCategories);
        AppendResolvableCategories(ExtraAccessoryCategories, accessoryCategories, extraCategories);
        _folders.Add(new FolderMenu("服装", wearCategories.ToArray()));
        _folders.Add(new FolderMenu("アクセサリ", accessoryCategories.ToArray()));
        _folders.Add(new FolderMenu("セット", new[]
        {
            "メイド服",
            "コスチューム",
            "下着",
            "身体"
        }));
        _folders.Add(new FolderMenu("プリセット", new[]
        {
            "服 / 身体",
            "服",
            "身体",
            "全て"
        }));
        _folders.Add(new FolderMenu("全て", new string[0]));
        _folders.Add(new FolderMenu("選択中", new string[0]));
        _categoryMPNDic.Add("顔", Mpn.head);
        _categoryMPNDic.Add("眉", Mpn.folder_mayu);
        _categoryMPNDic.Add("目", Mpn.folder_eye);
        _categoryMPNDic.Add("目ハイライト", Mpn.eye_hi);
        _categoryMPNDic.Add("ほくろ", Mpn.hokuro);
        _categoryMPNDic.Add("唇", Mpn.lip);
        _categoryMPNDic.Add("歯", Mpn.accha);

        _categoryMPNDic.Add("上まつげ", Mpn.folder_matsuge_up);
        _categoryMPNDic.Add("下まつげ", Mpn.folder_matsuge_low);
        _categoryMPNDic.Add("二重まぶた", Mpn.folder_futae);

        _categoryMPNDic.Add("前髪", Mpn.hairf);
        _categoryMPNDic.Add("後髪", Mpn.hairr);
        _categoryMPNDic.Add("横髪", Mpn.hairs);
        _categoryMPNDic.Add("エクステ髪", Mpn.hairt);
        _categoryMPNDic.Add("アホ毛", Mpn.hairaho);
        _categoryMPNDic.Add("肌", Mpn.folder_skin);
        _categoryMPNDic.Add("乳首", Mpn.chikubi);
        _categoryMPNDic.Add("タトゥー", Mpn.acctatoo);
        _categoryMPNDic.Add("アンダーヘア", Mpn.folder_underhair);
        _categoryMPNDic.Add("ボディ", Mpn.body);
        _categoryMPNDic.Add("帽子", Mpn.acchat);
        _categoryMPNDic.Add("ヘッドドレス", Mpn.headset);
        _categoryMPNDic.Add("トップス", Mpn.wear);
        _categoryMPNDic.Add("ボトムス", Mpn.skirt);
        _categoryMPNDic.Add("ワンピース", Mpn.onepiece);
        _categoryMPNDic.Add("水着", Mpn.mizugi);
        _categoryMPNDic.Add("ブラジャー", Mpn.bra);
        _categoryMPNDic.Add("パンツ", Mpn.panz);
        _categoryMPNDic.Add("靴下", Mpn.stkg);
        _categoryMPNDic.Add("靴", Mpn.shoes);
        _categoryMPNDic.Add(" 前髪 ", Mpn.acckami);
        _categoryMPNDic.Add("メガネ", Mpn.megane);
        _categoryMPNDic.Add("アイマスク", Mpn.acchead);
        _categoryMPNDic.Add("鼻", Mpn.acchana);
        _categoryMPNDic.Add("耳", Mpn.accmimi);
        _categoryMPNDic.Add("手袋", Mpn.glove);
        _categoryMPNDic.Add("ネックレス", Mpn.acckubi);
        _categoryMPNDic.Add("チョーカー", Mpn.acckubiwa);
        _categoryMPNDic.Add("リボン", Mpn.acckamisub);
        _categoryMPNDic.Add("乳首", Mpn.accnip);
        _categoryMPNDic.Add("腕", Mpn.accude);
        _categoryMPNDic.Add("へそ", Mpn.accheso);
        _categoryMPNDic.Add("足首", Mpn.accashi);
        _categoryMPNDic.Add("背中", Mpn.accsenaka);
        _categoryMPNDic.Add("しっぽ", Mpn.accshippo);
        _categoryMPNDic.Add("前穴", Mpn.accxxx);
        _categoryMPNDic.Add("メイド服", Mpn.set_maidwear);
        _categoryMPNDic.Add("コスチューム", Mpn.set_mywear);
        _categoryMPNDic.Add("下着", Mpn.set_underwear);
        _categoryMPNDic.Add("身体", Mpn.set_body);

        _categoryMPNDic.Add("鞏膜", Mpn.folder_eyewhite);

        _categoryMPNDic.Add("accanl", Mpn.accanl);
        _categoryMPNDic.Add("指甲", Mpn.accnail);
        _categoryMPNDic.Add("accvag", Mpn.accvag);
        _categoryMPNDic.Add("moza", Mpn.moza);

        _categoryMPNDic.Add("chikubicolor", Mpn.chikubicolor);

        // 上面按名字解析出来的 2.5 新槽位
        foreach (var pair in extraCategories) _categoryMPNDic.Add(pair.Key, pair.Value);

        foreach (var text in _categoryMPNDic.Keys)
            _menuMPNCategoryDic.Add(_categoryMPNDic[text], text);

        foreach (var obj in Enum.GetValues(typeof(MPN)))
        {
            var key = (MPN)obj;
            _mpnMenuListDictionary.Add(key, new List<MenuInfo>());
        }
    }

    private ConfigEntry<KeyboardShortcut> ShowCounter { get; set; }

    /// <summary>
    ///     COM3D2.5 新增的叠穿服装槽位。2.x 的枚举里没有这些名字（连编译用的 GameLibs 3.46.1 都还没有
    ///     outerwear），所以不能直接写 Mpn.xxx，只能按名字在运行时解析；解析不到就跳过。
    /// </summary>
    private static readonly Dictionary<string, string> ExtraWearCategories = new()
    {
        { "インナー", "shirt" },
        { "アウター", "outerwear" },
        { "ベスト", "vest" },
        { "ジャケット", "jacket" },
        { "スリップ", "slip" },
        { "水着上", "mizugi_top" },
        { "水着下", "mizugi_buttom" }
    };

    private static readonly Dictionary<string, string> ExtraAccessoryCategories = new()
    {
        { "顔アクセ", "accface" },
        { "腰", "acckoshi" },
        { "手持ち", "handitem" }
    };

    /// <summary>把「分类名 → MPN 名」中当前游戏版本能解析出来的项追加到分类列表。</summary>
    private static void AppendResolvableCategories(Dictionary<string, string> source, List<string> target,
        Dictionary<string, MPN> resolved)
    {
        foreach (var pair in source)
        {
            MPN mpn;
            if (!EnumUtil.TryParse(pair.Value, true, out mpn)) continue;
            if (mpn == Mpn.null_mpn) continue;
            if (resolved.ContainsKey(pair.Key)) continue;

            resolved.Add(pair.Key, mpn);
            target.Add(pair.Key);
        }
    }

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

    // MenuDataBase 条目的统计，排查「某个类别全空」时看日志就知道卡在哪一步
    private static int _dbAccepted;
    private static int _dbSkippedNoMpn;
    private static int _dbSkippedNoIcon;
    private static string _dbSkippedSamples = string.Empty;
    private static string _dbNoMpnSamples = string.Empty;

    private bool _isDraggingWindow;    private Vector2 _dragStartMousePosition;
    private Vector2 _dragStartWindowPosition;

    // 本帧 MouseDown 时的鼠标位置（逻辑屏幕坐标），点中空白处才拿它当拖动起点
    private Vector2 _mouseDownPosition;

    /// <summary>
    ///     代替 GUI.DragWindow()。它按 Event.delta 移动窗口，而 delta 是屏幕像素、
    ///     窗口矩形却在缩放后的逻辑坐标里，高倍率下窗口会跑得比鼠标快。
    ///     这里全程用 IMGUI 事件的坐标（OnGUI 里已经是逻辑坐标，和 _windowRect 同一套）。
    ///     注意不能用 Input.GetMouseButton 判断按下状态：ShowCounterHandler 里每帧都会调
    ///     Input.ResetInputAxes()，按键会在 OnGUI 时读成未按下。
    /// </summary>
    private void HandleWindowDragEvents()
    {
        var current = Event.current;

        if (current.type == EventType.MouseDown && current.button == 0)
            _mouseDownPosition = current.mousePosition;
        else if (current.type == EventType.MouseUp)
            _isDraggingWindow = false;

        if (!_isDraggingWindow) return;

        // 只在拖动事件里更新位置：Layout / Repaint 时 Event.current.mousePosition 不可靠，
        // 拿它算位移会让窗口乱跳（表现就是「拖动太敏感」）
        if (current.type != EventType.MouseDrag) return;

        _windowRect.position = _dragStartWindowPosition +
                               (current.mousePosition - _dragStartMousePosition);
    }

    /// <summary>
    ///     在窗口函数末尾调用。控件处理过的 MouseDown 会被 Event.Use() 变成 Used，
    ///     只有点在空白处才还是 MouseDown，据此区分点击与拖动。
    ///     （不能用 GUIUtility.hotControl 判断，窗口自己会占住它。）
    /// </summary>
    private void BeginWindowDragIfNeeded()
    {
        if (Event.current.type != EventType.MouseDown || Event.current.button != 0) return;

        _isDraggingWindow = true;
        _dragStartMousePosition = _mouseDownPosition;
        _dragStartWindowPosition = _windowRect.position;
    }

    public void OnGUI()
    {
        if (!_isVisible)
        {
            _isDraggingWindow = false;
            return;
        }

        // 用 GUI.matrix 整体缩放：窗口、字号、间距、贴图一起放大，不用逐个改布局常量
        var scale = UserConfig.ClampUiScale(UserConfig.Instance.UiScale);
        var previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f)) *
                     previousMatrix;
        try
        {
            // 逻辑（未缩放）屏幕尺寸，后面所有坐标都在这个空间里
            var screenWidth = Screen.width / scale;
            var screenHeight = Screen.height / scale;

            // 拖动要在 GUI.Window 之前生效，它用的是传进去的矩形
            HandleWindowDragEvents();

            // Calculate the new window size
            float newWidth = 800;
            float newHeight = 600;

            if (_isShowSetting)
            {
                // Update the windowRect size
                _windowRect.width = newWidth;
                _windowRect.height = newHeight;

                // Ensure that the window doesn't go off the screen
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Mathf.Max(0f, screenWidth - newWidth));
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Mathf.Max(0f, screenHeight - newHeight));

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
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Mathf.Max(0f, screenWidth - newWidth));
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Mathf.Max(0f, screenHeight - newHeight));

                _windowRect = GUI.Window(PluginInfo.WindowID, _windowRect,
                    GuiFilterSettingFunc, "PropMyItem " + PluginInfo.PluginVersion,
                    GuiStyles.WindowStyle);
                return;
            }

            // Update the windowRect size
            _windowRect.width = newWidth;
            _windowRect.height = newHeight;

            // Ensure that the window doesn't go off the screen
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Mathf.Max(0f, screenWidth - newWidth));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Mathf.Max(0f, screenHeight - newHeight));

            _windowRect = GUI.Window(PluginInfo.WindowID, _windowRect, GuiFunc,
                "PropMyItem " + PluginInfo.PluginVersion, GuiStyles.WindowStyle);
        }
        finally
        {
            // 不还原会影响游戏自己的 IMGUI 绘制
            GUI.matrix = previousMatrix;
        }
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
            // 鼠标是屏幕坐标，窗口矩形是（未缩放的）逻辑坐标，要先除以倍数再比
            var scale = UserConfig.ClampUiScale(UserConfig.Instance.UiScale);
            var mousePosition = new Vector2(Input.mousePosition.x / scale,
                (Screen.height - Input.mousePosition.y) / scale);

            if (_isVisible && _windowRect.Contains(mousePosition))
            {
                GameMain.Instance.MainCamera.SetControl(false);
                if (Event.current.type != EventType.KeyDown &&
                    Event.current.type != EventType.KeyUp)
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


    /// <summary>调整 UI 倍数：取整到 0.1、夹在允许范围内，改动立即存盘。</summary>
    private static void ChangeUiScale(float delta)
    {
        var current = UserConfig.Instance.UiScale;
        var next = UserConfig.ClampUiScale(current + delta);
        if (Math.Abs(next - current) < 0.001f) return;

        UserConfig.Instance.UiScale = next;
        UserConfig.Instance.Save();
    }


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
        flag = GUI.Toggle(new Rect(margin, num, width, GuiStyles.ControlHeight), flag,
            "室内で自動的に靴を脱ぐ ",
            GuiStyles.ToggleStyle);
        if (flag != UserConfig.Instance.IsAutoShoesHide)
        {
            UserConfig.Instance.IsAutoShoesHide = flag;
            UserConfig.Instance.Save();
        }

        num += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
        var flag2 = UserConfig.Instance.IsOutputInfoLog;
        flag2 = GUI.Toggle(new Rect(margin, num, width, GuiStyles.ControlHeight), flag2,
            "アイテム変更時のログ出力",
            GuiStyles.ToggleStyle);
        if (flag2 != UserConfig.Instance.IsOutputInfoLog)
        {
            UserConfig.Instance.IsOutputInfoLog = flag2;
            UserConfig.Instance.Save();
        }

        // UI 倍率：整个界面按倍数放大，方便高分辨率屏幕
        num += GuiStyles.ControlHeight + GuiStyles.Margin + GuiStyles.Margin;
        {
            var labelWidth = width * 0.42f;
            var buttonWidth = GuiStyles.ControlHeight * 1.5f;
            GUI.Label(new Rect(margin, num, labelWidth, GuiStyles.ControlHeight), "UI 倍率",
                GuiStyles.LabelStyle);

            var valueWidth = width - labelWidth - (buttonWidth + GuiStyles.Margin) * 2f;
            GUI.Label(new Rect(margin + labelWidth, num, valueWidth, GuiStyles.ControlHeight),
                UserConfig.Instance.UiScale.ToString("0.0") + "x", GuiStyles.LabelStyle);

            if (GUI.Button(
                    new Rect(margin + width - buttonWidth * 2f - GuiStyles.Margin, num, buttonWidth,
                        GuiStyles.ControlHeight), "-", GuiStyles.ButtonStyle))
                ChangeUiScale(-UserConfig.UiScaleStep);

            if (GUI.Button(new Rect(margin + width - buttonWidth, num, buttonWidth, GuiStyles.ControlHeight),
                    "+", GuiStyles.ButtonStyle))
                ChangeUiScale(UserConfig.UiScaleStep);
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
        if (GUI.Button(new Rect(margin, num, width, GuiStyles.ControlHeight), "戻る",
                GuiStyles.ButtonStyle))
            _isShowSetting = false;
        //this._isPluginKeyChange = false;
        num += GuiStyles.ControlHeight + GuiStyles.Margin;
        _windowRect.height = num;
        BeginWindowDragIfNeeded();
    }

    private void GuiFilterSettingFunc(int windowID)
    {
        var num = _windowRect.width - GuiStyles.Margin * 2f;
        var num2 = _windowRect.height - GuiStyles.Margin * 2f;
        var num3 = GuiStyles.ControlHeight + GuiStyles.Margin;
        var num4 = GuiStyles.Margin;
        var text = "フィルタ文字列：";
        GUI.Label(new Rect(num4, num3, GuiStyles.FontSize * text.Length, GuiStyles.ControlHeight),
            text,
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
        var position = new Rect(num4, num3, num,
            num2 - num3 - GuiStyles.ControlHeight - GuiStyles.Margin * 2f);
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
        if (GUI.Button(new Rect(num4, num3, num, GuiStyles.ControlHeight), "戻る",
                GuiStyles.ButtonStyle))
        {
            _isShowSetting = false;
            //this._isPluginKeyChange = false;
            _isShowFilterSetting = false;
        }

        num3 += GuiStyles.ControlHeight + GuiStyles.Margin;
        BeginWindowDragIfNeeded();
    }

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
                    if (_selectedMPN != Mpn.null_mpn || _folders[_selectedFolder].Name == "全て" ||
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
            BeginWindowDragIfNeeded();
        }
    }

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
                var position5 = new Rect(position4.x + position4.width + 5f, position4.y, 75f,
                    position4.height);

                GUI.Label(position, visibleMaidList[_selectedMaid].GetThumIcon(),
                    GuiStyles.LabelStyle);
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

    private void updateMaidEyePosY(Maid maid, float value)
    {
        var num = 5000f;
        var num2 = maid.body0.trsEyeL.localPosition.y * num;
        if (value < 0f) value = 0f;

        var localPosition = maid.body0.trsEyeL.localPosition;
        var localPosition2 = maid.body0.trsEyeR.localPosition;
        maid.body0.trsEyeL.localPosition = new Vector3(localPosition.x,
            Math.Max((num2 + value) / num, 0f),
            localPosition.z);
        maid.body0.trsEyeR.localPosition = new Vector3(localPosition.x,
            Math.Min((num2 - value) / num, 0f),
            localPosition.z);
    }

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
                _selectedMPN = Mpn.null_mpn;
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

    private void nextPattern(Maid maid, MPN mpn, string basename, string filename,
        bool isPrev = false)
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
                            if (i < variationMenuList.Count - 1)
                                fileName = variationMenuList[i + 1].FileName;
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
            _categoryScrollPosition =
                GUI.BeginScrollView(position, _categoryScrollPosition, viewRect);
            for (var i = 0; i < num; i++)
            {
                var position2 = new Rect(0f, (num2 + GuiStyles.Margin) * i, width, num2);
                GUI.enabled = _selectedCategory != i;
                if (GUI.Button(position2, _folders[_selectedFolder].Categories[i],
                        GuiStyles.ButtonStyle))
                {
                    _selectedPresetList.Clear();
                    _selectedItem = null;
                    _selectedVariationItem = null;
                    _scrollPosition.y = 0f;
                    var selectedMPN = Mpn.head;
                    if (_categoryMPNDic.TryGetValue(_folders[_selectedFolder].Categories[i],
                            out selectedMPN))
                    {
                        _selectedMPN = selectedMPN;
                    }
                    else
                    {
                        if (_folders[_selectedFolder].Name == "プリセット")
                        {
                            if (_folders[_selectedFolder].Categories[i] == "全て")
                            {
                                _selectedPresetList =
                                    GameMain.Instance.CharacterMgr.PresetListLoad();
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

                        _selectedMPN = Mpn.null_mpn;
                    }

                    _selectedCategory = i;
                }

                GUI.enabled = true;
            }

            GUI.EndScrollView();
            xPos += position.width + GuiStyles.Margin;
        }
    }

    private static int CompareCreateTime(string fileX, string fileY)
    {
        return DateTime.Compare(File.GetLastWriteTime(fileX), File.GetCreationTime(fileY));
    }

    private void guiDebug(ref float xPos, float yPos, float windowHeight)
    {
        var num = 200f;
        if (GUI.Button(new Rect(xPos, yPos, num, GuiStyles.ControlHeight), "プリセット保存",
                GuiStyles.ButtonStyle))
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
                                Mpn.megane,
                                Mpn.acckami,
                                Mpn.acckamisub,
                                Mpn.hairt,
                                Mpn.headset,
                                Mpn.acchat
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
                                    GameMain.Instance.CharacterMgr.PresetSet(item,
                                        _selectedPresetList[i]);
                            else
                                GameMain.Instance.CharacterMgr.PresetSet(maid,
                                    _selectedPresetList[i]);
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
                            Mpn.megane,
                            Mpn.acckami,
                            Mpn.acckamisub,
                            Mpn.hairt,
                            Mpn.headset,
                            Mpn.acchat
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
                                GameMain.Instance.CharacterMgr.PresetSet(item,
                                    _selectedPresetList[i]);
                        else
                            GameMain.Instance.CharacterMgr.PresetSet(maid2, _selectedPresetList[i]);
                    }
                }
            }

            GUI.EndScrollView();
            xPos += num + GuiStyles.ScrollWidth + GuiStyles.Margin;
        }
    }

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
        _selectedFilterText =
            GUI.TextField(position3, _selectedFilterText, GuiStyles.TextFieldStyle);
        num += num3 + 4;
        position2.Set(num, num2, GuiStyles.FontSize * 2, GuiStyles.ControlHeight);
        if (GUI.Button(position2, "▽", GuiStyles.ButtonStyle)) _isShowFilterSetting = true;

        num += GuiStyles.FontSize * 2 + GuiStyles.Margin;
        position2.Set(num, num2, GuiStyles.FontSize * 2, GuiStyles.ControlHeight);
        if (GUI.Button(position2, "x", GuiStyles.ButtonStyle)) _selectedFilterText = string.Empty;
    }

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
                    if (_mpnMenuListDictionary.TryGetValue(key, out collection))
                        list.AddRange(collection);
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
                    if (mpn != Mpn.set_maidwear && mpn != Mpn.set_mywear &&
                        mpn != Mpn.set_underwear &&
                        mpn != Mpn.set_body)
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
                                            if (menuInfo2.IconName != "_I_del.tex")
                                                list.Add(menuInfo);

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
        if (_selectedMPN == Mpn.set_maidwear || _selectedMPN == Mpn.set_mywear ||
            _selectedMPN == Mpn.set_underwear || _selectedMPN == Mpn.set_body)
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
                        continue;
                    }
                }
                else if (_selectedFilter == 2 && !menuInfo.IsMod)
                {
                    num9++;
                    menuInfo.IsHave = false;
                    continue;
                }

                if (_isFavFilter && !menuInfo.IsFavorite)
                {
                    num9++;
                    menuInfo.IsHave = false;
                }
                else if (!string.IsNullOrEmpty(_selectedFilterText) &&
                         menuInfo.ItemName.IndexOf(_selectedFilterText,
                             StringComparison.OrdinalIgnoreCase) ==
                         -1 && menuInfo.FilePath.IndexOf(_selectedFilterText,
                             StringComparison.OrdinalIgnoreCase) == -1)
                {
                    num9++;
                    menuInfo.IsHave = false;
                }
            }

        }

        num8 -= num9;
        var num10 = num8 % num4 == 0f ? 0 : 1;
        var num11 = (int)(windowHeight - yPos - GuiStyles.FontSize - GuiStyles.ControlHeight);
        var viewRect = new Rect(0f, 0f, num6 * num4, ((int)(num8 / num4) + num10) * num7);
        if (_folders[_selectedFolder].Name == "選択中")
            viewRect = new Rect(0f, 0f, num6 * num4, ((int)((num8 + 5) / num4) + num10) * num7);

        var position = new Rect(xPos, yPos + GuiStyles.ControlHeight,
            viewRect.width + GuiStyles.ScrollWidth,
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
                            if (menuInfo3.FileName.IndexOf(text,
                                    StringComparison.OrdinalIgnoreCase) == 0)
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
                if ((y <= num14 && num15 <= num12) || (num14 <= y && y <= num15) ||
                    (y <= num14 && num14 <= num12))
                {
                    if (menuInfo2.Icon == null && !string.IsNullOrEmpty(menuInfo2.IconName) &&
                        !menuInfo2.IsError)
                    {
                        var loaded = false;
                        if (!menuInfo2.IsOfficialMOD)
                            try
                            {
                                menuInfo2.Icon = ImportCM.CreateTexture(menuInfo2.IconName);
                                loaded = true;
                            }
                            catch (Exception e)
                            {
                                Log.LogMessage(e.ToString());
                            }

                        // 从 .tex 加载失败时退回从 .mod 文件里取内嵌图标
                        if (!loaded)
                        {
                            var modInfo = MenuModParser.parseMod(menuInfo2.FilePath);
                            if (modInfo != null) menuInfo2.Icon = modInfo.Icon;
                        }
                    }

                    var tooltip = menuInfo2.ItemName;
                    if (_folders[_selectedFolder].Name == "全て" ||
                        _folders[_selectedFolder].Name == "選択中")
                        tooltip = menuInfo2.CategoryName + "：" + menuInfo2.ItemName;

                    var position2 = new Rect(num6 * (num13 % num4),
                        num7 * (int)(num13 / num4), num6, num7);
                    var position3 = new Rect(position2.x, position2.y + position2.height - 20f, 20f,
                        20f);
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
                                    Log.LogMessage("[PropMyItem] change item = " +
                                                   menuInfo2.FileName);

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
                                    UserConfig.Instance.ColorLockList.Remove(menuInfo2.FileName
                                        .ToLower());
                                }
                                else
                                {
                                    menuInfo2.IsColorLock = true;
                                    UserConfig.Instance.ColorLockList.Add(
                                        menuInfo2.FileName.ToLower());
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
                                    UserConfig.Instance.ColorLockList.Remove(menuInfo2.FileName
                                        .ToLower());
                                }
                                else
                                {
                                    menuInfo2.IsColorLock = true;
                                    UserConfig.Instance.ColorLockList.Add(
                                        menuInfo2.FileName.ToLower());
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
                                    Log.LogMessage("[PropMyItem] change item = " +
                                                   menuInfo2.FileName);

                                if (isAllMaid)
                                    foreach (var maid1 in visibleMaidList3)
                                        maid1.SetProp(menuInfo2.MPN, menuInfo2.FileName,
                                            Path.GetFileName(menuInfo2.FileName).GetHashCode());
                                else
                                    visibleMaidList3[_selectedMaid].SetProp(menuInfo2.MPN,
                                        menuInfo2.FileName,
                                        Path.GetFileName(menuInfo2.FileName).GetHashCode());


                                if ((menuInfo2.MPN == Mpn.folder_futae ||
                                     menuInfo2.MPN == Mpn.folder_matsuge_low ||
                                     menuInfo2.MPN == Mpn.folder_matsuge_up ||
                                     menuInfo2.MPN == Mpn.folder_eye ||
                                     menuInfo2.MPN == Mpn.folder_mayu ||
                                     menuInfo2.MPN == Mpn.folder_skin ||
                                     menuInfo2.MPN == Mpn.folder_underhair ||
                                     menuInfo2.MPN == Mpn.chikubi) &&
                                    menuInfo2.ColorSetMenuList.Count > 0)
                                {
                                    var menuInfo6 = _selectedVariationItem.ColorSetMenuList[0];
                                    if (isAllMaid)
                                        foreach (var maid1 in visibleMaidList)
                                            maid1.SetProp(menuInfo2.ColorSetMPN, menuInfo6.FileName,
                                                Path.GetFileName(menuInfo6.FileName).GetHashCode());
                                    else
                                        visibleMaidList3[_selectedMaid].SetProp(
                                            menuInfo2.ColorSetMPN,
                                            menuInfo6.FileName,
                                            Path.GetFileName(menuInfo6.FileName).GetHashCode());
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
                        var position6 = new Rect(position2.x + position2.width - num,
                            position2.y + num3, 10f,
                            10f);
                        if (menuInfo2.MPN == Mpn.set_maidwear || menuInfo2.MPN == Mpn.set_mywear ||
                            menuInfo2.MPN == Mpn.set_underwear || menuInfo2.MPN == Mpn.set_body)
                            position6 = new Rect(position2.x + position2.width - num2,
                                position2.y + num3, 10f,
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
                Mpn.acchat,
                Mpn.headset,
                Mpn.wear,
                Mpn.skirt,
                Mpn.onepiece,
                Mpn.mizugi,
                Mpn.bra,
                Mpn.panz,
                Mpn.stkg,
                Mpn.shoes,
                Mpn.acckami,
                Mpn.megane,
                Mpn.acchead,
                Mpn.acchana,
                Mpn.accmimi,
                Mpn.glove,
                Mpn.acckubi,
                Mpn.acckubiwa,
                Mpn.acckamisub,
                Mpn.accnip,
                Mpn.accude,
                Mpn.accheso,
                Mpn.accashi,
                Mpn.accsenaka,
                Mpn.accshippo,
                Mpn.accxxx
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
        var position9 = new Rect(xPos, yPos, num6 * num4 + GuiStyles.ScrollWidth,
            GuiStyles.ControlHeight);
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
        if ((menuInfo2.MPN == Mpn.folder_futae || menuInfo2.MPN == Mpn.folder_matsuge_low ||
             menuInfo2.MPN == Mpn.folder_matsuge_up || menuInfo2.MPN == Mpn.folder_eye ||
             menuInfo2.MPN == Mpn.folder_mayu || menuInfo2.MPN == Mpn.folder_skin ||
             menuInfo2.MPN == Mpn.folder_underhair || menuInfo2.MPN == Mpn.chikubi) &&
            menuInfo2.ColorSetMenuList.Count > 0)
        {
            var menuInfo5 = _selectedVariationItem.ColorSetMenuList[0];
            visibleMaidList2.SetProp(menuInfo2.ColorSetMPN, menuInfo5.FileName,
                Path.GetFileName(menuInfo5.FileName).GetHashCode());
        }

        visibleMaidList2.AllProcProp();
    }

    private void guiSelectedVariation(ref float posX, float posY, MenuInfo itemMenuInfo,
        float iconWidth,
        float iconHeight, float windowHeight, string selectedFileName)
    {
        var count = itemMenuInfo.VariationMenuList.Count;
        var viewRect = new Rect(0f, 0f, iconWidth, count * (iconWidth + 4f));
        var position = new Rect(posX, posY + GuiStyles.ControlHeight,
            viewRect.width + GuiStyles.ScrollWidth,
            windowHeight - posY - GuiStyles.FontSize - GuiStyles.ControlHeight);
        _colorItemScrollPosition =
            GUI.BeginScrollView(position, _colorItemScrollPosition, viewRect);
        new Rect(0f, 0f, iconWidth, iconWidth);
        for (var i = 0; i < count; i++)
        {
            var menuInfo = itemMenuInfo.VariationMenuList[i];
            if (menuInfo.Icon == null && !string.IsNullOrEmpty(menuInfo.IconName) &&
                !menuInfo.IsError)
            {
                var loaded = false;
                if (!menuInfo.IsOfficialMOD)
                    try
                    {
                        menuInfo.Icon = ImportCM.CreateTexture(menuInfo.IconName);
                        loaded = true;
                    }
                    catch (Exception e)
                    {
                        Log.LogMessage("" + e);
                    }

                // 从 .tex 加载失败时退回从 .mod 文件里取内嵌图标
                if (!loaded)
                {
                    var modInfo = MenuModParser.parseMod(menuInfo.FilePath);
                    if (modInfo != null) menuInfo.Icon = modInfo.Icon;
                }
            }

            var tooltip = menuInfo.ItemName;
            if (_folders[_selectedFolder].Name == "全て" ||
                _folders[_selectedFolder].Name == "選択中")
                tooltip = menuInfo.CategoryName + "：" + menuInfo.ItemName;

            if (!string.IsNullOrEmpty(selectedFileName) &&
                menuInfo.FileName.IndexOf(selectedFileName, StringComparison.OrdinalIgnoreCase) ==
                0)
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
        }

        GUI.EndScrollView();
        posX += iconWidth + GuiStyles.ScrollWidth + 8f;
    }

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
            for (var i = 0; i < _selectedVariationItem.ColorSetMenuList.Count; i++)
            {
                var menuInfo = _selectedVariationItem.ColorSetMenuList[i];
                if (menuInfo.Icon == null && !string.IsNullOrEmpty(menuInfo.IconName) &&
                    !menuInfo.IsError)
                {
                    var loaded = false;
                    if (!menuInfo.IsOfficialMOD)
                        try
                        {
                            menuInfo.Icon = ImportCM.CreateTexture(menuInfo.IconName);
                            loaded = true;
                        }
                        catch (Exception e)
                        {
                            Log.LogMessage("" + e);
                        }

                    // 从 .tex 加载失败时退回从 .mod 文件里取内嵌图标
                    if (!loaded)
                    {
                        var modInfo = MenuModParser.parseMod(menuInfo.FilePath);
                        if (modInfo != null) menuInfo.Icon = modInfo.Icon;
                    }
                }

                if (!string.IsNullOrEmpty(value) &&
                    menuInfo.FileName.IndexOf(value, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (menuInfo.FileName.IndexOf("mugen", StringComparison.OrdinalIgnoreCase) !=
                        -1)
                        _isFreeColor = true;
                    else
                        _isFreeColor = false;

                    GUI.enabled = false;
                }

                if (GUI.Button(new Rect(posX, posY + 10f + (num + 1f) * i, num, num),
                        menuInfo.Icon))
                {
                    if (menuInfo.FileName.IndexOf("mugen", StringComparison.OrdinalIgnoreCase) !=
                        -1)
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
            }

            posX += num + 8f;
            return;
        }

        _isFreeColor = false;
    }

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
                var mpn = _selectedItem.MPN;
                var parts_COLOR = GetColorTarget(mpn);

                // 眼睛多一个「両目 / 左目 / 右目」的切换按钮
                if (mpn == Mpn.eye || mpn == Mpn.folder_eye)
                {
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

                    if (GUI.Button(new Rect(posX, posY, fontSize * 8, controlHeight), text,
                            buttonStyle))
                        _selectedEyeClorType =
                            _selectedEyeClorType == 2 ? 0 : _selectedEyeClorType + 1;

                    num = controlHeight + 8f + posY;
                }

                var partsColor = maid.Parts.GetPartsColor(parts_COLOR);
                string[] array =
                {
                    "色相",
                    "彩度",
                    "明度",
                    "対称",
                    "影率",
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

    /// <summary>把选中的部件映射成要改色的部位（眼睛由调用方按「両目/左目/右目」另行决定）。</summary>
    private static MaidParts.PARTS_COLOR GetColorTarget(MPN mpn)
    {
        if (mpn == Mpn.folder_matsuge_up) return MaidParts.PARTS_COLOR.MATSUGE_UP;
        if (mpn == Mpn.folder_matsuge_low) return MaidParts.PARTS_COLOR.MATSUGE_LOW;
        if (mpn == Mpn.folder_futae) return MaidParts.PARTS_COLOR.FUTAE;

        if (mpn == Mpn.hairf || mpn == Mpn.hairr || mpn == Mpn.hairt ||
            mpn == Mpn.hairs || mpn == Mpn.hairaho || mpn == Mpn.haircolor)
            return MaidParts.PARTS_COLOR.HAIR;

        if (mpn == Mpn.underhair || mpn == Mpn.folder_underhair)
            return MaidParts.PARTS_COLOR.UNDER_HAIR;

        if (mpn == Mpn.mayu || mpn == Mpn.folder_mayu) return MaidParts.PARTS_COLOR.EYE_BROW;
        if (mpn == Mpn.chikubi || mpn == Mpn.chikubicolor) return MaidParts.PARTS_COLOR.NIPPLE;

        return MaidParts.PARTS_COLOR.SKIN;
    }

    private void changeColor(MaidParts.PartsColor partsColor, MaidParts.PARTS_COLOR partsColorType,
        int[] values,
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

    public void LoadMenuFiles(bool isInit = false)
    {
        Log.LogMessage("[PropMyItem] LoadMenuFiles...st");
        try
        {
            var
                menuItems =
                    new List<SMenuInfo>(); //COM3D2.PropMyItem.PluginConfig.Instance.MenuItems;//new List<SMenuInfo>();
            var dictionary = new Dictionary<string, MenuInfo>();

            // 每次都重建 MPN 桶：以前只在 isInit 时重建，一旦中途抛异常就会留下一个空字典，
            // 之后所有道具都无处可放（界面全空）。用索引器赋值，枚举里万一有别名值也不会抛。
            _mpnMenuListDictionary = new Dictionary<MPN, List<MenuInfo>>();
            foreach (var obj in Enum.GetValues(typeof(MPN)))
                _mpnMenuListDictionary[(MPN)obj] = new List<MenuInfo>();

            _dbAccepted = 0;
            _dbSkippedNoMpn = 0;
            _dbSkippedNoIcon = 0;
            _dbSkippedSamples = string.Empty;
            _dbNoMpnSamples = string.Empty;

            if (!isInit)
            {
                using (var enumerator =
                       PluginConfig.Instance.MenuItems.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var smenuInfo = enumerator.Current;
                        if (!dictionary.ContainsKey(smenuInfo.FileName))
                            dictionary.Add(smenuInfo.FileName, new MenuInfo(smenuInfo));
                    }
                }
            }

            Log.LogMessage("[PropMyItem] buckets = " + _mpnMenuListDictionary.Count +
                           " isInit=" + isInit + " cache=" + dictionary.Count);

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
            // 先打一次分布：mod 扫描如果卡住/中断，至少这里的数据已经出来了
            LogMenuDistribution();
            GetModFiles(ref list, dictionary, dictionary2, dictionary3, ref menuItems); // 여기서 에러남
            Log.LogMessage("[PropMyItem] 完了3 " + menuItems.Count);
            SetVariationMenu(dictionary2, dictionary3, ref list);
            sort(false, true);
            setColorSet();
            LogMenuDistribution();
            PluginConfig.Instance.MenuItems = menuItems;
            PluginConfig.Instance.Save();
            if (dictionary.Count == 0) Log.LogMessage("[PropMyItem] 完了");

            _selectedFolder = 0;
            _selectedMPN = Mpn.null_mpn;
            _selectedCategory = -1;
            _selectedItem = null;
            _selectedVariationItem = null;
            _selectedPresetList.Clear();
            _selectedItem = null;
            _selectedVariationItem = null;
            _scrollPosition.y = 0f;
            var selectedMPN = Mpn.head;
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
                       PluginConfig.Instance.MenuItems.Count);
        Log.LogMessage("[PropMyItem] LoadMenuFiles...ed " + _mpnMenuListDictionary.Count);
    }

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
                            if (a.ColorNumber != b.ColorNumber)
                                return a.ColorNumber - b.ColorNumber;

                            return string.Compare(a.FileName, b.FileName);
                        });
        }
    }

    private void GetMainMenuFiles(ref List<MenuInfo> variationMenuList,
        Dictionary<string, MenuInfo> loadItems,
        Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic,
        ref List<SMenuInfo> saveItems)
    {
        _menuList.Clear();
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.menu",
            SearchOption.AllDirectories);
        var dictionary = new Dictionary<string, string>();
        foreach (var text in files)
        {
            var key = Path.GetFileName(text).ToLower();
            if (!dictionary.ContainsKey(key)) dictionary.Add(key, text);
        }

        var list = new List<string>(); //saveItems.Select(x=>x.FileName); //new List<string>();

        var menuDataBase = GameMain.Instance.MenuDataBase;

        Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles1 " + saveItems.Count);
        Log.LogMessage("[PropMyItem] MenuDataBase entries = " + menuDataBase.GetDataSize());
        // foreach (string text2 in menuFiles)
        for (var j = 0; j < menuDataBase.GetDataSize(); j++)
        {
            // 用游戏解析好的数据，不再自己读 .menu 文件（COM3D2.5 上格式有差异，手写解析器会整体失败）
            menuDataBase.SetIndex(j);
            var menuFileName = menuDataBase.GetMenuFileName();
            var menuInfo = CreateMenuInfoFromDataBase(menuFileName);
            ParseMainMenuFile(menuFileName, list, ref variationMenuList, loadItems, dictionary,
                favDic,
                colorLockDic, ref saveItems, menuInfo);
        }

        Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles2 " + saveItems.Count);
        if (saveItems.Count > 0)
            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles2 " +
                           saveItems[saveItems.Count - 1].FileName);

        var modMenuFiles = GameUty.ModOnlysMenuFiles;
        Log.LogMessage("[PropMyItem] mod menus = " + (modMenuFiles == null ? -1 : modMenuFiles.Length));
        var modIndex = 0;
        if (modMenuFiles != null)
            foreach (var menuFile in modMenuFiles)
            {
                ParseMainMenuFile(menuFile, list, ref variationMenuList, loadItems, dictionary, favDic,
                    colorLockDic,
                    ref saveItems);

                modIndex++;
                if (modIndex % 5000 == 0)
                    Log.LogMessage("[PropMyItem] mod progress " + modIndex + " / " +
                                   modMenuFiles.Length);
            }

        Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles3 " + saveItems.Count);
        if (saveItems.Count > 0)
            Log.LogMessage("[PropMyItem] PropMyItem.GetMainMenuFiles3 " +
                           saveItems[saveItems.Count - 1].FileName);
    }

    /// <summary>
    ///     把 MenuDataBase 里已经解析好的当前条目（调用方已 SetIndex）转成 MenuInfo。
    ///     游戏自己也是这么读的（SceneEdit.ReadMenuItemDataFromNative），而且这套 API 在
    ///     2.x / 3.x 上完全一致，比手写 .menu 二进制解析可靠得多。
    /// </summary>
    private MenuInfo CreateMenuInfoFromDataBase(string fileName)
    {
        var menuDataBase = GameMain.Instance.MenuDataBase;

        var raw = menuDataBase.GetCategoryMpn();
        var mpn = ToMpn(raw);
        if (mpn == Mpn.null_mpn)
        {
            // 原生层给的数值可能和托管枚举对不上，退回按名字解析一次
            var mpnText = menuDataBase.GetCategoryMpnText();
            if (!EnumUtil.TryParse(mpnText, true, out mpn) || mpn == Mpn.null_mpn)
            {
                _dbSkippedNoMpn++;
                if (_dbNoMpnSamples.Length < 300)
                    _dbNoMpnSamples += " " + fileName + "(" + raw + "/" + mpnText + ")";
                return null;
            }
        }

        // 图标只影响缩略图，缺了就缺了，不要因此丢掉整条（否则缓存会把「空图标」继承下去）
        var iconName = menuDataBase.GetIconS();
        if (string.IsNullOrEmpty(iconName))
        {
            _dbSkippedNoIcon++;
            if (_dbSkippedSamples.Length < 160) _dbSkippedSamples += " " + fileName;
        }

        _dbAccepted++;

        var itemName = menuDataBase.GetMenuName();
        if (string.IsNullOrEmpty(itemName)) itemName = menuDataBase.GetItemName();
        if (string.IsNullOrEmpty(itemName)) itemName = Path.GetFileNameWithoutExtension(fileName);

        return new MenuInfo
        {
            MPN = mpn,
            ItemName = itemName,
            IconName = iconName,
            Priority = menuDataBase.GetPriority(),
            ColorSetMPN = ToMpn(menuDataBase.GetColorSetMpn()),
            ColorSetMenuName = menuDataBase.GetMenuNameInColorSet()
        };
    }

    /// <summary>把原生层给的整数转成 MPN，越界或未定义时退回 null_mpn。</summary>
    private static MPN ToMpn(int value)
    {
        return Enum.IsDefined(typeof(MPN), value) ? (MPN)value : Mpn.null_mpn;
    }

    private int CountMenu(MPN mpn)
    {
        return _mpnMenuListDictionary.TryGetValue(mpn, out var list) ? list.Count : 0;
    }

    /// <summary>
    ///     排查用：列出条目最多的 MPN 桶。2.5 把服装拆成叠穿槽位后，靠这行日志能直接看出
    ///     道具实际落在哪个 MPN 上，不用再猜。
    /// </summary>
    private void LogMenuDistribution()
    {
        var entries = new List<KeyValuePair<MPN, List<MenuInfo>>>();
        foreach (var pair in _mpnMenuListDictionary)
            if (pair.Value.Count > 0)
                entries.Add(pair);

        entries.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));

        var text = "[PropMyItem] top MPN buckets:";
        for (var i = 0; i < entries.Count && i < 25; i++)
            text += " " + entries[i].Key + "=" + entries[i].Value.Count;

        Log.LogMessage(text);
        Log.LogMessage("[PropMyItem] db accepted=" + _dbAccepted +
                       " noMpn=" + _dbSkippedNoMpn +
                       " noIcon(kept)=" + _dbSkippedNoIcon +
                       " sample:" + _dbSkippedSamples);
        Log.LogMessage("[PropMyItem] db noMpn samples:" + _dbNoMpnSamples);
    }

    // lmao
    private void ParseMainMenuFile(string menuFile, List<string> list,
        ref List<MenuInfo> variationMenuList,
        Dictionary<string, MenuInfo> loadItems, Dictionary<string, string> dictionary,
        Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic,
        ref List<SMenuInfo> saveItems, MenuInfo prebuilt = null)
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
                    // prebuilt 来自 MenuDataBase；没有时（mod 菜单）才自己解析文件
                    var menuInfo = prebuilt;
                    if (menuInfo == null && !loadItems.TryGetValue(fileName, out menuInfo))
                        menuInfo = MenuModParser.ParseMenu(menuFile);

                    if (menuInfo != null && menuInfo.MPN != Mpn.null_mpn)
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
                        if (_menuMPNCategoryDic.TryGetValue(menuInfo.MPN, out empty))
                            menuInfo.CategoryName = empty;

                        list.Add(fileName);

                        // 图标只影响缩略图，不再决定条目要不要入库。
                        // 以前 IconName 为空时整条被丢掉，而缓存又会把这个「空图标」状态继承下去，
                        // 结果就是界面永久空白，还查不出原因。
                        var isVariation = Regex.IsMatch(menuFile, "_z\\d{1,4}") ||
                                          menuFile.Contains("_zurashi") ||
                                          menuFile.Contains("_mekure") ||
                                          menuFile.Contains("_porori") ||
                                          menuFile.Contains("_back");

                        if (isVariation && !string.IsNullOrEmpty(menuInfo.IconName))
                        {
                            variationMenuList.Add(menuInfo);
                        }
                        else
                        {
                            if (favDic.ContainsKey(menuInfo.FileName))
                                menuInfo.IsFavorite = true;

                            if (colorLockDic.ContainsKey(menuInfo.FileName))
                                menuInfo.IsColorLock = true;

                            menuInfo.ColorNumber = 0;
                            menuInfo.VariationMenuList.Add(menuInfo);
                            _mpnMenuListDictionary[menuInfo.MPN].Add(menuInfo);
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

    private void GetModFiles(ref List<MenuInfo> variationMenuList,
        Dictionary<string, MenuInfo> loadItems,
        Dictionary<string, string> favDic, Dictionary<string, string> colorLockDic,
        ref List<SMenuInfo> saveItems)
    {
        var list = new List<string>();
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.mod",
            SearchOption.AllDirectories);
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
                    if (_menuMPNCategoryDic.TryGetValue(menuInfo.MPN, out empty))
                        menuInfo.CategoryName = empty;

                    var text3 = fileName2.ToLower();
                    if (Regex.IsMatch(text3, "_z\\d{1,4}") || text3.Contains("_porori") ||
                        text3.Contains("_zurashi") || text3.Contains("_mekure") ||
                        text3.Contains("_back"))
                    {
                        variationMenuList.Add(menuInfo);
                    }
                    else
                    {
                        if (favDic.ContainsKey(menuInfo.FileName.ToLower()))
                            menuInfo.IsFavorite = true;

                        if (colorLockDic.ContainsKey(menuInfo.FileName.ToLower()))
                            menuInfo.IsColorLock = true;

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

    private void SetVariationMenu(Dictionary<string, string> favDic,
        Dictionary<string, string> colorLockDic,
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
                if (array.Length > 1)
                    int.TryParse(array[1].Remove(0, 3).Split('.', '_')[0], out colorNumber);
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

                    if (colorLockDic.ContainsKey(menuInfo.FileName.ToLower()))
                        menuInfo.IsColorLock = true;

                    menuInfo.ColorNumber = 0;
                    menuInfo.VariationMenuList.Add(menuInfo);
                    list2.Add(menuInfo);
                }
            }
        }

        foreach (var menuInfo3 in list2) _mpnMenuListDictionary[menuInfo3.MPN].Add(menuInfo3);
    }

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

    private class FolderMenu
    {
        public readonly string[] Categories;

        public readonly string Name = string.Empty;

        public FolderMenu(string name, string[] categories)
        {
            Name = name;
            Categories = categories;
        }
    }
}