
namespace BareBones.Services.PropertyTable
{
    public interface IPolyPropsExtension
    {
        bool CanParse(string text, int start);

        ParseResult Parse(string text, int start);
    }
}
