﻿
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class ParseColorExtension : IPolyPropsExtension
    {
        public static readonly Regex ColorRegex = new Regex(@"#([[a-fA-F0-9]{8}|[a-fA-F0-9]{6})");

        public bool CanParse(string text, int start, PolyPropsConfig config)
        {
            return ColorRegex.IsMatch(text, start);
        }

        public (object value, int charactersRead) Parse(string text, int start, PolyPropsConfig config)
        {
            var length = ColorRegex.Match(text, start).Length;

            var r = Convert.ToInt32(text.Substring(start + 1, 2), 16) / 255.0f; 
            var g = Convert.ToInt32(text.Substring(start + 3, 2), 16) / 255.0f;
            var b = Convert.ToInt32(text.Substring(start + 5, 2), 16) / 255.0f;
            var a = length == 7 ? 1.0f : Convert.ToInt32(text.Substring(start + 7, 2), 16) / 255.0f;

            return (new Color(r, g, b, a), length);
        }
    }
}