using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    class ConclusionSummary : XmlReportBase
    {
        public string CurrentHeader { get; set; }
        public override string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker, string prop, string template, Dictionary<string, string> ids)
        {
            switch (prop)
            {
                case "DispositionHeader":
                    {
                        var xe = obj as XElement;
                        var disposition = $"{xe.ElementValue("Category")} {xe.ElementValue("SubCat1")}";
                        if (disposition != CurrentHeader)
                        {
                            CurrentHeader = disposition;
                            return $"<div class=\"w3-row w3-border\"><div class=\"w3-container w3-light-grey\"><h5>{disposition}</h5></div></div>";
                        }
                        return "";
                    }
                case "Algo_List":
                    {
                        var result = "";
                        var algos = (obj as XElement).Elements("Table");
                        var total = algos.Count();
                        var c = 0;
                        foreach (var item in algos)
                        {
                            if (++c > 1 && c == total) result += " and ";
                            else if (c > 1) result += ", ";
                            result += item.ElementValue("Algo_Name");
                        }
                        return result;
                    }
                default:
                    break;
            }
            return base.Replace(s, obj, prms, worker, prop, template, ids);
        }
    }
}
