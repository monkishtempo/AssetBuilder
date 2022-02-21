using AssetBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AssetBuilder.Reports
{
    public class LogicCompare
    {
		private string mv = int.MaxValue.ToString();
		public Guid PageID { get; set; } = Guid.NewGuid();
		public string Title { get; set; }
		public string Description { get; set; }
		public string sourceUrl { get; set; }
        public string targetUrl { get; set; }
        public string algoid { get; set; }
        public HashSet<int> AlgoLimits { get; set; }
        public LogicCompare(string source, string target, string startAlgos, string limitToAlgos)
        {
            var i = 0;
            sourceUrl = source;
            targetUrl = target;
            algoid = startAlgos;
            AlgoLimits = new HashSet<int>(limitToAlgos.Split(',').Where(f => int.TryParse(f, out i)).Select(f => i));
			Description = $"Start Algos : {startAlgos}<br/>";
			if(AlgoLimits.Any())
            {
				Description += "Report limited to algos : " + string.Join(", ", AlgoLimits);
            }
        }

        public async Task<string> GetReport()
        {
            var report = "TableOutput/LogicReport/csv/" + algoid;
            var props = "TableOutput/PropertyReport/csv/" + algoid;

            var lr = new LogicCompareReport();
            var sourceData = new HashSet<string>((await (sourceUrl + report).GetContent()).Split('\n').Skip(1).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)));
            var targetData = new HashSet<string>((await (targetUrl + report).GetContent()).Split('\n').Skip(1).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)));
            var sourceProps = new HashSet<string>((await (sourceUrl + props).GetContent()).Split('\n').Skip(1).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)));
            var targetProps = new HashSet<string>((await (targetUrl + props).GetContent()).Split('\n').Skip(1).Select(f => f.Trim()).Where(f => !string.IsNullOrWhiteSpace(f)));

			lr.SourceAssets = getAssets(sourceData);
			lr.SourceNodes = getNodes(sourceData);
			lr.TargetAssets = getAssets(targetData);
			lr.TargetNodes = getNodes(targetData);

			lr.AddNodes = getNodes(sourceData.Except(targetData));
			lr.DelNodes = getNodes(targetData.Except(sourceData));

			lr.NewNodes = new HashSet<string>(lr.SourceNodes.Keys.Except(lr.TargetNodes.Keys));
			lr.OldNodes = new HashSet<string>(lr.TargetNodes.Keys.Except(lr.SourceNodes.Keys));

			lr.NewAssets = new HashSet<string>(lr.SourceAssets.Keys.Where(f => !lr.NewNodes.Contains(string.Join(":", f.Split(':').Take(2)))).Except(lr.TargetAssets.Keys));
			lr.OldAssets = new HashSet<string>(lr.TargetAssets.Keys.Where(f => !lr.OldNodes.Contains(string.Join(":", f.Split(':').Take(2)))).Except(lr.SourceAssets.Keys));

			lr.NewProps = new HashSet<string>(sourceProps.Except(targetProps));
			lr.OldProps = new HashSet<string>(targetProps.Except(sourceProps));

			foreach (var node in lr.NewNodes)
				lr.AddNodes.Remove(node);

			foreach (var node in lr.OldNodes)
				lr.DelNodes.Remove(node);

			foreach (var asset in lr.NewAssets)
			{
				var cl = CompareAsset.Create(asset);
				lr.AddNodes[cl.NodeKey].Remove(cl.Key);
			}

			foreach (var asset in lr.OldAssets)
			{
				var cl = CompareAsset.Create(asset);
				lr.DelNodes[cl.NodeKey].Remove(cl.Key);
			}

			var html = HtmlLogicCompareReport(lr);
			var script = "<script>function toggle(id) {var header = document.getElementById(id + '_Header'); var body = document.getElementById(id + '_Body'); header.style.display = header.style.display == '' ? 'none' : ''; body.style.display = body.style.display == '' ? 'none' : ''; }</script>";
            return html + script;
        }

		private string HtmlLogicCompareReport(LogicCompareReport report)
		{
			var head = $"<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width\"><title>Logic Comparison</title><link rel=\"stylesheet\" href=\"https://www.w3schools.com/w3css/4/w3.css\"><link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.6.3/css/font-awesome.min.css\"><script type=\"text/javascript\">var PageID = '{PageID}';</script><style>i.fa {{ cursor: pointer }}</style></head><body>";
			head += $"<div class=\"w3-container w3-light-grey\"><h2>@Title@</h2><p>@Description@</p></div><span class=\"w3-right\">User: @User@ Date: @Now@</span>";
			head = head.Replace("@Title@", Title);
			head = head.Replace("@Description@", Description);
			head = head.Replace("@Now@", DateTime.Now.ToString("MMM dd, yyyy HH:mm"));
			head = head.Replace("@User@", Environment.UserName);
			var foot = "</body></html><script type=\"text/javascript\" src=\"https://apps.expert-24.com/Comment/Script/e24Comment.js\"></script>";
			var sbs = new Dictionary<string, StringBuilder>();
			var algos = new HashSet<string>();

			var cols = new[] { report.NewNodes, report.OldNodes, report.NewAssets, report.OldAssets };
			var nods = new object[] { report.SourceNodes, report.TargetNodes, report.SourceAssets, report.TargetAssets };
			var tits = new[] { "Added Nodes", "Removed Nodes", "Addet Assets", "Removed Assets", "New change", "Previous Version", "New Properties", "Old Properties" };
			var nRows = new HashSet<string>();
			var aRows = new HashSet<string>();
			var sids = new Dictionary<string, Guid>();

			sids.Clear();
			for (int i = 0; i < 4; i++)
			{
				algos.Clear();
				if (i == 2) sids.Clear();
				foreach (var item in cols[i])
				{
					var split = item.Split(':');
					if (!sbs.ContainsKey(split[0])) sbs.Add(split[0], new StringBuilder($"<div class=\"w3-row w3-container\"><h1>Algo {split[0]}</h1></div>"));
					if (!algos.Contains(split[0]))
					{
						algos.Add(split[0]);
						if (i < 2 && !nRows.Contains(split[0]))
						{
							nRows.Add(split[0]);
							sids.Add(split[0], Guid.NewGuid());
							sbs[split[0]].Append($"<div id=\"{sids[split[0]]}_Header\" onclick=\"toggle('{sids[split[0]]}')\" class=\"w3-row w3-container\" style=\"\"><h5>Nodes <i class=\"w3-text-blue fa fa-chevron-down\"></i></h5></div><div id=\"{sids[split[0]]}_Body\" class=\"w3-row w3-container\" style=\"display:none;\">");
						}
						else if (i >= 2 && !aRows.Contains(split[0]))
						{
							aRows.Add(split[0]);
							sids.Add(split[0], Guid.NewGuid());
							if (nRows.Contains(split[0])) sbs[split[0]].Append("</div>");
							sbs[split[0]].Append($"<div id=\"{sids[split[0]]}_Header\" onclick=\"toggle('{sids[split[0]]}')\" class=\"w3-row w3-container\" style=\"\"><h5>Assets <i class=\"w3-text-blue fa fa-chevron-down\"></i></h5></div><div id=\"{sids[split[0]]}_Body\" class=\"w3-row w3-container\" style=\"display:none;\">");
						}
						var title = tits[i];
						sbs[split[0]].Append($"<div class=\"w3-col l6 m12\"><h5 onclick=\"toggle('{sids[split[0]]}')\">{title} <i class=\"w3-text-purple fa fa-chevron-up\"></i></h5><table class=\"w3-table w3-striped w3-bordered w3-card w3-section w3-small\" style=\"width:initial;\"><thead><tr><th>AlgoID</th><th>NodeID</th><th>Type</th><th>AssetID</th><th>NextID</th><th>X</th><th>Y</th></tr></thead><tbody>");
					}
					IEnumerable<CompareLine> nodes;
					if (i < 2) nodes = ((Dictionary<string, Dictionary<string, List<CompareLine>>>)nods[i])[item].SelectMany(f => f.Value);
					else nodes = ((Dictionary<string, List<CompareLine>>)nods[i])[item];
					foreach (var comp in nodes)
					{
						sbs[split[0]].Append($"<tr><td><a href=\"assetbuilder:Algo.{comp.AlgoID}\">{comp.AlgoID}</a></td><td>{comp.NodeID}</td><td>{comp.Type}</td><td><a href=\"{comp.GetLink()}\">{comp.AssetID}</a></td><td>{comp.NextNodeID}</td><td>{comp.GroupOrder}</td><td>{comp.DisplayOrder}</td></tr>");
					}
				}
				foreach (var algo in algos)
					sbs[algo].Append("</tbody></table></div>");
			}
			foreach (var algo in sbs.Keys)
				sbs[algo].Append("</div>");


			var changes = new[] { report.AddNodes.SelectMany(f => f.Value).SelectMany(f => f.Value), report.DelNodes.SelectMany(f => f.Value).SelectMany(f => f.Value) };
			var adddels = new[] { report.AddNodes, report.DelNodes };

			nRows.Clear();
			sids.Clear();
			for (int i = 0; i < 2; i++)
			{
				algos.Clear();
				foreach (var comp in changes[i])
				{
					var AlgoID = comp.AlgoID.ToString();
					if (!sbs.ContainsKey(AlgoID)) sbs.Add(AlgoID, new StringBuilder($"<div class=\"w3-row w3-container\"><h1>Algo {AlgoID}</h1></div>"));
					if (!algos.Contains(AlgoID))
					{
						algos.Add(AlgoID);
						if (!nRows.Contains(AlgoID))
						{
							nRows.Add(AlgoID);
							sids.Add(AlgoID, Guid.NewGuid());
							sbs[AlgoID].Append($"<div id=\"{sids[AlgoID]}_Header\" onclick=\"toggle('{sids[AlgoID]}')\" class=\"w3-row w3-container\" style=\"\"><h5>Changes <i class=\"w3-text-blue fa fa-chevron-down\"></i></h5></div><div id=\"{sids[AlgoID]}_Body\" class=\"w3-row w3-container\" style=\"display:none;\">");
						}
						sbs[AlgoID].Append($"<div class=\"w3-col l6 m12\"><h5 onclick=\"toggle('{sids[AlgoID]}')\">{tits[i + 4]} <i class=\"w3-text-purple fa fa-chevron-up\"></i></h5><div class=\"w3-responsive\"><table class=\"w3-table w3-striped w3-bordered w3-card w3-section w3-small\" style=\"width:initial;\"><thead><tr><th>AlgoID</th><th>NodeID</th><th>Type</th><th>AssetID</th><th>NextID</th><th>X</th><th>Y</th><th>Title</th><th>Media</th></tr></thead><tbody>");
					}
					var rev = (CompareLine)null;
					if(adddels[(i + 1) % 2].ContainsKey(comp.NodeKey) 
						&& adddels[(i + 1) % 2][comp.NodeKey].ContainsKey(comp.Key)) 
						rev = adddels[(i + 1) % 2][comp.NodeKey]?[comp.Key]?[0];
					var col = new string[5];
					if (rev != null)
					{
						if (rev.NextNodeID != comp.NextNodeID) col[0] = " class=\"w3-text-red\"";
						if (rev.GroupOrder != comp.GroupOrder) col[1] = " class=\"w3-text-red\"";
						if (rev.DisplayOrder != comp.DisplayOrder) col[2] = " class=\"w3-text-red\"";
						if (rev.Title != comp.Title) col[3] = " class=\"w3-text-red\"";
						if (rev.ImageSource != comp.ImageSource) col[4] = " class=\"w3-text-red\"";
					}
					sbs[AlgoID].Append($"<tr><td><a href=\"assetbuilder:Algo.{comp.AlgoID}\">{comp.AlgoID}</a></td><td>{comp.NodeID}</td><td>{comp.Type}</td><td><a href=\"{comp.GetLink()}\">{comp.AssetID}</a></td><td{col[0]}>{comp.NextNodeID}</td><td{col[1]}>{comp.GroupOrder}</td><td{col[2]}>{comp.DisplayOrder}</td><td{col[3]} style=\"word-break:break-all;\">{comp.Title}</td><td{col[4]} style=\"word-break:break-all;\">{comp.ImageSource}</td></tr>");
				}
				foreach (var algo in algos)
					sbs[algo].Append("</tbody></table></div></div>");
			}
			foreach (var algo in nRows)
				sbs[algo].Append("</div>");

			var props = new[] { report.NewProps, report.OldProps };

			nRows.Clear();
			sids.Clear();
			for (int i = 0; i < 2; i++)
			{
				algos.Clear();
				foreach (var comp in props[i])
				{
					var split = comp.GetSplit();
					if (split.Length < 6)
					{
						if (!sbs.ContainsKey(mv)) sbs.Add(mv, new StringBuilder());
						sbs[mv].Append("<div class=\"w3-col l6 m12\"><h5 class=\"w3-text-red\">Report not available</h5></div>");
						break;
					}
					var AlgoID = split[0];
					if (!sbs.ContainsKey(AlgoID)) sbs.Add(AlgoID, new StringBuilder($"<div class=\"w3-row w3-container\"><h1>Algo {AlgoID}</h1></div>"));
					if (!algos.Contains(AlgoID))
					{
						algos.Add(AlgoID);
						if (!nRows.Contains(AlgoID))
						{
							nRows.Add(AlgoID);
							sids.Add(AlgoID, Guid.NewGuid());
							sbs[AlgoID].Append($"<div id=\"{sids[AlgoID]}_Header\" onclick=\"toggle('{sids[AlgoID]}')\" class=\"w3-row w3-container\" style=\"\"><h5>Properties <i class=\"w3-text-blue fa fa-chevron-down\"></i></h5></div><div id=\"{sids[AlgoID]}_Body\" class=\"w3-row w3-container\" style=\"display:none;\">");
						}
						sbs[AlgoID].Append($"<div class=\"w3-col l6 m12\"><h5 onclick=\"toggle('{sids[AlgoID]}')\">{tits[i + 6]} <i class=\"w3-text-purple fa fa-chevron-up\"></i></h5><table class=\"w3-table w3-striped w3-bordered w3-card w3-section w3-small\" style=\"width:initial;\"><thead><tr><th>AlgoID</th><th>NodeID</th><th>Type</th><th>AssetID</th><th>Key</th><th>Value</th></tr></thead><tbody>");
					}
					sbs[AlgoID].Append($"<tr><td>{split[0]}</td><td>{split[1]}</td><td>{split[2]}</td><td>{split[3]}</td><td>{split[4]}</td><td style=\"white-space:pre-wrap;\">{split[5].Replace("\\n", Environment.NewLine)}</td></tr>");
				}
				foreach (var algo in algos)
					sbs[algo].Append("</tbody></table></div>");
			}
			foreach (var algo in nRows)
				sbs[algo].Append("</div>");

			if (!sbs.Any()) sbs.Add(mv, new StringBuilder("<div class\"w3-container\"><h1>No Algo Changes</h1></div>"));
			return string.Concat(head, string.Join("", sbs.Where(f => f.Key == mv || AlgoLimits.Count == 0 || AlgoLimits.Contains(int.Parse(f.Key))).OrderBy(f => int.Parse(f.Key)).Select(f => f.Value)), foot);
		}
		private Dictionary<string, List<CompareLine>> getAssets(IEnumerable<string> list)
		{
			var dict = new Dictionary<string, List<CompareLine>>();
			foreach (var item in list.Select(f => CompareLine.Create(f)))
			{
				if (!dict.ContainsKey(item.Key)) dict.Add(item.Key, new List<CompareLine>());
				dict[item.Key].Add(item);
			}
			return dict;
		}

		private Dictionary<string, Dictionary<string, List<CompareLine>>> getNodes(IEnumerable<string> list)
		{
			var dict = new Dictionary<string, Dictionary<string, List<CompareLine>>>();
			foreach (var item in list.Select(f => CompareLine.Create(f)))
			{
				var key = item.NodeKey;
				var assetKey = item.Key;
				if (!dict.ContainsKey(key)) dict.Add(key, new Dictionary<string, List<CompareLine>>());
				if (!dict[key].ContainsKey(assetKey)) dict[key].Add(assetKey, new List<CompareLine>());
				dict[key][assetKey].Add(item);
			}
			return dict;
		}
	}
}

