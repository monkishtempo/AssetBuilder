using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    internal class DispositionChange : XmlReportBase
    {
        public void RunReport()
        {
            RunReport(rp, "@Layout@");
        }

        internal XElement GetPayLoad(XDocument targetXml, XDocument sourceXml)
        {
            var targets = new Dictionary<string, XElement>();
            var targetDispositions = new HashSet<string>();
            var sourceDispositions = new HashSet<string>();

            foreach (var item in targetXml.Element("NewDataSet").Elements("Table3"))
            {
                var name = item.Name.LocalName;
                var id = item.Elements().First().Value;
                targets.Add($"{name}_{id}", item);
                if (item.Element("Silent")?.Value == "false")
                {
                    var disposition = $"{item.ElementValue("Category")} {item.ElementValue("Sub_x0020_Category_x0020_1")} {item.ElementValue("Sub_x0020_Category_x0020_2")}";
                    targetDispositions.Add(disposition);
                }
            }

            var report = new Dictionary<string, Dictionary<string, List<Tuple<string, string>>>>();

            foreach (var item in sourceXml.Element("NewDataSet").Elements("Table3").Where(f => f.Element("Silent")?.Value == "false"))
            {
                var name = item.Name.LocalName;
                var id = item.Elements().First().Value;
                var key = $"{name}_{id}";
                var sourceDisposition = $"{item.ElementValue("Category")} {item.ElementValue("Sub_x0020_Category_x0020_1")} {item.ElementValue("Sub_x0020_Category_x0020_2")}";
                sourceDispositions.Add(sourceDisposition);
                var targetDisposition = "";
                if (targets.ContainsKey(key))
                {
                    var target = targets[key];
                    targetDisposition = $"{target.ElementValue("Category")} {target.ElementValue("Sub_x0020_Category_x0020_1")} {target.ElementValue("Sub_x0020_Category_x0020_2")}";
                }
                if (targetDisposition == sourceDisposition) continue;
                if (!report.ContainsKey(sourceDisposition)) report.Add(sourceDisposition, new Dictionary<string, List<Tuple<string, string>>>());
                if (!report[sourceDisposition].ContainsKey(targetDisposition)) report[sourceDisposition].Add(targetDisposition, new List<Tuple<string, string>>());
                report[sourceDisposition][targetDisposition].Add(Tuple.Create(id, item.ElementValue("Expert_x0020_Statement")));
            }

            XElement root = new XElement("root");

            foreach (var source in report.OrderBy(f => f.Key))
            {
                var sd = new XElement("Disposition", new XElement("Source", source.Key));
                root.Add(sd);
                foreach (var target in source.Value.OrderBy(f => f.Key))
                {
                    var disp = new XElement("Disposition", new XElement("Source", source.Key), new XElement("Target", target.Key));
                    sd.Add(disp);
                    foreach (var conclusion in target.Value)
                    {
                        disp.Add(new XElement("Conclusion", new XElement("ConclusionID", conclusion.Item1), new XElement("Expert", conclusion.Item2)));
                    }
                }
            }

            var added = new XElement("Added", new XElement("Colour", "green"), new XElement("Operation", "Added"));
            var removed = new XElement("Removed", new XElement("Colour", "red"), new XElement("Operation", "Removed"));
            bool b = false;
            var node = added;//.Element("Dispositions");

            foreach (var item in sourceDispositions.Except(targetDispositions).OrderBy(f => f))
            {
                node.Add(new XElement("Disposition", new XElement("Text", item)));
                b = true;
            }

            if (b) root.Add(added);
            b = false;
            node = removed;//.Element("Dispositions");

            foreach (var item in targetDispositions.Except(sourceDispositions).OrderBy(f => f))
            {
                node.Add(new XElement("Disposition", new XElement("Text", item)));
                b = true;
            }

            if (b) root.Add(removed);

            if(!root.HasElements)
            {
                root.Add(new XElement("NoDispositions", "True"));
            }

            return root;
        }
    }
}
