using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lemonadeWebApi.Interfaces
{
    public interface IStreamHandler:IDisposable
    {
        Task ContinueParse();
    }
}
