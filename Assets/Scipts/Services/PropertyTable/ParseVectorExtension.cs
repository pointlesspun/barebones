
using System;
using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class ParseVectorExtension : IPolyPropsExtension
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

        public (object value, int charactersRead) Parse(string text, int start)
        {
            // skip one character then parse a list
            var (list, charactersRead) = PolyPropsParser.ParseList(text, start + 1, Config);

            if (charactersRead > 1)
            {
                if (list.Count == 2)
                {
                    return (new Vector2(Convert.ToSingle(list[0]), Convert.ToSingle(list[1])), charactersRead + 1);
                }
                else if (list.Count == 3)
                {
                    return (new Vector3(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2])), charactersRead + 1);
                }
                else if (list.Count == 4)
                {
                    return (new Vector4(Convert.ToSingle(list[0]), Convert.ToSingle(list[1]), Convert.ToSingle(list[2]), Convert.ToSingle(list[3])), charactersRead + 1);
                }
            }

            Config.Log(ParseUtil.GetLineAndColumn(text, start), "failed to parse Vector");
            return PolyPropsParser.Error<object>();
        }
    }
}
