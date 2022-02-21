using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Reports
{
    public class LanguageSummary
    {
        public static string[] colours = new[] { "Red", "blue", "green", "pale-blue", "yellow", "deep-purple", "deep-orange", "brown", "gray", "indigo", "khaki", "blue-gray", "aqua", "pink", "lime", "orange" };
        public static string[] titles = { "Title", "Algo", "Question", "Answer", "Conclusion", "Bullet", "Category", "Sub Category", "Conclusion Category", "Conclusion SubCat1", "Conclusion SubCat2", "Map", "", "Group", "Literal" };

        public string Title => "Language Summary";
        public string SubTitle { get; set; }

        public string MainTemplate { get { return Target != null ? "@TwoColumn@" : "@OneColumn@"; } }

        public JNode Source { get; set; }
        public JNode Target { get; set; }

        public Dictionary<string, Changes> Diffs { get; set; } = new Dictionary<string, Changes>();
        public int TotalUnmatched { get; set; }

        public bool HasDiffs => Diffs != null && Diffs.Any();

        public List<Error> Errors { get; set; } = new List<Error>();

        public LanguageSummary(JNode source, JNode target)
        {
            if (source["Error"] != null) Errors.Add(new Error { Message = source["Error"], Source = "Source" });
            Source = source;
            Target = target;
            if (target != null)
            {
                if (target["Error"] != null) Errors.Add(new Error { Message = target["Error"], Source = "Target" });
                var sk = source.Values.Where(f => f.Key != "Error").Select(f => f.Key).ToList();
                var tk = target.Values.Where(f => f.Key != "Error").Select(f => f.Key).ToList();
                foreach (var item in source.Where(f => f.Key != "Error"))
                {
                    var changes = new Changes();
                    Diffs.Add(item.Key, changes);
                    if (tk.Contains(item.Key))
                    {
                        changes.Additions.AddRange(item.Values.Select(f => f.Value).Except(target[item.Key].Values.Select(f => f.Value)));
                        changes.Deletions.AddRange(target[item.Key].Values.Select(f => f.Value).Except(item.Values.Select(f => f.Value)));
                        changes.Updates.AddRange(item.Values.Select(f => f.Value).Intersect(target[item.Key].Values.Select(f => f.Value)));
                    }
                    else
                    {
                        changes.Additions.AddRange(item.Values.Select(f => f.Value));
                    }
                }
                foreach (var item in tk.Except(sk))
                {
                    var changes = new Changes();
                    Diffs.Add(item, changes);
                    changes.Deletions.AddRange(target[item].Values.Select(f => f.Value));
                }
                TotalUnmatched = Diffs.Sum(f => f.Value.AdditionCount + f.Value.DeletionCount);
            }
        }
    }

    public class Changes
    {
        public List<string> Additions { get; set; } = new List<string>();
        public List<string> Deletions{ get; set; } = new List<string>();
        public List<string> Updates { get; set; } = new List<string>();

        public int AdditionCount => Additions.Count;
        public int DeletionCount => Deletions.Count;
        public int UpdateCount => Updates.Count;
    }

    public class Error
    {
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
