using System;

namespace AssetBuilder.Models
{
    public class EnvironmentUrl : IComparable<EnvironmentUrl>
    {
        public string Name { get; set; }

        public string BaseUrl { get; set; }

        #region IComparable
        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="other">An object to compare with this instance. Comparisons use 'InvariantCultureIgnoreCase'.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// Value
        /// 
        /// Meaning
        /// 
        /// Less than zero
        /// 
        /// This instance precedes <paramref name="other" /> in the sort order.
        /// 
        /// Zero
        /// 
        /// This instance occurs in the same position in the sort order as <paramref name="other" />.
        /// 
        /// Greater than zero
        /// 
        /// This instance follows <paramref name="other" /> in the sort order.</returns>
        public int CompareTo(EnvironmentUrl other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion
    }
}