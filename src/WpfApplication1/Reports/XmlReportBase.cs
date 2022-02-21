using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    class XmlReportBase : ReportBase<XElement>
    {
        public override string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker, string prop, string template, Dictionary<string, string> ids)
        {
            string result;
            if (TryDefaultReplace(prop, template, ids, out result)) return result;
            switch (prop)
            {
                case "LocalName":
                    return obj is XElement ? ((XElement)obj).Name.LocalName : obj.ToString();
                case "ChildCount":
                    return ((XElement)obj).Elements().Count().ToString();
                case "Tables":
                case "Fields":
                case "Values":
                    var repl = "";
                    IEnumerable<string> items;
                    if (prop == "Fields") items = ((XElement)obj).Elements().First().Elements().Select(f => f.Name.LocalName).Distinct();
                    else items = ((XElement)obj).Elements().Select(f => f.Name.LocalName).Distinct();
                    foreach (var item in items)
                    {
                        var nt = GetTemplate(template).Replace("@Element", "@" + item);
                        repl += Replace(nt, prop == "Fields" ? item : obj, prms, worker);
                    }
                    return repl;
            }

            if (obj != null && obj is XElement)
            {
                var p = ((XElement)obj).Element(prop);
                object value = p?.Value;
                if (p == null && value == null)
                {
                    if (value == null && prms?.ContainsKey(prop) == true) value = prms[prop];
                    else { template = prop; value = obj; }
                }
                if (template != null)
                {
                    template = GetTemplate(template);
                    if (p != null)
                    {
                        var repl = "";
                        foreach (var item in ((XElement)obj).Elements(prop))
                        {
                            repl += Replace(template, item, prms, worker);
                        }
                        return repl;
                    }
                    else return Replace(template, value, prms, worker);
                }
                else return value?.ToString();
            }
            return "";
        }
    }
}
