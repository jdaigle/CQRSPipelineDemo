using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRSPipeline.DemoAPI
{
    public class CommandContext
    {
        public AdventureWorksDbContext DbContext { get; set; }
        public object CurrentCommand { get; set; }
    }
}
