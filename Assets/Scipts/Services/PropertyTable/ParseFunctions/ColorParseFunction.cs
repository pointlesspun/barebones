
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class ColorParseFunction : RegexParseFunction
    {
        public static readonly Regex DefaultColorRegex = new Regex(@"#([[a-fA-F0-9]{8}|[a-fA-F0-9]{6})");

        public ColorParseFunction()
        {
            Matcher = DefaultColorRegex;
        }

        public override object Map(string text, int idx, int length)
        {
            var r = Convert.ToInt32(text.Substring(idx + 1, 2), 16) / 255.0f; 
            var g = Convert.ToInt32(text.Substring(idx + 3, 2), 16) / 255.0f;
            var b = Convert.ToInt32(text.Substring(idx + 5, 2), 16) / 255.0f;
            var a = length == 7 ? 1.0f : Convert.ToInt32(text.Substring(idx + 7, 2), 16) / 255.0f;

            return new Color(r, g, b, a);
        }
    }
}