public static class CSV
{
	static Regex csv = new Regex("(?<=^\"|,\")([^\"]|\"\")*(?=\"$|\",)");

	public static string[] GetSplit(this string line)
	{
		return csv.Matches(line.Trim()).OfType<Match>().Select(f => f.Value.Replace("\"\"", "\"")).ToArray();
	}
}

public class CompareNode
{
	public int AlgoID { get; set; }
	public int NodeID { get; set; }
	public string NodeKey { get { return $"{AlgoID}:{NodeID}"; } }

	public CompareNode(int AlgoID, int NodeID)
	{
		this.AlgoID = AlgoID;
		this.NodeID = NodeID;
	}
}

public class CompareAsset : CompareNode
{
	public int AssetID { get; set; }
	public int NodeTypeID { get; set; }
	public NodeType Type { get; set; }
	public string Key { get { return $"{AlgoID}:{NodeID}:{AssetID}:{NodeTypeID}"; } }

	public CompareAsset(int AlgoID, int NodeID, int AssetID, int NodeTypeID) : base(AlgoID, NodeID)
	{
		this.AssetID = AssetID;
		this.NodeTypeID = NodeTypeID;
		this.Type = ((NodeType)NodeTypeID);
		if (!Enum.IsDefined(typeof(NodeType), this.Type))
		{
			if ((((int)this.Type) & 32) == 32) this.Type = NodeType.Question;
			else if ((((int)this.Type) & 64) == 64) this.Type = NodeType.Answer;
		}
	}

