using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Classes
{
    public class KeyEqualityComparer<T> : IEqualityComparer<T>
    {
        private Func<T, object> F { get; }

        public KeyEqualityComparer(Func<T, object> getkey)
        {
            this.F = getkey;
        }
        public bool Equals(T x, T y)
        {
            return F(x).Equals(F(y));
        }

        public int GetHashCode(T x)
        {
            return F(x).GetHashCode();
        }
    }
}
