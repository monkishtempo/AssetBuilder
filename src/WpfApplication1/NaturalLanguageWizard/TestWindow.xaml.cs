using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace NaturalLanguageWizard
{
    public enum SnipRange
    {
        Exclusive,
        Inclusive,
        LeftEnded,
        RightEnded
    }

    public static class TestWindowUtils
    {        
        public static int GetValueAsInt(this XmlNode item, string name)
        {
            return int.Parse(item.GetValue(name));
        }

        public static string GetValue(this XmlNode item, string name)
        {
            return item[name].InnerText;
        }

        public static void Trim(this List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Trim();
            }
        }

        public static bool StartsWithAny(this string s, params char[] chars)
        {
            return chars.Any(c => s.StartsWith(c));
        }

        public static bool StartsWith(this string s, char c)
        {
            return s[0] == c;
        }

        public static char LastChar(this string s)
        {
            return s[s.Length - 1];
        }        

        public static string SnipOut(this string s, int start, int end)
        {
            return Snip(s, start, end, SnipRange.Exclusive);
        }

        public static string Snip(this string s, int start, int end, SnipRange range)
        {
            switch (range)
            {
                case SnipRange.Exclusive:
                    return s.Substring(start + 1, end - start - 1);

                case SnipRange.Inclusive:
                    return s.Substring(start, end - start + 1);

                case SnipRange.LeftEnded:
                    return s.Substring(start, end - start);

                case SnipRange.RightEnded:
                    return s.Substring(start + 1, end - start);

                default:
                    return string.Empty;
            }           
        }

        public static void AddIfDoesntContain(this List<string> list, string s)
        {
            if (!list.Contains(s))
            {
                list.Add(s);
            }
        }

        public static string InsertClause(this string s, ref int start, int end, string temp)
        {
            return s.Substring(0, start--) + temp + s.Substring(end + 1);
        }

        public static bool Contains(this string s, ref int index, int start, string needle)
        {
            index = s.IndexOf(needle, start);

            return (index > -1);
        }

        public static bool Contains(this string s, ref int index, int start, char needle)
        {
            index = s.IndexOf(needle, start);

            return (index > -1);
        }

        public static string ReplaceWord(this string s, string oldWord, string newWord)
        {
            return Regex.Replace(s, "\\b" + oldWord + "\\b", newWord);
        }
    }

    public static class NLTag
    {
        public static readonly string QP = "qp";
        public static readonly string QS = "qs";
        public static readonly string QT = "qt";
        public static readonly string QV = "qv";
        public static readonly string QA = "qa";
        public static readonly string CC = "cc";
        public static readonly string QD = "qd";
    }

    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    /// 
    public partial class TestWindow : Window
    {
        #region Constructor

        public TestWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Static strings

        // link to encyclopaedia
		static string EncyclopaediaLink = System.Configuration.ConfigurationManager.AppSettings["EncyclopaediaLink"];
            //System.Configuration.ConfigurationManager.AppSettings["EncyclopaediaLink"];

        // link to assets
		static string TextAssetLocation = System.Configuration.ConfigurationManager.AppSettings["TextAssetLocation"];
            //System.Configuration.ConfigurationManager.AppSettings["TextAssetLocation"];

        #endregion

        #region Private members

        // gender char - initialise to U for Unknown?
        private char gender = 'U';

        // dob - initialise to 18 years old?
        private DateTime dob = DateTime.Now - new TimeSpan(6575, 0, 0, 0, 0);

        // self - initialise to false
        private bool self = false;

        XmlNode xn;

        #endregion

        #region Check supplied variables

        private bool HasVariable(string varName)
        {
            //return Request.Form.AllKeys.Contains(varName);

            object control = FindName(varName);
            if (control is CheckBox) return (control as CheckBox).IsChecked == true;
            else if (control is RadioButton) return (control as RadioButton).IsChecked == true;
            return true;
        }

        private string[] GetVariable(string varName)
        {
            //return Request.Form.GetValues(varName);

            return new string[0];
        }

        private bool ClauseSupplied(string clause)
        {
            //return Request.Form["__EVENTTARGET"].Contains(clause);

            return true;
        }

        private bool HasPerson()
        {
            return HasVariable("person");
        }

        private bool HasGender()
        {
            return HasVariable("Gender");
        }

        private char GetGender()
        {            
            return GetVariable("Gender")[0].Split('|')[1][0];
        }

        #endregion

        #region Add controls

        private void AddToConfigPanel(Control c)
        {
            configPanel.Children.Add(c);
        }

        private void AddNewLine()
        {
            //AddToConfigPanel(new HtmlGenericControl("br"));
        }

        private void AddRadioButton(string text, string id, string groupName)
        {
            AddToConfigPanel(new RadioButton { Content = text, Name = id, GroupName = groupName });
        }

        private void AddRadioButton(string text, string id, string groupName, string tooltip)
        {
            AddToConfigPanel(new RadioButton { Content = text, Name = id, GroupName = groupName, ToolTip = tooltip });
        }

        private void AddCheckBox(string text, string id)
        {
            AddToConfigPanel(new CheckBox { Name = id, Content = text });
        }

        private void AddCheckBox(string text, string id, string tooltip)
        {
            AddToConfigPanel(new CheckBox { Name = id, Content = text, ToolTip = tooltip });
        } 

        private void AddButton(string text)
        {
            AddToConfigPanel(new Button { Content = text });
        }

        private void AddLabel(string text)
        {
            AddToConfigPanel(new Label { Content = text });
        }

        private void AddLabel(string text, KeyValuePair<string, string> style)
        {
            var label = new Label { Content = text };
            //label.Style.Add(style); TODO

            AddToConfigPanel(label);
        }

        private void AddTextBox(string id)
        {
            AddToConfigPanel(new TextBox { Name = id });
        }

        #endregion

        #region Add natural language construct

        private void AddCondition(XmlNode item)
        {
            var recID = item.GetValue("RecID");

            AddLabel(recID + ") ");

            AddCheckBox(item.GetValue("Possible_Condition"), string.Format("{0}|{1}|", "cc", recID), recID);

            AddNewLine();
        }

        private void StartNewQuestion(XmlNode item)
        {
            AddNewLine();

            AddLabel(item.GetValue("id") + ") ");

            var text = item.GetValue("Clinical_Statement");

            var style = new KeyValuePair<string, string>("font-weight", "bold");

            AddLabel(text, style);

            AddNewLine();
        }

        private void AddQPAsCheckBox(XmlNode item)
        {
            var text = item.GetValue("Clinical_Answer");

            var questionID = item.GetValue("id");

            var tooltip = questionID;

            var id = string.Format("{0}|{1}|{2}|", "qp", questionID, item.GetValue("AnsID"));

            AddCheckBox(text, id, tooltip);
        }

        private void AddQS(XmlNode item)
        {
            var id = string.Format("{0}|{1}|", "qs", item.GetValue("id"));

            AddTextBox(id);

            AddButton(item.GetValue("Clinical_Answer"));

            AddNewLine();
        }

        private void AddQPAsRadioButton(XmlNode item)
        {
            var questionID = item.GetValue("id");

            var ansID = item.GetValue("AnsID");

            var groupName = string.Format("{0}|{1}|", "qp", questionID);

            var id = string.Format("{0}|{1}|{2}|", "qp", questionID, ansID);

            var text = item.GetValue("Clinical_Answer");

            var tooltip = ansID;

            AddRadioButton(text, id, groupName, tooltip);
        }

        private void AddQuestion(XmlNode item, ref string qid)
        {
            var questionID = item.GetValue("id");

            if (qid != questionID)
            {
                StartNewQuestion(item);
            }

            qid = questionID;

            if (NodeTypeIs33Or53(item) && AnswerTypeIsNot64(item))
            {
                AddQPAsCheckBox(item);
            }

            else if (AnswerTypeIs81(item))
            {
                AddQS(item);
            }

            else
            {
                AddQPAsRadioButton(item);
            }
        }

        private void AddSelfCheckBox()
        {
            AddCheckBox("Self", "person");

            AddNewLine();
        }

        private void AddGenderRadioButtons()
        {
            AddRadioButton("Male", "Gender|M|", "Gender");

            AddRadioButton("Female", "Gender|F|", "Gender");

            AddRadioButton("Unknown", "Gender|U|", "Gender");

            AddNewLine();
        }

        private void GenerateConstructsFromXml(XmlNode xn)
        {
            string qid = "";

            foreach (XmlNode item in xn.SelectNodes("/*"))
            {
                if (item.GetValue("type") == "Conclusion")
                {
                    AddCondition(item);
                }

                if (item.GetValue("type") == "Question")
                {
                    AddQuestion(item, ref qid);
                }
            }
        }

        #endregion

        #region Check node types

        private bool NodeTypeIs33Or53(XmlNode item)
        {
            return (item.GetValueAsInt("NodeTypeID") == 33 || item.GetValueAsInt("NodeTypeID") == 53);
        }

        private bool AnswerTypeIsNot64(XmlNode item)
        {
            return item.GetValueAsInt("AnswerTypeID") != 64;
        }

        private bool AnswerTypeIs81(XmlNode item)
        {
            return item.GetValueAsInt("AnswerTypeID") == 81;
        }

        #endregion        

        #region Initialise

        private XmlDocument GetXmlDocument(List<string> res)
        {
            var doc = new XmlDocument();

            doc.AppendChild(doc.CreateElement("root"));

            foreach (var item in res)
            {
                if (!string.IsNullOrEmpty(item) && (item.StartsWithAny('c', 'q')))
                {                    
                    XmlElement elem = doc.CreateElement("Asset");

                    elem.Attributes.Append(doc.CreateAttribute("ID"));
                    elem.Attributes.Append(doc.CreateAttribute("Type"));

                    if (item.StartsWith('c'))
                    {
                        elem.Attributes["Type"].Value = "Conclusion";
                    }

                    if (item.StartsWith('q'))
                    {
                        elem.Attributes["Type"].Value = "Question";
                    }

                    elem.Attributes["ID"].Value = item.Split('|')[1];

                    doc.DocumentElement.AppendChild(elem);
                }
            }

            return doc;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HasPerson())
            {
                self = true;
            }

            //if (HasGender())
            //{
            //    gender = GetGender();
            //}

            var res = GenerateParsedText(TextBox1.Text, false);

            var doc = GetXmlDocument(res);                       

            xn = AssetBuilder.DataAccess.getDataNode("nl_AssetInfo", new string[] {"@xml", doc.OuterXml }, false);

            if (res.Contains("person"))
            {
                AddSelfCheckBox();
            }

            if (res.Contains("Gender|F|") || res.Contains("Gender|M|"))
            {
                AddGenderRadioButtons();
            }            

            GenerateConstructsFromXml(xn);
            
            litOutput.Text = GenerateParsedText(TextBox1.Text, true)[0].Replace("\n", "<br/>");
        }

        protected bool HasPair(int QuestionID, int AnswerID)
        {
            string qString = string.Format("qp|{0}|", QuestionID);

            string aString = string.Format("qp|{0}|{1}|", QuestionID, AnswerID);

            if (HasVariable(aString))
            {
                return true;
            }

            if (HasVariable(qString))
            {
                return GetVariable(qString)[0] == aString;
            }

            return false;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
        }

        public string GetValue(int QuestionID, string tag)
        {
            return GetValue(QuestionID, 0, tag);
        }

        #endregion

        #region Create HTML controls

        private string GenerateTable(int QuestionID)
        {
            string rv = "";
            XmlNodeList xnl = xn.SelectNodes(string.Format("/Table1[TableID={0} and AnsID!=4 and AnsID!=3]", QuestionID));
            string row = "";
            bool first = true;
            string thead = "<thead><tr><th></th>";

            foreach (XmlNode item in xnl)
            {
                string a = item.GetValue("Answer_Text");
                string q = item.GetValue("Question");

                int qid = item.GetValueAsInt("id");
                int aid = item.GetValueAsInt("AnsID");

                bool sel = HasPair(qid, aid);

                if (q != row)
                {
                    if (row != string.Empty)
                    {
                        first = false;
                    }

                    if (rv != string.Empty)
                    {
                        rv += "</tr>";
                    }

                    rv += string.Format("<tr><td class=\"e24TableRowHeader\">{0}</td>", q);

                    row = q;
                }

                if (first)
                {
                    thead += string.Format("<th>{0}</th>", a);
                }

                rv += string.Format("<td class=\"e24TableCell\">{0}</td>", sel ? "<img src=\"images/tick.png\"/>" : "<img src=\"images/cross.png\"/>");
            }

            thead += "</tr></thead>";

            return string.Format("<table class=\"e24Table\" border=\"0\" cellSpacing=\"1\" cellPadding=\"0\">{0}<tbody>{1}</tbody></table>", thead, rv);

        }

        public string GetValue(int QuestionID, int AnswerID, string tag)
        {
            if (tag == NLTag.QP)
            {
                return HasPair(QuestionID, AnswerID).ToString();
            }

            if (tag == NLTag.CC)
            {
                return HasVariable(string.Format("cc|{0}|", QuestionID)).ToString();
            }

            if (tag == NLTag.QA)
            {
                var result = string.Empty;

                var xnl = xn.SelectNodes(string.Format("/Table1[id={0} and AnsID!=4 and AnsID!=3]", QuestionID));

                foreach (XmlNode item in xnl)
                {
                    var aid = item.GetValueAsInt("AnsID");

                    if (HasPair(QuestionID, aid))
                    {
                        if (result != "")
                        {
                            result += '|';
                        }

                        result += GenerateSpan(item.GetValue("Answer_Text"), string.Format("qp|{0}|{1}|", QuestionID, aid), "red");
                    }
                }

                return result;
            }

            if (IsValueQuestionTag(tag) && HasVariable(string.Format("qs|{0}|", QuestionID)))
            {
                return GetVariable(string.Format("qs|{0}|", QuestionID))[0];
            }

            if (tag == NLTag.QT)
            {
                return GenerateTable(QuestionID);
            }

            return "";
        }

        private bool IsValueQuestionTag(string tag)
        {
            return (tag == NLTag.QS || tag == NLTag.QV || tag == NLTag.QD);
        }

        private string ChangeToFeminineGender(string s)
        {
            s = s.ReplaceWord("he", "she");
            s = s.ReplaceWord("He", "She");
            s = s.ReplaceWord("HE", "SHE");
            s = s.ReplaceWord("his", "her");
            s = s.ReplaceWord("His", "Her");
            s = s.ReplaceWord("HIS", "HER");
            s = s.ReplaceWord("him", "her");
            s = s.ReplaceWord("Him", "Her");
            s = s.ReplaceWord("HIM", "HER");

            return s;
        }

        private int[] CreatePositioners()
        {
            var pos = new int[3];

            pos[0] = -1;

            return pos;
        }

        private string GetTextAsset(string location)
        {
            var temp = string.Empty;

            var req = (HttpWebRequest)HttpWebRequest.Create(TextAssetLocation + location);

            try
            {
                var resp = (HttpWebResponse)req.GetResponse();

                var sr = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.Default);

                temp = sr.ReadToEnd().Replace("\r", "").Replace("\n", " ");

                ReplaceRelativeLinks(ref temp, TextAssetLocation);

                sr.Close();

                resp.Close();
            }

            catch (Exception ex)
            {
                temp += " " + ex.Message;
            }

            return temp;
        }

        private void AddTildeDelimited(ref string s, List<string> res)
        {
            var pos = CreatePositioners();

            while (s.Contains(ref pos[0], pos[0] + 1, '~'))
            {
                if (!s.Contains(ref pos[1], pos[0] + 1, '~'))
                {
                    break;
                }

                var temp = s.Snip(pos[0] + 2, pos[1], SnipRange.LeftEnded);

                var peek = s[pos[0] + 1];

                if (peek == 'W')
                {
                    temp = temp.Substring(2);
                }

                else if (peek == 'I')
                {
                    temp = string.Format("<img src=\"VEImages/{0}\" alt=\"VEImages/{0}\"/>", temp);
                }

                else if (peek == 'T')
                {
                    temp = GetTextAsset(temp);
                }

                else if (peek == 'E' && EncyclopaediaLink != null)
                {
                    string[] encyclink = temp.Split('#');

                    temp = string.Format("<a href=\"{0}?ArticleID={1}\">{2}</a>", EncyclopaediaLink, encyclink[1], encyclink[0]);
                }

                else
                {
                    string clause = string.Format("Gender|{0}|", peek);

                    res.AddIfDoesntContain(clause);

                    if (peek == gender)
                    {
                        temp = GenerateSpan(temp, clause, "red");
                    }
                    else
                    {
                        temp = string.Empty;
                    }
                }

                s.InsertClause(ref pos[0], pos[1], temp);
            }
        }

        private bool AddAge(ref string s, List<string> res, string temp, ref int[] pos)
        {
            double nod, ml;

            int st, ed;

            ml = 0;

            switch (temp[7])
            {
                case 'y':
                case 'Y':
                    ml = 365.25;
                    break;

                case 'm':
                case 'M':
                    ml = 30.4375;
                    break;

                case 'w':
                case 'W':
                    ml = 7;
                    break;

                case 'd':
                case 'D':
                    ml = 1;
                    break;

                default:
                    break;
            }

            if (!int.TryParse(temp.Substring(0, 3), out st))
            {
                return false;
            }

            if (!int.TryParse(temp.Substring(4, 3), out ed))
            {
                return false;
            }

            nod = (DateTime.Now - dob).TotalDays;

            var clause = string.Format("Age|{0}|{1}|{2}|", st, ed, ml);

            res.AddIfDoesntContain(clause);

            if (ml > 0 && nod >= st * ml && nod < ed * ml)
            {
                s = s.InsertClause(ref pos[0], pos[1], temp.Substring(8));
            }

            else
            {
                s = s.InsertClause(ref pos[0], pos[1], string.Empty);
            }

            return true;
        }

        private string GetSeparatorText(string temp, string clause)
        {
            var separator = string.Empty;

            var lastSeparator = string.Empty;

            switch (clause.LastChar())
            {
                case 'l':
                    separator = ", ";
                    lastSeparator = string.Empty;
                    break;

                case 'r':
                    separator = Environment.NewLine;
                    lastSeparator = string.Empty;
                    break;

                case 'a':
                    separator = ", ";
                    lastSeparator = " and ";
                    break;

                case 'o':
                    separator = ", ";
                    lastSeparator = " or ";
                    break;

                case 'b':
                    separator = "BULLET";
                    lastSeparator = string.Empty;
                    break;
            }

            return GenerateList(temp, separator, lastSeparator);
        }

        private string GetQuestionPairText(string temp, List<string> res, bool apply, ref int qid)
        {
            string[] qp = temp.Substring(2).Split('|');

            string testclause = string.Format("qp|{0}|{1}|", qp[0], qp[1]);

            res.AddIfDoesntContain(testclause);

            int aid;

            if (apply && qp.Length > 2 && int.TryParse(qp[0], out qid) && int.TryParse(qp[1], out aid) && bool.Parse(GetValue(qid, aid, "qp")))
            {
                return GenerateSpan(qp[2], testclause, "red");
            }

            else
            {
                return (qp.Length > 3 ? GenerateSpan(qp[3], qp[0], "red") : "");
            }
        }

        private string GetConclusionText(string temp, List<string> res, bool apply)
        {
            var cc = temp.Substring(2).Split('|');

            var testclause = string.Format("cc|{0}|", cc[0]);

            res.AddIfDoesntContain(testclause);

            int cid;

            if (apply && cc.Length > 1 && int.TryParse(cc[0], out cid) && bool.Parse(GetValue(cid, 0, "cc")))
            {
                return GenerateSpan(cc[1], testclause, "red");

            }
            else
            {
                return (cc.Length > 2 ? GenerateSpan(cc[2], testclause, "red") : "");
            }
        }

        private string GetQuestionValue(string temp, string clause, List<string> res, bool apply, int qid)
        {
            if (clause == "nt")
            {
                //int value = int.Parse(GetValue(_TraversalID, qid, "qv"));
                return "$$$nt$$$";
                //s = ChooseTable(s.Substring(0, pos[0]), value);
            }

            else
            {
                var qp = temp.Substring(2).Split('|');

                var testclause = string.Format("{0}|{1}|", temp.Substring(0, 2), qp[0]);

                res.AddIfDoesntContain(testclause);

                return (apply ? GenerateSpan(GetValue(qid, clause), testclause, "red") : "");
            }
        }

        private void AddQuestion(ref string s, List<string> res, string temp, ref int[] pos, bool apply)
        {
            var qid = default(int);

            var clause = temp.Substring(0, 2);

            if (clause.StartsWith('l'))
            {
                temp = GetSeparatorText(temp, clause);
            }

            else if (clause == "qp")
            {
                temp = GetQuestionPairText(temp, res, apply, ref qid);
            }

            else if (clause == "cc")
            {
                temp = GetConclusionText(temp, res, apply);
            }

            else if (int.TryParse(temp.Substring(2), out qid))
            {
                temp = GetQuestionValue(temp, clause, res, apply, qid);
            }

            s = s.InsertClause(ref pos[0], pos[1], temp);
        }

        private void AddBraceDelimited(ref string s, List<string> res, bool apply)
        {
            var pos = CreatePositioners();

            while (pos[0] + 1 < s.Length && s.Contains(ref pos[0], pos[0] + 1, '{'))
            {
                if (!s.Contains(ref pos[1], pos[0] + 1, '}'))
                {
                    break;
                }

                // Very clever nesting code
                pos[2] = pos[0];

                var start = pos[0];

                var nested = false;

                while (s.Contains(ref pos[2], pos[2] + 1, '{') && pos[2] < pos[1])
                {
                    pos[0] = pos[2];

                    nested = true;
                }

                if (pos[1] == -1)
                {
                    break;
                }

                var temp = s.SnipOut(pos[0], pos[1]);

                if (temp.Length > 3 && temp[3] == '-')
                {
                    if (!AddAge(ref s, res, temp, ref pos))
                    {
                        break;
                    }
                }

                else
                {
                    AddQuestion(ref s, res, temp, ref pos, apply);                    
                }

                if (nested)
                {
                    pos[0] = start - 1;
                }
            }
        }

        private void AddSelfText(ref string s, List<string> res)
        {
            var pos = CreatePositioners();

            while (s.Contains(ref pos[0], pos[0] + 1, '['))
            {
                string clause = "person";

                res.AddIfDoesntContain(clause);

                if (!s.Contains(ref pos[1], pos[0] + 1, ']'))
                {
                    break;
                }

                //Very clever nesting code
                //pos[2] = pos[0];
                //while ((pos[2] = s.IndexOf('[', pos[2] + 1)) > -1 && pos[2] < pos[1])
                //    pos[1] = s.IndexOf(']', pos[1] + 1);
                //if (pos[1] == -1) break;

                var temp = s.Substring(pos[0], pos[1] - pos[0]);

                if (temp.Contains(ref pos[2], 1, '/'))
                {
                    var spanContent = string.Empty;

                    if (self)
                    {
                        spanContent = temp.Substring(pos[2] + 1);
                    }

                    else if (gender == 'F')
                    {
                        temp = temp.Substring(0, pos[2] + 1);

                        temp = ChangeToFeminineGender(temp);

                        spanContent = temp.Substring(1, temp.Length - 2);
                    }

                    else
                    {
                        spanContent = temp.Substring(1, pos[2] - 1);
                    }


                    s = s.InsertClause(ref pos[0], pos[1], GenerateSpan(spanContent, "person", "red"));
                }
            }
        }

        public List<string> GenerateParsedText(string s, bool apply)
        {
            var res = new List<string>();

            AddTildeDelimited(ref s, res);

            AddBraceDelimited(ref s, res, apply);

            AddSelfText(ref s, res);            

            res.Insert(0, s);

            return res;
        }        

        private string GenerateSpan(string temp, string clause, string colour)
        {
            if (string.IsNullOrEmpty(temp))
            {
                return string.Empty;
            }

            colour = (ClauseSupplied(clause) ? colour : "#0000aa");

            return string.Format("<span style=\"color:{0}\">{1}</span>", colour, temp);
        }

        private void ReplaceRelativeLinks(ref string s, string p, string linkText)
        {
            var pos = CreatePositioners();

            while (s.Contains(ref pos[0], pos[0] + 1, linkText + "="))
            {
                if(!s.Contains(ref pos[1], pos[0] + 1, '\"'))
                {
                    break;
                }

                if(!s.Contains(ref pos[2], pos[1] + 1, '\"'))
                {
                    break;
                }

                var temp = s.SnipOut(pos[1], pos[2]);

                if (!temp.StartsWith("http"))
                {
                    s = s.Substring(0, pos[1] + 1) + p + temp + s.Substring(pos[2]);
                }
            }
        }

        private void ReplaceRelativeLinks(ref string s, string p)
        {
            ReplaceRelativeLinks(ref s, p, "href");

            ReplaceRelativeLinks(ref s, p, "src");            
        }

        private string GenerateListItems(List<string> list, string separator, string lastSeparator)
        {
            var temp = string.Empty;

            for (var i = 0; i < list.Count; i++)
            {
                if (separator != "BULLET")
                {
                    if (i == list.Count - 1 && list.Count > 1 && !string.IsNullOrEmpty(lastSeparator))
                    {
                        temp += lastSeparator;
                    }

                    else if (!string.IsNullOrEmpty(temp))
                    {
                        temp += separator;
                    }

                    temp += list[i];
                }

                else
                {
                    temp += string.Format("<li>{0}</li>", list[i]);
                }
            }

            return temp;
        }

        private string GenerateList(string temp, string separator, string lastSeparator)
        {
            var list = new List<string>(temp.Substring(2).Split('|'));

            list.Trim();

            list.RemoveAll(delegate(string s) { return string.IsNullOrEmpty(s) || s.StartsWith("'Error: "); });

            temp = string.Empty;

            var isNonEmptyBulletList = (separator == "BULLET" && list.Count > 0);

            if (isNonEmptyBulletList)
            {
                temp += "<ul>";
            }

            temp += GenerateListItems(list, separator, lastSeparator);            

            if (isNonEmptyBulletList)
            {
                temp += "</ul>";
            }

            return temp;
        }

        #endregion 
    }
}
