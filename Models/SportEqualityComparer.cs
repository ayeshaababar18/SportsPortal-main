using System.Collections.Generic;

namespace SportsPortal.Models
{
    public class SportEqualityComparer : IEqualityComparer<Sport>
    {
        public bool Equals(Sport? x, Sport? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.SportID == y.SportID;
        }

        public int GetHashCode(Sport obj)
        {
            return obj.SportID.GetHashCode();
        }
    }
}
