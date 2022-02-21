using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for NaturalLanguage.xaml
    /// </summary>
    public partial class NaturalLanguage : Controls.ABWindow
    {
        public string NLText
        {
            get
            {
                //TextRange tr = new TextRange(text.Document.ContentStart, text.Document.ContentEnd);
                //return tr.Text;
                return text.Text;
            }
            //set
            //{
            //    TextRange tr = new TextRange(text.Document.ContentStart, text.Document.ContentEnd);
            //    tr.Text = value;
            //    process();
            //}
        }
        public string HTMLText { set { render.NavigateToString(string.Format("<html><head></head><body>{0}</body></html>", value)); } }

		static string EncyclopaediaLink = System.Configuration.ConfigurationManager.AppSettings["EncyclopaediaLink"];
		static string TextAssetLocation = System.Configuration.ConfigurationManager.AppSettings["TextAssetLocation"];

        private char gender = 'U';
        private DateTime dob = DateTime.Now - new TimeSpan(6575, 0, 0, 0, 0);
        private bool self = false;

        string _TraversalID = "";
        //int _MemberID;

        XmlNode xn;
        WebBrowser render;
        StackPanel controls;
        TextBox text;

        public NaturalLanguage(TextBox tb)
        {
            InitializeComponent();
            text = tb;
            Window browser = new Window();
            Grid bgr = new Grid();
            browser.Content = bgr;
            render = new WebBrowser();
            bgr.Children.Add(render);
            browser.Show();

            Window questions = new Window();
            controls = new StackPanel();
            questions.Content = controls;
            questions.Show();

            //render.NavigateToString("<html><head></head><body></body></html>");

        }

        bool RequestContains(string key)
        {
            object control = FindName(key);
            if (control is CheckBox) return (control as CheckBox).IsChecked == true;
            else if (control is RadioButton) return (control as RadioButton).IsChecked == true;
            return true;
        }

        string RequestValue(string key)
        {
            return "";
        }

        bool RequestFormContains(string clause)
        {
            return true;
        }

        void AddLabelControl(string text)
        {
            controls.Children.Add(new Label { Content = text });
        }

        void AddBoldLabelControl(string text)
        {
            controls.Children.Add(new Label { Content = text, FontWeight = FontWeights.Bold });
        }

        void AddButtonControl(string text)
        {
            controls.Children.Add(new Button { Content = text });
        }

        void AddCheckBoxControl(string ID, string text)
        {
            AddCheckBoxControl(ID, text, "");
        }

        void AddCheckBoxControl(string ID, string text, string toolTip)
        {
            CheckBox cb = new CheckBox { Uid = ID, Content = text, ToolTip = toolTip };
            RoutedEventHandler updated = delegate(object sender, RoutedEventArgs e) { HTMLText = genderText(NLText, true)[0].Replace("\n", "<br/>"); };
            cb.Checked += updated;
            cb.Unchecked += updated;
            controls.Children.Add(cb);
        }

        void AddRadioControl(string ID, string text, string groupName)
        {
            AddRadioControl(ID, text, groupName, "");
        }

        void AddRadioControl(string ID, string text, string groupName, string toolTip)
        {
            controls.Children.Add(new RadioButton { Uid = ID, Content = text, GroupName = groupName, ToolTip = toolTip });
        }

        void AddTextBoxControl(string ID)
        {
            controls.Children.Add(new TextBox());
        }

        void AddBreakControl()
        {
            controls.Children.Add(new Label());
        }

        void process()
        {
            if (RequestContains("person")) self = true;
            //if (RequestContains("Gender")) gender = RequestValue("Gender").Split('|')[1][0];
            List<string> res = genderText(NLText, false);
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("root"));

            foreach (var item in res)
            {
                if (item != "" && (item[0] == 'c' || item[0] == 'q'))
                {
                    XmlElement elem = doc.CreateElement("Asset");
                    elem.Attributes.Append(doc.CreateAttribute("ID"));
                    elem.Attributes.Append(doc.CreateAttribute("Type"));
                    if (item[0] == 'c') elem.Attributes["Type"].Value = "Conclusion";
                    if (item[0] == 'q') elem.Attributes["Type"].Value = "Question";
                    elem.Attributes["ID"].Value = item.Split('|')[1];
                    doc.DocumentElement.AppendChild(elem);
                }
            }
            xn = DataAccess.getDataNode("nl_AssetInfo", new string[] {
                "@xml", doc.OuterXml
            }, false);
            if (res.Contains("person"))
            {
                AddCheckBoxControl("person", "Self");
                //configPanel.Controls.Add(new HtmlGenericControl("br"));
            }
            if (res.Contains("Gender|F|") || res.Contains("Gender|M|"))
            {
                AddRadioControl("Gender|M|", "Male", "Gender");
                AddRadioControl("Gender|F|", "Female", "Gender");
                AddRadioControl("Gender|U|", "Unknown", "Gender");
                //configPanel.Controls.Add(new HtmlGenericControl("br"));
            }

            string qid = "";
            List<string> ids = new List<string>();

            foreach (XmlNode item in xn.SelectNodes("/*"))
            {
                if (item["type"].InnerText == "Conclusion")
                {
                    string id = string.Format("{0}|{1}|", "cc", item["RecID"].InnerText);
                    if (ids.Contains(id)) continue;
                    ids.Add(id);
                    AddCheckBoxControl(id, item["Possible_Condition"].InnerText);
                    //configPanel.Controls.Add(new HtmlGenericControl("br"));
                }
                if (item["type"].InnerText == "Question")
                {
                    if (qid != item["id"].InnerText)
                    {
                        AddBreakControl();
                        AddLabelControl(item["id"].InnerText + ") ");
                        AddBoldLabelControl(item["Clinical_Statement"].InnerText);
                        //configPanel.Controls.Add(new HtmlGenericControl("br"));
                    }
                    qid = item["id"].InnerText;
                    if ((item["NodeTypeID"].InnerText == "33" || item["NodeTypeID"].InnerText == "53") && item["AnswerTypeID"].InnerText != "64")
                    {
                        string id = string.Format("{0}|{1}|{2}|", "qp", item["id"].InnerText, item["AnsID"].InnerText);
                        if (ids.Contains(id)) continue;
                        ids.Add(id);
                        AddCheckBoxControl(id, item["Clinical_Answer"].InnerText, item["id"].InnerText);
                    }
                    else if (item["AnswerTypeID"].InnerText == "81")
                    {
                        string id = string.Format("{0}|{1}|", "qs", item["id"].InnerText);
                        if (ids.Contains(id)) continue;
                        ids.Add(id);
                        AddTextBoxControl(id);
                        AddButtonControl(item["Clinical_Answer"].InnerText);
                        //configPanel.Controls.Add(new HtmlGenericControl("br"));
                    }
                    else
                    {
                        string id = string.Format("{0}|{1}|{2}|", "qp", item["id"].InnerText, item["AnsID"].InnerText);
                        if (ids.Contains(id)) continue;
                        ids.Add(id);
                        AddRadioControl(id,
                            item["Clinical_Answer"].InnerText,
                            string.Format("{0}|{1}|", "qp", item["id"].InnerText),
                            item["AnsID"].InnerText);
                    }
                }
            }
            HTMLText = genderText(NLText, true)[0].Replace("\n", "<br/>");
        }

        protected bool hasPair(int QuestionID, int AnswerID)
        {
            string qString = string.Format("qp|{0}|", QuestionID);
            string aString = string.Format("qp|{0}|{1}|", QuestionID, AnswerID);
            if (RequestContains(aString)) return true;
            if (RequestContains(qString)) return RequestValue(qString) == aString;
            return false;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
        }

        public string GetValue(string TraversalID, int QuestionID, string Value)
        {
            return GetValue(TraversalID, QuestionID, 0, Value);
        }

        public string GetValue(string TraversalID, int QuestionID, int AnswerID, string Value)
        {
            if (Value == "qp") return hasPair(QuestionID, AnswerID).ToString();
            if (Value == "cc") return RequestContains(string.Format("cc|{0}|", QuestionID)).ToString();
            if (Value == "ca" && RequestContains(string.Format("cc|{0}|", QuestionID)))
            {
            }
            if (Value == "qa")
            {
                string result = "";
                XmlNodeList xnl = xn.SelectNodes(string.Format("/Table1[id={0} and AnsID!=4 and AnsID!=3]", QuestionID));
                foreach (XmlNode item in xnl)
                {
                    int aid = int.Parse(item["AnsID"].InnerText);
                    if (hasPair(QuestionID, aid))
                    {
                        if (result != "") result += '|';
                        result += span(item["Answer_Text"].InnerText, string.Format("qp|{0}|{1}|", QuestionID, aid), "red");
                    }
                }
                return result;
            }
            if ((Value == "qs" || Value == "qv" || Value == "qd") && RequestContains(string.Format("qs|{0}|", QuestionID))) return RequestValue(string.Format("qs|{0}|", QuestionID));
            if (Value == "qt")
            {
                string rv = "";
                XmlNodeList xnl = xn.SelectNodes(string.Format("/Table1[TableID={0} and AnsID!=4 and AnsID!=3]", QuestionID));
                string row = "";
                bool first = true;
                string thead = "<thead><tr><th></th>";
                foreach (XmlNode item in xnl)
                {
                    string a = item["Answer_Text"].InnerText;
                    string q = item["Question"].InnerText;
                    int qid = int.Parse(item["id"].InnerText);
                    int aid = int.Parse(item["AnsID"].InnerText);
                    bool sel = hasPair(qid, aid);
                    if (q != row)
                    {
                        if (row != "") first = false;
                        if (rv != "") rv += "</tr>";
                        rv += string.Format("<tr><td class=\"e24TableRowHeader\">{0}</td>", q);
                        row = q;
                    }
                    if (first) thead += string.Format("<th>{0}</th>", a);
                    rv += string.Format("<td class=\"e24TableCell\">{0}</td>", sel ? "<img src=\"images/tick.png\"/>" : "<img src=\"images/cross.png\"/>");
                }
                thead += "</tr></thead>";
                return string.Format("<table class=\"e24Table\" border=\"0\" cellSpacing=\"1\" cellPadding=\"0\">{0}<tbody>{1}</tbody></table>", thead, rv);
            }
            return "";
        }

        public List<string> genderText(string s, bool apply)
        {
            string temp;
            int[] pos = new int[3];
            pos[0] = -1;
            bool cont = true;
            List<string> res = new List<string>();

            while ((pos[0] = s.IndexOf('~', pos[0] + 1)) > -1 && cont)
            {
                if ((pos[1] = s.IndexOf('~', pos[0] + 1)) == -1) break;
                if (pos[1] - pos[0] < 2) break;
                temp = s.Substring(pos[0] + 2, pos[1] - (pos[0] + 2));
                if (s[pos[0] + 1] == 'W')
                {
                    s = s.Substring(0, pos[0]--) + temp.Substring(2) + s.Substring(pos[1] + 1);
                }
                else if (s[pos[0] + 1] == 'I')
                {
                    temp = "<img src=\"VEImages/" + temp + "\" alt=\"VEImages/" + temp + "\"/>";
                    s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else if (s[pos[0] + 1] == 'T')
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(TextAssetLocation + temp);
                    try
                    {
                        using (HttpWebResponse resp = (HttpWebResponse) req.GetResponse())
                        {
                            StreamReader sr = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.Default);
                            temp = sr.ReadToEnd().Replace("\r", "").Replace("\n", " ");
                            replaceRelativeLinks(ref temp, TextAssetLocation);
                            sr.Close();
                            resp.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        temp += " " + ex.Message;
                    }
                    s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else if (s[pos[0] + 1] == 'E' && EncyclopaediaLink != null)
                {
                    string[] encyclink = temp.Split('#');
                    temp = "<a href=\"" + EncyclopaediaLink + "?ArticleID=" + encyclink[1] + "\">" + encyclink[0] + "</a>";
                    s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else
                {
                    string clause = string.Format("Gender|{0}|", s[pos[0] + 1]);
                    if (!res.Contains(clause)) res.Add(clause);
                    if (s[pos[0] + 1] == gender)
                        s = s.Substring(0, pos[0]--) + span(temp, clause, "red") + s.Substring(pos[1] + 1);
                    else
                        s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
                }
            }

            double nod, ml;
            int st, ed;
            pos[0] = -1;
            cont = true;

            while (pos[0] + 1 < s.Length && (pos[0] = s.IndexOf('{', pos[0] + 1)) > -1 && cont)
            {
                if ((pos[1] = s.IndexOf('}', pos[0] + 1)) == -1) break;

                // Very clever nesting code
                pos[2] = pos[0];
                int start = pos[0];
                bool nested = false;
                while ((pos[2] = s.IndexOf('{', pos[2] + 1)) > -1 && pos[2] < pos[1])
                {
                    pos[0] = pos[2];
                    nested = true;
                }
                if (pos[1] == -1) break;

                temp = s.Substring(pos[0] + 1, pos[1] - (pos[0] + 1));
                if (temp.Length > 3 && temp[3] == '-')
                {
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
                    if (!int.TryParse(temp.Substring(0, 3), out st)) break;
                    if (!int.TryParse(temp.Substring(4, 3), out ed)) break;
                    nod = (DateTime.Now - dob).TotalDays;
                    string clause = string.Format("Age|{0}|{1}|{2}|", st, ed, ml);
                    if (!res.Contains(clause)) res.Add(clause);
                    if (ml > 0 && nod >= st * ml && nod < ed * ml)
                        s = s.Substring(0, pos[0]--) + temp.Substring(8) + s.Substring(pos[1] + 1);
                    else
                        s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
                }
                else if (temp.Length >= 2)
                {
                    int qid;
                    string clause = temp.Substring(0, 2);
                    if (clause == "ll")
                    {
                        temp = generateList(temp, ", ", "");
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "lr")
                    {
                        temp = generateList(temp, Environment.NewLine, "");
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "la")
                    {
                        temp = generateList(temp, ", ", " and ");
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "lo")
                    {
                        temp = generateList(temp, ", ", " or ");
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "lb")
                    {
                        temp = generateList(temp, "BULLET", "");
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "qp")
                    {
                        string[] qp = temp.Substring(2).Split('|');
                        string testclause = string.Format("qp|{0}|{1}|", qp[0], qp[1]);
                        if (!res.Contains(testclause)) res.Add(testclause);
                        int aid;
                        if (apply && qp.Length > 2 && int.TryParse(qp[0], out qid) && int.TryParse(qp[1], out aid) && bool.Parse(GetValue(_TraversalID, qid, aid, "qp")))
                            s = s.Substring(0, pos[0]--) + span(qp[2], testclause, "red") + s.Substring(pos[1] + 1);
                        else
                            s = s.Substring(0, pos[0]--) + (qp.Length > 3 ? span(qp[3], qp[0], "red") : "") + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "cc" || clause == "qc")
                    {
                        string[] cc = temp.Substring(2).Split('|');
                        string testclause = string.Format("cc|{0}|", cc[0]);
                        if (!res.Contains(testclause)) res.Add(testclause);
                        int cid;
                        if (apply && cc.Length > 1 && int.TryParse(cc[0], out cid) && bool.Parse(GetValue(_TraversalID, cid, 0, clause)))
                            s = s.Substring(0, pos[0]--) + span(cc[1], testclause, "red") + s.Substring(pos[1] + 1);
                        else
                            s = s.Substring(0, pos[0]--) + (cc.Length > 2 ? span(cc[2], testclause, "red") : "") + s.Substring(pos[1] + 1);
                    }
                    else if (int.TryParse(temp.Substring(2), out qid))
                    {
                        if (clause == "nt")
                        {
                            //int value = int.Parse(GetValue(_TraversalID, qid, "qv"));
                            s = s.Substring(0, pos[0]--) + "$$$nt$$$" + s.Substring(pos[1] + 1);
                            //s = ChooseTable(s.Substring(0, pos[0]), value);
                        }
                        else
                        {
                            string[] qp = temp.Substring(2).Split('|');
                            string testclause = string.Format("{0}|{1}|", temp.Substring(0, 2), qp[0]);
                            if (!res.Contains(testclause)) res.Add(testclause);
                            s = s.Substring(0, pos[0]--) + (apply ? span(GetValue(_TraversalID, qid, clause), testclause, "red") : "") + s.Substring(pos[1] + 1);
                        }
                    }
                    else s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                if (nested) pos[0] = start - 1;
            }

            pos[0] = -1;
            cont = true;

            while ((pos[0] = s.IndexOf('[', pos[0] + 1)) > -1 && cont)
            {
                string clause = "person";
                if (!res.Contains(clause)) res.Add(clause);
                if ((pos[1] = s.IndexOf(']', pos[0] + 1)) == -1) break;

                //Very clever nesting code
                //pos[2] = pos[0];
                //while ((pos[2] = s.IndexOf('[', pos[2] + 1)) > -1 && pos[2] < pos[1])
                //    pos[1] = s.IndexOf(']', pos[1] + 1);
                //if (pos[1] == -1) break;

                temp = s.Substring(pos[0], pos[1] - pos[0]);
                if ((pos[2] = temp.IndexOf('/', 1)) > -1)
                {
                    if (self)
                        s = s.Substring(0, pos[0]--) + span(temp.Substring(pos[2] + 1), "person", "red") + s.Substring(pos[1] + 1);
                    else if (gender == 'F')
                    {
                        temp = temp.Substring(0, pos[2] + 1);
                        temp = Regex.Replace(temp, "\\bhe\\b", "she");
                        temp = Regex.Replace(temp, "\\bHe\\b", "She");
                        temp = Regex.Replace(temp, "\\bHE\\b", "SHE");
                        temp = Regex.Replace(temp, "\\bhis\\b", "her");
                        temp = Regex.Replace(temp, "\\bHis\\b", "Her");
                        temp = Regex.Replace(temp, "\\bHIS\\b", "HER");
                        temp = Regex.Replace(temp, "\\bhim\\b", "her");
                        temp = Regex.Replace(temp, "\\bHim\\b", "Her");
                        temp = Regex.Replace(temp, "\\bHIM\\b", "HER");
                        s = s.Substring(0, pos[0]--) + span(temp.Substring(1, temp.Length - 2), "person", "red") + s.Substring(pos[1] + 1);
                    }
                    else
                        s = s.Substring(0, pos[0]--) + span(temp.Substring(1, pos[2] - 1), "person", "red") + s.Substring(pos[1] + 1);
                }
            }

            res.Insert(0, s);
            return res;
        }

        private string span(string temp, string clause, string col)
        {
            if (RequestFormContains(clause))
                temp = string.Format("<span style=\"color:{0}\">{1}</span>", col, temp);
            else
                temp = string.Format("<span style=\"color:{0}\">{1}</span>", "#0000aa", temp);
            return temp;
        }

        private void replaceRelativeLinks(ref string s, string p)
        {
            string temp;
            int[] pos = new int[3];
            pos[0] = -1;
            bool cont = true;

            while ((pos[0] = s.IndexOf("href=", pos[0] + 1)) > -1 && cont)
            {
                if ((pos[1] = s.IndexOf('\"', pos[0] + 1)) == -1) break;
                if ((pos[2] = s.IndexOf('\"', pos[1] + 1)) == -1) break;
                temp = s.Substring(pos[1] + 1, pos[2] - (pos[1] + 1));
                if (!temp.StartsWith("http"))
                {
                    s = s.Substring(0, pos[1] + 1) + p + temp + s.Substring(pos[2]);
                }
            }

            while ((pos[0] = s.IndexOf("src=", pos[0] + 1)) > -1 && cont)
            {
                if ((pos[1] = s.IndexOf('\"', pos[0] + 1)) == -1) break;
                if ((pos[2] = s.IndexOf('\"', pos[1] + 1)) == -1) break;
                temp = s.Substring(pos[1] + 1, pos[2] - (pos[1] + 1));
                if (!temp.StartsWith("http"))
                {
                    s = s.Substring(0, pos[1] + 1) + p + temp + s.Substring(pos[2]);
                }
            }
        }

        private string generateList(string temp, string seperator, string lastSeperator)
        {
            List<string> list = new List<string>(temp.Substring(2).Split('|'));
            for (int i = 0; i < list.Count; i++)
                list[i] = list[i].Trim();
            list.RemoveAll(delegate(string isvalid) { return string.IsNullOrEmpty(isvalid) || isvalid.StartsWith("'Error: "); });
            temp = "";
            if (seperator == "BULLET" && list.Count > 0) temp += "<ul>";
            for (int i = 0; i < list.Count; i++)
            {
                if (seperator != "BULLET")
                {
                    if (i == list.Count - 1 && list.Count > 1 && !string.IsNullOrEmpty(lastSeperator)) temp += lastSeperator;
                    else if (!string.IsNullOrEmpty(temp)) temp += seperator;
                    temp += list[i];
                }
                else
                {
                    temp += "<li>" + list[i] + "</li>";
                }
            }
            if (seperator == "BULLET" && list.Count > 0) temp += "</ul>";
            return temp;
        }
    }
}
