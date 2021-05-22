using System;
using System.Collections.Generic;
using System.Linq;

namespace BareE.Widgets
{
    internal class creditSorter : IEqualityComparer<AssetCredits>, IComparer<AssetCredits>
    {
        public int Compare(AssetCredits x, AssetCredits y)
        {
            if (x.CreditType == y.CreditType)
            {
                return x.Asset.CompareTo(y.Asset);
            }
            else
            {
                return x.CreditType.CompareTo(y.CreditType);
            }
        }

        public bool Equals(AssetCredits x, AssetCredits y)
        {
            if (x.CreditType != y.CreditType) return false;
            if (x.Asset != y.Asset) return false;
            if (x.Filenames.Length != y.Filenames.Length) return false;
            if (x.Authors.Length != y.Authors.Length) return false;
            if (x.Licenses.Length != y.Authors.Length) return false;
            foreach (String f in x.Filenames)
                if (!y.Filenames.Contains(f))
                    return false;
            foreach (String f in x.Authors)
                if (!y.Authors.Contains(f))
                    return false;
            foreach (String f in x.Licenses)
                if (!y.Licenses.Contains(f))
                    return false;

            return true;
        }

        public int GetHashCode(AssetCredits obj)
        {
            return $"{obj.CreditType}{obj.Asset}".GetHashCode();
        }
    }
}
