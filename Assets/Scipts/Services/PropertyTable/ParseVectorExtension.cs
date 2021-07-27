
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class ParseVectorExtension : IPolyPropsParseFunction
    {
        public static readonly string VectorPrefix = "v[";

        public PolyPropsConfig Config { get; set; }

        public bool CanParse(string text, int start)
        {
            return text.IsMatch(VectorPrefix, start, true);
        }

        public ParseVectorExtension()
        {
        }

        public ParseVectorExtension(PolyPropsConfig config)
        {
            Config = config;
        }

        public ParseResult Parse(string text, int start)
        {
            // skip one character then parse a list
            var listResult = PolyPropsParser.ParseList(text, start + 1, Config);

            if (listResult.isSuccess)
            {
                var list = listResult.value as List<object>;

                if (list.Count == 2)
                {
                    return new ParseResult(new Vector2(Convert.ToSingle(list[0]), Convert.ToSingle(list[1])), listResult.charactersRead + 1, true);
                }
                else if (list.Count == 3)
                {
                    return new ParseResult(new Vector3(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2])), 
                            listResult.charactersRead + 1, true);
                }
                else if (list.Count == 4)
                {
                    return new ParseResult(new Vector4(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2]), Convert.ToSingle(list[3])),
                                listResult.charactersRead + 1, true);
                }
            }

            Config.Log(ParseUtil.GetLineAndColumn(text, start), "failed to parse Vector");
            return listResult;
        }
    }
}
