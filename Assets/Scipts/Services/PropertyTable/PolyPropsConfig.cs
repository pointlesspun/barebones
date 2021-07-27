using System;

namespace BareBones.Services.PropertyTable
{
    public class PolyPropsConfig
    {
        public static readonly PolyPropsConfig Default = new PolyPropsConfig();

        public string WhiteSpace { get; set; } = " \t\r\n";

        public string Separators { get; set; } = " \t\n\r,[]{}";

        public string MapDelimiters { get; set; } = "{}";

        public string ListDelimiters { get; set; } = "[]";

        public string StringDelimiters { get; set; } = "\"'";

        public string KeyValueSeparator { get; set; } = ":";

        public string CompositeValueSeparator { get; set; } = ",";

        public string SingleLineCommentToken { get; set; } = "//";

        public string UnquotedStringsDelimiters { get; set; } = "\n\r,[]{}:";

        public string BooleanTrue { get; set; } = "true";

        public string BooleanFalse { get; set; } = "false";

        public string NullValue { get; set; } = "null";

        public Action<(int, int), string> Log { get; set; } = (position, msg) => Console.WriteLine(msg);

        public IPolyPropsExtension ParseExtensions { get; set; }

        /*public Func<string, int, PolyPropsConfig, bool> CanParse { get; set; }

        public Func<string, int, PolyPropsConfig, (object, int)> Parse { get; set; }*/
    }
}