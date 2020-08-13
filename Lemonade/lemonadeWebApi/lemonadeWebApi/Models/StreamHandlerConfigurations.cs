using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lemonadeWebApi.Models
{
    public class StreamHandlerConfigurations
    {
        public long MaxMemoryUsage { get; set; }
        public long CheckForMemoryUsageIterationsInterval { get; set; }
        
    }
}
