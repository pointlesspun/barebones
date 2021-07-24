using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ParseUtil
{
    /**
     * Returns the number of whitespace characters at the beginning of the string
     */
    public static int GetWhiteSpaceLength(this string s, string whiteSpace = " \n\t\r", int startIndex = 0)
    {
        var i = startIndex;
        while (whiteSpace.IndexOf(s[i]) >= 0)
        {
            i++;
        }

        return i - startIndex;
    }

    /** Returns number of matching characters in str1 and str2 starting at the given positions */
    public static int MatchLength(string str1, string str2, int start1 = 0, int start2 = 0)
    {
        var idx1 = start1;
        var idx2 = start2;

        while (idx1 < str1.Length && idx2 < str2.Length && str1[idx1] == str2[idx2] )
        {
            idx1++;
            idx2++;
        }

        return idx2 - start2;
    }

    /**
     * Get the length from a string starting with the given delimiter until and including the end of the delimiter
     */
    public static int ReadScopedString(this string str, int start, char delimiter = '"', char escapeChar ='\\')
    {
        var idx = start;

        if (str[idx] == delimiter)
        {
            idx++;

            while (idx < str.Length && str[idx] != delimiter)
            {
                if (str[idx] == escapeChar)
                {
                    idx++;
                }

                idx++;
            }

            if (idx < str.Length && str[idx] == delimiter)
            {
                return (idx + 1) - start;
            }
            
            Debug.LogWarning("ParseUtil.ReadScopedString string does not end with a delimiter (" + delimiter + ").");
            return -1;
        }
        else
        {
            Debug.LogWarning("ParseUtil.ReadScopedString string does not start with a delimiter (" + delimiter + ").");
            return -1;
        }
    }

    /**
     * Returns the index of the first character matching in str starting at the given index
     */
    public static int IndexOfAny(this string str, string matches, int startIndex = 0)
    {
        var idx = startIndex;

        while(idx < str.Length && matches.IndexOf(str[idx]) == -1)
        {
            idx++;
        }

        return idx < str.Length ? idx : -1;
    }

    /**
     * Returns the index of the first character NOT matching in str starting at the given index
     */
    public static int Skip(this string str, string matches, int startIndex = 0)
    {
        var idx = startIndex;

        while (idx < str.Length && matches.IndexOf(str[idx]) >= 0)
        {
            idx++;
        }

        //return idx < str.Length ? idx : -1;
        return idx;
    }
}

