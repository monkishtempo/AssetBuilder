using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AssetBuilder.Models;

namespace AssetBuilder.Services
{
    public class AlgoStatusWebService : IWebService
    {
        public List<EnvironmentUrl> GetEnvironmentUrls(string baseUri)
        {
            var results = new List<EnvironmentUrl>();
            if (string.IsNullOrWhiteSpace(baseUri)) return results;

            var source = new Uri(new Uri(baseUri), "webbuilder/TraversalService/TableOutput/ab_builderdefaults/json");
            var tmp = source.AbsoluteUri.GetContent<JNode>();

            if (tmp != null)
            {
                results = tmp.SelectMany(f => f)
                    .Where(f => f["EnvironmentType"].Value.In("Traversal Report", "QA Report"))
                    .Select(f => new EnvironmentUrl
                    {
                        Name = f["EnvironmentName"],
                        BaseUrl = f["EnvironmentUrl"].Value.Replace("/WebBuilder/TraversalService/QAReport/{TraversalID}", "")
                    }).ToList();
            }

            return results;
        }

        public List<AlgoLoadInformation> GetAlgosForEnvironment(EnvironmentUrl environment)
        {
            var results = new List<AlgoLoadInformation>();
            if (string.IsNullOrWhiteSpace(environment.Name) || string.IsNullOrWhiteSpace(environment.BaseUrl))
            {
                return results;
            }

            var remoteUrl = new Uri(new Uri(environment.BaseUrl), "/webbuilder/data.asmx");
            var tmp = DataAccess.getDataNode("ab_updateasset", new[] {"@xml", "<root command=\"algos\" />"}, false, remoteUrl.AbsoluteUri);
            var data = XElement.Parse(tmp.OuterXml);

            results = data.Elements().Select(a => new AlgoLoadInformation
            {
                Id = int.Parse(a.Element("AssetID")?.Value ?? "-1"),
                Name = a.Element("Title")?.Value ?? "Error reading algo name",
                Loaded = DateTime.Parse(a.Element("promoted")?.Value ?? string.Empty)
            }).ToList();

            return results;
        }

        public string GetAlgoStatus(string baseUri, int algoId)
        {
            if (string.IsNullOrWhiteSpace(baseUri)) return "Unknown";

            var remoteUrl = new Uri(new Uri(baseUri), "/webbuilder/data.asmx");
            var args = new[] {"@PropertyType", "Algo", "@DataID", algoId.ToString(), "@PropertyName", "Version"};
            var tmp = DataAccess.getDataNode("dsp_GetProperty", args, false, remoteUrl.AbsoluteUri);
            var data = XElement.Parse(tmp.OuterXml);

            return data.Element("Table")?.Element("PropertyValue")?.Value ?? "Unknown";
        }
    }
}