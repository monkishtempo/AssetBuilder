using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace StringCompare
{
    public class LD
    {
        public static int[,] LevenshteinDistance(string s, string t)
        {
            int m = s.Length, n = t.Length;
            // for all i and j, d[i,j] will hold the Levenshtein distance between
            // the first i characters of s and the first j characters of t
            // note that d has (m+1)*(n+1) values
            var d = new int[m + 1, n + 1];

            int max = m > n ? m : n;

            for (int i = 1; i <= max; i++)
            {
                if (i <= m) d[i, 0] = i;
                if (i <= n) d[0, i] = i;
            }

            for (int j = 1; j <= n; j++)
            {
                for (int i = 1; i <= m; i++)
                {
                    int sc = s[i - 1] == t[j - 1] ? 0 : 1;
                    int del = d[i - 1, j] + 1;
                    int ins = d[i, j - 1] + 1;
                    int sub = d[i - 1, j - 1] + sc;
                    d[i, j] = del < ins ? (del < sub ? del : sub) : (ins < sub ? ins : sub);
                }
            }

            return d;
        }

        public static int[,] LevenshteinDistance<T>(IList<T> s, IList<T> t, IComparer<T> comparer) where T :class
        {
            //var res = new List<Difference<T>>();
            int m = s.Count(), n = t.Count();
            // for all i and j, d[i,j] will hold the Levenshtein distance between
            // the first i characters of s and the first j characters of t
            // note that d has (m+1)*(n+1) values
            var d = new int[m + 1, n + 1];

            int max = m > n ? m : n;

            for (int i = 1; i <= max; i++)
            {
                if (i <= m) d[i, 0] = i;
                if (i <= n) d[0, i] = i;
            }

            for (int j = 1; j <= n; j++)
            {
                for (int i = 1; i <= m; i++)
                {
                    int sc = comparer.Compare(s[i - 1], t[j - 1]) == 0 ? 0 : 1;
                    int del = d[i - 1, j] + 1;
                    int ins = d[i, j - 1] + 1;
                    int sub = d[i - 1, j - 1] + sc;
                    d[i, j] = del < ins ? (del < sub ? del : sub) : (ins < sub ? ins : sub);
                }
            }

            return d;
        }

        public static IList<DifferenceSets<T>> GetChangeSets<T>(IList<T> a1, IList<T> a2, IComparer<T> comparer) where T : class
        {
            var d = LevenshteinDistance(a1, a2, comparer);
            int x = d.GetUpperBound(0), y = d.GetUpperBound(1);
            List<DifferenceSets<T>> changes = new List<DifferenceSets<T>>();
            DifferenceSets<T> lastdiff = null;
            //string op;
            while (x > 0 || y > 0)
            {
                T oldvalue = null;
                T newvalue = null;
                ChangeType ct = ChangeType.None;
                if (x > 0 && y > 0)
                {
                    //if ((d[x - 1, y] < d[x - 1, y - 1] && d[x - 1, y] < d[x, y - 1]) || (d[x - 1, y] == d[x - 1, y - 1] && comparer.Compare(a1[x - 2], a2[y - 1]) == 0 && comparer.Compare(a1[x - 2], a2[y - 2]) != 0)) { oldvalue = a1[--x]; ct = ChangeType.Delete; }
                    //else if (d[x - 1, y - 1] <= d[x, y - 1]) { oldvalue = a1[--x]; newvalue = a2[--y]; ct = ChangeType.Substitute; }
                    //else { newvalue = a2[--y]; ct = ChangeType.Insert; }
                    if (d[x - 1, y] < d[x - 1, y - 1] && d[x - 1, y] < d[x, y - 1]) ct = ChangeType.Delete;
                    else if (d[x - 1, y - 1] < d[x - 1, y] && d[x - 1, y - 1] < d[x, y - 1]) ct = ChangeType.Substitute;
                    else if (d[x, y - 1] < d[x - 1, y - 1] && d[x, y - 1] < d[x - 1, y]) ct = ChangeType.Insert;
                    else
                    {
                        var sd = 1;
                        var sx = 1;
                        var sy = 1;
                        if (d[x - 1, y - 1] == d[x - 1, y]) while (x - ++sx >= 0 && comparer.Compare(a1[x - sx], a2[y - 1]) != 0) { if (x - sx > 0 && d[x - (sx + 1), y - 1] < d[x - (sx + 1), y]) { sy = int.MaxValue; break; } } else sx = int.MaxValue;
                        if (d[x - 1, y - 1] == d[x, y - 1]) while (y - ++sy >= 0 && comparer.Compare(a1[x - 1], a2[y - sy]) != 0) { if (y - sy > 0 && d[x - 1, y - (sy + 1)] < d[x, y - (sy + 1)]) { sy = int.MaxValue; break; } } else sy = int.MaxValue;
                        if (sx < int.MaxValue || sy < int.MaxValue)
                            while (x - ++sd >= 0 && y - sd >= 0 && comparer.Compare(a1[x - sd], a2[y - sd]) != 0) { if (x - sd > 0 && y - sd > 0 && (d[x - (sd + 1), y - (sd + 1)] > d[x - sd, y - (sd + 1)] || d[x - (sd + 1), y - (sd + 1)] > d[x - (sd + 1), y - sd])) { sd = int.MaxValue; break; } }
                        else sy = int.MaxValue;
                        if (sx <= x && sx < sy && sx < sd) ct = ChangeType.Delete;
                        else if (sy <= y && sy < sx && sy < sd) ct = ChangeType.Insert;
                        else if (sx <= x && sy <= y && sx == sy && sx < sd) ct = ChangeType.Delete;
                        else ct = ChangeType.Substitute;
                    }
                    if (ct == ChangeType.Substitute) { oldvalue = a1[--x]; newvalue = a2[--y]; }
                    else if (ct == ChangeType.Delete) { oldvalue = a1[--x]; }
                    else if (ct == ChangeType.Insert) { newvalue = a2[--y]; }
                }
                else if (x == 0) { newvalue = a2[--y]; ct = ChangeType.Insert; }
                else if (y == 0) { oldvalue = a1[--x]; ct = ChangeType.Delete; }
                if (comparer.Compare(oldvalue, newvalue) != 0)
                {
                    if (lastdiff?.Type == ct || lastdiff?.Type == ChangeType.Substitute)
                    {
                        if (oldvalue != null) lastdiff.OldValues.Insert(0, oldvalue);
                        if (newvalue != null) lastdiff.NewValues.Insert(0, newvalue);
                    }
                    else
                    {
                        lastdiff = new DifferenceSets<T>(oldvalue, newvalue, ct);
                        changes.Add(lastdiff);
                    }
                }
                else lastdiff = null;
            }
            changes.Reverse();
            return changes;
        }

    }

    public class Difference<T>
    {
        public T OldValue { get; set; }
        public T NewValue { get; set; }
        public ChangeType Type { get; set; }

        public override string ToString()
        {
            string o = $"{OldValue}";
            string n = $"{NewValue}";
            return $"{Type} {o}{(!string.IsNullOrEmpty(o) && !string.IsNullOrEmpty(n) ? " for " : "")}{(!string.IsNullOrEmpty(n) ? n : "")}";
        }
    }

    public class DifferenceSets<T>
    {
        public List<T> OldValues { get; set; }
        public List<T> NewValues { get; set; }
        public ChangeType Type { get; set; }

        public DifferenceSets(T oldvalue, T newvalue, ChangeType type)
        {
            if (oldvalue != null) OldValues = new List<T> { oldvalue };
            if (newvalue != null) NewValues = new List<T> { newvalue };
            Type = type;
        }

        public override string ToString()
        {
            string o = $"{Concat(OldValues)}";
            string n = $"{Concat(NewValues)}";
            return $"{Type} {o}{(!string.IsNullOrEmpty(o) && !string.IsNullOrEmpty(n) ? " for " : "")}{(!string.IsNullOrEmpty(n) ? n : "")}";
        }

        static string Concat(IList<T> values)
        {
            return values?.Aggregate("", (current, item) => current + $"{item}");
        }
    }

    public enum ChangeType
    {
        None,
        Delete,
        Substitute,
        Insert
    }
}