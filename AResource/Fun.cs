using System.Collections;
using System;
using System.Text;
using Data;

public class Fun
{
    public static string EnsureFileNameWithExtenision_XML(string name)
    {
        if (name.IndexOf(Setting.XMLExtension) == -1)
            name += Setting.XMLExtension;
        return name;
    }
    public static string EnsureFileNameWithOutExtenision_XML(string name)
    {
        int pos=name.IndexOf(Setting.XMLExtension);
        if (pos != -1)
            name = name.Substring(0, pos);
        return name;
    }
    public static string ParseColor_NGUIRGB(Data.Color color)
    {
        Color32 c32 = new Color32();
        c32 = color;
        return string.Format("[{0}{1}{2}]", Fun.ClampByteIn2(c32.r), Fun.ClampByteIn2(c32.g), Fun.ClampByteIn2(c32.b));
    }
    public static string BytesToString(byte[] arr)
    {
        if (arr == null || arr.Length == 0)
            return string.Empty;

        int i = arr.Length - 1;

        for (; i >= 0; --i)
        {
            if (arr[i] != 0)
                break;
        }
        return new string(Encoding.UTF8.GetChars(arr, 0, i + 1));
    }


    public static string ClampByteIn2(byte b)
    {
        string s = Convert.ToString(b, 16);
        if (s.Length == 1)
        {
            s=s.Insert(0, "0");
        }
        return s;
    }
};
