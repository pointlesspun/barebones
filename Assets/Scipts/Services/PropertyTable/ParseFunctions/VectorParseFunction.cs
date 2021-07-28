
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class VectorParseFunction : IPolyPropsParseFunction
    {
        public static readonly string DefaultVectorPrefix = "v[";

        public string VectorPrefix { get; set; } = DefaultVectorPrefix;

        public int ListOffset { get; set; } = 1;

        public IPolyPropsParseFunction ListFunction { get; set; }

        public Action<(int, int), string> Log { get; set; }

        public bool CanParse(string text, int start)
        {
            return text.IsMatch(VectorPrefix, start, true);
        }

        public ParseResult Parse(string text, int start) => Parse(text, start, ListFunction, ListOffset, Log);

        public static ParseResult Parse(string text, int start, IPolyPropsParseFunction listFunction, int listOffset, Action<(int, int), string> log = null)
        {
            // skip one character then parse a list
            var listResult = listFunction.Parse(text, start + listOffset);

            if (listResult.isSuccess)
            {
                var list = listResult.value as List<object>;

                if (list.Count == 2)
                {
                    return new ParseResult(new Vector2(Convert.ToSingle(list[0]), Convert.ToSingle(list[1])), listResult.charactersRead + listOffset, true);
                }
                else if (list.Count == 3)
                {
                    return new ParseResult(new Vector3(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2])), 
                            listResult.charactersRead + listOffset, true);
                }
                else if (list.Count == 4)
                {
                    return new ParseResult(new Vector4(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2]), Convert.ToSingle(list[3])),
                                listResult.charactersRead + listOffset, true);
                }
                else
                {
                    log?.Invoke(ParseUtil.GetLineAndColumn(text, start), "Vector can only handle 2 to 4 numbers, found " + list.Count + ".");
                    return new ParseResult(null, listResult.charactersRead, false);
                }
            }

            log?.Invoke(ParseUtil.GetLineAndColumn(text, start), "failed to parse Vector");
            return listResult;
        }
    }
}
