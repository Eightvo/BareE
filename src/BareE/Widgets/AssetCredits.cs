
using System;

namespace BareE.Widgets
{
    public struct AssetCredits
    {
        public String Asset { get; set; }

        public AssetCreditType CreditType { get; set; }
        public String[] Filenames { get; set; }
        public String[] Authors { get; set; }
        public String[] Licenses { get; set; }
        public String[] Urls { get; set; }
        public String[] Text { get; set; }
    }
}
