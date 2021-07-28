
namespace BareBones.Services.PropertyTable
{
    public interface IPolyPropsParseFunction
    {
        bool CanParse(string text, int start = 0);

        ParseResult Parse(string text, int start = 0);
    }
}
