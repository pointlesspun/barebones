
namespace BareBones.Services.PropertyTable
{
    public interface IPolyPropsParseFunction
    {
        bool CanParse(string text, int start);

        ParseResult Parse(string text, int start);
    }
}
