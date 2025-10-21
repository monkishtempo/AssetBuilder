using AssetBuilder.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class CompareReport : ReportBase<ReportPayload>
    {
        static string[] colours = new[] { "blue", "green", "pale-blue", "yellow", "deep-purple", "deep-orange", "brown", "gray", "indigo", "khaki", "blue-gray", "aqua", "pink", "lime", "orange" };
        static string[] objecttitles = new[] { "Algo {0} - {1}", "Question {0} - {1}", "Answer {0} - {1}", "Conclusion {0} - {1}", "Bullet {0}", "Category {0}", "Subcategory {0}", "Conclusion Category {0}", "Subcat1 {0}", "Subcat2 {0}", "", "TextAsset {0} - {1}", "Groupname {0}", "Literal {0}", "Title {0}" };
        static string[] linktitles = new[] { "Algo.{0}", "Question.{0}", "Answer.{0}", "Conclusion.{0}", "Bullet.{0}" };
        static string[] scriptdata = new[] { "ALGOID In ({0})({1})", "QUESTIONID In ({0})({1})", "ANSID In ({0})({1})", "RECID In ({0})({1})", "BPID In ({0})({1})" };

        double fieldcount;
        int diffcount;

        public double Total;
        public string WorkerTemplate;

        public bool AddCommentColumn { get; set; }

        public CompareReport(Dictionary<string, string> Prms, Window owner)
        {
            prms = Prms;
            Owner = owner;
        }
        public void RunReport()
        {
            OnStarted(new EventArgs());
            Templates = new Dictionary<string, string>();
            diffcount = 0;
            var s = GetTemplate("Layout");
            RunReport(rp, s);
        }

        public override string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker, string prop, string template, Dictionary<string, string> ids)
        {
            switch (prop)
            {
                case "hideComments":
                    {
                        ReportPayload rp = obj as ReportPayload;
                        return string.IsNullOrWhiteSpace(rp.Comments) ? "w3-hide" : "";
                    }
                case "hideAsset":
                    {
                        ReportPayload rp = obj as ReportPayload;
                        if (template == "Original")
                        {
                            return string.IsNullOrEmpty(rp.Description) ? "w3-hide" : "";
                        }
                        else
                        {
                            return string.IsNullOrEmpty(rp.AlternateDescription) ? "w3-hide" : "";
                        }
                    }
                case "Location":
                    {
                        var l = template == "Original" ? prms["TL"] : prms["SL"];
                        var e = template == "Original" ? prms["T"] : prms["S"];
                        var location = e ?? "";
                        location += l != "" ? $" ({l})" : "";
                        return location;
                    }
                case "Diffs":
                    {
                        var index = 0;
                        var orig = template == "Original";
                        ReportPayload rp = obj as ReportPayload;
                        var col = orig ? "red" : "blue";
                        var elem = orig ? "s" : "u";
                        var desc = orig ? rp.Description : rp.AlternateDescription;
                        var ades = orig ? rp.AlternateDescription : rp.Description;

                        if (string.IsNullOrEmpty(ades)) return desc.XmlEncode();

                        var repl = "";
                        foreach (var diff in rp?.Diffs)
                        {
                            var Values = template == "Original" ? diff.OldValues : diff.NewValues;
                            if (Values != null && Values[0].Index < desc.Length)
                            {
                                var upto = Values.Last().Index + Values.Last().Length; // Values.Sum(f => f.Length);
                                repl += $"<span style=\"white-space:pre-wrap\">{desc.Substring(index, Values[0].Index - index).XmlEncode()}</span>";
                                repl += $"<{elem} title=\"{diff.ToString().XmlEncode()}\" class=\"w3-text-{col}\" style=\"white-space:pre-wrap\">{desc.Substring(Values[0].Index, upto - Values[0].Index).XmlEncode()}</{elem}>";
                                index = upto; //length + Values[0].Index;
                            }
                        }
                        if (index < desc.Length)
                        {
                            repl += $"<span style=\"white-space:pre-wrap\">{desc.Substring(index, desc.Length - index).XmlEncode()}</span>";
                        }
                        if (!orig || string.IsNullOrEmpty(rp.Description)) worker.ReportProgress((int)(((++diffcount) / fieldcount) * 100));
                        return repl;
                    }
                case "Objects":
                    if (template == "Field")
                    {

                    }
                    break;
            }
            return base.Replace(s, obj, prms, worker, prop, template, ids);
        }

        private XElement GetElement(string result)
        {
            var elem = XElement.Parse(result);
            if(elem.Name == "NewDataSet") return new XElement(new XElement("root", new XAttribute("date", elem.Element("Table")?.Element("datetimestamp")?.Value), elem.Element("Table")?.Element("Content")));
            return elem;
        }

        public ReportPayload GetLanguageComparePayload(Dictionary<string, string> prms, Dictionary<string, Task<string>> targetTasks, Dictionary<string, Task<string>> sourceTasks, BackgroundWorker worker)
        {
            ReportPayload rp = ReportPayload.CreateScaffold(prms["Title"], prms["Subtitle"], "Report Content", "deep-purple");
            rp.Objects[0].Objects = new List<ReportPayload>();
            var targets = new Dictionary<string, XElement>();
            var index2 = 0;
            var assettype = 0;
            var id = 0;
            var c = 0;
            DateTime dt;
            foreach (var item in targetTasks)
            {
                XElement elem = GetElement(item.Value.Result);
                targets.Add(item.Key, elem);
                if (!sourceTasks.ContainsKey(item.Key))
                {
                    var split = item.Key.Split(':');
                    int.TryParse(split[0], out assettype);
                    if (assettype == 0) assettype = 15;
                    if (!int.TryParse(string.Join(":", split.Skip(1)), out id)) id = ++index2;
                    var asset = new ReportPayload { Index1 = assettype, Index2 = id, ScriptData = "", LinkTitle = assettype < 6 ? $"<a href=\"assetbuilder:{string.Format(linktitles[assettype - 1], id)}\">{string.Format(objecttitles[assettype - 1], id, "")}</a>" : "", Title = string.Format(objecttitles[assettype - 1], id, ""), Objects = new List<ReportPayload>(), Colour = colours[assettype - 1] };
                    if (DateTime.TryParse(elem.AttributeValue("date"), out dt)) { asset.Date = dt;  asset.AlternateDate = dt; }
                    if (asset.LinkTitle == "" && assettype == 12 && split.Length > 1) asset.LinkTitle = split[1];
                    foreach (var target in elem.Elements())
                    {
                        var f = target.Name.LocalName.Split('_')[0];
                        var livetext = target?.Value ?? "";
                        var devtext = "";
                        asset.Objects.Add(new ReportPayload { Colour = colours[assettype - 1], Title = f, Description = livetext, AlternateDescription = devtext });
                        fieldcount++;
                    }
                    rp.Objects[0].Objects.Add(asset);
                }
            }
            var total = (double)sourceTasks.Count();
            foreach (var item in sourceTasks)
            {
                var split = item.Key.Split(':');
                int.TryParse(split[0], out assettype);
                if (assettype == 0) assettype = 15;
                if (!int.TryParse(string.Join(":", split.Skip(1)), out id)) id = ++index2;
                var asset = new ReportPayload { Index1 = assettype, Index2 = id, ScriptData = "", LinkTitle = assettype < 6 ? $"<a href=\"assetbuilder:{string.Format(linktitles[assettype - 1], id)}\">{string.Format(objecttitles[assettype - 1], id, "")}</a>" : split[1], Title = string.Format(objecttitles[assettype - 1], id, ""), Objects = new List<ReportPayload>(), Colour = colours[assettype - 1] };
                XElement elem = GetElement(item.Value.Result);
                if (DateTime.TryParse(elem.AttributeValue("date"), out dt)) asset.AlternateDate = dt;
                var add = false;
                foreach (var source in elem.Elements())
                {
                    var f = source.Name.LocalName.Split('_')[0];
                    var devtext = source?.Value ?? "";
                    var livetext = ""; // TargetXml.XPathSelectElement($"NewDataSet/{name}[*[1] = {id}]/{field}")?.Value ?? "";
                    if (targets.ContainsKey(item.Key)) livetext = targets[item.Key].Element(source.Name.LocalName)?.Value ?? "";
                    if (devtext != livetext)
                    {
                        add = true;
                        asset.Objects.Add(new ReportPayload { Colour = colours[assettype - 1], Title = f, Description = livetext, AlternateDescription = devtext });
                        fieldcount++;
                    }
                }
                if (add)
                {
                    rp.Objects[0].Objects.Add(asset);
                }
                worker.ReportProgress((int)((++c) / total * 100));
            }
            return rp;
        }

        public ReportPayload GetUrlComparePayload(string comp1, string comp2)
        {
            ReportPayload rp = ReportPayload.CreateScaffold("Url Comparison", "", "Payload", "deep-purple", compareType: CompareType.Line);
            rp.Objects[0].Objects[0].Objects.Add(new ReportPayload(CompareType.Line) { Description = comp1, AlternateDescription = comp2 });
            return rp;
        }

        DateTime MinDate = new DateTime(1970, 1, 1);
        public ReportPayload GetComparePayload(Dictionary<string, string> prms, XDocument targetXml, XDocument sourceXml, BackgroundWorker worker, bool silent)
        {
            fieldcount = 0;
            ReportPayload rp = ReportPayload.CreateScaffold(prms["Title"], prms["Subtitle"], "Report Content", "deep-purple");
            rp.Objects[0].Objects = new List<ReportPayload>();

            Dictionary<string, string> props = new Dictionary<string, string> { { "Algo", "Table" }, { "Question", "Table1" }, { "Answer", "Table2" }, { "Conclusion", "Table3" }, { "Bullet", "Table4" } };
            Dictionary<string, string[]> fields = new Dictionary<string, string[]>
                        {
                            { "Table", new [] { "Algo_Name", "Word_Merge", "WM2" } },
                            { "Table1", new [] { "Expert_x0020_Statement", "Lay_x0020_Statement", "Question", "Explanation", "Category", "Sub_x0020_Category_x0020_1", "Sub_x0020_Category_x0020_2", "Question_x0020_Type", "State" } },
                            { "Table2", new [] { "Expert_x0020_Statement", "Lay_x0020_Statement", "Answer", "Explanation", "Category", "Sub_x0020_Category_x0020_1", "Sub_x0020_Category_x0020_2" } },
                            { "Table3", new [] { "Expert_x0020_Statement", "Lay_x0020_Statement", "Explanation", "More_x0020_Detail", "Category", "Sub_x0020_Category_x0020_1", "Sub_x0020_Category_x0020_2", "Bullets" } },
                            { "Table4", new [] { "Bullet" } },
                            { "Table6", new [] { "Properties" } },
                        };
            var fi = fields.Keys.ToList();
            var total = (double)targetXml.Root.Elements().Count();
            var targets = new Dictionary<string, XElement>();
            foreach (var item in targetXml.Element("NewDataSet").Elements())
            {
                var name = item.Name.LocalName;
                var i = fi.IndexOf(name);
                if (i == -1) continue;
                int id = int.Parse(item.Elements().First().Value);
                if (name == "Table6")
                {
                    var key = item.Element("Type")?.Value;
                    var pid = item.Element("AssetID")?.Value;
                    if (props.ContainsKey(key) && targets.ContainsKey($"{props[key]}_{pid}"))
                    {
                        var algoid = item.Element("AlgoID")?.Value;
                        var nodeid = item.Element("NodeID")?.Value;
                        var target = targets[$"{props[key]}_{pid}"];
                        var propname = "_" + (item.Element("PropertyName")?.Value ?? "");
                        if (algoid != null && nodeid != null) propname += $"_{algoid}_{nodeid}";
                        var propvalue = item.Element("PropertyValue")?.Value ?? "";
                        target.Add(new XElement(propname, propvalue));
                    }
                }
                else if (!targets.ContainsKey($"{name}_{id}")) targets.Add($"{name}_{id}", item);
                //worker.ReportProgress((int)((++c) / total * 25));
            }
            total = sourceXml.Root.Elements().Count();
            var sources = new Dictionary<string, ReportPayload>();
            var added = new HashSet<string>();
            var c = 0;
            foreach (var item in sourceXml.Element("NewDataSet").Elements())
            {
                var name = item.Name.LocalName;
                var i = fi.IndexOf(name);
                if (i == -1) continue;
                //if (name == "Table") continue;
                if(silent)
                {
                    if (name.In("Table", "Table4", "Table5")) continue;
                    if (name == "Table1" && item.Element("Question_x0020_Type").Value.NotIn("Calculated", "Counter Set", "Counter Check", "Conclusion Check", "Calculated Question Check", "User/Language Check", "Custom Question", "EMR Lookup", "Dynamic Transfer", "Extended Conclusion Check")) continue;
                    if (name == "Table2" && item.Element("Answer_x0020_Type").Value.NotIn("Derived from conclusion", "Derived from question", "Value: Calculated", "Conclusion Check", "Value: Sort < > =")) continue;
                    if (name == "Table3" && item.Element("Silent").Value.NotIn("true")) continue;                    
                }
                else
                {
                    if (name == "Table1" && item.Element("Question_x0020_Type").Value.In("Calculated", "Counter Set", "Counter Check", "Conclusion Check", "Calculated Question Check", "User/Language Check", "Custom Question", "EMR Lookup", "Dynamic Transfer", "Extended Conclusion Check")) continue;
                    if (name == "Table2" && item.Element("Answer_x0020_Type").Value.In("Derived from conclusion", "Derived from question", "Value: Calculated", "Conclusion Check", "Value: Sort < > =")) continue;
                    if (name == "Table3" && item.Element("Silent").Value.In("true")) continue;
                }
                if (name == "Table6")
                {
                    var key = item.Element("Type")?.Value;
                    var pid = item.Element("AssetID")?.Value;
                    if (props.ContainsKey(key) && sources.ContainsKey($"{props[key]}_{pid}"))
                    {
                        var propkey = $"{props[key]}_{pid}";
                        var algoid = item.Element("AlgoID")?.Value;
                        var nodeid = item.Element("NodeID")?.Value;
                        var propname = "_" + (item.Element("PropertyName")?.Value ?? "");
                        if (algoid != null && nodeid != null) propname += $"_{algoid}_{nodeid}";
                        var propvalue = item.Element("PropertyValue")?.Value ?? "";
                        var devtext = propvalue;
                        var livetext = null as string;
                        if (targets.ContainsKey(propkey)) livetext = targets[propkey].Element(propname)?.Value ?? "";
                        if (devtext != livetext)
                        {
                            sources[propkey].Objects.Add(new ReportPayload { Colour = "red", Title = "Property - " + propname, Description = livetext ?? "", AlternateDescription = devtext });
                            fieldcount++;
                            if (!added.Contains(propkey))
                            {
                                added.Add(propkey);
                                rp.Objects[0].Objects.Add(sources[propkey]);
                            }
                        }
                    }
                }
                else
                {
                    int assettype = i + 1;
                    int id = int.Parse(item.Elements().First().Value);
                    var asset = new ReportPayload { Index1 = assettype, Index2 = id, ScriptData = string.Format(scriptdata[i], id, item.Element(fields[name][0]).Value), LinkTitle = $"<a href=\"assetbuilder:{string.Format(linktitles[i], id)}\">{string.Format(objecttitles[i], id, item.Element(fields[name][0]).Value)}</a>", Title = string.Format(objecttitles[i], id, item.Element(fields[name][0]).Value), Objects = new List<ReportPayload>(), Colour = colours[i] };
                    bool add = false;
                    var key = $"{name}_{id}";
                    foreach (var field in fields[name])
                    {
                        var f = field.Split('_')[0];
                        var devtext = item.Element(field)?.Value ?? "";
                        var livetext = ""; // TargetXml.XPathSelectElement($"NewDataSet/{name}[*[1] = {id}]/{field}")?.Value ?? "";
                        if (targets.ContainsKey(key)) livetext = targets[key].Element(field)?.Value ?? "";
                        if (devtext != livetext)
                        {
                            if (field == "Bullets") { devtext = System.Web.HttpUtility.HtmlDecode(devtext); livetext = System.Web.HttpUtility.HtmlDecode(livetext); }
                            add = true;
                            asset.Objects.Add(new ReportPayload { Colour = colours[i], Title = field.Replace("_x0020_", " "), Description = livetext, AlternateDescription = devtext });
                            fieldcount++;
                        }
                    }
                    if (add)
                    {
                        var d = MinDate;
                        if (targets.ContainsKey(key))
                            d = DateTime.Parse(targets[key].Element("datetimestamp").Value);
                        var ad = DateTime.Parse(item.Element("datetimestamp").Value);
                        asset.Date = d;
                        asset.AlternateDate = ad;
                        if (AddCommentColumn && asset.Content != ContentExists.RightOnly)
                        {
                            var comments = DataAccess.getData("dsp_GetComments", "@AssetTypeID", $"{assettype}", "@AssetID", $"{id}", "@Date", asset.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                            asset.Comments = comments?.Element("Table")?.Element("Comments")?.Value ?? "";
                        }
                        added.Add(key);
                        rp.Objects[0].Objects.Add(asset);
                    }
                    if (!sources.ContainsKey(key)) sources.Add(key, asset);
                }
                worker.ReportProgress((int)((++c) / total * 100));
            }
            rp.Objects[0].Objects.Sort((a, b) =>
            {
                if (a.Index1 != b.Index1) return a.Index1 - b.Index1;
                return a.Index2 - b.Index2;
            });
            return rp;
        }

    }
}