	public static CompareAsset Create(string key)
	{
		var ints = key.Split(':').Select(f => int.Parse(f)).ToArray();
		return new CompareAsset(ints[0], ints[1], ints[2], ints[3]);
	}

	public string GetLink()
	{
		var link = "assetbuilder:";
		if (Type.In(NodeType.Start, NodeType.Transfer)) link += "Algo";
		else if (Type.In(NodeType.Stop, NodeType.ChildConclusion)) link += "Conclusion";
		else if (Type.In(NodeType.ChildQuestion)) link += "Question";
		else link += Type.ToString();
		return link + "." + AssetID;
	}
}

public class CompareLine : CompareAsset
{
	public int NextNodeID { get; set; }
	public int DisplayOrder { get; set; }
	public double Counter { get; set; }
	public int GroupID { get; set; }
	public int GroupOrder { get; set; }
	public string Title { get; set; }
	public string ImageSource { get; set; }

	public string Line { get; private set; }

	private CompareLine(int AlgoID, int NodeID, int AssetID, int NodeTypeID) : base(AlgoID, NodeID, AssetID, NodeTypeID)
	{

	}

	public new static CompareLine Create(string line)
	{
		var props = line.GetSplit(); // csv.Matches(line.Trim()).OfType<Match>().Select(f => f.Value.Replace("\"\"", "\"")).ToArray(); //line.Split(',').Select(f => f[1..^1]).ToArray();
		int i = 0;
		var ints = props.Where(f => (i++).In(0, 1, 2, 3, 4, 5, 7, 8)).Select(f => int.Parse(f)).ToArray();
		var cl = new CompareLine(ints[0], ints[1], ints[2], ints[4]);
		cl.NextNodeID = ints[3];
		cl.DisplayOrder = ints[5];
		cl.Counter = double.Parse(props[6]);
		cl.GroupID = ints[6];
		cl.GroupOrder = ints[7];
		cl.Title = props[9];
		cl.ImageSource = props[10];
		cl.Line = line;
		return cl;
	}
}

public enum NodeType
{
	Start = 1,
	Conclusion = 4,
	ChildConclusion = 44,
	Stop = 8,
	Transfer = 16,
	Question = 32,
	ChildQuestion = 45,
	Answer = 64
}

public class LogicCompareReport
{
    public Dictionary<string, Dictionary<string, List<CompareLine>>> SourceNodes { get; set; }
    public Dictionary<string, List<CompareLine>> SourceAssets { get; set; }
    public Dictionary<string, Dictionary<string, List<CompareLine>>> TargetNodes { get; set; }
    public Dictionary<string, List<CompareLine>> TargetAssets { get; set; }
    public HashSet<string> NewNodes { get; set; }
    public HashSet<string> OldNodes { get; set; }
    public HashSet<string> NewAssets { get; set; }
    public HashSet<string> OldAssets { get; set; }
    public Dictionary<string, Dictionary<string, List<CompareLine>>> AddNodes { get; set; }
    public Dictionary<string, Dictionary<string, List<CompareLine>>> DelNodes { get; set; }
    public HashSet<string> NewProps { get; set; }
    public HashSet<string> OldProps { get; set; }
}
