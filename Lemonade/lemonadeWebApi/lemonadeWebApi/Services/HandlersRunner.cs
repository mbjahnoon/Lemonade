using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using lemonadeWebApi.Interfaces;

namespace lemonadeWebApi.Services
{
    public class HandlersRunner:IHandlersRunner
    {
        private readonly List<IStreamHandler> _handlers;
        private long _numOfhandlersRunning = 0;
        public HandlersRunner()
        {
            _handlers = new List<IStreamHandler>();
        }

        public async Task<bool> IsAllHandlersFinished() // currently here for debug
        {
            while (true)
            {
                if (Interlocked.Read(ref _numOfhandlersRunning) == 0) return true;
                await Task.Delay(200);
            }
        }

        public async void AddHandlerAndRun(IStreamHandler handler)
        {
            Interlocked.Increment(ref _numOfhandlersRunning);
            _handlers.Add(handler);
            await handler.ContinueParse();
            _handlers.Remove(handler);
            handler.Dispose();
            Interlocked.Decrement(ref _numOfhandlersRunning);
        }

        public void Dispose()
        {
            foreach (var handler in _handlers)
            {
                handler?.Dispose();
            }
        }
    }
}
