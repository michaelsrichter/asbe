using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asbe.send
{
    public class ASBEConfig
    {
        public string SBConnectionString { get; set; }
        public string QueueName { get; set; }
        public int ParallelDegrees { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
    }
}
