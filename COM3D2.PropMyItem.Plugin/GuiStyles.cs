using UnityEngine;

namespace COM3D2.PropMyItem.Plugin
{
    // Token: 0x02000006 RID: 6
    public class GuiStyles
    {
        // Token: 0x04000007 RID: 7
        private static readonly int _fontSize = 12;

        // Token: 0x04000008 RID: 8
        private static readonly float _windowHeight = 12f;

        // Token: 0x04000009 RID: 9
        private static readonly float _scrollWidth = 20f;

        // Token: 0x0400000A RID: 10
        private static GUIStyle _windowStyle;

        // Token: 0x0400000B RID: 11
        private static GUIStyle _labelStyle;

        // Token: 0x0400000C RID: 12
        private static GUIStyle _textfieldStyle;

        // Token: 0x0400000D RID: 13
        private static GUIStyle _buttonStyle;

        // Token: 0x0400000E RID: 14
        private static GUIStyle _toggleStyle;

        // Token: 0x0400000F RID: 15
        private static GUIStyle _listStyle;

        // Token: 0x04000010 RID: 16
        private static GUIStyle _boxStyle;

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000017 RID: 23 RVA: 0x00002790 File Offset: 0x00000990
        public static int FontSize => _fontSize;

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x06000018 RID: 24 RVA: 0x00002797 File Offset: 0x00000997
        public static float WindowHeight => _windowHeight;

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000019 RID: 25 RVA: 0x0000279E File Offset: 0x0000099E
        public static float ScrollWidth => _scrollWidth;

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x0600001A RID: 26 RVA: 0x000027A5 File Offset: 0x000009A5
        public static float ControlHeight => FontSize * 2;

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x0600001B RID: 27 RVA: 0x000027AF File Offset: 0x000009AF
        public static float Margin => FontSize * 0.5f;

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x0600001C RID: 28 RVA: 0x000027C0 File Offset: 0x000009C0
        public static GUIStyle WindowStyle
        {
            get
            {
                if (_windowStyle == null)
                {
                    _windowStyle = new GUIStyle("box");
                    _windowStyle.fontSize = FontSize;
                    _windowStyle.alignment = TextAnchor.UpperRight;
                    _windowStyle.normal.textColor = Color.white;
                }

                return _windowStyle;
            }
        }

        // Token: 0x1700000A RID: 10
        // (get) Token: 0x0600001D RID: 29 RVA: 0x0000281C File Offset: 0x00000A1C
        public static GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle("label");
                    _labelStyle.fontSize = FontSize;
                    _labelStyle.alignment = TextAnchor.MiddleLeft;
                    _labelStyle.normal.textColor = Color.white;
                }

                return _labelStyle;
            }
        }

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x0600001E RID: 30 RVA: 0x00002878 File Offset: 0x00000A78
        public static GUIStyle TextFieldStyle
        {
            get
            {
                if (_textfieldStyle == null)
                {
                    _textfieldStyle = new GUIStyle("textfield");
                    _textfieldStyle.fontSize = FontSize;
                    _textfieldStyle.alignment = TextAnchor.MiddleLeft;
                    _textfieldStyle.normal.textColor = Color.white;
                }

                return _textfieldStyle;
            }
        }

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x0600001F RID: 31 RVA: 0x000028D4 File Offset: 0x00000AD4
        public static GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle("button");
                    _buttonStyle.fontSize = FontSize;
                    _buttonStyle.alignment = TextAnchor.MiddleCenter;
                    _buttonStyle.normal.textColor = Color.white;
                }

                return _buttonStyle;
            }
        }

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000020 RID: 32 RVA: 0x0000292F File Offset: 0x00000B2F
        public static GUIStyle ToggleStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _toggleStyle = new GUIStyle("toggle");
                    _toggleStyle.fontSize = FontSize;
                }

                return _toggleStyle;
            }
        }

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000021 RID: 33 RVA: 0x00002960 File Offset: 0x00000B60
        public static GUIStyle ListStyle
        {
            get
            {
                if (_listStyle == null)
                {
                    _listStyle = new GUIStyle();
                    _listStyle.onHover.background =
                        _listStyle.hover.background = new Texture2D(2, 2);
                    _listStyle.padding.left = _listStyle.padding.right = 4;
                    _listStyle.padding.top = _listStyle.padding.bottom = 1;
                    _listStyle.normal.textColor = _listStyle.onNormal.textColor = Color.white;
                    _listStyle.hover.textColor = _listStyle.onHover.textColor = Color.white;
                    _listStyle.active.textColor = _listStyle.onActive.textColor = Color.white;
                    _listStyle.focused.textColor = _listStyle.onFocused.textColor = Color.blue;
                    _listStyle.fontSize = FontSize;
                }

                return _listStyle;
            }
        }

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x06000022 RID: 34 RVA: 0x00002A99 File Offset: 0x00000C99
        public static GUIStyle BoxStyle
        {
            get
            {
                if (_listStyle == null) _boxStyle = new GUIStyle("box");

                return _boxStyle;
            }
        }
    }
}