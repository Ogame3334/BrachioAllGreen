using System;
using UnityEngine;

public static class EnumToString
{
    public static string Enum2String<T>(int i) where T : Enum {
        return ((T)Enum.ToObject(typeof(T), i)).ToString();
    }
}
