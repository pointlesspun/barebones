using System.Collections.Generic;

namespace BareBones.Services.PropertyTable
{
    public interface IPropertyTable
    {
        void Clear();
        void Create<T>(int category, string id, T value);
        void Delete<T>(int category, string id);
        Dictionary<string, object> GetProperties(int category);
        T Read<T>(int category, string id, T value);
        void Update<T>(int category, string id, T value);

        void Add(PropertyTable other);
    }
}