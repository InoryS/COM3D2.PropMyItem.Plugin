using System;

namespace COM3D2.PropMyItem.Plugin;

public static class EnumUtil
{
    public static T Parse<T>(string value)
    {
        return Parse<T>(value, true);
    }

    public static T Parse<T>(string value, bool ignoreCase)
    {
        return (T)Enum.Parse(typeof(T), value, ignoreCase);
    }

    public static bool TryParse<T>(string value, out T result)
    {
        return TryParse(value, true, out result);
    }

    public static bool TryParse<T>(string value, bool ignoreCase, out T result)
    {
        bool result2;
        try
        {
            result = (T)Enum.Parse(typeof(T), value, ignoreCase);
            result2 = true;
        }
        catch
        {
            result = default;
            result2 = false;
        }

        return result2;
    }
}