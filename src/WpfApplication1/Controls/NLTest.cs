using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;

using ListItem = AssetBuilder.ViewModels.ListItem;

namespace AssetBuilder.Controls
{
    public class NLTest
    {
        private bool nest = true;

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
        public string HTMLText
        {
            set
            {
                render.NavigateToString(string.Format(@"<html><head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=UTF-8"">
    <link rel=""stylesheet"" type=""text/css"" href=""https://www.w3schools.com/w3css/4/w3.css"" />
    <style type=""text/css"">
        .highlight
        {{
            background-color: red;
            color: white;
        }}
        .e24subtext
        {{
            font-size: smaller;
            font-style: italic;
        }}
    </style>
    <script type=""text/javascript"">
        function highlight_all(o)
        {{
            var col = o.getAttribute('spanid');
            var l = document.getElementsByTagName('span');
            for(var i=0; i < l.length; i++)
            {{
                if(l[i].getAttribute('spanid') == col) 
                {{
                    l[i].setAttribute('oldcolor', l[i].style.color);
                    l[i].removeAttribute('style');
                    l[i].className = 'highlight';
                }}
            }}
        }}

        function unhighlight_all(o)
        {{
            var l = document.getElementsByTagName('span');
            for(var i=0; i < l.length; i++)
            {{
                if (l[i].className == 'highlight')
                {{
                    l[i].className = '';
                    l[i].style.color = l[i].getAttribute('oldcolor');
                }}
            }}
        }}
    </script>

</head><body><div class=""w3-container"" style=""white-space:pre-wrap;"">
{0}
</div></body></html>", value.Replace("target=\"_blank\"", "")));
            }
        }

        string EncyclopaediaLink = AssetBuilder.Properties.Settings.Default.EncyclopaediaLink;
        string TextAssetLocation = AssetBuilder.Properties.Settings.Default.TextAssetLocation;

        private char gender = 'U';
        private DateTime dob = DateTime.Now - new TimeSpan(6575, 0, 0, 0, 0);
        private bool self = false;

        string _TraversalID = "";
        //int _MemberID;

        XmlNode xn;
        WebBrowser render;
        Panel controls;
        TextBox text;
        Thickness margin = new Thickness(5);
        ComboBox algos;
        double fontsize = 14.667;
        static Window browser = null;
        static Window questions = null;
        
        public bool AddControls { get { return controls != null; } }

        private NLTest(string s)
        {
            List<string> res = genderText(s, 0);
            xn = subProcess(res);
            GenerateCondiutinsAndControls(res);
        }

        public NLTest(TextBox tb)
        {
            if (EncyclopaediaLink.StartsWith("/"))
            {
                var uri = new Uri(AssetBuilder.Properties.Settings.Default.WebService);
                EncyclopaediaLink = new Uri(uri, EncyclopaediaLink).AbsoluteUri;
            }

            Rect r = System.Windows.SystemParameters.WorkArea;
            tb.TextChanged += new TextChangedEventHandler(tb_TextChanged);
            //tb.Unloaded += new RoutedEventHandler(tb_Unloaded);
            //InitializeComponent();
            text = tb;
            if (browser != null && browser.IsLoaded) browser.Content = null;
            else browser = new Window
            {
                Title = "Testing Output",
                Top = browser?.Top ?? 0,
                Left = browser?.Left ?? r.Width / 2,
                Width = browser?.Width ?? r.Width / 2,
                Height = browser?.Height ?? r.Height
            };
            Grid bgr = new Grid();
            browser.Content = bgr;
            browser.FontSize = fontsize;
            render = new WebBrowser();
            render.Navigating += Render_Navigating;
            bgr.Children.Add(render);
            browser.Show();
            browser.Activate();

            if (questions != null && questions.IsLoaded) questions.Content = null;
            else questions = new Window
            {
                Title = "Testing Input",
                Top = questions?.Top ?? 0,
                Left = questions?.Left ?? 0,
                Width = questions?.Width ?? r.Width / 2,
                Height = questions?.Height ?? r.Height
            };
            ScrollViewer sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            questions.Content = sv;
            StackPanel mainPanel = new StackPanel();
            sv.Content = mainPanel;
            WrapPanel wr = AddWrapPanel(mainPanel);
            Button clear = new Button { Margin = margin, Content = "Clear all" };
            clear.Click += new RoutedEventHandler(clear_Click);
            wr.Children.Add(clear);
            algos = new ComboBox { MinWidth = 200, Margin = margin, FontSize = fontsize };
            XmlNode xn = DataAccess.getDataNode("nl_AssetInfo", new string[] {
                            "@xml", "<root><GetAlgos/></root>"
                         }, false);
            foreach (XmlNode item in xn.ChildNodes)
            {
                CheckBox cb = new CheckBox { Content = new ListItem { ID = int.Parse(item["AlgoID"].InnerText), Value = item["Algo_Name"].InnerText } };
                cb.Checked += new RoutedEventHandler(cb_Checked);
                cb.Unchecked += new RoutedEventHandler(cb_Checked);
                algos.Items.Add(cb);
            }
            algos.Loaded += new RoutedEventHandler(lb_Loaded);
            algos.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);
            wr.Children.Add(algos);
            controls = new StackPanel();
            mainPanel.Children.Add(controls);
            questions.Show();
            questions.Activate();

