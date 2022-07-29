using System.Collections.Generic;
using AssetBuilder.UM.Models;

namespace AssetBuilder.Classes
{
    public class UserRoleComparer : IEqualityComparer<UserRole>
    {
        public bool Equals(UserRole x, UserRole y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.sprojoid == y.sprojoid && x.Id == y.Id && x.Name == y.Name && x.Assigned == y.Assigned;
        }

        public int GetHashCode(UserRole obj)
        {
            unchecked
            {
                var hashCode = (obj.sprojoid != null ? obj.sprojoid.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Id;
                hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ obj.Assigned.GetHashCode();
                return hashCode;
            }
        }
    }
}