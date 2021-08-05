
namespace BareBones.Services.PropertyTable
{
    public delegate int ParseOperation(string text, int start);

    public interface IParseFunction
    {
        string Name { get; set; }

        bool CanParse(string text, int start = 0);

        ParseResult Parse(string text, int start = 0);
    }
}
