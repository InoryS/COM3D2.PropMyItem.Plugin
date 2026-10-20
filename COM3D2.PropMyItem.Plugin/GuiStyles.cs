using UnityEngine;

namespace COM3D2.PropMyItem.Plugin;

public class GuiStyles
{
    private static GUIStyle _windowStyle;

    private static GUIStyle _labelStyle;

    private static GUIStyle _textfieldStyle;

    private static GUIStyle _buttonStyle;

    private static GUIStyle _toggleStyle;

    private static GUIStyle _listStyle;

    private static GUIStyle _boxStyle;

    public static int FontSize { get; } = 12;

    public static float WindowHeight { get; } = 12f;

    public static float ScrollWidth { get; } = 20f;

    public static float ControlHeight => FontSize * 2;

    public static float Margin => FontSize * 0.5f;

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

    public static GUIStyle BoxStyle
    {
        get
        {
            if (_listStyle == null) _boxStyle = new GUIStyle("box");

            return _boxStyle;
        }
    }
}