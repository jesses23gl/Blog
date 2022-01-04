using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Helper
{
    public interface ICaching
    {
        object Get(string cacheKey);
        void Set(string cacheKey, object cacaheValue);
    }
}