            process(2);
        }

        private void Render_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if(e.Uri != null && e.Uri.Scheme.In("http", "https"))
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Cancel = true;
            }
        }

        DispatcherTimer timer;

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (timer != null) timer.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            timer = null;
            process(1);
        }

        void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (sender as ComboBox).SelectedItem = null;
        }

        TextAdorner mySelectText;

        void lb_Loaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer al = AdornerLayer.GetAdornerLayer(sender as ComboBox);
            mySelectText = new TextAdorner(sender as ComboBox, "... Multi Select ...");
            al.Add(mySelectText);
        }

        List<int> selectedAlgos = new List<int>();

        void cb_Checked(object sender, RoutedEventArgs e)
        {
            ComboBox cb = algos;
            selectedAlgos.Clear();
            string test = "";
            foreach (var item in cb.Items)
            {
                if (item is CheckBox)
                {
                    CheckBox ib = item as CheckBox;
                    if ((bool)ib.IsChecked)
                    {
                        ListItem li = ib.Content as ListItem;
                        selectedAlgos.Add(li.ID);
                        if (test != "") test += ", ";
                        test += li.ID;
                    }
                }
            }
            mySelectText.Text = test;
            process(1);
        }

        void clear_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in qControls)
            {
                UIElement control = item.Value;
                if (control is CheckBox) (control as CheckBox).IsChecked = false;
                else if (control is RadioButton) (control as RadioButton).IsChecked = false;
                else if (control is TextBox) (control as TextBox).Text = "";
            }
            HTMLText = genderText(NLText, 2)[0]; //.Replace("\n", "<br/>");
        }

        Dictionary<string, UIElement> qControls = new Dictionary<string, UIElement>();
        Dictionary<string, string> conditionString = new Dictionary<string, string>();
        Dictionary<string, UIElement> lastControls = new Dictionary<string, UIElement>();
        UIElement lastUpdated = null;
        int ageValue = 0;
        double ageMulti = 365.25;

        void updated(object sender, RoutedEventArgs e)
        {
            lastUpdated = sender as UIElement;
            if (sender is RadioButton)
            {
                RadioButton rb = sender as RadioButton;
                if (rb.Uid.StartsWith("Gender")) gender = rb.Uid[7];
            }
            if (sender is CheckBox)
            {
                CheckBox cb = sender as CheckBox;
                if (cb.Uid.StartsWith("person")) self = (bool)cb.IsChecked;
            }
            if ((sender is TextBox || sender is RadioButton) && (sender as Control).Uid.StartsWith("Age"))
            {
                if (sender is TextBox)
                {
                    TextBox tb = sender as TextBox;
                    int.TryParse(tb.Text, out ageValue);
                }
                else if (sender is RadioButton)
                {
                    RadioButton rb = sender as RadioButton;
                    if (rb.Uid == "Age|Y|" && (bool)rb.IsChecked) ageMulti = 365.25;
                    if (rb.Uid == "Age|M|" && (bool)rb.IsChecked) ageMulti = 30.4375;
                    if (rb.Uid == "Age|W|" && (bool)rb.IsChecked) ageMulti = 7;
                    if (rb.Uid == "Age|D|" && (bool)rb.IsChecked) ageMulti = 1;
                }
                dob = DateTime.Now.AddDays(-(ageValue * ageMulti));
                var days = ageValue * ageMulti;
                foreach (var item in qControls.Where(f => f.Key.StartsWith("AR_")))
                {
                    var m = item.Key.Substring(3, 1);
                    var multi = (m == "y" ? 365.25 : (m == "m" ? 30.4375 : (m == "w" ? 7 : 1)));
                    var d1 = int.Parse(item.Key.Substring(4, 3)) * multi;
                    var d2 = int.Parse(item.Key.Substring(7, 3)) * multi;
                    (item.Value as CheckBox).IsChecked = days >= d1 && days < d2;
                }
            }
            HTMLText = genderText(NLText, 1)[0]; //.Replace("\n", "<br/>");
        }

        bool RequestContains(string key)
        {
            if (key.StartsWith("qc"))
            {
                string qp = "qs" + key.Substring(2);
                if (qControls.Any(f => f.Key.StartsWith(qp) && !string.IsNullOrEmpty((f.Value as TextBox).Text))) return true;
                qp = "qp" + key.Substring(2);
                if (qControls.Any(f => f.Key.StartsWith(qp) && (f.Value as ToggleButton).IsChecked == true)) return true;
            }
            if (key.StartsWith("qn"))
            {
                string qp = "qp" + key.Substring(2);
                if (qControls.Any(f => f.Key.StartsWith(qp) && !f.Key.EndsWith("|4|") && !f.Key.EndsWith("|3|") && (f.Value as ToggleButton).IsChecked == true)) return true;
            }
            if (!qControls.ContainsKey(key)) return false;
            UIElement control = qControls[key];
            if (control is CheckBox) return (control as CheckBox).IsChecked == true;
            else if (control is RadioButton) return (control as RadioButton).IsChecked == true;
            else if (control is TextBox) return !string.IsNullOrEmpty((control as TextBox).Text);
            return false;
        }

        string RequestValue(string key)
        {
            if (!qControls.ContainsKey(key)) return "";
            UIElement control = qControls[key];
            if (control is TextBox) return (control as TextBox).Text;
            return "";
        }

        bool RequestFormContains(string clause)
        {
            return (lastUpdated != null && lastUpdated.Uid == clause);
        }

        private WrapPanel AddWrapPanel(Panel pnl)
        {
            WrapPanel wr = new WrapPanel();
            pnl.Children.Add(wr);
            return wr;
        }

        void AddLabelControl(string text, Panel pnl, SolidColorBrush col = null)
        {
            pnl.Children.Add(new Label { Content = text, FontSize = fontsize, Margin = margin, Foreground = col ?? Brushes.Black });
        }

        void AddBoldLabelControl(string text, Panel pnl)
        {
            pnl.Children.Add(new Label { Content = text, FontWeight = FontWeights.Bold, FontSize = fontsize, Margin = margin });
        }

        void AddButtonControl(string text, Panel pnl)
        {
            pnl.Children.Add(new Button { Content = text, FontSize = fontsize, Margin = margin });
        }

        void AddCheckBoxControl(string ID, string text, Panel pnl)
        {
            AddCheckBoxControl(ID, text, null, pnl);
        }

        void AddCheckBoxControl(string ID, string text, string toolTip, Panel pnl, bool withUpdateEvent = true)
        {
            CheckBox cb = new CheckBox { Uid = ID, Name = ID.Replace('|', '_'), Content = text, ToolTip = toolTip, FontSize = fontsize, Margin = margin, VerticalContentAlignment = VerticalAlignment.Center };
            if (lastControls.ContainsKey(ID) && lastControls[ID] is CheckBox) cb.IsChecked = (lastControls[ID] as CheckBox).IsChecked;
            qControls.Add(ID, cb);
            if (withUpdateEvent)
            {
                cb.Checked += updated;
                cb.Unchecked += updated;
            }
            else cb.IsEnabled = false;
            pnl.Children.Add(cb);
        }

        RadioButton AddRadioControl(string ID, string text, string groupName, Panel pnl)
        {
            return AddRadioControl(ID, text, groupName, null, pnl);
        }

        RadioButton AddRadioControl(string ID, string text, string groupName, string toolTip, Panel pnl)
        {
            RadioButton rb = new RadioButton { Uid = ID, Name = ID.Replace('|', '_'), Content = text, GroupName = groupName, ToolTip = toolTip, FontSize = fontsize, Margin = margin, VerticalContentAlignment = VerticalAlignment.Center };
            if (lastControls.ContainsKey(ID) && lastControls[ID] is RadioButton) rb.IsChecked = (lastControls[ID] as RadioButton).IsChecked;
            qControls.Add(ID, rb);
            rb.Checked += updated;
            rb.Unchecked += updated;
            pnl.Children.Add(rb);
            return rb;
        }

        TextBox AddTextBoxControl(string ID, Panel pnl, string tooltip)
        {
            TextBox tb = new TextBox { Uid = ID, Name = ID.Replace('|', '_'), MinWidth = 200, FontSize = fontsize, Margin = margin, ToolTip = tooltip, VerticalContentAlignment = VerticalAlignment.Center };
            if (lastControls.ContainsKey(ID) && lastControls[ID] is TextBox) tb.Text = (lastControls[ID] as TextBox).Text;
            qControls.Add(ID, tb);
            tb.SelectionChanged += updated;
            pnl.Children.Add(tb);
            return tb;
        }

        void AddBreakControl(Panel pnl)
        {
            //pnl.Children.Add(new Label());
        }

        public void GenerateCondiutinsAndControls(List<string> res)
        {
            if (res.Contains("person"))
            {
                if (AddControls)
                {
                    AddBoldLabelControl("Person answering questions", controls);
                    AddCheckBoxControl("person", "Self", controls);
                }
            }
            if (res.Contains("person") || res.Contains("Gender|F|") || res.Contains("Gender|M|"))
            {
                conditionString.Add("Gender|M|", "MALE");
                conditionString.Add("Gender|F|", "FEMALE");
                conditionString.Add("Gender|I|", "GENDER COMPLICATED");
                conditionString.Add("Gender|U|", "GENDER UNKNOWN");
                if (AddControls)
                {
                    AddBoldLabelControl("Gender of person of interest", controls);
                    WrapPanel wr = AddWrapPanel(controls);
                    AddRadioControl("Gender|M|", "Male", "Gender", wr);
                    AddRadioControl("Gender|F|", "Female", "Gender", wr);
                    AddRadioControl("Gender|I|", "It's complicated", "Gender", wr);
                    AddRadioControl("Gender|U|", "Unknown", "Gender", wr);
                }
            }
            if (res.Contains("Age|Y|") || res.Contains("Age|M|") || res.Contains("Age|W|") || res.Contains("Age|D|"))
            {
                if (res.Contains("Age|Y|"))
                    conditionString.Add("Age|Y|", "BETWEEN {0} AND {1} YEARS");
                if (res.Contains("Age|M|"))
                    conditionString.Add("Age|M|", "BETWEEN {0} AND {1} MONTHS");
                if (res.Contains("Age|W|"))
                    conditionString.Add("Age|W|", "BETWEEN {0} AND {1} WEEKS");
                if (res.Contains("Age|D|"))
                    conditionString.Add("Age|D|", "BETWEEN {0} AND {1} DAYS");
                if (AddControls)
                {
                    AddBoldLabelControl("Age of person of interest", controls);
                    string multi = (ageMulti == 365.25 ? " years" : (ageMulti == 30.4375 ? " months" : (ageMulti == 7 ? " weeks" : " days")));
                    AddLabelControl($"Text parsed as {ageValue}{multi} old unless specified below", controls, Brushes.Red);
                    WrapPanel wr = AddWrapPanel(controls);
                    TextBox tb = AddTextBoxControl("Age", wr, "Age");
                    if (!string.IsNullOrEmpty(tb.Text)) int.TryParse(tb.Text, out ageValue);
                    ageMulti = 365.25;
                    if (res.Contains("Age|Y|"))
                    {
                        RadioButton rb = AddRadioControl("Age|Y|", "Years", "Age", wr);
                        if ((bool)rb.IsChecked) ageMulti = 365.25;
                    }
                    if (res.Contains("Age|M|"))
                    {
                        RadioButton rb = AddRadioControl("Age|M|", "Months", "Age", wr);
                        if ((bool)rb.IsChecked) ageMulti = 30.4375;
                    }
                    if (res.Contains("Age|W|"))
                    {
                        RadioButton rb = AddRadioControl("Age|W|", "Weeks", "Age", wr);
                        if ((bool)rb.IsChecked) ageMulti = 7;
                    }
                    if (res.Contains("Age|D|"))
                    {
                        RadioButton rb = AddRadioControl("Age|D|", "Days", "Age", wr);
                        if ((bool)rb.IsChecked) ageMulti = 1;
                    }
                    dob = DateTime.Now.AddDays(-(ageValue * ageMulti));
                    //configPanel.Controls.Add(new HtmlGenericControl("br"));
                    var ageranges = Regex.Matches(NLText, @"{([0-9]{3})-([0-9]{3})(d|w|m|y)");
                    if (ageranges.Count > 0)
                    {
                        StackPanel sp = new StackPanel { Background = Brushes.LightGray, Margin = new Thickness(10, 0, 10, 0) };
                        controls.Children.Add(sp);
                        AddLabelControl("Age ranges used in this asset will be ticked based on age entered", sp);
                        wr = AddWrapPanel(sp);
                        HashSet<string> ageids = new HashSet<string>();
                        foreach (Match item in ageranges)
                        {
                            ageids.Add($"AR_{item.Groups[3].Value}{item.Groups[1].Value}{item.Groups[2].Value}");
                        }
                        foreach (var item in ageids)
                        {
                            var start = int.Parse(item.Substring(4, 3));
                            var end = int.Parse(item.Substring(7, 3));
                            var m = item.Substring(3, 1);
                            multi = (m == "y" ? " years" : (m == "m" ? " months" : (m == "w" ? " weeks" : " days")));

                            AddCheckBoxControl(item, $"{start} - {end}{multi}", null, wr, false);
                        }
                    }
                }
            }

            foreach (var item in res.Where(f => f.StartsWith("qt")))
            {
                if (!conditionString.ContainsKey(item)) conditionString.Add(item, string.Format("(DISPLAY TABLE {0})", item.Split('|')[1]));
            }

            string qid = "";
            WrapPanel qWrap = null;
            List<string> ids = new List<string>();

            if (xn != null)
            {
                foreach (XmlNode item in xn.SelectNodes("*"))
                {
                    if (item["type"].InnerText == "Conclusion")
                    {
                        string id = string.Format("{0}|{1}|", "cc", item["RecID"].InnerText);
                        string ca = string.Format("{0}|{1}|", "ca", item["RecID"].InnerText);
                        string ce = string.Format("{0}|{1}|", "ce", item["RecID"].InnerText);
                        if (!conditionString.ContainsKey(ca)) conditionString.Add(ca, "(DISPLAY CONCLUSION '" + item["Lay_Condition"].InnerText + "')");
                        if (!conditionString.ContainsKey(ce)) conditionString.Add(ce, "(DISPLAY CONCLUSION '" + item["Explanation"].InnerText + "')");
                        if (ids.Contains(id)) continue;
                        ids.Add(id);
                        conditionString.Add(id, item["Possible_Condition"].InnerText);
                        if (AddControls)
                        {
                            WrapPanel wr = AddWrapPanel(controls);
                            AddLabelControl(item["id"].InnerText + ") ", wr);
                            AddCheckBoxControl(id, item["Possible_Condition"].InnerText, wr);
                        }
                    }
                    if (item["type"].InnerText == "Group")
                    {
                        string id = string.Format("{0}|{1}|", "gc", item["GroupID"].InnerText);
                        string ca = string.Format("{0}|{1}|", "ga", item["GroupID"].InnerText);
                        if (!conditionString.ContainsKey(ca)) conditionString.Add(ca, "(DISPLAY GROUP '" + item["GroupName"].InnerText + "')");
                        if (ids.Contains(id)) continue;
                        ids.Add(id);
                        conditionString.Add(id, item["GroupName"].InnerText);
                        if (AddControls)
                        {
                            WrapPanel wr = AddWrapPanel(controls);
                            AddLabelControl(item["id"].InnerText + ") ", wr);
                            AddCheckBoxControl(id, item["GroupName"].InnerText, wr);
                        }
                    }
                    if (item["type"].InnerText == "Question")
                    {
                        if (AddControls && qid != item["id"].InnerText)
                        {
                            WrapPanel wr = AddWrapPanel(controls);
                            AddBreakControl(wr);
                            AddLabelControl(item["id"].InnerText + ") ", wr);
                            AddBoldLabelControl(item["Clinical_Statement"].InnerText, wr);
                            qWrap = AddWrapPanel(controls);
                            //configPanel.Controls.Add(new HtmlGenericControl("br"));
                        }
                        qid = item["id"].InnerText;
                        if (res.Contains("qa|" + qid + "|") && !conditionString.ContainsKey("qa|" + qid + "|")) conditionString.Add("qa|" + qid + "|", "(DISPLAY ANSWER '" + item["Clinical_Statement"].InnerText + "')");
                        if ((item["NodeTypeID"].InnerText == "33" || item["NodeTypeID"].InnerText == "53") && item["AnswerTypeID"].InnerText != "64")
                        {
                            string id = string.Format("{0}|{1}|{2}|", "qp", item["id"].InnerText, item["AnsID"].InnerText);
                            if (ids.Contains(id)) continue;
                            ids.Add(id);
                            conditionString.Add(id, item["Clinical_Statement"].InnerText + " : " + item["Clinical_Answer"].InnerText);
                            if (AddControls) AddCheckBoxControl(id, item["Clinical_Answer"].InnerText, item["id"].InnerText, qWrap);
                        }
                        else if (item["AnswerTypeID"].InnerText == "81")
                        {
                            string id = string.Format("{0}|{1}|", "qs", item["id"].InnerText);
                            if (ids.Contains(id)) continue;
                            ids.Add(id);
                            conditionString.Add(id, "(DISPLAY VALUE '" + item["Clinical_Statement"].InnerText + "')");
                            if (AddControls) AddTextBoxControl(id, qWrap, item["AnsID"].InnerText);
                        }
                        else
                        {
                            string id = string.Format("{0}|{1}|{2}|", "qp", item["id"].InnerText, item["AnsID"].InnerText);
                            string qc = string.Format("{0}|{1}|", "qc", item["id"].InnerText);
                            if (ids.Contains(id)) continue;
                            ids.Add(id);
                            conditionString.Add(id, item["Clinical_Statement"].InnerText + " : " + item["Clinical_Answer"].InnerText);
                            if (!conditionString.ContainsKey(qc)) conditionString.Add(qc, item["Clinical_Statement"].InnerText + " IS answered");
                            if (AddControls) AddRadioControl(id,
                                item["Clinical_Answer"].InnerText,
                                string.Format("{0}|{1}|", "qp", item["id"].InnerText),
                                item["AnsID"].InnerText, qWrap);
                        }
                    }
                }
            }
        }

        public static XmlNode subProcess(List<string> res, List<int> selectedAlgos = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("root"));

            foreach (var item in res)
            {
                if (item != "" && (item[0] == 'c' || item[0] == 'g' || item[0] == 'q') && item.Contains('|'))
                {
                    XmlElement elem = doc.CreateElement("Asset");
                    elem.Attributes.Append(doc.CreateAttribute("ID"));
                    elem.Attributes.Append(doc.CreateAttribute("Type"));
                    if (item[0] == 'c') elem.Attributes["Type"].Value = "Conclusion";
                    if (item[0] == 'g') elem.Attributes["Type"].Value = "Group";
                    if (item[0] == 'q') elem.Attributes["Type"].Value = "Question";
                    var id = item.Split('|')[1];
                    if (id.StartsWith("-") && id.Length > 2) id = id.Substring(1);
                    elem.Attributes["ID"].Value = id;
                    doc.DocumentElement.AppendChild(elem);
                }
                if (item != "" && item.StartsWith("qp") && item.Contains('|') && item.Split('|').Length > 2)
                {
                    XmlElement elem = doc.CreateElement("Asset");
                    elem.Attributes.Append(doc.CreateAttribute("ID"));
                    elem.Attributes.Append(doc.CreateAttribute("Type"));
                    elem.Attributes["Type"].Value = "Answer";
                    elem.Attributes["ID"].Value = item.Split('|')[2];
                    doc.DocumentElement.AppendChild(elem);
                }
            }
            if (selectedAlgos != null)
            {
                foreach (var item in selectedAlgos)
                {
                    XmlElement elem = doc.CreateElement("Algo");
                    elem.Attributes.Append(doc.CreateAttribute("ID")).Value = item.ToString();
                    doc.DocumentElement.AppendChild(elem);
                }
            }
            var xn = doc.DocumentElement.HasChildNodes ? DataAccess.getDataNode("nl_AssetInfo", new string[] {
                "@xml", doc.OuterXml
            }, false) : doc.DocumentElement;

            return xn;
        }

        static Regex removeWhiteSpace = new Regex(">\\s*<");

        public static string GetNLConditionString(string s)
        {
            if (s.StartsWith("<")) s = removeWhiteSpace.Replace(s, "><");
            var nl = new NLTest(s);
            return nl.genderText(s, 2)[0];
        }

        void process(int code)
        {
            lastControls = new Dictionary<string, UIElement>(qControls);
            qControls.Clear();
            controls.Children.Clear();
            conditionString.Clear();
            spanid = null;
            lastUpdated = null;
            if (RequestContains("person")) self = true;
            //if (RequestContains("Gender")) gender = RequestValue("Gender").Split('|')[1][0];
            List<string> res = genderText(NLText, 0);
            xn = subProcess(res, selectedAlgos);

           if (xn.Name == "Error")
            {
                if (xn.FirstChild != null && xn.FirstChild.InnerText != null) HTMLText = xn.FirstChild.InnerText; //.Replace("\n", "<br/>");
                else HTMLText = "Unknown error";
                return;
            }

            string currentType = "";
            string currentError = "";
            WrapPanel errorWrap = null;
            Label errorLabel = null;

            foreach (XmlNode item in xn.SelectNodes("*[type='Missing' or type='Not Loaded']"))
            {
                string type = item["missingtype"].InnerText;
                string error = item["type"].InnerText;
                string id = item["id"].InnerText;
                if (type != currentType || error != currentError)
                {
                    errorWrap = AddWrapPanel(controls);
                    errorLabel = new Label { Content = error + " " + type + " ", Foreground = Brushes.Red, FontSize = 14, FontWeight = FontWeights.Bold };
                    errorWrap.Children.Add(errorLabel);
                }
                else
                {
                    errorLabel.Content = errorLabel.Content.ToString().Replace(type + " ", type + "s ");
                    errorLabel.Content += ", ";
                }
                errorLabel.Content += id;
                currentType = type;
                currentError = error;
            }

            GenerateCondiutinsAndControls(res);

            if (controls.Children.Count == 0)
                HTMLText = genderText(NLText, 1)[0]; //.Replace("\n", "<br/>");
            else
                HTMLText = genderText(NLText, code)[0]; //.Replace("\n", "<br/>");
        }

        protected bool hasPair(int QuestionID, int AnswerID)
        {
            string qString = string.Format("qp|{0}|", QuestionID);
            string aString = string.Format("qp|{0}|{1}|", QuestionID, AnswerID);
            string sString = string.Format("qs|{0}|", QuestionID);
            if (RequestContains(aString)) return true;
            if (RequestContains(qString)) return RequestValue(qString) == aString;
            if (RequestContains(sString)) return RequestAnswerID(sString) == AnswerID;
            return false;
        }

        private int RequestAnswerID(string key)
        {
            int AnswerID = 0;
            UIElement control = qControls[key];
            if (control is TextBox && int.TryParse((control as TextBox).ToolTip.ToString(), out AnswerID)) return AnswerID;
            return -1;
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
            if (Value == "cc") return RequestContains($"cc|{QuestionID}|").ToString();
            if (Value == "gc") return RequestContains($"gc|{QuestionID}|").ToString();
            if (Value == "qc") return RequestContains($"qc|{QuestionID}|").ToString();
            if (Value == "qn")
            {
                if (RequestContains($"qp|{QuestionID}|4|") && !RequestContains($"qn|{QuestionID}|")) return "True";
                if (RequestContains($"qn|{QuestionID}|")) return "False";
                return "Omit";
            }
            if (Value == "ca" && RequestContains($"cc|{Math.Abs(QuestionID)}|"))
            {
                XmlNode xnl = xn.SelectSingleNode($"Table[id={Math.Abs(QuestionID)}]");
                if (xnl != null)
                {
                    if (QuestionID < 0) return firstLower(xnl["Lay_Condition"].InnerText);
                    return genderText(xnl["Lay_Condition"].InnerText, 1)[0];
                }
            }
            if (Value == "ce" && RequestContains($"cc|{Math.Abs(QuestionID)}|"))
            {
                XmlNode xnl = xn.SelectSingleNode($"Table[id={Math.Abs(QuestionID)}]");
                if (xnl != null)
                {
                    if (QuestionID < 0) return firstLower(xnl["Explanation"].InnerText);
                    return genderText(xnl["Explanation"].InnerText, 1)[0];
                }
            }
            if (Value == "ga" && RequestContains(string.Format("gc|{0}|", Math.Abs(QuestionID))))
            {
                XmlNode xnl = xn.SelectSingleNode(string.Format("Table2[id={0}]", Math.Abs(QuestionID)));
                if (xnl != null)
                {
                    if (QuestionID < 0) return firstLower(xnl["GroupName"].InnerText);
                    return genderText(xnl["GroupName"].InnerText, 1)[0];
                }
            }
            if (Value == "qa")
            {
                string result = "";
                XmlNodeList xnl = xn.SelectNodes(string.Format("Table1[id={0} and AnsID!=4 and AnsID!=3]", Math.Abs(QuestionID)));
                foreach (XmlNode item in xnl)
                {
                    int aid = int.Parse(item["AnsID"].InnerText);
                    if (hasPair(Math.Abs(QuestionID), aid))
                    {
                        if (result != "") result += '|';
                        var res = item["Answer_Text"].InnerText;
                        if (QuestionID < 0) res = genderText(firstLower(res), 1)[0];
                        result += span(res, string.Format("qp|{0}|{1}|", QuestionID, aid), "red");
                    }
                }
                return result;
            }
            if (Value == "qs" && RequestContains(string.Format("qs|{0}|", QuestionID))) return RequestValue(string.Format("qs|{0}|", QuestionID));
            if (Value == "qv")
            {
                string input = RequestValue(string.Format("qs|{0}|", QuestionID));
                double dValue;
                if (double.TryParse(input, out dValue)) return dValue.ToString("#,##0.###############", System.Globalization.CultureInfo.CurrentCulture);
                else return string.Format("<span style=\"background-color: #ffdddd\">'Error: No value for question {0}'</span>", QuestionID);
            }
            if (Value == "qd")
            {
                string input = RequestValue(string.Format("qs|{0}|", QuestionID));
                DateTime dValue;
                if (DateTime.TryParse(input, out dValue)) return dValue.ToString("dd MMM yyyy");
                else return string.Format("<span style=\"background-color: #ffdddd\">'Error: No value for question {0}'</span>", QuestionID);
            }
            if (Value == "qt")
            {
                string rv = "";
                XmlNodeList xnl = xn.SelectNodes(string.Format("Table1[TableID={0} and AnsID!=4 and AnsID!=3]", QuestionID));
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
                    rv += string.Format("<td class=\"e24TableCell\">{0}</td>", sel ? "<img src=\"" + Directory.GetCurrentDirectory() + "/images/tick.png\"/>" : "<img src=\"" + Directory.GetCurrentDirectory() + "/images/cross.png\"/>");
                }
                thead += "</tr></thead>";
                return string.Format("<table class=\"e24Table\" border=\"0\" cellSpacing=\"1\" cellPadding=\"0\">{0}<tbody>{1}</tbody></table>", thead, rv);
            }
            return "";
        }

        private string firstLower(string s)
        {
            if (s.Length > 0) return s.Substring(0, 1).ToLower() + (s.Length > 1 ? s.Substring(1) : "");
            return s;
        }

        private List<Tuple<Regex, string>> Pronouns = new List<Tuple<Regex, string>>() { Tuple.Create(new Regex("(?i)\\bhe\\b"), "she"), Tuple.Create(new Regex("(?i)\\bhis\\b"), "her"), Tuple.Create(new Regex("(?i)\\bhim\\b"), "her") };
        private List<Tuple<Regex, string>> Pronouns_I = new List<Tuple<Regex, string>>() { Tuple.Create(new Regex("(?i)\\bhe\\b"), "they"), Tuple.Create(new Regex("(?i)\\bhis\\b"), "their"), Tuple.Create(new Regex("(?i)\\bhim\\b"), "them"), Tuple.Create(new Regex("(?i)\\bdoes\\b"), "do"), Tuple.Create(new Regex("(?i)\\bgoes\\b"), "go"), Tuple.Create(new Regex("(?i)\\bhas\\b"), "have"), Tuple.Create(new Regex("(?<!('|\\bi))s(?=\\/$)"), "") };
        public List<string> genderText(string s, int apply)
        {
            string os = s;

            if (apply < 2) s = Regex.Replace(s, "<date([>]|[:](?<format>[^>]*)[>])", m => (string.IsNullOrWhiteSpace(m.Groups["format"].Value) ? DateTime.Now.ToString() : DateTime.Now.ToString(m.Groups["format"].Value)));
            else s = Regex.Replace(s, "<date([>]|[:](?<format>[^>]*)[>])", m => m.Value.XmlEncode());

            string temp;
            int[] pos = new int[3];
            pos[0] = -1;
            bool cont = true;
            List<string> res = new List<string>();
            if (s.Contains("<age>"))
            {
                res.Add("Age|Y|");
                if (apply < 2)
                    s = s.Replace("<age>", ((int)((DateTime.Now - dob).TotalDays / 365.25)).ToString());
                else
                    s = s.Replace("<age>", "<age>".XmlEncode());
            }

            if (s.Contains("<c>"))
            {
                s = s.Replace("<c>", "<c>".XmlEncode());
            }

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
                    string domain = DataAccess.Domain;
                    temp = "<img src=\"" + domain + "/VEImages/" + temp + "\" alt=\"VEImages/" + temp + "\"/>";
                    s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else if (s[pos[0] + 1] == 'T')
                {
                    WebRequest req = WebRequest.Create(TextAssetLocation + temp);
                    try
                    {
                        using (WebResponse resp = req.GetResponse())
                        {
                            using (StreamReader sr = new StreamReader(resp.GetResponseStream(), System.Text.Encoding.Default))
                            {
                                if (temp.Contains(".html"))
                                    temp = sr.ReadToEnd().Replace("\r", "").Replace("\n", " ");
                                else
                                    temp = sr.ReadToEnd(); //.Replace("\r", "").Replace("\n", " ");
                                replaceRelativeLinks(ref temp, TextAssetLocation);
                                sr.Close();
                                resp.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        temp += " " + ex.Message;
                    }
                    var TAres = genderText(temp, apply);
                    res.AddRange(TAres);
                    s = s.Substring(0, pos[0]--) + TAres[0] + s.Substring(pos[1] + 1);
                }
                else if (s[pos[0] + 1] == 'E' && EncyclopaediaLink != null)
                {
                    string[] encyclink = temp.Split('#');
                    temp = "<a target=\"_blank\" href=\"" + EncyclopaediaLink + "?ArticleID=" + encyclink[1] + "\">" + encyclink[0] + "</a>";
                    s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else if (s[pos[0] + 1] == 'E' && EncyclopaediaLink == null)
                {
                    int cs = 1;
                    if (pos[1] < s.Length - 2 && s[pos[1] + 1] == 10 && s[pos[1] + 2] == 13) cs = 3;
                    else if (pos[1] < s.Length - 2 && s[pos[1] + 1] == 13 && s[pos[1] + 2] == 10) cs = 3;
                    else if (pos[1] < s.Length - 1 && s[pos[1] + 1] == 10) cs = 2;
                    else if (pos[1] < s.Length - 1 && s[pos[1] + 1] == 13) cs = 2;
                    s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + cs);
                }
                else
                {
                    var clause = string.Format("Gender|{0}|", s[pos[0] + 1]);
                    if (!res.Contains(clause)) res.Add(clause);
                    if (apply < 2 || !conditionString.ContainsKey(clause))
                    {
                        if (s[pos[0] + 1] == gender)
                            s = s.Substring(0, pos[0]--) + span(temp, clause, "red") + s.Substring(pos[1] + 1);
                        else
                            s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
                    }
                    else
                    {
                        s = OutputClause(clause, s, pos, new string[] { temp }, conditionString[clause]);
                    }
                }
            }

            double nod, ml;
            int st, ed;
            pos[0] = -1;
            cont = true;
            if (apply == 0) s = os;

            while (pos[0] + 1 < s.Length && (pos[0] = s.IndexOf('{', pos[0] + 1)) > -1 && cont)
            {
                if ((pos[1] = GetEndIndex(s, '{', '}', pos[0] + 1)) == -1) break;

                // Very clever nesting code
                pos[2] = pos[0];
                var start = pos[0];
                var nested = false;
                //while ((pos[2] = s.IndexOf('{', pos[2] + 1)) > -1 && pos[2] < pos[1])
                //{
                //    pos[0] = pos[2];
                //    nested = true;
                //}
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
                    var clause = string.Format("Age|{0}|", temp[7].ToString().ToUpper());
                    if (!res.Contains(clause)) res.Add(clause);
                    if (apply < 2 || !conditionString.ContainsKey(clause))
                    {
                        if(apply == 0)
                            s = s.Substring(0, pos[0]--) + temp.Substring(8) + s.Substring(pos[1] + 1);
                        else if (ml > 0 && nod >= st * ml && nod < ed * ml)
                            s = s.Substring(0, pos[0]--) + temp.Substring(8) + s.Substring(pos[1] + 1);
                        else
                            s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
                    }
                    else
                    {
                        s = OutputClause(clause, s, pos, new string[] { temp.Substring(8) }, string.Format(conditionString[clause], st, ed));
                    }
                }
                else if (temp.Length >= 2)
                {
                    int qid;
                    var clause = temp.Substring(0, 2);
                    if (clause == "ll")
                    {
                        temp = generateList(temp, ", ", "", apply);
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "lr")
                    {
                        temp = generateList(temp, Environment.NewLine, "", apply);
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "la")
                    {
                        temp = generateList(temp, ", ", " and ", apply);
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "lo")
                    {
                        temp = generateList(temp, ", ", " or ", apply);
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "lb")
                    {
                        temp = generateList(temp, "BULLET", "", apply);
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "ln")
                    {
                        temp = generateList(temp, "OL", "", apply);
                        s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                    }
                    else if (clause.StartsWith("x"))
                    {
                        var cond = string.Format("Gender|{0}|", s[pos[0] + 2].In('x', 'f') ? 'F' : s[pos[0] + 2].In('y', 'm') ? 'M' : 'U');
                        if (!res.Contains(cond)) res.Add(cond);

                        var xx = GetSplit(temp.Substring(2), '{', '}', '|');
                        if (apply < 2 || !conditionString.ContainsKey(cond))
                        {
                            
                            if (clause == "x" + gender.ToString().ToLower()
                                || clause == "xx" && gender == 'F'
                                || clause == "xy" && gender == 'M'
                                || clause == "xz" && gender.NotIn('M', 'F'))
                                s = s.Substring(0, pos[0]--) + span(xx[0], cond, "red") + s.Substring(pos[1] + 1);
                            else
                                s = s.Substring(0, pos[0]--) + (xx.Length > 1 ? span(xx[1], cond, "red") : "") + s.Substring(pos[1] + 1);
                        }
                        else
                        {
                            s = OutputClause(cond, s, pos, xx, conditionString[cond]);
                        }
                    }
                    else if (clause == "qp")
                    {
                        string[] qp = GetSplit(temp.Substring(2), '{', '}', '|');
                        string testclause = string.Format("qp|{0}|{1}|", qp[0], qp[1]);
                        if (!res.Contains(testclause)) res.Add(testclause);
                        int aid;
                        if (apply < 2 || !conditionString.ContainsKey(testclause))
                        {
                            if (apply == 0)
                                s = s.Substring(0, pos[0]--) + (qp.Length > 2 ? qp[2] : "") + (qp.Length > 3 ? qp[3] : "") + s.Substring(pos[1] + 1);
                            else if (apply == 1 && qp.Length > 2 && int.TryParse(qp[0], out qid) && int.TryParse(qp[1], out aid) && bool.Parse(GetValue(_TraversalID, qid, aid, "qp")))
                                s = s.Substring(0, pos[0]--) + span(qp[2], testclause, "red") + s.Substring(pos[1] + 1);
                            else
                                s = s.Substring(0, pos[0]--) + (qp.Length > 3 ? span(qp[3], qp[0], "red") : "") + s.Substring(pos[1] + 1);
                        }
                        else
                        {
                            s = OutputClause(testclause, s, pos, qp.Skip(2), conditionString[testclause]);
                        }
                    }
                    else if (clause == "cc" || clause == "gc" || clause == "qc" || clause == "qn")
                    {
                        string[] cc = GetSplit(temp.Substring(2), '{', '}', '|');
                        string testclause = string.Format("{1}|{0}|", cc[0], clause);
                        if (!res.Contains(testclause)) res.Add(testclause);
                        int cid;
                        bool checkomit;
                        if (apply < 2 || !conditionString.ContainsKey(testclause))
                        {
                            if (apply == 0)
                                s = s.Substring(0, pos[0]--) + (cc.Length > 1 ? cc[1] : "") + (cc.Length > 2 ? cc[2] : "") + s.Substring(pos[1] + 1);
                            else if (cc.Length > 1 && int.TryParse(cc[0], out cid) &&
                                bool.TryParse(GetValue(_TraversalID, cid, 0, clause), out checkomit))
                            {
                                if (checkomit)
                                    s = s.Substring(0, pos[0]--) + span(cc[1], testclause, "red") + s.Substring(pos[1] + 1);
                                else
                                    s = s.Substring(0, pos[0]--) + (cc.Length > 2 ? span(cc[2], testclause, "red") : "") + s.Substring(pos[1] + 1);
                            }
                            else
                            {
                                s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
                            }
                        }
                        else
                        {
                            s = OutputClause(testclause, s, pos, cc.Skip(1), conditionString[testclause]);
                        }
                    }
                    else if (clause == "tc")
                    {
                        s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
                    }
                    else if (clause == "tp")
                    {
                        s = s.Substring(0, pos[0]--) + s.Substring(pos[1] + 1);
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
                            string[] qp = GetSplit(temp.Substring(2), '{', '}', '|');
                            string testclause = "";
                            if (clause == "qv" || clause == "qd") testclause = string.Format("qs|{0}|", qp[0]);
                            else testclause = string.Format("{0}|{1}|", temp.Substring(0, 2), qp[0]);
                            if (!res.Contains(testclause)) res.Add(testclause);
                            if (apply < 2 || !conditionString.ContainsKey(testclause))
                            {
                                s = s.Substring(0, pos[0]--) + (apply == 1 ? span(GetValue(_TraversalID, qid, clause), testclause, "red") : "") + s.Substring(pos[1] + 1);
                            }
                            else
                            {
                                s = OutputClause(testclause, s, pos, qp.Skip(1), conditionString[testclause]);
                            }
                        }
                    }
                    else s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                }
                else s = s.Substring(0, pos[0]--) + temp + s.Substring(pos[1] + 1);
                if (nested) pos[0] = start - 1;
            }

            pos[0] = -1;
            cont = true;
            if (apply == 0) s = os;

            if (apply < 2)
            {
                while ((pos[0] = s.IndexOf('[', pos[0] + 1)) > -1 && cont)
                {
                    var clause = "person";
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
                        else if (gender == 'F' || gender == 'I')
                        {
                            temp = temp.Substring(0, pos[2] + 1);
                            var prns = gender == 'F' ? Pronouns : Pronouns_I;
                            foreach (var pronoun in prns)
                            {
                                temp = pronoun.Item1.Replace(temp, f =>
                                {
                                    if (f.Value[0] > 96) return pronoun.Item2;
                                    else if (f.Value[1] > 96) return pronoun.Item2[0].ToString().ToUpper() + pronoun.Item2.Substring(1);
                                    else return pronoun.Item2.ToUpper();
                                });
                            }
                            s = s.Substring(0, pos[0]--) + span(temp.Substring(1, temp.Length - 2), "person", "red") + s.Substring(pos[1] + 1);
                        }
                        else
                            s = s.Substring(0, pos[0]--) + span(temp.Substring(1, pos[2] - 1), "person", "red") + s.Substring(pos[1] + 1);
                    }
                }

                while ((pos[0] = s.IndexOf("&#", pos[0] + 1)) > -1 && cont)
                {
                    if ((pos[1] = s.IndexOf(';', pos[0] + 2)) == -1) break;

                    temp = s.Substring(pos[0] + 2, pos[1] - (pos[0] + 2));
                    int ascii;
                    if (int.TryParse(temp, out ascii))
                    {
                        s = s.Substring(0, pos[0]--) + (char)ascii + s.Substring(pos[1] + 1);
                    }
                    else pos[0] += 2;
                }
            }

            res.Insert(0, s.Replace("***open***", "{").Replace("***close***", "}").Replace("***tilda***", "~"));
            return res;
        }

        private static string ExtractArrays(string temp, string separator)
        {
            var matches = Regex.Matches(temp, "\x07(.*?)\x07");
            if (matches.Count == 0) return temp;
            List<string[]> cols = new List<string[]>();
            foreach (Match match in matches)
            {
                cols.Add(match.Groups[1].Value.Split('|'));
            }
            int count = cols.Min(f => f.Length);
            var blown = "";
            for (int i = 0; i < count; i++)
            {
                if (blown != "") blown += separator;
                int mi = 0;
                blown += Regex.Replace(temp, "\x07(.*?)\x07", m => cols[mi++][i]);
            }
            return blown;
        }

        private int GetEndIndex(string s, char begin, char end, int start)
        {
            if (!nest) return s.IndexOf(end, start);
            var i = start;
            var any = new char[] { begin, end };
            var n = 0;
            while (i < s.Length && (i = s.IndexOfAny(any, i)) > -1)
            {
                if (s[i] == begin) n++;
                else if (s[i] == end) n--;
                if (n < 0) return i;
                i++;
            }
            return -1;
        }

        private string[] GetSplit(string s, char begin, char end, char split)
        {
            if (!nest) return s.Split(split);
            var res = new List<string>();
            var start = 0;
            var i = start;
            var any = new char[] { begin, end, split };
            var n = 0;
            while (i < s.Length && (i = s.IndexOfAny(any, i)) > -1)
            {
                if (s[i] == begin) n++;
                else if (s[i] == end) n--;
                else if ((s[i] == split && n == 0))
                {
                    res.Add(s.Substring(start, i - start));
                    start = i + 1;
                }
                if (n < 0) break;
                i++;
            }
            res.Add(s.Substring(start));
            return res.ToArray();
        }

        int[] rgb = { 0, 1, 2 };
        int[] prog = { 29, 53, 11 };
        int[] max = { 350, 200, 300 };
        string spanid = null;

        string getNextColor()
        {
            int i = -1;
            rgb[0] = (rgb[0] + prog[0]) % 100;
            rgb[1] = (rgb[1] + prog[1]) % 100;
            rgb[2] = (rgb[2] + prog[2]) % 100;
            int fc = rgb[0];
            int sc = rgb[1];
            int tc = rgb[2];
            if (fc >= sc && fc >= tc) i = 0;
            if (sc >= fc && sc >= tc) i = 1;
            if (tc >= fc && tc >= sc) i = 2;
            double t = fc + sc + tc;
            fc = (int)Math.Min((fc / t) * max[i], 255);
            sc = (int)Math.Min((sc / t) * max[i], 255);
            tc = (int)Math.Min((tc / t) * max[i], 255);
            string color = string.Format("#{0:X2}{1:X2}{2:X2}", fc, sc, tc);
            return color;
        }

        private string OutputClause(string clause, string s, int[] pos, IEnumerable<string> cl, string condition)
        {
            spanid = Guid.NewGuid().ToString();
            condition = condition.Replace("{", "***open***").Replace("}", "***close***").Replace("~", "***tilda***");
            string[] cc = cl.ToArray();
            string color = getNextColor();
            string end = s.Substring(pos[1] + 1);
            s = s.Substring(0, pos[0]--);
            if (condition.StartsWith("(DISPLAY"))
            {
                s += span(condition, "doit", "red", spanid);
            }
            else
            {
                s += span("(IF ", "doit", color, spanid);
                if (cc[0] == "" && cc.Length > 1)
                {
                    s += span("NOT '" + condition + "' THEN \"", "doit", color, spanid);
                    s += span(cc[1], "doit", "black");
                    s += span("\"", "doit", color, spanid);
                }
                else
                {
                    s += span("'" + condition + "' THEN \"", "doit", color, spanid);
                    s += span(cc[0], "doit", "black");
                    s += span("\"", "doit", color, spanid);
                    if (cc.Length > 1 && cc[1] != "")
                    {
                        s += span(" OTHERWISE \"", "doit", color, spanid);
                        s += span(cc[1], "doit", "black");
                        s += span("\"", "doit", color, spanid);
                    }
                }
                s += span(")", "doit", color, spanid);
            }
            s += end;
            return s;
        }

        private string span(string temp, string clause, string col, string hover = null)
        {
            if (string.IsNullOrEmpty(temp)) return "";
            if (string.IsNullOrEmpty(genderText(temp, 1)[0])) return temp;
            if (RequestFormContains(clause) || clause == "doit")
                temp = string.Format("<span style=\"color:{0}\"{2}>{1}</span>", col, temp, hover != null ? " spanid=\"sp" + hover + "\" onmouseover=\"highlight_all(this)\" onmouseout=\"unhighlight_all(this)\"" : "");
            else
                temp = string.Format("<span style=\"color:{0}\">{1}</span>", "#0000aa", temp);
            string s;
            while ((s = Regex.Replace(temp, @"<span style=""color:(#0000aa|red)""><\/span>", "")) != temp)
                temp = s;
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

        private Dictionary<string, string> listTypes = new Dictionary<string, string>
        {
            {"BULLET", "ul"},
            {"OL", "ol"},
        };

        private string generateList(string temp, string separator, string lastSeperator, int code = 1)
        {
            temp = "??" + ExtractArrays(temp.Substring(2), "|");
            List<string> list = new List<string>(GetSplit(genderText(temp, code)[0].Substring(2), '{', '}', '|'));
            for (int i = 0; i < list.Count; i++)
                list[i] = list[i].Trim();
            list.RemoveAll(delegate (string isvalid) { return string.IsNullOrEmpty(isvalid) || isvalid.StartsWith("'Error: "); });
            temp = "";
            if (listTypes.ContainsKey(separator) && list.Count > 0) temp += "<" + listTypes[separator] + ">";
            for (int i = 0; i < list.Count; i++)
            {
                if (!listTypes.ContainsKey(separator))
                {
                    if (i == list.Count - 1 && list.Count > 1 && !string.IsNullOrEmpty(lastSeperator)) temp += lastSeperator;
                    else if (!string.IsNullOrEmpty(temp)) temp += separator;
                    temp += list[i];
                }
                else
                {
                    temp += "<li>" + list[i] + "</li>";
                }
            }
            if (listTypes.ContainsKey(separator) && list.Count > 0) temp += "</" + listTypes[separator] + ">";
            return temp;
        }
    }
}
