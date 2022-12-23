using BareE.DataStructures;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BareE.Messages
{

    [MessageAttribute("EmitAsset")]
    public struct EmitAsset
    {
        public String Type;
        public AttributeCollection ReferenceDefinition;
    }
    [MessageAttribute("EmitMeta")]
    public struct EmitMeta
    {
        public String Type;
        public AttributeCollection MetaDefinition;
    }
}
