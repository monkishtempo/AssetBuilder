using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Reports
{
    public class LanguageSummaryReport : ContentReport<LanguageSummary>
    {
        public override string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker, string prop, string template, Dictionary<string, string> ids)
        {
            switch (prop)
            {
                case "Key":
                    {
                        if (obj is JNode && int.TryParse((obj as JNode).Key, out var i))
                        {
                            return LanguageSummary.titles[i];
                        }
                        else
                        {
                            var p = obj.GetType().GetProperty(prop);
                            if (p != null && int.TryParse(p.GetValue(obj).ToString(), out i))
                            {
                                return LanguageSummary.titles[i];
                            }
                        }
                        break;
                    }
                default:
                    {
                        var j = obj as JNode;
                        if (j != null)
                        {
                            if (j.IsDictionary && j[prop].Value != null)
                                return j[prop].Value;
                            if (j.IsDictionary && (j[prop].IsArray || j[prop].IsDictionary))
                                return j[prop];
                        }
                        break;
                    }
            }
            return base.Replace(s, obj, prms, worker, prop, template, ids);
        }
    }
}
