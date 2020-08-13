using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lemonadeWebApi.Interfaces
{
    public interface IHandlersRunner:IDisposable
    {
        Task<bool> IsAllHandlersFinished();
        Task AddHandlerAndRun(IStreamHandler handler);
    }
}
