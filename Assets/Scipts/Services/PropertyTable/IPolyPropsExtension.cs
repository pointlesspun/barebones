
namespace BareBones.Services.PropertyTable
{
    public interface IPolyPropsExtension
    {
        bool CanParse(string text, int start, PolyPropsConfig config);

        (object value, int charactersRead) Parse(string text, int start, PolyPropsConfig config);
    }
}
