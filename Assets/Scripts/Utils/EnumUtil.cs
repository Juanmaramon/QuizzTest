using UnityEngine;
using System.Collections;

public static class EnumUtil
{
    public static T ToEnum<T>(this string value, T defaultValue)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        return (T)System.Enum.Parse(typeof(T), value, true);
    }

    public static string[] GetNames<T>()
    {
        return System.Enum.GetNames(typeof(T));
    }

	public static int GetCount<T>()
	{
		return System.Enum.GetNames(typeof(T)).Length;;
	}
}