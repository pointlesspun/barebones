
using System;

namespace BareBones.Services.PropertyTable
{
    public abstract class AbstractParseFunction : IParseFunction
    {
        public string Name { get; set; }

        public Action<(int, int), string> Log { get; set; }

        public Action<string> OnMatch { get; set; }

        public abstract bool CanParse(string text, int start);

        public abstract ParseResult Parse(string text, int start);

        public override string ToString()
        {
            return Name;
        }
    }
}
