using BareE.DataStructures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LilScrapper.Components
{
    [Component("Animation")]
    public struct Animation
    {
        public String AnimationName { get; set; }
        public String State { get; set; }
        public int T { get; set; }
        public long lastUpdate { get; set; }
    }
}
