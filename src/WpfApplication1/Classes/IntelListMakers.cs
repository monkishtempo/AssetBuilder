using System.Collections.Generic;
using System.Windows.Controls;
using System.Xml;
using System;
using System.Text.RegularExpressions;

namespace AssetBuilder.Controls
{
    static class IntelListMakers
    {
        static char[] pipeEnd = "{|}".ToCharArray();
        static char[] curlyEnd = "}".ToCharArray();
        static char[] squareEnd = "]".ToCharArray();
        static char[] tildaEnd = "~".ToCharArray();
        static char[] greaterThanEnd = ">".ToCharArray();

        public static Dictionary<string, IntelModel> mylistmakers = new Dictionary<string, IntelModel>()
            {
                { "~E", new IntelModel { getListMethod = new Intel.GetList(getArticlesTilda), endings = tildaEnd } },
                { "Regex:(^|.*[.])[^A-za-z]*\\[$", new IntelModel { values = new List<IntelItem>() { "Does he/Do you]", "He/You]", "He has/You have]", "He is/You are]", "His/Your]", "Is he/Are you]", "Is his/Are your]" }, endings = squareEnd }},
                { "[", new IntelModel { values = new List<IntelItem>() { "does he/do you]", "he/you]", "he has/you have]", "he is/you are]", "him/you]", "his/your]", "is he/are you]", "is his/are your]" }, endings = squareEnd }},
                { "{", new IntelItem[] {
                    new IntelItem{ Value = "cc", Display = "cc - Conclusion check", ToolTip = new Info{ Width="700", Image="/images/Conclusion.png", Title = "Natural Language - Conclusion check", Body="ccToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "gc", Display = "gc - Group check", ToolTip = new Info{ Width="700", Image="/images/group_32x32.png", Title = "Natural Language - Group check", Body="gcToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qc", Display = "qc - Question check", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Question check", Body="qcToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qn", Display = "qn - Question None", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Question check", Body="qcToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "la", Display = "la - List with 'and'", ToolTip = new Info{ Width="700", Title = "Natural Language - List with 'and'", Body="laToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "lb", Display = "lb - Bulleted list", ToolTip = new Info{ Width="700", Title = "Natural Language - Bulleted list", Body="lbToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "ln", Display = "ln - Ordered list", ToolTip = new Info{ Width="700", Title = "Natural Language - Ordered list", Body="lnToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "ll", Display = "ll - Linear list", ToolTip = new Info{ Width="700", Title = "Natural Language - Linear list", Body="llToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "lo", Display = "lo - List with 'or'", ToolTip = new Info{ Width="700", Title = "Natural Language - List with 'or'", Body="loToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "lr", Display = "lr - List with hard returns", ToolTip = new Info{ Width="700", Title = "Natural Language - List with hard returns", Body="lrToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "ca", Display = "ca - Conclusion lay statement", ToolTip = new Info{ Width="700", Image="/images/Conclusion.png", Title = "Natural Language - Conclusion lay statement", Body="caToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "ga", Display = "ga - Group Name", ToolTip = new Info{ Width="700", Image="/images/group_32x32.png", Title = "Natural Language - Group check", Body="gaToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qa", Display = "qa - Answer from question", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Answer from question", Body="qaToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "ce", Display = "ce - Conclusion explanation", ToolTip = new Info{ Width="700", Image="/images/Conclusion.png", Title = "Natural Language - Conclusion explanation", Body="ceToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qd", Display = "qd - Date from question", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Date from question", Body="qdToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qp", Display = "qp - Question/Answer pair check", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Question/Answer pair check", Body="qpToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qs", Display = "qs - Free text from question", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Free text from question", Body="qsToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qt", Display = "qt - Html table from question", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Html table from question", Body="qtToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "qv", Display = "qv - Numeric value from question", ToolTip = new Info{ Width="700", Image="/images/Question.png", Title = "Natural Language - Numeric value from question", Body="qvToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "xf", Display = "xf - Gender condition is female", ToolTip = new Info{ Width="700", Image="/images/Female.png", Title = "Natural Language - Gender condition is female", Body="xxToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "xm", Display = "xm - Gender condition is male", ToolTip = new Info{ Width="700", Image="/images/Male.png", Title = "Natural Language - Gender condition is male", Body="xyToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "xx", Display = "xx - Gender condition is female", ToolTip = new Info{ Width="700", Image="/images/Female.png", Title = "Natural Language - Gender condition is female", Body="xxToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "xy", Display = "xy - Gender condition is male", ToolTip = new Info{ Width="700", Image="/images/Male.png", Title = "Natural Language - Gender condition is male", Body="xyToolTip" }.ProvideValue(null) },
                    new IntelItem{ Value = "xz", Display = "xz - Gender condition is not male or female", ToolTip = new Info{ Width="700", Image="/images/Male-Female.png", Title = "Natural Language - Gender condition is not male or female", Body="xzToolTip" }.ProvideValue(null) },
                    }},
                { "Regex:{c[ae]$", new IntelModel { getListMethod = new Intel.GetList(getConclusionsBrace), endings = curlyEnd }},
{ "{cc", new IntelModel { getListMethod = new Intel.GetList(getConclusions), endings = pipeEnd }},
{ "{ga", new IntelModel { getListMethod = new Intel.GetList(getGroupsBrace), endings = curlyEnd }},
{ "{gc", new IntelModel { getListMethod = new Intel.GetList(getGroups), endings = pipeEnd }},
{ "{qc", new IntelModel { getListMethod = new Intel.GetList(getQuestions), endings = pipeEnd }},
{ "{qn", new IntelModel { getListMethod = new Intel.GetList(getMultiSelectQuestions), endings = pipeEnd }},
{ "Regex:{q[p]$", new IntelModel { getListMethod = new Intel.GetList(getQuestions), endings = pipeEnd }},
{ "Regex:{q[adstv]$", new IntelModel { getListMethod = new Intel.GetList(getQuestionsBrace), endings = curlyEnd }},
{ "Regex:{q[vsd][0-9]+\\|$", new IntelModel { getListMethod = new Intel.GetList(getAnswers), endings = curlyEnd }},
{ "Regex:{qp[0-9]+\\|$", new IntelModel { getListMethod = new Intel.GetList(getAnswers), endings = pipeEnd }},
{ "Regex:{qp[0-9]+\\|[0-9]*\\|$", new IntelModel { IsStatic=true, getListMethod = new Intel.GetList(getQPHelperText), endings = pipeEnd }},
{ "Regex:{qp[0-9]+\\|[0-9]*\\|.*\\|$", new IntelModel { IsStatic=true, getListMethod = new Intel.GetList(getQPHelperText), endings = pipeEnd }},
{ "</", new IntelModel { NoDelay = true, append = ">", getListMethod = new Intel.GetList(getOpenElement), endings = greaterThanEnd }},
    };

        public static Dictionary<string, IntelModel> conclookup = new Dictionary<string, IntelModel>()
{
{ "FullMatch:", new IntelModel { getListMethod = new Intel.GetList(getConclusionsNoAppend) }},
    };

        public static Dictionary<string, IntelModel> makeList(string prompt, XmlNode defaults, string table)
        {
            //Dictionary<string, IntelModel> list = new Dictionary<string, IntelModel>();
            List<IntelItem> list = new List<IntelItem>();
            foreach (XmlNode item in defaults.SelectNodes(string.Format("*[*[1] = '{0}' and not(Exclude)]", table)))
            {
                string val = item.ChildNodes[1].InnerText;
                int tv;
                if (int.TryParse(val, out tv) && tv < 0) val = "(" + val + ")";
                list.Add(new IntelItem { Value = val, Display = item.ChildNodes[2].InnerText });
            }

            return new Dictionary<string, IntelModel>(){
{ prompt, list.ToArray() },
    };
        }

        static Dictionary<string, string> cache = new Dictionary<string, string>();

        public static void clearCache()
        {
            cache.Clear();
        }

        static string getString(string assettype, string id, string fieldname)
        {
            string key = string.Format("{0}-{1}-{2}", assettype, id, fieldname);
            if (cache.ContainsKey(key)) return cache[key];
            XmlNode xe = DataAccess.getDataNode("ab_GetAsset", new string[] {
    "@AssetTypeID", assettype,
    "@AssetID", id
    }, false);
            string ret = xe["Table"][fieldname].InnerText;
            if (!cache.ContainsKey(key)) cache.Add(key, ret);
            return ret;
        }

        static List<IntelItem> getOpenElement(string search, TextBox tb)
        {
            string opentag = NLExtensions.OpenElement;
            if (opentag == "" || !opentag.StartsWith(search)) return new List<IntelItem>();
            return new List<IntelItem>() { new IntelItem() { Value = NLExtensions.OpenElement + ">", Display = "</" + NLExtensions.OpenElement + ">" } };
        }

        static List<IntelItem> getQPHelperText(string search, TextBox tb)
        {
            string[] ids = getIds("qp", tb.Text, tb.SelectionStart - search.Length).Split('|');
            if (ids.Length >= 2)
            {
                string question = getString("2", ids[0], "Question");
                string answer = getString("3", ids[1], "Answer_Text");
                return new List<IntelItem>() { string.Format("Text to display if question[{0}]/answer[{1}] pair is {4}reached\n\n{2}\n\n{3}", ids[0], ids[1], question, answer, ids.Length > 2 ? "not " : "") };
            }
            else
                return new List<IntelItem>();
        }

        static int findStart(string s)
        {
            int cc = 0;
            int x = s.Length - 1;
            while (x >= 0 && !(cc == 0 && s[x] == '{'))
            {
                if (s[x] == '}') cc++;
                if (s[x] == '{') cc--;
                x--;
            }
            return x;
        }

        static string getIds(string clause, string text, int location)
        {
            string substring = text.Substring(0, location);
            int qpStart = findStart(substring); //substring.LastIndexOf("{" + clause);
            if (substring.Length < qpStart + 3 || !Regex.IsMatch(substring.Substring(qpStart + 1, 2), clause)) return "";
            int lastPipe = substring.LastIndexOf('|');
            if (qpStart > -1 && lastPipe > qpStart)
                return substring.Substring(qpStart + 3, lastPipe - (qpStart + 3));
            else return "";
        }

        static List<IntelItem> getArticlesTilda(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), -1, "#", format:"{2}{1}{0}~");
        }

        static List<IntelItem> getQuestionsBrace(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 2, "}");
        }

        static List<IntelItem> getGroupsBrace(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 13, "}");
        }

        static List<IntelItem> getConclusionsBrace(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 4, "}");
        }

        static List<IntelItem> getQuestions(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 2);
        }

        static List<IntelItem> getMultiSelectQuestions(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 2, additionalParameters: new string[] { "@type", "2" });
        }

