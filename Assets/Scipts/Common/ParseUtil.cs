using System;
using System.Globalization;

public struct ParseResult
{
    public object value;
    public int charactersRead;
    public bool isSuccess;

    public ParseResult(object value, int charactersRead, bool isSuccess)
    {
        this.value = value;
        this.charactersRead = charactersRead;
        this.isSuccess = isSuccess;
    }
};
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

    public static bool IsMatch(this string str, string token, int strPosition, bool ignoreCase = false) =>
        ignoreCase
            ? MatchLength(str.ToLowerInvariant(), token.ToLowerInvariant(), strPosition, 0) == token.Length
            : MatchLength(str, token, strPosition, 0) == token.Length;

    /** Returns number of matching characters in str1 and str2 starting at the given positions */
    public static int MatchLength(string str1, string str2, int start1 = 0, int start2 = 0)
    {
        var idx1 = start1;
        var idx2 = start2;

        while (idx1 < str1.Length && idx2 < str2.Length && str1[idx1] == str2[idx2])
        {
            idx1++;
            idx2++;
        }

        return idx2 - start2;
    }

    public static bool CompareIgnoreCase(this char c1, char c2) => char.ToLower(c1) == char.ToLower(c2);

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
                return Math.Min(str.Length, (idx + 1)) - start;
            }
        }
        return -1;
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
        return idx;
    }

    /**
     * Given a string, determines the line and column the current index is at
     */
    public static (int line, int column) GetLineAndColumn(this string text, int idx)
    {

        var column = 0;
        var line = 0;

        do
        {
            if (idx > 0)
            {
                if (idx < text.Length && (text[idx] == '\n' || text[idx] == '\r'))
                {
                    line++;
                    idx--;
                    break;
                }

                idx--;

                if (idx < text.Length && (text[idx] == '\n' || text[idx] == '\r'))
                {
                    line++;
                    idx--;
                    break;
                }

                column++;
            }
        } while (idx > 0);


        while (idx > 0)
        {
            if (text[idx] == '\n' || text[idx] == '\r')
            {
                line++;
            }

            idx--;
        }

        return (line, column);
    }

    public static bool IsUnsignedLongOrShort(this string numberString) =>
        (numberString.Length > 2 && char.ToLower(numberString[numberString.Length - 2]) == 'u');


    public static int SkipUntil(string text, int start, Func<char,bool> condition)
    {
        var idx = start;

        while (idx < text.Length && !condition(text[idx])) 
        {
            idx++;
        }

        return idx;
    }

    public static int SkipUntil(string text, int start, Func<string, int, bool> condition)
    {
        var idx = start;

        while (idx < text.Length && !condition(text, idx))
        {
            idx++;
        }

        return idx;
    }

    public static object ParseNumber(this string numberString)
    {
        if (String.Empty == numberString)
        {
            throw new ArgumentException("Attempting to parse an empty string.");
        }

        if (numberString.Length > 2 && char.ToLower(numberString[1]) == 'x')
        {
            return Convert.ToInt32(numberString, 16);
        }

        var lastCharacter = numberString[numberString.Length - 1];

        if (char.IsDigit(lastCharacter))
        {
            bool hasExponent = numberString.IndexOf('e') >= 0 || numberString.IndexOf('E') >= 0;
            if (numberString.IndexOf('.') >= 0 || hasExponent)
            {
                if (hasExponent)
                {
                    return double.Parse(numberString, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
                }

                return double.Parse(numberString);
            }

            return int.Parse(numberString);
        }

        lastCharacter = char.ToLower(lastCharacter);

        if ("fulmsbd".IndexOf(lastCharacter) < 0)
        {
            throw new ArgumentException( "found unknown character ending: '" + lastCharacter + "'.");
        }
        
        if (lastCharacter == 'm')
        {
            return Decimal.Parse(numberString.Substring(0, numberString.Length - 1));
        }
        else if (lastCharacter == 'f')
        {
            return float.Parse(numberString.Substring(0, numberString.Length - 1));
        }
        else if (lastCharacter == 'd')
        {
            bool hasExponent = numberString.IndexOf('e') >= 0 || numberString.IndexOf('E') >= 0;
            if (hasExponent)
            {
                return double.Parse(numberString.Substring(0, numberString.Length - 1), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            }

            return double.Parse(numberString.Substring(0, numberString.Length - 1));
        }
        else if (lastCharacter == 'u')
        {
            return uint.Parse(numberString.Substring(0, numberString.Length - 1));
        }
        else if (lastCharacter == 's')
        {
            if (ParseUtil.IsUnsignedLongOrShort(numberString))
            {
                return ushort.Parse(numberString.Substring(0, numberString.Length - 2));
            }

            return short.Parse(numberString.Substring(0, numberString.Length - 1));
        }
        else if (lastCharacter == 'b')
        {
            return byte.Parse(numberString.Substring(0, numberString.Length - 1));
        }
        else if (lastCharacter == 'l')
        {
            if (ParseUtil.IsUnsignedLongOrShort(numberString))
            {
                return ulong.Parse(numberString.Substring(0, numberString.Length - 2));
            }

            return long.Parse(numberString.Substring(0, numberString.Length - 1));
        }

        throw new NotImplementedException("... yeah so something went wrong on the implementation side. Sorry.");
    }
}