using AssetBuilder.Properties;
using RiskCalc.classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using AssetBuilder;
using Algo = RiskCalc.classes.Algo;

namespace RiskCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AssetBuilder.Controls.ABWindow
    {
        Dictionary<char, Dictionary<int, int>> life = new Dictionary<char, Dictionary<int, int>>();
        Dictionary<int, int> MLife = new Dictionary<int, int>();
        Dictionary<int, int> FLife = new Dictionary<int, int>();

        public MainWindow()
        {
            InitializeComponent();
            Title = Window1.windowTitle;
            XmlDocument doc = new XmlDocument();
            doc.Load("RiskCalc/Xml/ConditionOffset.xml");
            foreach (XmlNode item in doc.SelectNodes("root/Conditions/Condition"))
            {
                AlgoSettings.Add(int.Parse(item.Attributes["algoid"].InnerText),
                    new Algo
                    {
                        ID = int.Parse(item.Attributes["algoid"].InnerText),
                        ScalingFactor = double.Parse(item.Attributes["ScalingFactor"].InnerText),
                        OffsetFormula = "=" + item.Attributes["OffsetFormula"].InnerText
                    }
                );
            }

            doc.Load("RiskCalc/Xml/LifeExpectancy.xml");
            foreach (XmlNode item in doc.SelectNodes("/root/LifeExpectancy"))
            {
                char gender = item.Attributes["Gender"].Value[0];
                int age = int.Parse(item.Attributes["Age"].Value);
                int value = int.Parse(item.Attributes["Value"].Value);
                if (gender == 'M') MLife.Add(age, value);
                if (gender == 'F') FLife.Add(age, value);
            }
            life.Add('M', MLife);
            life.Add('F', FLife);

            DDLAlgo.Items.Clear();
            string ra = Window1.RiskMotherAlgo.ToString();
            XmlNode an = AssetBuilder.DataAccess.getDataNode("ab_getitems", new string[] { "@boxid", "0", "@assettypeid", "1", "@algoid", ra, "@searchword", "%" }, false);
            foreach (var algo in an.SelectNodes("*[BoxID=5 and ID and not(ID=" + ra + ")]").OfType<XmlNode>().OrderBy(o => o["Description"].InnerText))
            {
                DDLAlgo.Items.Add(new ComboBoxItem { Content = algo["Description"].InnerText, Tag = algo["ID"].InnerText });
            }

            XmlNode variables = AssetBuilder.DataAccess.getDataNode("ab_TableEdit", new string[] {
                "@TableName", "Variables",
                "@xml", "<root command=\"get\" />",
            }, false);

            var usertypes = variables.SelectNodes("Table/value_UserType").OfType<XmlNode>().Select(f => f.InnerText).Distinct();
            Dictionary<string, string> lookup = new Dictionary<string, string>();

            foreach (var item in usertypes)
            {
                string text = item == "" ? "Default" : item;
                if (item != "")
                {
                    XmlNode xe = AssetBuilder.DataAccess.getDataNode("ab_GetAsset", new string[] {
                                    "@AssetTypeID", "3",
                                    "@AssetID", item
                                }, false);
                    text = xe?["Table"]?["Clinical_Answer"]?.InnerText;
                }
                if (text == null) continue;
                lookup.Add(item, text);
                variableLookup.Add(lookup[item], new Dictionary<string, double>());

                DDLUserType.Items.Add(lookup[item]);
            }

            if (DDLUserType.Items.Count == 0) DDLUserType.Items.Add("Default");
            DDLUserType.SelectedIndex = 0;

            var vl = variables.SelectNodes("Table").OfType<XmlNode>().Where(f => lookup.ContainsKey(f["value_UserType"].InnerText)).Select(f => new
            {
                Group = lookup[f["value_UserType"].InnerText],
                Key = "<" + f["value_Variable"].InnerText + ">",
                Value = double.Parse(f["value_Value"].InnerText)
            });
            foreach (var item in vl)
                variableLookup[item.Group].Add(item.Key, item.Value);
        }

        Dictionary<string, Dictionary<string, double>> variableLookup = new Dictionary<string, Dictionary<string, double>>();
        Dictionary<int, Algo> AlgoSettings = new Dictionary<int, Algo>();
        List<Control> controls = new List<Control>();
        List<History> history = new List<History>();
        List<Algo> algos = new List<Algo>();
        List<ScenarioHistory> entries;
        ObservableCollection<KeyValuePair<string, TimeSpan>> timings;
        DateTime then = DateTime.Now;
        int age = 18;
        char gender = 'F';
        int time = 10;
        double scalingfactor = 0;
        string offsetformula = "";

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetAlgo();
        }

        private void ResetAlgo()
        {
            int algoid = getAlgoID();
            if (algoid > 0)
            {
                ClearTimings();
                ListEntries(algoid);

                LoadAlgo(algoid);

                CreateAlgoForm();

                CreateRiskFormulas();

                updateControls();
            }
        }

        private void SetTimings()
        {
            TimeSpan total = new TimeSpan(timings.Select(f => f.Value.Ticks).Sum());
            timings.Add(new KeyValuePair<string, TimeSpan>("Total", total));
            Timings.ItemsSource = timings;
        }

        private int getAlgoID()
        {
            int algoid = 0;
            var sel = DDLAlgo.SelectedItem;
            if (sel != null && sel is ComboBoxItem)
            {
                ComboBoxItem cbi = sel as ComboBoxItem;
                if (cbi.Tag == null || !int.TryParse(cbi.Tag.ToString(), out algoid)) return 0;
            }
            return algoid;
        }

        private void ListEntries(int algoid)
        {
            Saved.Items.Clear();
            RiskHistory risks = (RiskHistory)AssetBuilder.Extension.DeSerialise(Settings.Default.RiskEntry);
            if (risks != null && DDLAlgo.SelectedItem != null && risks.MyHistory.ContainsKey(algoid))
            {
                Dictionary<string, List<ScenarioHistory>> scenarios = risks.MyHistory[algoid];
                foreach (var scenario in scenarios)
                {
                    Saved.Items.Add(new ListBoxItem { Content = scenario.Key, Tag = scenario.Value });
                }
            }
            AddTiming("List Saved Scenarios");
        }

        Dictionary<int, TextBox> ftb = new Dictionary<int, TextBox>();

        private void CreateRiskFormulas()
        {
            Formulas.Children.Clear();
            ftb.Clear();

            var formulas = from a in algos
                           from q in a.Questions.Where(q => q.NodeTypeID == 37 && q.Category == "Risk factors")
                           from f in q.Answers.Where(f => f.NodeTypeID == 81)
                           select new { Question = q, Answer = f, NodeID = q.NodeID };

            foreach (var item in formulas.Where(f => f.Question.Risk == RiskType.CurrentRisk).OrderBy(f => f.Question.Text))
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                TextBox tb = new TextBox { Text = item.Answer.Formula, TextWrapping = TextWrapping.Wrap, Tag = item.Answer };
                TextBlock graph = new TextBlock { Text = "graph", Tag = tb, Foreground = Brushes.Blue, TextDecorations = TextDecorations.Underline, Margin = new Thickness(10, 4, 10, 0) };
                tb.TextChanged += tb_TextChanged;
                graph.MouseDown += graph_MouseDown;
                sp.Children.Add(graph);
                sp.Children.Add(new Label { Content = item.Question.Text, ToolTip = $"Q{item.Question.ID} A{item.Answer.ID}"});
                Formulas.Children.Add(sp);
                Formulas.Children.Add(tb);
                ftb.Add(item.NodeID, tb);
            }

            AddTiming("Create Formulas");
        }

        void graph_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBlock).Tag as TextBox;
            new CurveVisualiser.MainWindow(tb).Show();
        }

        bool asynchronous = false;

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            asynchronous = tb.IsFocused;
            Answer answer = tb.Tag as Answer;
            answer.Formula = tb.Text;
            tb.SetValue(TextBox.BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 240, 240)));
            updateControls(sender, e);
        }

        private void CreateAlgoForm()
        {
            Controls.Children.Clear();
            controls.Clear();
            //var con = algos.SelectMany(f => f.Questions.Where(q => q.NodeTypeID == 38)).SelectMany(q => q.Conclusions).Select(c => new { ID = c.ID, Text = c.Text }).Distinct();
            var alg = from a in algos
                      select a;

            var con = (from a in algos
                       from q in a.Questions.Where(q => q.NodeTypeID == 38)
                       from c in q.Conclusions.Where(c => !a.Conclusions.Any(ac => ac.ID == c.ID))
                       select new { ID = c.ID, Text = c.Text, Type = ControlType.Conclusion, Silent = c.Silent, Main = c.Main })
                      .Distinct().OrderByDescending(o => o.Main).ThenByDescending(o => o.Silent).ThenBy(o => o.Text);

            var cal = (from a in algos
                       from q in a.Questions.Where(q => q.NodeTypeID == 37 || q.NodeTypeID == 36)
                       from c in Enumerable.Repeat(q, q.NodeTypeID == 36 ? 1 : 0).Concat(q.Questions.Where(c => !a.Questions.Any(ac => ac.ID == c.ID)))
                       select new { ID = c.ID, Text = c.Text, Type = ControlType.Value })
                       .Distinct().OrderBy(o => o.Text);

            var che = (from a in algos
                       from q in a.Questions.Where(q => q.NodeTypeID == 39 && !a.Questions.Any(ac => ac.ID == q.NextNodeID))
                       where !cal.Any(f => f.ID == q.NextNodeID)
                       select new { ID = q.NextNodeID, Text = q.Text.Replace("Check calculated value from ", ""), Type = ControlType.Value })
                       .Distinct().OrderBy(o => o.Text);

            var que = (from a in algos
                       from q in a.Questions.Where(q => q.NodeTypeID != 37 && q.NodeTypeID != 38 && q.NodeTypeID != 39 && q.NodeTypeID != 34 && q.NodeTypeID != 35)
                       from qa in q.Answers.Where(qa => qa.ID != 3)
                       select new { ID = q.ID, Text = q.Text + " - " + qa.Text, Type = ControlType.Pair, AnswerID = qa.ID, QuestionType = q.NodeTypeID, AnswerType = qa.NodeTypeID })
                       .Distinct().OrderBy(o => o.Text);

            StackPanel demo = new StackPanel { Orientation = Orientation.Horizontal };
            RadioButton male = new RadioButton { Content = "Male", GroupName = "Gender", Margin = new Thickness(0, 0, 10, 0), Tag = new { Type = ControlType.Gender } };
            RadioButton female = new RadioButton { Content = "Female", GroupName = "Gender", Margin = new Thickness(0, 0, 10, 0), Tag = new { Type = ControlType.Gender }, IsChecked = true };
            TextBox age = new TextBox { Text = "18", Width = 50, Margin = new Thickness(0, 0, 10, 0), Tag = new { Type = ControlType.Age } };
            TextBlock agetext = new TextBlock { Text = "Age", Margin = new Thickness(0, 0, 10, 0) };
            TextBox time = new TextBox { Text = "10", Width = 50, Margin = new Thickness(0, 0, 10, 0), Tag = new { Type = ControlType.Time } };
            TextBlock timetext = new TextBlock { Text = "Time", Margin = new Thickness(0, 0, 10, 0) };
            AddControl(male, demo);
            AddControl(female, demo);
            AddControl(age, demo);
            demo.Children.Add(agetext);
            AddControl(time, demo);
            demo.Children.Add(timetext);
            Controls.Children.Add(demo);

            foreach (var algo in alg)
            {
                StackPanel AlgoHeader = new StackPanel { Orientation = Orientation.Horizontal };
                TextBox sf = new TextBox { Text = algo.ScalingFactor.ToString(), Width = 100, Margin = new Thickness(0, 0, 10, 0), Tag = new { Type = ControlType.ScalingFactor } };
                TextBlock sftext = new TextBlock { Text = "ScalingFactor", Margin = new Thickness(0, 0, 10, 0) };
                TextBox of = new TextBox { Text = algo.OffsetFormula, Width = 300, Margin = new Thickness(0, 0, 10, 0), Tag = new { Type = ControlType.OffsetFormula } };
                TextBlock oftext = new TextBlock { Text = "OffsetFormula", Margin = new Thickness(0, 0, 10, 0) };
                AddControl(sf, AlgoHeader);
                AlgoHeader.Children.Add(sftext);
                AddControl(of, AlgoHeader);
                AlgoHeader.Children.Add(oftext);
                Controls.Children.Add(AlgoHeader);
            }
            foreach (var conclusion in con)
            {
                CheckBox cb = new CheckBox { Tag = conclusion, ToolTip = string.Format("C{0}", conclusion.ID) };
                if (conclusion.Silent)
                    cb.Content = new TextBlock { Foreground = Brushes.Red, Text = conclusion.Text };
                else
                    cb.Content = conclusion.Text;
                if (conclusion.Main) cb.IsChecked = true;
                AddControl(cb, Controls);
            }
            foreach (var calc in cal.Concat(che).OrderBy(c => c.Text))
            {
                StackPanel sp = new StackPanel { Orientation = Orientation.Horizontal };
                TextBox tb = new TextBox { Width = 100, Margin = new Thickness(0, 0, 10, 0), Tag = calc };
                TextBlock text = new TextBlock { Text = calc.Text, ToolTip = string.Format("Q{0}", calc.ID) };
                AddControl(tb, sp);
                sp.Children.Add(text);
                Controls.Children.Add(sp);
            }
            foreach (var quest in que)
            {
                CheckBox cb = new CheckBox { Content = quest.Text, Tag = quest, ToolTip = string.Format("Q{0} A{1}", quest.ID, quest.AnswerID) };
                AddControl(cb, Controls);
            }
            Button track = new Button { Content = "Track", Margin = new Thickness(0, 10, 0, 0), Width = 150 };
            track.Click += track_Click;
            Controls.Children.Add(track);

            AddTiming("Create Input Form");
        }

        void track_Click(object sender, RoutedEventArgs e)
        {
            AssetBuilder.Controls.AlgoTracker at = new AssetBuilder.Controls.AlgoTracker();
            at.textBox1.Text = Tracking.ToString();
            at.Show();
        }

        private void LoadAlgo(int algoid)
        {
            XmlNode xn = AssetBuilder.DataAccess.getDataNode("usp_GetAlgoNodes", new string[] { "@AlgoID", algoid.ToString() }, false);
            AddTiming("Call WebService");

            algos.Clear();
            Algo algo = null;
            Question question = null;

            var nodes = xn.SelectNodes("*[AlgoID]").OfType<XmlNode>();
            foreach (var node in nodes)
            {
                algoid = int.Parse(node["AlgoID"].InnerText);
                int nodeid = int.Parse(node["NodeID"].InnerText);
                int nextnodeid = int.Parse(node["NextNodeID"].InnerText);
                int nodetypeid = int.Parse(node["NodeTypeID"].InnerText);
                int assetid = int.Parse(node["AssetID"].InnerText);
                if (node.Name != "Table") algo = algos.Where(f => f.AlgoID == algoid).First();
                if (node.Name == "Table2" || nodetypeid == 44 || nodetypeid == 45) question = algo.Questions.Where(f => f.NodeID == nodeid && IsQuestion(f)).First();
                Asset newasset = null;

                switch (node.Name)
                {
                    case "Table":
                        newasset = new Algo
                        {
                            Text = node["Algo_Name"].InnerText,
                            OffsetFormula = node["WM2"].InnerText,
                        };
                        algo = newasset as Algo;
                        string scale = node["Word_Merge"].InnerText;
                        double scalingfactor = 0;
                        if (scale.StartsWith("=") && double.TryParse(scale.Substring(1), out scalingfactor)) algo.ScalingFactor = scalingfactor;
                        else if (AlgoSettings.ContainsKey(algoid)) algo.ScalingFactor = AlgoSettings[algoid].ScalingFactor;
                        if (!algo.OffsetFormula.StartsWith("=") && AlgoSettings.ContainsKey(algoid)) algo.OffsetFormula = AlgoSettings[algoid].OffsetFormula;
                        algos.Add(algo);
                        break;
                    case "Table1":
                        newasset = new Question
                        {
                            Text = node["Clinical_Statement"].InnerText,
                            Category = node["Category"].InnerText,
                            Subcategory = node["Subcategory"].InnerText,
                            Risk = (RiskType)int.Parse(node["Algo_Risk_Change_ID"].InnerText),
                        };
                        if (nodetypeid == 45)
                            question.Questions.Add(newasset as Question);
                        else
                            algo.Questions.Add(newasset as Question);
                        break;
                    case "Table2":
                        string formula = "";
                        if (question.NodeTypeID == 37 && assetid == 4)
                            formula = "<c>=" + question.Counter.ToString();
                        else formula = node["Answer_Text"].InnerText;
                        newasset = new Answer
                        {
                            Text = node["Clinical_Answer"].InnerText,
                            Formula = formula,
                            Min = double.Parse(node["Min_Value"].InnerText) * double.Parse(node["Multiplier"].InnerText),
                            Max = double.Parse(node["Max_Value"].InnerText) * double.Parse(node["Multiplier"].InnerText),
                        };
                        question.Answers.Add(newasset as Answer);
                        break;
                    case "Table3":
                        newasset = new Conclusion
                        {
                            Text = node["Lay_Condition"]?.InnerText,
                            Silent = bool.Parse(node["Silent"]?.InnerText ?? "False"),
                            Main = node["cat1"]?.InnerText == "Risk Models" && node["cat2"]?.InnerText == "1"
                        };
                        if (nodetypeid == 44)
                            question.Conclusions.Add(newasset as Conclusion);
                        else
                            algo.Conclusions.Add(newasset as Conclusion);
                        break;
                    case "Table5":
                        newasset = new Transfer();
                        algo.Transfers.Add(newasset as Transfer);
                        break;
                    case "Table6":
                        newasset = new Stop();
                        algo.Stops.Add(newasset as Stop);
                        break;
                    default:
                        break;
                }
                if (newasset != null)
                {
                    newasset.AlgoID = algoid;
                    newasset.NodeID = nodeid;
                    newasset.NextNodeID = nextnodeid;
                    newasset.ID = assetid;
                    newasset.NodeTypeID = nodetypeid;
                    newasset.Algo = algo;
                    newasset.Counter = double.Parse(node["Counter"].InnerText);
                }
            }

            AddTiming("Extract Asset Types");
            algos.ForEach(a => a.PopulateNodes());
        }

        private void ClearTimings()
        {
            timings = new ObservableCollection<KeyValuePair<string, TimeSpan>>();
            then = DateTime.Now;
        }

        private void AddTiming(string message)
        {
            timings.Add(new KeyValuePair<string, TimeSpan>(message, DateTime.Now - then));
            then = DateTime.Now;
        }

        private void AddControl(Control control, Panel panel)
        {
            controls.Add(control);
            panel.Children.Add(control);
            if (control is RadioButton)
            {
                RadioButton rb = control as RadioButton;
                rb.Checked += updateControls;
            }
            if (control is CheckBox)
            {
                CheckBox cb = control as CheckBox;
                cb.Checked += updateControls;
                cb.Unchecked += updateControls;
            }
            else if (control is TextBox)
            {
                TextBox tb = control as TextBox;
                tb.TextChanged += updateControls;
            }
        }

        private bool suspend = false;
        private IEnumerable<int> sQTypes = new int[] { 32, 42 };
        private int[] sATypes = { 64 };

        void updateControls(object sender, RoutedEventArgs e)
        {
            if (suspend) return;
            asynchronous = (sender as Control).IsFocused;
            ClearTimings();
            Control control = sender as Control;
            if (sender is CheckBox && (sender as CheckBox).IsChecked == true)
            {
                var dyn = control.Tag as dynamic;
                if (dyn.Type == ControlType.Pair &&
                    (sQTypes.Any(f => f == dyn.QuestionType) || sATypes.Any(f => f == dyn.AnswerType)))
                {
                    suspend = true;
                    var matches = controls.Select(f => new { Control = f as CheckBox, Tag = f.Tag as dynamic }).Where(f => f.Tag.Type == ControlType.Pair && f.Control.IsChecked == true && f.Tag.ID == dyn.ID && f.Tag != dyn);
                    foreach (var match in matches)
                    {
                        match.Control.IsChecked = false;
                    }
                    suspend = false;
                }
            }
            updateControls();

            foreach (var tb in ftb)
            {
                if (history.Any(f => f.NodeID == tb.Key))
                    tb.Value.SetResourceReference(Control.BorderBrushProperty, "HighlightedBrush");
                else
                    tb.Value.SetResourceReference(Control.BorderBrushProperty, "NormalBrush");
            }
        }

        void updateControls()
        {
            entries = new List<ScenarioHistory>();
            history.Clear();
            foreach (var item in controls)
            {
                var tag = item.Tag as dynamic;
                double value = 0;
                string text = "";
                if (item is TextBox) text = (item as TextBox).Text;
                if ((item is ToggleButton && (item as ToggleButton).IsChecked == true) || (text != "" && double.TryParse(text, out value)) || (item is TextBox && tag.Type == ControlType.OffsetFormula))
                {
                    ScenarioHistory sh = new ScenarioHistory { Text = text, Type = (ControlType)tag.Type };
                    switch (sh.Type)
                    {
                        case ControlType.Conclusion:
                            history.Add(new History { ConclusionID = tag.ID });
                            sh.ID = tag.ID;
                            break;
                        case ControlType.Value:
                            history.Add(new History { QuestionID = tag.ID, Value = value });
                            sh.ID = tag.ID;
                            break;
                        case ControlType.Pair:
                            history.Add(new History { QuestionID = tag.ID, AnswerID = tag.AnswerID });
                            sh.ID = tag.ID;
                            sh.AnswerID = tag.AnswerID;
                            break;
                        case ControlType.Age:
                            age = (int)value;
                            break;
                        case ControlType.Gender:
                            gender = (item as RadioButton).Content.ToString()[0];
                            sh.Text = (item as RadioButton).Content.ToString();
                            break;
                        case ControlType.ScalingFactor:
                            scalingfactor = value;
                            break;
                        case ControlType.OffsetFormula:
                            offsetformula = text;
                            break;
                        case ControlType.Time:
                            time = (int)value;
                            break;
                        default:
                            break;
                    }
                    entries.Add(sh);
                }
            }

            AddTiming("Set History");
            RunAlgos();
            SetTimings();
        }

        private void RunAlgos()
        {
            Tracking.Clear();
            foreach (var algo in algos)
            {
                RunAlgo(algo, DDLUserType.SelectedItem.ToString());
                AddTiming("Run " + algo.Text);
            }
            if (asynchronous)
            {
                Thread t = new Thread(new ThreadStart(CollateResults));
                t.Start();
            }
            else CollateResults();
        }

        Regex vrx = new Regex("<[^>]+>");
        StringBuilder Tracking = new StringBuilder();

        private void RunAlgo(Algo algo, string usertype)
        {
            double counter = 0;
            Asset a = algo;
            string s = string.Format("NodeType : {0} , Algo : {1} , Node : {2} , Asset : {3} , AnswerID : -1", a.NodeTypeID, a.AlgoID, a.NodeID, a.ID);
            Tracking.AppendLine(s);
            while (a != null)
            {
                a = a.GetNextNode();
                if (a != null) s = string.Format("NodeType : {0} , Algo : {1} , Node : {2} , Asset : {3} , AnswerID : ", a.NodeTypeID, a.AlgoID, a.NodeID, a.ID);
                if (a is Conclusion)
                {
                    history.Add(new History { ConclusionID = a.ID, NodeID = a.NodeID });
                    s += "-1";
                }
                else if (a is Question)
                {
                    var question = a as Question;
                    var ifnot = question.Answers.Where(ans => ans.ID == 3);
                    IEnumerable<Asset> match = null;

                    switch (a.NodeTypeID)
                    {
                        case 34:
                            {
                                match = question.Answers.Where(ans => ans.Text[0] == gender);
                            }
                            break;
                        case 35:
                            {
                                match = question.Answers.Where(ans => (age * 365.25) >= ans.Min && (age * 365.25) < ans.Max);
                            }
                            break;
                        case 37:
                            {
                                match = question.Answers.Where(ans => ans.NodeTypeID == 81 || ans.NodeTypeID == 64);
                                if (match.Any())
                                {
                                    Answer calc = match.First() as Answer;
                                    string formula = calc.Formula;
                                    bool sc = formula.StartsWith("<c>=");
                                    formula = formula.Replace("<c>=", "");
                                    formula = formula.Replace("<c>", counter.ToString());
                                    formula = formula.Replace("=", "");
                                    formula = formula.Replace("<age>", age.ToString());
                                    if (formula.Contains("dbo."))
                                    {
                                        formula = formula.Replace("dbo.", "");
                                        formula = formula.Replace("'", "");
                                    }

                                    foreach (Match m in vrx.Matches(formula))
                                    {
                                        string replace = "0";
                                        if (variableLookup[usertype].ContainsKey(m.Value)) replace = variableLookup[usertype][m.Value].ToString();
                                        else if (variableLookup["Default"].ContainsKey(m.Value)) replace = variableLookup["Default"][m.Value].ToString();
                                        formula = formula.Replace(m.Value, replace);
                                    }

                                    formula = "|" + formula + "|";
                                    char c = (char)96;
                                    foreach (var input in question.Questions)
                                    {
                                        c = (char)(c + 1);
                                        Regex r = new Regex("\\b" + c + "\\b");
                                        double val = 0;
                                        var values = history.Where(h => h.QuestionID == input.ID).Select(h => (double)h.Value);
                                        if (values.Any()) val = values.First();
                                        formula = r.Replace(formula, val.ToString());
                                    }

                                    formula = formula.Replace("|", "");
                                    double value = Evaluator.Eval(formula);
                                    if (sc) counter = value;
                                    history.Add(new History { QuestionID = question.ID, AnswerID = calc.ID, Value = value, NodeID = question.NodeID });
                                }
                            }
                            break;
                        case 38:
                            {
                                match = question.Conclusions.Where(c => history.Any(h => h.ConclusionID == c.ID));
                            }
                            break;
                        case 39:
                            {
                                var value = history.Where(h => h.QuestionID == a.NextNodeID);
                                if (value.Any())
                                {
                                    double val = (double)value.Last().Value;
                                    match = question.Answers.Where(ans => ans.Min <= val && ans.Max > val);
                                }
                                else match = question.Answers.Where(ans => history.Any(h => h.QuestionID == question.ID && h.AnswerID == ans.ID)); //match = new Answer[0];
                            }
                            break;
                        case 41:
                            {
                                match = question.Answers.Where(ans => ans.Min <= counter && ans.Max > counter);
                            }
                            break;
                        default:
                            {
                                match = question.Answers.Where(ans => history.Any(h => h.QuestionID == question.ID && h.AnswerID == ans.ID));
                            }
                            break;
                    }

                    if (match.Any())
                    {
                        a = match.First();
                        s += a.ID;
                    }
                    else a = null;

                    if (a == null && ifnot.Any())
                    {
                        a = ifnot.First();
                        s += 3;
                    }
                    if (a != null) counter += a.Counter;
                }
                else s += "-1";
                if (a != null) Tracking.AppendLine(s);
            }
        }

        private void CollateResults()
        {
            var questions = (from a in algos
                             from q in a.Questions.Where(q => q.Risk != RiskType.NoRisk)
                             select new { ID = q.ID, Factor = q.Subcategory, Risk = q.Risk }).Distinct();

            var results = from c in questions.Where(c => c.Risk == RiskType.CurrentRisk)
                          from r in questions.Where(r => r.Risk == RiskType.ReducedRisk && r.Factor == c.Factor).DefaultIfEmpty(new { ID = -1, Factor = c.Factor, Risk = RiskType.ReducedRisk })
                          from m in questions.Where(m => m.Risk == RiskType.MinimumRisk && m.Factor == c.Factor).DefaultIfEmpty(new { ID = -1, Factor = c.Factor, Risk = RiskType.MinimumRisk })
                          from ch in history.Where(ch => ch.QuestionID == c.ID).DefaultIfEmpty(new History { Value = null })
                          from rh in history.Where(rh => rh.QuestionID == r.ID).DefaultIfEmpty(new History { Value = null })
                          from mh in history.Where(mh => mh.QuestionID == m.ID).DefaultIfEmpty(new History { Value = null })
                          select new
                          {
                              Factor = c.Factor,
                              Current = ch.Value ?? 0,
                              Reduced = rh.Value ?? ch.Value ?? 0,
                              Minimum = mh.Value ?? ch.Value ?? 0,
                          };

            // Math.Round(Math.Exp((double)( ---- )), 2)

            var totals = Enumerable.Repeat(new
            {
                Factor = "Total",
                Current = results.Sum(r => r.Current),
                Reduced = results.Sum(r => r.Reduced),
                Minimum = results.Sum(r => r.Minimum),
            }, 1);

            string offset = offsetformula.Replace("=", "").Replace("<time>", time.ToString()).Replace("time", time.ToString()).Replace("<age>", age.ToString()).Replace("age", age.ToString());
            double offsetvalue = Evaluator.Eval(offset);
            var risks = new ObservableCollection<Result>(from t in totals
                                                         select new Result
                                                         {
                                                             Factor = "Risk",
                                                             Current = Math.Round((1 - Math.Exp(-scalingfactor * time * Math.Exp(t.Current + offsetvalue))) * 100, 2) + "%",
                                                             Reduced = Math.Round((1 - Math.Exp(-scalingfactor * time * Math.Exp(t.Reduced + offsetvalue))) * 100, 2) + "%",
                                                             Minimum = Math.Round((1 - Math.Exp(-scalingfactor * time * Math.Exp(t.Minimum + offsetvalue))) * 100, 2) + "%"
                                                         });

            if ((bool)GetValue(LogRR, RadioButton.IsCheckedProperty))
                SetValue(Results, DataGrid.ItemsSourceProperty, new ObservableCollection<Result>(results.OrderBy(o => o.Factor).Concat(totals).Select(q => new Result
                {
                    Factor = q.Factor,
                    Current = Math.Round(q.Current, 2).ToString(),
                    Reduced = Math.Round(q.Reduced, 2).ToString(),
                    Minimum = Math.Round(q.Minimum, 2).ToString()
                })));
            else
                SetValue(Results, DataGrid.ItemsSourceProperty, new ObservableCollection<Result>(results.OrderBy(o => o.Factor).Concat(totals).Select(q => new Result
                {
                    Factor = q.Factor,
                    Current = Math.Round(Math.Exp(q.Current), 2).ToString(),
                    Reduced = Math.Round(Math.Exp(q.Reduced), 2).ToString(),
                    Minimum = Math.Round(Math.Exp(q.Minimum), 2).ToString()
                })));
            SetValue(Totals, DataGrid.ItemsSourceProperty, risks);
            if (asynchronous)
            {
                Dispatcher.BeginInvoke((Action)delegate () { AddTiming("Show Results"); });
                Dispatcher.BeginInvoke((Action)delegate () { RenderGraph(totals.First().Current); });
            }
            else
            {
                AddTiming("Show Results");
                RenderGraph(totals.First().Current);
            }
        }

        object GetValue(UIElement ui, DependencyProperty dp)
        {
            if (ui.Dispatcher.CheckAccess())
                return ui.GetValue(dp);
            else
            {
                object result = null;
                ui.Dispatcher.Invoke((Action)delegate () { result = GetValue(ui, dp); });
                return result;
            }
        }

        void SetValue(UIElement ui, DependencyProperty dp, object value)
        {
            if (ui.Dispatcher.CheckAccess())
                ui.SetValue(dp, value);
            else
                ui.Dispatcher.Invoke((Action)delegate () { SetValue(ui, dp, value); });
        }

        private void RenderGraph(double totalLogRR)
        {
            if (double.IsNaN(totalLogRR) || double.IsInfinity(totalLogRR)) totalLogRR = 0;
            Pen shapeOutlinePen = new Pen(Brushes.Black, 2);
            Pen bluePen = new Pen(Brushes.Blue, 4);
            Pen redPen = new Pen(Brushes.Red, 1);
            DrawingGroup dGroup = new DrawingGroup();
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            using (DrawingContext dc = dGroup.Open())
            {
                double xc = 2000;
                double yc = 800;
                double xd = xc / 10;
                double yd = yc / 10;
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, new Rect(-175, -50, xc + 275, yc + 125));

                for (int i = 0; i < 11; i++)
                {
                    dc.DrawLine(shapeOutlinePen, new Point(0, i * yd), new Point(xc, i * yd));
                    dc.DrawLine(shapeOutlinePen, new Point(i * xd, 0), new Point(i * xd, yc));

                    string xl = (i * 10).ToString();
                    string yl = i * 10 + "%";

                    AssetBuilder.Extension.DrawText(dc, xl, i * xd, yc + 20, TextAlignment.Center, System.Windows.VerticalAlignment.Top, pixelsPerDip, Brushes.Black);
                    AssetBuilder.Extension.DrawText(dc, yl, -20, (10 - i) * yd, TextAlignment.Right, System.Windows.VerticalAlignment.Center, pixelsPerDip, Brushes.Black);
                }

                Point last = new Point(0, yc);
                Point lastdiff = last;
                bool ha = false;
                List<double> values = new List<double>();
                List<double> diffs = new List<double>();

                for (int i = 0; i < 101; i++)
                {
                    string offset = offsetformula.Replace("=", "").Replace("<time>", i.ToString()).Replace("time", i.ToString()).Replace("<age>", age.ToString()).Replace("age", age.ToString());
                    double offsetvalue = Evaluator.Eval(offset);
                    double value = (1 - Math.Exp(-scalingfactor * i * Math.Exp(totalLogRR + offsetvalue))) * 100;
                    values.Add(value);
                    if (i == 0) diffs.Add(value); else diffs.Add(value - values[i - 1]);
                }

                double maxdiff = diffs.Max();

                for (int i = 0; i < 101; i++)
                {
                    double diff = diffs[i];
                    double value = values[i];
                    Point next = new Point(
                        AssetBuilder.Extension.Scale(i, 100, 0, xc),
                        AssetBuilder.Extension.Scale(value, 100, yc, 0)
                    );
                    Point nextdiff = new Point(
                        AssetBuilder.Extension.Scale(i, 100, 0, xc),
                        AssetBuilder.Extension.Scale(diff, maxdiff, yc, 0)
                    );
                    if (!ha && value >= 50 && life.ContainsKey(gender) && life[gender].ContainsKey(age) && DDLAlgo.SelectedItem.ToString().Contains("Death"))
                    {
                        ha = true;
                        int AgeAtDeath = i + age;
                        int death = life[gender][age];
                        int HealthAge = ((death - AgeAtDeath) + age);
                        Point hap = new Point(
                            AssetBuilder.Extension.Scale(95, 100, 0, xc),
                            AssetBuilder.Extension.Scale(10, 100, yc, 0)
                        );
                        Point lep = new Point(
                            AssetBuilder.Extension.Scale(95, 100, 0, xc),
                            AssetBuilder.Extension.Scale(90, 100, yc, 0)
                        );
                        double dim = Math.Min(AssetBuilder.Extension.Scale(8, 100, 0, yc), AssetBuilder.Extension.Scale(8, 100, 0, xc));
                        dc.DrawEllipse(Brushes.Green, shapeOutlinePen, hap, dim, dim);
                        AssetBuilder.Extension.DrawText(dc, HealthAge.ToString(), hap.X, hap.Y, TextAlignment.Center, System.Windows.VerticalAlignment.Center, pixelsPerDip, Brushes.White);
                        dc.DrawEllipse(Brushes.Red, shapeOutlinePen, lep, dim, dim);
                        AssetBuilder.Extension.DrawText(dc, AgeAtDeath.ToString(), lep.X, lep.Y, TextAlignment.Center, System.Windows.VerticalAlignment.Center, pixelsPerDip, Brushes.White);
                    }
                    dc.DrawLine(redPen, lastdiff, nextdiff);
                    dc.DrawLine(bluePen, last, next);
                    last = next;
                    lastdiff = nextdiff;
                    if (i == time)
                    {
                        dc.DrawEllipse(Brushes.Red, shapeOutlinePen, next, 10, 10);
                    }
                }
            }

            DrawingImage dImageSource = new DrawingImage(dGroup);
            graph.Source = dImageSource;
            AddTiming("Render Graph");
        }

        private bool IsQuestion(Question q)
        {
            return q.NodeTypeID != 44 && q.NodeTypeID != 45 && (q.NodeTypeID & 32) == 32;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetAlgo();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = ScenarioName.Text;
            int algoid = getAlgoID();
            if (name != "" && algoid > 0)
            {
                ClearTimings();
                RiskHistory risks = (RiskHistory)AssetBuilder.Extension.DeSerialise(AssetBuilder.Properties.Settings.Default.RiskEntry);
                if (risks == null) risks = new RiskHistory();
                if (!risks.MyHistory.ContainsKey(algoid)) risks.MyHistory.Add(algoid, new Dictionary<string, List<ScenarioHistory>>());
                var scenarios = risks.MyHistory[algoid];
                if (!scenarios.ContainsKey(name)) scenarios.Add(name, entries);
                else scenarios[name] = entries;
                AssetBuilder.Properties.Settings.Default.RiskEntry = AssetBuilder.Extension.Serialise(risks);
                Settings.Default.Save();
                ListEntries(algoid);
                SetTimings();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem lbi = getSavedItem();
            if (lbi != null)
            {
                ClearTimings();
                RiskHistory risks = (RiskHistory)AssetBuilder.Extension.DeSerialise(AssetBuilder.Properties.Settings.Default.RiskEntry);
                int algoid = getAlgoID();
                risks.MyHistory[algoid].Remove(lbi.Content.ToString());
                AssetBuilder.Properties.Settings.Default.RiskEntry = AssetBuilder.Extension.Serialise(risks);
                Settings.Default.Save();
                ListEntries(algoid);
                SetTimings();
            }
        }

        private void Saved_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LoadSaved();
        }

        private void Saved_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSaved();
        }

        ListBoxItem getSavedItem()
        {
            object sel = Saved.SelectedItem;
            if (sel != null && sel is ListBoxItem)
            {
                return sel as ListBoxItem;
            }
            return null;
        }

        private void LoadSaved()
        {
            ListBoxItem lbi = getSavedItem();
            if (lbi != null && lbi.Tag != null && lbi.Tag is List<ScenarioHistory>)
            {
                List<ScenarioHistory> scenario = (List<ScenarioHistory>)lbi.Tag;
                foreach (var item in controls)
                {
                    var tag = item.Tag as dynamic;
                    ControlType type = tag.Type;
                    switch (type)
                    {
                        case ControlType.Conclusion:
                            (item as CheckBox).IsChecked = scenario.Any(f => f.Type == type && f.ID == tag.ID);
                            break;
                        case ControlType.Value:
                            (item as TextBox).Text = scenario.Where(f => f.Type == type && f.ID == tag.ID).Select(f => f.Text).FirstOrDefault();
                            break;
                        case ControlType.Pair:
                            (item as CheckBox).IsChecked = scenario.Any(f => f.Type == type && f.ID == tag.ID && f.AnswerID == tag.AnswerID);
                            break;
                        case ControlType.Gender:
                            (item as ToggleButton).IsChecked = scenario.Any(f => f.Type == type && f.Text == (item as RadioButton).Content.ToString());
                            break;
                        default:
                            (item as TextBox).Text = scenario.Where(f => f.Type == type).Select(f => f.Text).FirstOrDefault();
                            break;
                    }
                }
                ScenarioName.Text = lbi.Content.ToString();
            }
        }
    }

    public enum ControlType
    {
        Conclusion,
        Value,
        Pair,
        Age,
        Gender,
        ScalingFactor,
        OffsetFormula,
        Time
    }
}
