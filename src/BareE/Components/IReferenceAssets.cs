using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Components
{
    public interface IReferenceAssets
    {
        IEnumerable<Tuple<String, String>> AssetReferences { get; }
    }
}
