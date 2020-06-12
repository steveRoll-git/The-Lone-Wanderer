using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtension
{
    public static string Substr(this string s, int from, int to)
    {
        return s.Substring(from, to - from);
    }
}