        static List<IntelItem> getAnswers(string search, TextBox tb)
        {
            string qid = getIds("q[pvsd]", tb.Text, tb.SelectionStart - search.Length);
            if (qid != "")
                return getDataForList("question:" + qid + (search != "" ? " " + search : ""), 3);
            else
                return new List<IntelItem>();
        }

        static List<IntelItem> getConclusions(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 4);
        }

        static List<IntelItem> getGroups(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 13);
        }

        static List<IntelItem> getConclusionsNoAppend(string search, TextBox tb)
        {
            return getDataForList(string.Format("%{0}%", search), 4, "");
        }

        static List<IntelItem> getDataForList(string search, int AssetType, string append = "|", string[] additionalParameters = null, string format = null)
        {
            List<string> prms = qcat.getParameters(false, 3, AssetType, ref search);
            prms[1] = "5";
            if (additionalParameters != null) prms.AddRange(additionalParameters);
            XmlNode xn = DataAccess.getDataNode("ab_getItems", prms.ToArray(), false);
            if (format == null) format = "{0}{1}";

            List<IntelItem> ret = new List<IntelItem>();
            foreach (XmlNode item in xn.SelectNodes("//*[BoxID='5' and ID and Description]"))
            {
                ret.Add(new IntelItem { Value = string.Format(format, item["ID"].InnerText, append, item["Description"].InnerText), ToolTip = item["ID"].InnerText, Display = item["Description"].InnerText });
            }
            return ret;
        }
    }
}
