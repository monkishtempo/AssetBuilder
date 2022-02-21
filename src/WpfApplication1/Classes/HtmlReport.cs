using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AssetBuilder.Classes
{
    public class HtmlReport
    {
        public string ExtBase { get; set; }
        private string ExtSummary;
        private string ExtConclusion;

        public HtmlReport(string @base)
        {
            ExtBase = @base;
            ExtSummary = ExtBase + "Traversal/SummaryAsync/{0}";
            ExtConclusion = ExtBase + "Traversal/ConclusionAsync/{0}";
        }

        public string getAgeString(DateTime dob, DateTime calcDate)
        {
            var days = (int)(calcDate - dob).TotalDays;
            var weeks = days / 7;
            var months = (calcDate.Year * 12 + calcDate.Month) - (dob.Year * 12 + dob.Month);
            if (dob.Day > calcDate.Day) months--;
            var years = calcDate.Year - dob.Year;
            if (dob.Month > calcDate.Month || (dob.Month == calcDate.Month && dob.Day > calcDate.Day)) years--;
            if (days < 1) return "newborn";
            if (days < 14) return string.Format("{0} day{1} old", days, days > 1 ? "s" : "");
            if (weeks < 13) return string.Format("{0} weeks old", weeks);
            if (months < 24) return string.Format("{0} months old", months);
            return string.Format("{0} years old", years);
        }

        private string templateSection<T>(string template, string html, IEnumerable<T> array)
        {
            var content = "";
            foreach (var item in array)
                content += template.PropertyReplace(item);
            return html.Replace("$CONTENT$", content); ;
        }

        public void writeTraversalReport(string TraversalID)
        {
            var authUri = new Uri(new Uri(ExtBase), $"Token/GetAsync/service.client/secret");
            var auth = Extension.GetWebRequest(authUri.AbsoluteUri);
            var jAuth = JNode.CreateFromJson(auth);
            var headers = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {jAuth["accessToken"].Value}" }
            };
            var report = JNode.CreateFromJson(Extension.GetWebRequest(string.Format(ExtSummary, TraversalID), headers))["data"];
            var conclusions = JNode.CreateFromJson(Extension.GetWebRequest(string.Format(ExtConclusion, TraversalID), headers))["data"];

            var templates = new Dictionary<string, string>
            {
                { "page", "<!DOCTYPE html><html xmlns=\"http://www.w3.org/1999/xhtml\"><head><link rel=\"stylesheet\" href=\"https://www.w3schools.com/w3css/4/w3.css\" /><link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.6.3/css/font-awesome.min.css\" /></head><body>$CONTENT$</body></html>" },
                { "member", "<div class=\"w3-half\"><table class=\"w3-table w3-striped w3-bordered w3-card w3-section\"><tbody><tr class=\"w3-orange\"><th style=\"border-top-left-radius: 10px\">Property</th><th style=\"border-top-right-radius: 10px\">Value</th></tr>$CONTENT$</tbody></table></div>" },
                { "row", "<tr><th>$FIELD$</th><td>$VALUE$</td></tr>" },
                { "algos", "<div class=\"w3-half\"><table class=\"w3-table w3-striped w3-bordered w3-half w3-card w3-section\"><tbody><tr class=\"w3-blue\"><th style=\"border-top-left-radius: 10px\">AlgoID</th><th style=\"border-top-right-radius: 10px\">Algo name</th></tr>$CONTENT$</tbody></table></div>" },
                { "algorow", "<tr><th><a href=\"assetbuilder:Algo.$ID$\">$ID$</a></th><td>$NAME$</td></tr>" },
                { "questions", "<div class=\"w3-row-padding\"><table class=\"w3-table w3-striped w3-bordered w3-card w3-section\"><tbody><tr class=\"w3-green\"><th style=\"border-top-left-radius: 10px\"><text>QuestionID</text></th><th><text>Question</text></th><th style=\"border-top-right-radius: 10px\"><text>Answers</text></th></tr>$CONTENT$</tbody></table></div>" },
                { "questionrow", "<tr><th><a href=\"assetbuilder:Question.$ID$\">$ID$</a></th><td>$QUESTION$</td><td>$ANSWERS$</td></tr>" },
                { "answer", "<span>$CONTENT$</span>" },
                { "answerneg", "<span class=\"w3-text-red\">NOT - $CONTENT$</span>" },
                { "answervalue", "<span>$VALUE$ $CONTENT$</span>" },
                { "report", "<div class=\"w3-row-padding\">$CONTENT$</div>" },
                { "conclusions", "<div class=\"w3-row-padding\"><table class=\"w3-table w3-striped w3-bordered w3-card w3-section\"><tbody><tr class=\"w3-yellow\"><th style=\"border-top-left-radius: 10px\"><text>ConclusionID</text></th><th><text>Possible condition</text></th><th style=\"border-top-right-radius: 10px\"><text>Explanation</text></th></tr>$CONTENT$</tbody></table></div>" },
                { "conclusionrow", "<tr><th><a href=\"assetbuilder:Conclusion.@ID@\">@ID@</a></th><td style=\"white-space: nowrap;\">@EXPERT@</td><td>@EXPLANATION@</td></tr>" },
            };

            var html = templates["report"];
            var memberfields = new[] { "MemberID", "Gender", "DOB", "Language", "UserType", "Disposition", "Started" };
            var membercontent = "";
            var ageString = "";
            XmlDocument member = new XmlDocument();
            member.LoadXml("<Table></Table>");
            var genderValue = report.SelectMany(f => f["answers"]).Where(f => f["nodeType"] == "34" && (bool)f["isAnswered"]).FirstOrDefault()["displayText"].Value;
            var ageValue = report.SelectMany(f => f["answers"]).Where(f => f["value"] != null && (bool)f["isAnswered"] && f["nodeType"] == "35").FirstOrDefault()["value"].Value;
            DateTime dob = DateTime.MinValue; DateTime started = DateTime.MinValue; int age;
            if (!DateTime.TryParse(ageValue, out dob) && int.TryParse(ageValue, out age)) dob = DateTime.Now - new TimeSpan((int)(age * 365.25), 0, 0, 0);
            if (dob != DateTime.MinValue) member.DocumentElement.AppendChild(member.CreateElement("DOB")).InnerText = dob.ToString("MMM dd, yyyy");
            if (genderValue != null) member.DocumentElement.AppendChild(member.CreateElement("Gender")).InnerText = genderValue;
            member.DocumentElement.AppendChild(member.CreateElement("Started")).InnerText = report.SelectMany(f => f["answers"]).Min(f => f["createdDateTime"].Value);
            if (member["Table"] != null && member["Table"]["DOB"] != null && member["Table"]["Started"] != null && DateTime.TryParse(member["Table"]["DOB"].InnerText, out dob) && DateTime.TryParse(member["Table"]["Started"].InnerText, out started))
                ageString = getAgeString(dob, started);

            foreach (XmlNode item in member["Table"].ChildNodes)
            {
                var name = item.Name;
                var value = item.InnerText;
                DateTime d;
                if (DateTime.TryParse(value, out d)) value = d.ToString("MMM dd, yyyy") + (name == "DOB" && ageString != "" ? " (" + ageString + ")" : " - " + d.ToString("HH:mm:ss"));
                if (memberfields.Contains(item.Name))
                    membercontent += templates["row"].Replace("$FIELD$", item.Name).Replace("$VALUE$", value);
            }

            var algoscontent = "";
            HashSet<string> algos = new HashSet<string>();

            foreach (var item in report.Where(f => f["algoId"] != null && !algos.Contains(f["algoId"].Value)))
            {
                var algoid = item["algoId"].Value;
                var algoname = item["algoId"].Value;
                algos.Add(algoid);
                algoscontent += templates["algorow"].Replace("$ID$", algoid).Replace("$NAME$", algoname);
            }

            //var stub = Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.IndexOf("QAReport"));
            //var reportlink = new Uri(new Uri(ExtBase), "/Traversal/" + TraversalID).AbsoluteUri;
            //if (!string.IsNullOrWhiteSpace(algoscontent)) algoscontent += "<tr><td colspan=\"2\"><a href=\"" + reportlink + "\">Report</a></td></tr>";

            var questioncontent = "";
            var currentquestion = "";
            var currentanswers = "";

            foreach (var item in report.SelectMany(f => f["answers"]).Where(f => f["isAnswered"] == "True"))
            {
                if (item["assetId"] != null && item["assetId"].Value != "0" && item["AnswerID"].Value != "4" && item["AnsTypeID"].Value != "18")
                {
                    var id = item["assetId"].Value;
                    var qt = item.Parent.Parent["displayText"].Value;
                    if (id != currentquestion)
                    {
                        questioncontent = questioncontent.Replace("$ANSWERS$", currentanswers);
                        questioncontent += templates["questionrow"].Replace("$ID$", id).Replace("$QUESTION$", qt);
                        currentanswers = "";
                    }
                    var sv = item["value"].Value;
                    var at = item["displayText"].Value;
                    var aid = int.Parse(item["answerId"].Value);
                    if (at.Length > 0 && at[0] == '?') at = "";
                    if (currentanswers != "") currentanswers += "<br/>";
                    var temp = "";
                    if (sv == "" && aid < 0) temp = templates["answerneg"];
                    else if (!string.IsNullOrEmpty(sv)) temp = templates["answervalue"];
                    else temp = templates["answer"];
                    currentanswers += temp.Replace("$VALUE$", sv).Replace("$CONTENT$", at);
                    currentquestion = id;
                }
            }

            var concs = conclusions.Select(f => new { Priority = (int)f["priority"], Disposition = f["category1"] + " " + f["subCategory"], SubCat2 = f["category2"].Value, ID = (int)f["assetId"], EXPERT = f["displayText"].Value, EXPLANATION = f["explanation"].Value });
            if (concs.Any())
                membercontent += templates["row"].Replace("$FIELD$", "Disposition").Replace("$VALUE$", concs.First().Disposition + "<br/>" + string.Join("<br/>", concs.Where(f => f.Priority == concs.First().Priority).Select(f => string.Format("{0} ({1}) - {2}", f.EXPERT, f.ID, f.SubCat2))));
            questioncontent = questioncontent.Replace("$ANSWERS$", currentanswers);
            var content = templates["member"].Replace("$CONTENT$", membercontent);
            if (!string.IsNullOrWhiteSpace(algoscontent)) content += templates["algos"].Replace("$CONTENT$", algoscontent);
            if (!string.IsNullOrWhiteSpace(questioncontent)) content += templates["questions"].Replace("$CONTENT$", questioncontent);
            if (concs.Any()) content += templateSection(templates["conclusionrow"], templates["conclusions"], concs);
            html = html.Replace("$CONTENT$", content);
            Window w = new Window();
            WebBrowser wb = new WebBrowser();
            wb.NavigateToString(templates["page"].Replace("$CONTENT$", html));
            w.Content = wb;
            w.Show();
        }

    }
}
