using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using AssetBuilder.Controls;
using System.Threading.Tasks;
using AssetBuilder.AssetControls;
using AssetBuilder.Classes;
using AssetBuilder.Controls.AssetControls;
using AssetBuilder.ViewModels;
using Visio = Microsoft.Office.Interop.Visio;
using Application = System.Windows.Application;
using AssetType = AssetBuilder.Classes.AssetType;
using Colors = System.Windows.Media.Colors;
using ListItem = AssetBuilder.ViewModels.ListItem;
using MenuItem = System.Windows.Controls.MenuItem;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for qcat.xaml
    /// </summary>
    public partial class qcat : UserControl
    {
        #region Fields

        private const int Atc = 16;
        private const int ListWidth = 300;

        private static List<string> prmentry = new List<string>(new[] { "data", "cat1", "subcat", "cat2", "algo", "question", "answer", "conclusion", "bullet", "count", "date", "user", "type", "order" });
        private static List<string> prmname = new List<string>(new[] { "@dataid", "@catid", "@subcatid", "@cat2id", "@algoid", "@questionid", "@answerid", "@conclusionid", "@bulletid", "@count", "@date", "@user", "@type", "order" });
        private static readonly char[] Comma = ",".ToCharArray();

        private readonly string[] _idColumn = { "TITLEID", "ALGOID", "QUESTIONID", "ANSID", "RECID", "BPID", "TIMEID", "BUID", "", "", "", "MAPID", "TEXTID", "GROUPID", "", "CMAPID" };
        private readonly GridLength[] _savedGridWidths = new GridLength[5];
        private readonly Dictionary<string, int> _catTrans = new Dictionary<string, int>() {
            { "2:2", 6 },
            { "2:3", 7 },
            { "3:2", 6 },
            { "3:3", 7 },
            { "4:2", 8 },
            { "4:3", 9 },
            { "4:4", 10 },
        };
        private readonly Dictionary<string, string> _catFields = new Dictionary<string, string>() {
            { "2:2", "Category" },
            { "2:3", "Subcategory" },
            { "3:2", "Category" },
            { "3:3", "Subcategory" },
            { "4:2", "Category" },
            { "4:3", "SubCat1" },
            { "4:4", "SubCat2" },
        };
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>()
        {
            { "1", "<Properties ACD_AEP=\"0\" ACD_UC-GM=\"0\" ACD_STP=\"0\" ACD_EIA=\"0\" ACD_RIA=\"0\" ACD_ASC=\"0\" Category=\"\" ConsDescription=\"\" ConsFlag=\"\" ConsName=\"\" Description=\"\" Module=\"\" NoAnticipatedCallDistributions=\"False\" NoRelatedAlgorithms=\"True\" TargetOldestAge=\"125\" TargetYoungestAge=\"0\" Version=\"100.01\" />$$ALT$$<Properties ClinicalRationale=\"\" ConsMessage=\"\" ConsRationale=\"\" TransferQuestionNumber=\"\" TransferType=\"AlgoTransfer\" />" },
            { "2", "<Properties ClinicalRationale=\"\" ClinQuestAbrv=\"\" ConsQuestAbrv=\"\" ConsQuestion=\"\" ConsRationale=\"\" ConsUnsureText=\"\" LayQuestion=\"\" TextNote=\"\" UnsureInstruction=\"\" />" },
            { "4:XPath:Table[Information = 'true']", "<Properties LanguageRef=\"ENGL\" />" },
            { "4:XPath:Table[Silent = 'true']", "<Properties ClinicalIssues=\"\" ClinicalRationale=\"\" ConsInterimSC=\"\" ConsMessage=\"\" ConsRationale=\"\" IntervalInMinutes=\"\" IntervalQuantity=\"\" IntervalUnit=\"\" MessageToPatient=\"\" SelfCareName=\"\" SymptomPattern=\"\" TextNote=\"\" TransferQuestionNumber=\"\" TransferType=\"Recommendation\" Type=\"ASC\" />" },
            { "4:WatchoutCondition", "<Properties LanguageRef=\"ENGL\" />" },
            { "4:SelfCare", "<Properties ClinicalIssues=\"\" ClinicalRationale=\"\" ConsInterimSC=\"\" ConsMessage=\"\" ConsRationale=\"\" IntervalInMinutes=\"\" IntervalQuantity=\"\" IntervalUnit=\"\" MessageToPatient=\"\" SelfCareName=\"\" SymptomPattern=\"\" TextNote=\"\" TransferQuestionNumber=\"\" TransferType=\"Recommendation\" Type=\"ASC\" />" },
            { "4:Action", "<Properties ClinicalIssues=\"\" ClinicalRationale=\"\" ConsInterimSC=\"\" ConsMessage=\"\" ConsRationale=\"\" IntervalInMinutes=\"\" IntervalQuantity=\"\" IntervalUnit=\"minute\" MessageToPatient=\"\" SelfCareName=\"\" SymptomPattern=\"\" TextNote=\"\" TransferQuestionNumber=\"\" TransferType=\"Recommendation\" Type=\"\" />" },
            { "4", "<Properties ClinicalIssues=\"\" ClinicalRationale=\"\" ConsInterimSC=\"\" ConsMessage=\"\" ConsRationale=\"\" IntervalInMinutes=\"\" IntervalQuantity=\"\" IntervalUnit=\"minute\" MessageToPatient=\"\" SelfCareName=\"\" SymptomPattern=\"\" TextNote=\"\" TransferQuestionNumber=\"\" TransferType=\"Recommendation\" Type=\"\" />" },
        };
        private readonly Dictionary<string, TextAdorner> _textAdorners = new Dictionary<string, TextAdorner>();

        private int[][] _cats = new int[Atc][];
        private bool[] _open = new bool[Atc];
        private string[][] _text = new string[Atc][];
        private int[] _search = ResetSearch();
        private Intel _intellisense;
        private Dictionary<string, IntelModel>[] _lists;
        private int __AssetTypeID = -1;
        private int _searchTypeId = 3;
        private bool _noAssets;
        private int _lastBoxId;
        private DispatcherTimer _timer;
        private bool _listOpen;
        private object _lockObj = new object();
        private ListItem _clickedAsset;
        private bool _showAll;
        private readonly AssetListViewModel _assetListViewModel;

        internal ListBox[] lb = new ListBox[5];
        internal TextBox[] tb = new TextBox[5];
        internal bool[] filter = new bool[5];
        internal Button[] ba = new Button[5];
        internal Button[] bd = new Button[5];
        internal Button[] bc = new Button[5];
        internal Button[] be = new Button[5];

        public string AssetId = "x";
        public static string CurrentSearch = "";
        public static string CurrentSearchSql = "";
        public readonly string[] TableNames = { "TITLE", "ALGO_START", "QUESTION", "ANSWER", "RECOMMENDATION", "BULLET", "BULLET_USE", "CATEGORY", "", "", "", "MAP", "TEXTASSET" };

        #endregion Fields

        #region Presentation Properties

        #endregion Presentation Properties

        #region Properties

        public Window1 Form { get; set; }

        public XmlNode Defaults
        {
            get => BuilderDefaults;
            set => BuilderDefaults = value;
        }

        public static XmlNode BuilderDefaults { get; set; }

        public static Dictionary<int, Dictionary<int, int>> AssetFlags { get; set; }

        public bool IsEditing { get; set; }

        public assetControl LoadedAsset { get; set; }

        public int AssetTypeId
        {
            get => _AssetTypeId;
            set
            {
                if (IsEditing)
                {
                    var dr = System.Windows.Forms.MessageBox.Show("There is another asset currently being edited. Do you want to save the changes?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (!LoadedAsset.Save())
                            Window1.RadioToggle(Form.assetGroup, _AssetTypeId);
                    }
                    else if (dr == System.Windows.Forms.DialogResult.No) IsEditing = false;
                    else if (dr == System.Windows.Forms.DialogResult.Cancel) Window1.RadioToggle(Form.assetGroup, _AssetTypeId);
                }

                if (IsEditing) return;

                AddContextMenus(value);
                if (value == 0 && _AssetTypeId != 0)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        _savedGridWidths[i] = categoryGrid.ColumnDefinitions[i].Width;
                        if (i == 4) categoryGrid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
                        else categoryGrid.ColumnDefinitions[i].Width = new GridLength(0);
                    }
                }

                if (_AssetTypeId == 0 && value != 0)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        categoryGrid.ColumnDefinitions[i].Width = _savedGridWidths[i];
                    }
                }

                Window1.setStatus("Accessing Data...");
                if (_AssetTypeId >= 0) SaveCats();

                if (Form != null) Form.assetCanvas.Children.Clear();

                _AssetTypeId = Math.Abs(value);
                for (var i = 0; i < 5; i++)
                {
                    if (value >= 0 && _text[_AssetTypeId] != null) tb[i].Text = _text[_AssetTypeId][i];
                    else tb[i].Text = "";
                }

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }

                _searchTypeId = _search[_AssetTypeId];
                if (Form != null) Window1.RadioToggle(Form.searchGroup, _searchTypeId);

                if (value >= 0 && _cats[_AssetTypeId] != null && _cats[_AssetTypeId][4] > 0)
                {
                    _noAssets = true;
                    var topCat = -1;
                    for (var i = 0; i < 4; i++)
                    {
                        if (_cats[_AssetTypeId][i] > 0 && i > topCat) topCat = i;
                    }
                    if (topCat == -1) _noAssets = false;
                    Populate(0);
                    for (var i = 0; i < 4; i++)
                    {
                        if (i == topCat) _noAssets = false;
                        if (_cats[_AssetTypeId][i] > 0) lb[i].SelectedValue = _cats[_AssetTypeId][i];
                        else lb[i].UnselectAll();
                    }
                    _noAssets = false;
                    if (_cats[_AssetTypeId][4] == 0) lb[4].UnselectAll();// TODO: To ViewModel
                    else
                    {
                        lb[4].SelectedValue = _cats[_AssetTypeId][4];// TODO: To ViewModel
                        if (_open[_AssetTypeId] && lb[4].SelectedValue != null) LoadAsset(lb[4].SelectedValue.ToString());// TODO: To ViewModel
                    }
                }
                else
                {
                    Populate(0, _AssetTypeId, textBox5.Text);
                }

                Window1.setStatus("");
            }
        }

        public int SearchTypeId
        {
            get => _searchTypeId;
            set
            {
                _searchTypeId = value;
                Populate(null as ListBox);
            }
        }

        public bool IsDesignTime => DesignerProperties.GetIsInDesignMode(this);

        private int _AssetTypeId
        {
            get => __AssetTypeID;
            set
            {
                __AssetTypeID = value;
                if (Form != null)
                {
                    if (__AssetTypeID == 15 || __AssetTypeID == 4 && Window1.AllowConclusionMap) Form.rtbMap.Visibility = Visibility.Visible; else Form.rtbMap.Visibility = Visibility.Collapsed;
                    if (__AssetTypeID == 6 || __AssetTypeID == 4) Form.rtbTimes.Visibility = Visibility.Visible; else Form.rtbTimes.Visibility = Visibility.Collapsed;
                    if (__AssetTypeID == 0 || __AssetTypeID == 1) Form.rtbTitles.Visibility = Visibility.Visible; else Form.rtbTitles.Visibility = Visibility.Collapsed;
                }

                if (_intellisense != null) _intellisense.Dispose();

                if (__AssetTypeID > 0 && __AssetTypeID < 6 && Form != null)
                {
                    _intellisense = new Intel(textBox5, Form.bubbleCanvas, _lists[__AssetTypeID]);
                }
                else
                {
                    _intellisense = null;
                }
            }
        }

        #endregion Properties

        #region Constructor

        public qcat()
        {
            InitializeComponent();
            lb = new[] { listBox1, listBox2, listBox3, listBox4, listBox5 };
            tb = new[] { textBox1, textBox2, textBox3, textBox4, textBox5 };
            filter = new[] { false, false, false, false, false };
            ba = new[] { btnAdd1, btnAdd2, btnAdd3, btnAdd4, btnAdd5 };
            bd = new[] { btnDelete1, btnDelete2, btnDelete3, btnDelete4, btnDelete5 };
            bc = new[] { btnClear1, btnClear2, btnClear3, btnClear4, btnClear5 };
            be = new[] { btnEdit1, btnEdit2, btnEdit3, btnEdit4, btnEdit5 };
            IsEditing = false;

            _assetListViewModel = new AssetListViewModel();
            listBox5.DataContext = _assetListViewModel;
        }

        #endregion Constructor

        #region Public Methods

        public void InitDictionaries()
        {
            _lists = new[] {
                null,
                IntelListMakers.makeList("Regex:(?i)type:([0-9]+,)*$", Defaults, "cmbAssessment"),
                IntelListMakers.makeList("Regex:(?i)type:([0-9]+,)*$", Defaults, "cmbType"),
                IntelListMakers.makeList("Regex:(?i)type:([0-9]+,)*$", Defaults, "cmbAnsType"),
                new Dictionary<string, IntelModel> {
                    { "Regex:(?i)type:([0-9]+,)*$", new[] {
                        new IntelItem { Value = "1", Display = Window1.McKesson_Mode ? "Self Care" : "Silent" },
                        new IntelItem { Value = "2", Display = Window1.McKesson_Mode ? "Watchout Condition" : "Information" },
                        new IntelItem { Value = "3", Display = "Not " + (Window1.McKesson_Mode ? "Self Care" : "Silent") },
                        new IntelItem { Value = "4", Display = "Not " + (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                        new IntelItem { Value = "5", Display = "Neither " + (Window1.McKesson_Mode ? "Self Care" : "Silent") + " or " + (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                        new IntelItem { Value = "6", Display = "Both " + (Window1.McKesson_Mode ? "Self Care" : "Silent") + " and " + (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                    }}
                },
                new Dictionary<string, IntelModel>(),
            };

            _lists[2]["Regex:(?i)type:([0-9]+,)*$"].values.AddRange(Defaults.SelectNodes("*[*[1] = 'cmbHistory' and not(Exclude)]").OfType<XmlNode>().Select(f => new IntelItem { Value = (200 + int.Parse(f.ChildNodes[1].InnerText)).ToString(), Display = f.ChildNodes[2].InnerText }));
            _lists[2]["Regex:(?i)type:([0-9]+,)*$"].values.AddRange(Defaults.SelectNodes("*[*[1] = 'cmbComments' and not(Exclude)]").OfType<XmlNode>().Select(f => new IntelItem { Value = (100 + int.Parse(f.ChildNodes[1].InnerText)).ToString(), Display = f.ChildNodes[2].InnerText }));

            foreach (var dict in _lists)
                if (dict != null)
                {
                    foreach (var item in dict)
                    {
                        item.Value.endings = Comma;
                        item.Value.AppendEnding = true;
                    }
                    dict.Add("Regex:(?i)date:$", new[] {
                        new IntelItem { Value = DateTime.Now.ToString("yyyy-MM-dd"), Display = "Today" },
                        new IntelItem { Value = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), Display = "Yesterday" },
                        new IntelItem { Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"), Display = "Last Week" },
                        new IntelItem { Value = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"), Display = "Last Month" },
                        new IntelItem { Value = DateTime.Now.ToString("yyyy-01-01"), Display = "This Year" },
                    });
                }

            _AssetTypeId = __AssetTypeID;
        }

        public void AddContextMenus(int value)
        {
            var categoryTypes = Defaults.SelectNodes(string.Format("*[ContextMenu='{0}' and AssetTypeID='{1}']", "ListContext", value)).OfType<XmlNode>()
                .Select(f => new { ID = int.Parse(f["ID"].InnerText), Description = f["Description"].InnerText, BoxID = int.Parse(f["BoxID"].InnerText) });
            if (Window1.CategoryEnabled && categoryTypes.Any())
            {
                for (var i = 0; i < 4; i++)
                {
                    var items = categoryTypes.Where(f => f.BoxID == i + 1);
                    if (items.Any())
                    {
                        var cm = new ContextMenu();
                        cm.Opened += ContextMenuLoaded;
                        foreach (var item in items)
                        {
                            var m = new MenuItem { Header = item.Description, CommandParameter = item };
                            m.Click += lb_Context_Click;
                            cm.Items.Add(m);
                        }

                        lb[i].ContextMenu = cm;
                    }
                    else
                    {
                        lb[i].ContextMenu = null;
                    }
                }
            }
            else
            {
                for (var i = 0; i < 4; i++)
                {
                    lb[i].ContextMenu = null;
                }
            }
        }

        public void ClearCats()
        {
            _cats = new int[Atc][];
            _open = new bool[Atc];
            _text = new string[Atc][];
            _search = ResetSearch();
            _lastBoxId = 0;
        }

        public void Repopulate(int boxId)
        {
            if (boxId == 1) _lastBoxId = 0;
            if (boxId == 2) _lastBoxId = 1;
            if (boxId == 3)
            {
                _lastBoxId = AssetTypeId == 6 ? 1 : 2;
            }

            if (boxId == 4) _lastBoxId = 1;
            if (boxId == 5 || boxId == -1)
            {
                if (boxId == 5) Form.assetCanvas.Children.Clear();
                _lastBoxId = AssetTypeId == 1 ? 1 : 4;
            }

            var id = 0;
            var selectedItem = _assetListViewModel.AllAssets.FirstOrDefault(x => x.IsSelected);
            if (boxId == -1 && selectedItem != null) id = selectedItem.ID;

            Populate(null as ListBox);
            if (id > 0)
            {
                LoadedAsset = null;
                lb[4].SelectedItem = SelectItem(lb[4], id);// TODO: To ViewModel
            }
        }

        public static List<string> getParameters(bool noAssets, int _SearchTypeID, int assettypeid, ref string searchWord)
        {
            return getParameters(noAssets, _SearchTypeID, 0, assettypeid, 0, 0, 0, 0, ref searchWord);
        }

        public static List<string> getParameters(bool noAssets, int _SearchTypeID, int boxid, int assettypeid, int dataid, int catid, int subcatid, int cat2id, ref string searchword)
        {
            while (searchword.IndexOf(" AND ", StringComparison.CurrentCulture) >= 0 && searchword.Length > searchword.IndexOf(" AND ", StringComparison.CurrentCulture) + 5)
            {
                searchword = string.Format("{0}%{1}",
                    searchword.Substring(0, searchword.IndexOf(" AND ", StringComparison.CurrentCulture)),
                    searchword.Substring(searchword.IndexOf(" AND ", StringComparison.CurrentCulture) + 5));
            }

            object[] prmvalue = { dataid, catid, subcatid, cat2id, 0, 0, 0, 0, 0, 0, null, null, 0, "" };
            string[] prmtypes = { "int", "int", "int", "int", "ints", "int", "int", "int", "int", "int", "DateTime", "string", "ints", "string" };
            bool[] prminc = { dataid > 0, catid > 0, subcatid > 0, cat2id > 0, false, false, false, false, false, false, false, false, false, false };

            if (searchword.Contains(":"))
            {
                var newSearch = "";
                var seg = searchword.Split(' ');
                foreach (var item in seg)
                {
                    if (item.Contains(':'))
                    {
                        var prmindex = prmentry.FindIndex(f => item.StartsWith(f, StringComparison.CurrentCultureIgnoreCase));
                        if (prmindex > -1)
                        {
                            switch (prmtypes[prmindex])
                            {
                                case "int":
                                case "ints":
                                    var clause = item.Substring(item.IndexOf(':') + 1);
                                    var lit = false;
                                    var aid = 0;
                                    if (prmtypes[prmindex] == "ints" && (clause.Contains('*') || clause.Contains(',')))
                                    {
                                        string[] list = { "", "" };
                                        foreach (var ta in clause.Split(','))
                                        {
                                            var talgoid = ta;
                                            if (talgoid.StartsWith("(") && talgoid.EndsWith(")")) lit = true; else lit = false;
                                            if (lit) talgoid = talgoid.Substring(1, talgoid.Length - 2);
                                            if (prmentry[prmindex] == "algo" && talgoid.EndsWith("*") && talgoid.Length > 1)
                                            {
                                                var tas = talgoid.Substring(0, talgoid.Length - 1);
                                                if (int.TryParse(tas, out aid))
                                                {
                                                    var index = aid >= 0 ? 0 : 1;
                                                    var xn = DataAccess.getDataNode("usp_getalgos", new[] {
                                                        "@algo_list", "(" + Math.Abs(aid) + ")"
                                                    }, false);
                                                    foreach (XmlNode nc in xn.SelectNodes("Table/algoid"))
                                                    {
                                                        if (list[index] != "") list[index] += ",";
                                                        list[index] += nc.InnerText;
                                                    }
                                                }
                                            }
                                            else if (int.TryParse(talgoid, out aid))
                                            {
                                                var index = aid >= 0 || lit ? 0 : 1;
                                                if (list[index] != "") list[index] += ",";
                                                list[index] += lit ? aid : Math.Abs(aid);
                                            }
                                        }

                                        if (list[0] != "" || list[1] != "")
                                        {
                                            prminc[prmindex] = true;
                                            prmvalue[prmindex] = list[0];
                                            if (list[1] != "") prmvalue[prmindex] += "***NEG***" + list[1];
                                        }
                                    }
                                    else if (assettypeid == prmindex - 3 && assettypeid <= 5 && clause.Contains(','))
                                    {
                                        prminc[prmindex] = true;
                                        prmvalue[prmindex] = "***INC***" + clause;
                                    }
                                    else
                                    {
                                        if (clause.StartsWith("(") && clause.EndsWith(")")) lit = true; else lit = false;
                                        if (lit) clause = clause.Substring(1, clause.Length - 2);
                                        if (int.TryParse(clause, out aid))
                                        {
                                            prminc[prmindex] = true;
                                            prmvalue[prmindex] = (aid < 0 && !lit ? "***NEG***" : "") + (lit ? aid : Math.Abs(aid));
                                        }
                                    }

                                    continue;
                                case "DateTime":
                                    var dt = DateTime.MinValue;
                                    if (DateTime.TryParse(item.Substring(item.IndexOf(':') + 1), out dt))
                                    {
                                        prminc[prmindex] = true;
                                        prmvalue[prmindex] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                    }

                                    continue;
                                case "string":
                                    var s = item.Substring(item.IndexOf(':') + 1);
                                    if (s.Length > 0)
                                    {
                                        prminc[prmindex] = true;
                                        prmvalue[prmindex] = "%" + s + "%"; ;
                                    }

                                    continue;
                            }
                        }
                    }
                    if (newSearch != "") newSearch += " ";
                    newSearch += item;
                }
                searchword = newSearch;
            }

            var sendSearch = "@@@###@@@";

            if (!noAssets) sendSearch = ((_SearchTypeID & 1) == 1 ? "%" : "") + searchword + ((_SearchTypeID & 2) == 2 ? "%" : "");
            if (!searchword.StartsWith("%") && !searchword.StartsWith("Regex:")) sendSearch = System.Text.RegularExpressions.Regex.Replace(sendSearch, @"(?:\[)", @"[${0}]");

            CurrentSearchSql = sendSearch;

            var prms = new List<string>(new[] { "@boxid", boxid.ToString(), "@assettypeid", assettypeid.ToString(), "@searchword", sendSearch });
            for (var i = 0; i < prmentry.Count; i++)
            {
                if (prminc[i])
                {
                    prms.Add(prmname[i]);
                    prms.Add(prmvalue[i].ToString());
                }
            }
            return prms;
        }

        public static void SetSearch(string searchWord)
        {
            if (searchWord.StartsWith("Regex:")) CurrentSearch = searchWord.Substring(6);
            else
            {
                if (searchWord.StartsWith("%"))
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(searchWord, @"(?:\%)", @"\w*");
                else
                {
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(searchWord, @"(?:\\|\[|\^|\$|\.|\||\?|\*|\+|\(|\))", @"\${0}");
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(CurrentSearch, @"(?:\%)", @"(?s:.*?)");
                }
                CurrentSearch = System.Text.RegularExpressions.Regex.Replace(CurrentSearch, @"(?:[_])", @".{1}");
            }
        }

        public static void SetSearch(string searchWord, bool matchCase, bool wholeWord, bool useWildcards)
        {
            if (useWildcards)
            {
                CurrentSearch = System.Text.RegularExpressions.Regex.Replace(searchWord, @"(?:\\|\[|\^|\$|\.|\||\+|\(|\)|\%)", @"\${0}");
                if (wholeWord)
                {
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(CurrentSearch, @"\?", @"\w");
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(CurrentSearch, @"\*", @"\w+?");
                }
                else
                {
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(CurrentSearch, @"\?", @".");
                    CurrentSearch = System.Text.RegularExpressions.Regex.Replace(CurrentSearch, @"\*", @".+?");
                }
            }
            else
            {
                SetSearch(searchWord);
            }

            if (wholeWord) CurrentSearch = @"\b" + CurrentSearch + @"\b";
            if (matchCase) CurrentSearch = @"(?-i)" + CurrentSearch;
        }

        public void SetButtons(bool enabled)
        {
            for (var i = 0; i < 5; i++)
            {
                ba[i].IsEnabled = enabled;
                be[i].IsEnabled = enabled;
                bd[i].IsEnabled = enabled;
            }

            btnList5.IsEnabled = enabled;
        }

        public void SetButtons()
        {
            if (!Window1.IsReviewerOrEditor)
            {
                SetButtons(true);
            }

            if (Form != null) if (AssetTypeId == 4 && !Window1.EditTranslation) btnEdit3.IsEnabled = false; else btnEdit3.IsEnabled = true;
            if (Form != null) if (!Window1.IsReviewerOrEditor && AssetTypeId == 4 && lb[1].SelectedIndex > -1) btnTitle2.Visibility = Visibility.Visible; else btnTitle2.Visibility = Visibility.Collapsed;

            if (!Window1.PriorityEnabled)
            {
                btnUp2.IsEnabled = false;
                btnDown2.IsEnabled = false;
                btnUp3.IsEnabled = false;
                btnDown3.IsEnabled = false;
            }
            else
            {
                if (!Window1.IsReviewerOrEditor && lb[1].SelectedIndex > 0) btnUp2.IsEnabled = true; else btnUp2.IsEnabled = false;
                if (!Window1.IsReviewerOrEditor && lb[1].SelectedIndex > -1 && lb[1].SelectedIndex < lb[1].Items.Count - 1) btnDown2.IsEnabled = true; else btnDown2.IsEnabled = false;
                if (!Window1.IsReviewerOrEditor && lb[2].SelectedIndex > 0) btnUp3.IsEnabled = true; else btnUp3.IsEnabled = false;
                if (!Window1.IsReviewerOrEditor && lb[2].SelectedIndex > -1 && lb[2].SelectedIndex < lb[2].Items.Count - 1) btnDown3.IsEnabled = true; else btnDown3.IsEnabled = false;
            }

            if (Window1.IsReviewerOrEditor || AssetTypeId == 0 || AssetTypeId == 13)
            {
                SetButtons(false);
                if (Window1.IsBuilderOrAdmin && Window1.EditTranslation || Window1.IsEditor) btnEdit5.IsEnabled = true;
            }
        }

        public void ClearAdorners()
        {
            if (_listOpen) CloseList();
            for (var i = 0; i < tb.Length; i++)
            {
                tb[i].clearAdornerLayer();
                if (_textAdorners.ContainsKey(tb[i].Name)) _textAdorners.Remove(tb[i].Name);
            }
        }

        public void SetAdorners()
        {
            string[] values = {
                filter[0] ? "Filter Data Set" : "Add or edit Data Set",
                filter[1] ? "Filter Category" : "Add or edit Category",
                filter[2] ? "Filter Category" : "Add or edit Category",
                filter[3] ? "Filter Category" : "Add or edit Category",
                "Search or add assets here"
            };
            for (var i = 0; i < tb.Length; i++)
            {
                var al = tb[i].clearAdornerLayer();
                if (al == null) continue;

                var ta = new AssetBuilder.Controls.TextAdorner(tb[i], values[i], Colors.LightGray);
                if (_textAdorners.ContainsKey(tb[i].Name)) _textAdorners[tb[i].Name] = ta;
                else _textAdorners.Add(tb[i].Name, ta);
                ta.Text = tb[i].Text == "" ? "" : " ";
                al.Add(ta);
            }
        }

        public void UpdateAdorner(TextBox atb)
        {
            if (_textAdorners.ContainsKey(atb.Name))
            {
                _textAdorners[atb.Name].Text = atb.Text == "" ? "" : " ";
            }
        }

        public void AddNew()// TODO: To ViewModel
        {
            if (!CanCreate()) return;

            ResetTranslation();
            var ac = LoadAsset("new");
            if (ac != null)
            {
                if (textBox5.Text != "") ac.expert.InnerText = textBox5.Text;
                ac.setNew();
            }
        }

        public void ResetTranslation()
        {
            if (Window1.EditTranslation || Window1.ShowTranslation)
            {
                Form.rtbTranslation.IsChecked = false;
                Form.rtbShowTranslation.IsChecked = false;
                Form.Change_ShowTranslation(null, null);
            }
        }

        public void DeleteAssets(int box)// TODO: To ViewModel
        {
            if (lb[box - 1].SelectedIndex > -1)
            {
                for (var cc = box + 1; cc < 6; cc++)
                {
                    if (cc != 4 && lb[cc - 1].Items.Count > 0)
                    {
                        MessageBox.Show("Cannot delete, as this asset has child assets");
                        return;
                    }
                }

                var doc = GetAssetXml("delete");
                AddCategories(doc);
                doc.DocumentElement.AddAttribute("boxid", box.ToString());
                doc.DocumentElement.AddAttribute("assettype", AssetTypeId.ToString());

                foreach (var item in lb[box - 1].SelectedItems)
                {
                    var li = item as ListItem;
                    var add = doc.DocumentElement.AddElement("Delete", "");
                    if (AssetTypeId == 12) add.AddAttribute("id", li.Value);
                    else add.AddAttribute("id", li.ID.ToString());
                }

                var prms = new List<string>(new[] { "@xml", doc.OuterXml });
                if (AssetTypeId == 12) prms.AddRange(new[] { "@AssetTypeID", "12" });

                var xn = DataAccess.getData("ab_UpdateAsset", prms.ToArray(), true);
                var result = ParseDeleteResults(xn);
                if (result)
                {
                    var mbr = MessageBox.Show("This will delete all assets selected permanently and they " +
                                              "can never be re-created with the same unique Identifiers.\r\n\r\n" +
                                              "Although none of these assets are in use there may " +
                                              "be some references to them in visio drawings\r\n\r\n" +
                                              "Are you sure you want to continue?", "Warning", MessageBoxButton.YesNo);
                    if (mbr == MessageBoxResult.Yes)
                    {
                        doc.DocumentElement.AddAttribute("confirmed", "1");
                        prms[1] = doc.OuterXml;
                        xn = DataAccess.getData("ab_UpdateAsset", prms.ToArray(), true);
                        result = ParseDeleteResults(xn);
                        foreach (XmlElement item in doc.DocumentElement.SelectNodes("Delete"))
                        {
                            Task.Run(() => SaaS.Instance.DeleteAssetFromSaas(((AssetType)AssetTypeId).ToString(), item.AttributeIntValue("id")));
                        }
                    }
                    else result = false;
                }

                if (!result) return;

                Repopulate(box);
            }
        }

        public void OpenList()
        {
            Form.listTextBox.Text = "";
            var ta = new TextAdorner(Form.listTextBox, "Please enter each individual \nasset on a new line, then \nclick “Create Assets”.", Colors.LightGray);

            if (_textAdorners.ContainsKey(Form.listTextBox.Name)) _textAdorners[Form.listTextBox.Name] = ta;
            else _textAdorners.Add(Form.listTextBox.Name, ta);

            var al = Form.listTextBox.clearAdornerLayer();
            al.Add(ta);
            Form.listPanel.Visibility = Visibility.Visible;
            var da = new ThicknessAnimation
            {
                From = new Thickness(0),
                To = new Thickness(0, 0, ListWidth, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            BeginAnimation(MarginProperty, da);

            if (Form.assetCanvas.Children.Count > 0)
            {
                var f = LoadedAsset.AssetDockPanel;
                f.BeginAnimation(MarginProperty, da);
            }

            var wa = new DoubleAnimation
            {
                From = 0,
                To = ListWidth,
                Duration = da.Duration
            };
            Form.listPanel.BeginAnimation(WidthProperty, wa);

            _listOpen = true;
        }

        public void CloseList()
        {
            Form.listTextBox.clearAdornerLayer();
            var da = new ThicknessAnimation
            {
                To = new Thickness(0),
                From = new Thickness(0, 0, ListWidth, 0),
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };

            if (Form.assetCanvas.Children.Count > 0)
            {
                var f = LoadedAsset.AssetDockPanel;
                f.BeginAnimation(MarginProperty, da);
            }

            BeginAnimation(MarginProperty, da);
            Form.listPanel.Visibility = Visibility.Hidden;
            _listOpen = false;
        }

        public assetControl LoadAsset(string id)
        {
            return LoadAsset(id, "", "");
        }

        public assetControl LoadAsset(string id, string search, string fromId)
        {
            System.Diagnostics.Trace.WriteLine($"Starting loadAsset({id},{search},{fromId})");
            Window1.window.HideBrowsers();
            if (id == AssetId && LoadedAsset != null && Form.assetCanvas.Children.Count > 0) return LoadedAsset;
            var reedit = false;

            if (IsEditing)
            {
                if (Window1.AutoSave)
                {
                    if (!LoadedAsset.Save()) return null;
                    reedit = true;
                }
                else
                {
                    var dr = System.Windows.Forms.MessageBox.Show("There is another asset currently being edited. Do you want to save the changes?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (!LoadedAsset.Save())
                            return null;
                    }
                    else if (dr == System.Windows.Forms.DialogResult.Cancel) return null;
                }
            }

            var xe = DataAccess.getDataNode("ab_GetAsset", new[] {
                "@AssetTypeID",_AssetTypeId.ToString(),
                "@AssetID",id
            }, false);

            if (xe["Table"] == null) return null;

            assetControl ac = null;
            if (AssetTypeId == (int)AssetType.Title) ac = new Title(xe);
            if (AssetTypeId == (int)AssetType.Algo) ac = new AssetControls.Algo(xe);
            if (AssetTypeId == (int)AssetType.Question) ac = new Question(xe);
            if (AssetTypeId == (int)AssetType.Answer) ac = new Answer(xe);
            if (AssetTypeId == (int)AssetType.Conclusion) ac = new Conclusion(xe);
            if (AssetTypeId == (int)AssetType.Bullet) ac = new Bullet(xe);
            if (AssetTypeId == (int)AssetType.Map) ac = new Map(xe);
            if (AssetTypeId == (int)AssetType.TextAsset) ac = new TextAsset(xe);
            if (AssetTypeId == (int)AssetType.Group) ac = new Group(xe);
            if (AssetTypeId == (int)AssetType.ConclusionMap) ac = new CMAP(xe);
            if (ac == null) return ac;

            if (id != "new")
            {
                AddRecentItem(id, ac.expert.InnerText, search, fromId);
            }

            ac.Margin = new Thickness(0, 0, 0, 0);
            LoadedAsset = ac;
            AssetId = id;
            ac.cat = this;
            Form.assetCanvas.Children.Clear();
            Form.assetCanvas.Children.Add(ac);
            if (LoadedAsset != null && reedit) LoadedAsset.goEdit();
            if (Window1.AllowProperties) UpdatePropertyCounts();
            return ac;
        }

        public void FullLoadAsset(string ID)
        {
            Window1.setStatus("Accessing Data...");
            var ac = LoadAsset(ID);
            if (ac == null)
            {
                Window1.setStatus("");
                return;
            }

            var topCat = -1;
            foreach (var item in ac.cats)
            {
                var cat = ac.asset["Table"][ac.cats[item.Key]];
                if (cat == null) continue;
                var cid = int.Parse(cat.InnerText);
                if ((lb[item.Key].SelectedItem == null || (lb[item.Key].SelectedItem as ListItem).ID != cid) && item.Key > topCat) topCat = item.Key;
            }

            _noAssets = true;
            for (var i = 0; i < 4; i++)
            {
                if (i == topCat) _noAssets = false;
                if (ac.cats.ContainsKey(i))
                {
                    var cat = ac.asset["Table"][ac.cats[i]];
                    if (cat == null) continue;
                    var cid = int.Parse(ac.asset["Table"][ac.cats[i]].InnerText);
                    if (lb[i].SelectedItem == null || (lb[i].SelectedItem as ListItem).ID != cid) lb[i].SelectedValue = cid;
                }
            }

            _noAssets = false;

            if (int.TryParse(ID, out var result)) lb[4].SelectedValue = result; // TODO: To ViewModel

            Window1.setStatus("");
        }

        public void LoadAssetFromList(string id, string search, string fromId)
        {
            var textSearch = search;
            Window1.setStatus("Accessing Data...");
            CurrentSearch = @"(?<![0-9])" + fromId + @"(?![0-9])";
            var ac = LoadAsset(id, search, fromId);
            var prms = new List<string>(new[] { "@boxid", "0", "@assettypeid", _AssetTypeId.ToString(), "@searchword", search, "@count", "0" });
            if (search.StartsWith("@") && search.Contains(":"))
            {
                prms[5] = "";
                var newprms = search.Split(':');
                if (newprms.Length > 1)
                {
                    textSearch = search.Replace("@", "").Replace("id", "");
                    prms.AddRange(new[] { newprms[0], newprms[1] });
                }
            }

            Populate(prms.ToArray());
            if (ac == null)
            {
                Window1.setStatus("");
                return;
            }

            textBox5.Text = textSearch;
            lb[4].SelectedValue = int.Parse(id);// TODO: To ViewModel
            Window1.setStatus("");

            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

        public int[] GetCats()
        {
            int[] catIds = {
                             lb[0].SelectedItem == null ? 0 : (lb[0].SelectedItem as ListItem).ID,
                             lb[1].SelectedItem == null ? 0 : (lb[1].SelectedItem as ListItem).ID,
                             lb[2].SelectedItem == null ? 0 : (lb[2].SelectedItem as ListItem).ID,
                             lb[3].SelectedItem == null ? 0 : (lb[3].SelectedItem as ListItem).ID,
                             _assetListViewModel.AllAssets.Any(i => i.IsSelected) ? _assetListViewModel.AllAssets.First(i => i.IsSelected).ID : 0
                         };
            return catIds;
        }

        public XmlDocument GetAssetXml(string command, string tableName)
        {
            CreateUpdateXml(command, out var doc, out var root);
            InsertSelectedAssets(tableName, root);
            return doc;
        }

        public XmlDocument GetAssetXml(string command)
        {
            XmlDocument doc;
            XmlElement root;
            CreateUpdateXml(command, out doc, out root);
            return doc;
        }

        public XmlDocument GetAssetXml(string command, bool allListed)
        {
            CreateUpdateXml(command, out var doc, out var root);
            if (allListed) InsertAllAssets(TableNames[AssetTypeId], root); else InsertSelectedAssets(TableNames[AssetTypeId], root);
            return doc;
        }

        public XmlDocument GetAssetXml(string command, string tableName, string stringData)
        {
            CreateUpdateXml(command, out var doc, out var root);
            InsertSelectedAssets(tableName, root, stringData);
            return doc;
        }

        public static void CreateUpdateXml(string command, out XmlDocument doc, out XmlElement root)
        {
            doc = new XmlDocument();
            root = doc.CreateElement("root");
            doc.AppendChild(root);
            root.Attributes.Append(doc.CreateAttribute("command")).Value = command;
        }

        public void InsertAsset(string tableName, XmlElement root, string assetid)
        {
            var table = root.AppendChild(root.OwnerDocument.CreateElement(tableName));
            var id = root.OwnerDocument.CreateAttribute("id");
            id.Value = assetid;
            if (assetid == AssetId)
            {
                table.Attributes.Append(root.OwnerDocument.CreateAttribute("open")).Value = "True";
            }

            table.Attributes.Append(id);
        }

        public static void OpenVisio(string filename)
        {
            try
            {
                var vis = VisioInterface.GetVisio() ?? new Visio.Application();
                vis.Documents.Open(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + filename, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal object SelectItem(ListBox l, int p)
        {
            foreach (var item in l.Items)
            {
                if (item is ListItem listItem && listItem.ID == p)
                {
                    return listItem;
                }
            }

            return null;
        }

        internal void SetCategoryLanguage()
        {
            var keyStart = AssetTypeId + ":";
            if (Window1.MultiTextLanguage)
            {
                SetCategoryLanguage(_catTrans.Where(f => f.Key.StartsWith(keyStart)).Select(f => int.Parse(f.Key.Substring(keyStart.Length))));
                return;
            }

            foreach (var item in _catTrans.Where(f => f.Key.StartsWith(keyStart)))
            {
                var box = int.Parse(item.Key.Substring(keyStart.Length));
                foreach (ListItem li in lb[box - 1].Items)
                {
                    li.Language = Window1.ShowTranslation ? DataAccess.getCategoryLanguage(item.Value, li.Value, Window1.TranslationLanguage) : "";
                }
            }
        }

        internal void SetCategoryLanguage(IEnumerable<int> boxes)
        {
            var box = 0;
            var assets = boxes.SelectMany(f => lb[(box = f) - 1].Items.OfType<ListItem>()).Select(f => new { Box = box, CT = _catTrans[AssetTypeId + ":" + box], Item = f });
            var doc = new XDocument();
            XElement root;
            doc.Add(root = new XElement("root"));
            foreach (var asset in assets)
            {
                asset.Item.Language = "";
                if (Window1.ShowTranslation)
                    root.Add(new XElement("Asset", new XAttribute("CT", asset.CT), new XAttribute("Box", asset.Box), new XAttribute("ID", asset.Item.ID), new XAttribute("Text", asset.Item.Value)));
            }

            if (Window1.ShowTranslation)
            {
                var trans = DataAccess.getLanguage(int.MaxValue, doc.ToString(), Window1.TranslationLanguage);
                foreach (XmlNode item in trans.SelectNodes("Asset[@Language]"))
                {
                    var sBox = int.Parse(item.Attributes["Box"].Value);
                    var id = int.Parse(item.Attributes["ID"].Value);
                    assets.First(f => f.Box == sBox && f.Item.ID == id).Item.Language = item.Attributes["Language"].Value;
                }
            }
        }

        #endregion Public Methods

        #region Event Handlers
        private void ContextMenuLoaded(object sender, RoutedEventArgs e)
        {
            var cm = (ContextMenu)sender;
            var listBox = (ListBox)cm.PlacementTarget;

            var cat = -1;

            if (listBox.SelectedItems.Count > 0) cat = ((ListItem)listBox.SelectedItem).CategoryTypeID;

            foreach (var m in cm.Items.OfType<MenuItem>())
            {
                var item = Cast(new { ID = 0, Description = "", BoxID = 0 }, m.CommandParameter);
                m.IsChecked = item.ID == cat;
            }
        }

        private void lb_Context_Click(object sender, RoutedEventArgs e)
        {
            var item = Cast(new { ID = 0, Description = "", BoxID = 0 }, ((MenuItem)sender).CommandParameter);
            var root = GetAssetXml("setcategorytype");
            root.DocumentElement.Attributes.Append(root.CreateAttribute("AssetTypeID")).Value = AssetTypeId.ToString();
            root.DocumentElement.Attributes.Append(root.CreateAttribute("BoxID")).Value = item.BoxID.ToString();

            foreach (var listitem in lb[item.BoxID - 1].SelectedItems)
            {
                var li = (ListItem)listitem;
                var cat = root.DocumentElement.AppendChild(root.CreateElement("Category"));
                cat.Attributes.Append(root.CreateAttribute("ID")).Value = li.ID.ToString();
                if (item.ID != 0) cat.Attributes.Append(root.CreateAttribute("CategoryTypeID")).Value = item.ID.ToString();
                li.CategoryTypeID = item.ID;
            }

            var xn = DataAccess.getData("ab_UpdateAsset", new[] {
                    "@xml", root.OuterXml
                }, true);
        }

        private void textBox5_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetButtons();
            if (_timer != null) _timer.Stop();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(TimerTick);
            _timer.Start();
            UpdateAdorner(sender as TextBox);

            if (LoadedAsset == null) return;

            var setSearch = SanitiseSearch((sender as TextBox).Text);
            SetSearch(setSearch);
            foreach (var item in LoadedAsset.TextChildren)
            {
                if (CurrentSearch == "" && !item.Key.EndsWith("Language"))
                {
                    NLExtensions.clearAdornerLayer(item.Value);
                }
                else if (item.Value.IsVisible)
                {
                    NLExtensions.textBox_AdornAndValidate(item.Value, null);
                }
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer = null;
            Populate(null as ListBox);
        }

        private static int GetBox(object sender)
        {
            var b = sender as Button;
            var box = int.Parse(b.CommandParameter.ToString());
            return box;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            var box = GetBox(sender);
            lb[box - 1].UnselectAll();
            tb[box - 1].Text = "";
            if (box == 1)
            {
                _lastBoxId = 0;
                Populate(0);
            }
            else Populate(lb[box - 1]);
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            var box = GetBox(sender);
            if (box == 5)
            {
                AddNew();

            }
            else if (tb[box - 1].Text != "")
            {
                var doc = GetAssetXml("add");
                AddCategories(doc);
                doc.DocumentElement.AddAttribute("boxid", box.ToString());
                doc.DocumentElement.AddAttribute("assettype", AssetTypeId.ToString());
                var add = doc.DocumentElement.AddElement("Add", tb[box - 1].Text);
                var priority = 1;
                if (lb[box - 1].Items.Count > 0)
                    priority = (lb[box - 1].Items[lb[box - 1].Items.Count - 1] as ListItem).Priority + 1;
                add.AddAttribute("Priority", priority.ToString());
                var xn = DataAccess.getData("ab_UpdateAsset", new[] {
                    "@xml", doc.OuterXml
                }, true);
                if (xn == null || xn.Name.LocalName.Contains("Error"))
                {
                    throw new Exception("category failed, " + xn.Value);
                }

                Repopulate(box);
                tb[box - 1].Text = "";
            }
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var box = GetBox(sender);

            DeleteAssets(box);
        }

        private void ListClick(object sender, RoutedEventArgs e)
        {
            if (!CanCreate()) return;

            if (Form.listPanel.Visibility == Visibility.Visible)
            {
                CloseList();
            }
            else
            {
                OpenList();
            }
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            var box = GetBox(sender);
            if (lb[box - 1].SelectedItem == null) // TODO: To ViewModel
            {
                System.Windows.Forms.MessageBox.Show("Must select an entry from the list to edit", "Error");
                return;
            }
            if (box == 5)
            {
                if (LoadedAsset == null)
                {
                    LoadAsset(_assetListViewModel.AllAssets.First(x => x.IsSelected).ID.ToString());
                }

                if (LoadedAsset.asset["Table"]["Locked"].InnerText == "true") return;

                LoadedAsset.goEdit();
            }
            else
            {
                var li = lb[box - 1].SelectedItem as ListItem;
                var ct = AssetTypeId + ":" + box;
                if (Window1.EditTranslation && _catTrans.ContainsKey(ct))
                {
                    var doc = new XmlDocument();
                    doc.AppendChild(doc.CreateElement("root"));
                    var add = doc.DocumentElement.AddElement(_catFields[ct], tb[box - 1].Text);
                    DataAccess.setLanguage(_catTrans[ct], li.Value, doc, Window1.TranslationLanguage);

                    Repopulate(box);
                    lb[box - 1].SelectedItem = SelectItem(lb[box - 1], li.ID);
                    lb[box - 1].Focus();
                }
                else if (tb[box - 1].Text != "")
                {
                    var doc = GetAssetXml("edit");
                    doc.DocumentElement.AddAttribute("boxid", box.ToString());
                    doc.DocumentElement.AddAttribute("assettype", AssetTypeId.ToString());
                    var add = doc.DocumentElement.AddElement("Edit", tb[box - 1].Text);

                    add.AddAttribute("Priority", li.Priority.ToString());
                    add.AddAttribute("ID", li.ID.ToString());
                    add.AddAttribute("OldValue", li.Value);

                    var xn = DataAccess.getData("ab_UpdateAsset", new[] {
                        "@xml", doc.OuterXml
                    }, true);
                    if (xn == null || xn.Name.LocalName.Contains("Error"))
                    {
                        throw new Exception("category failed, " + xn.Value);
                    }

                    Repopulate(box);
                    lb[box - 1].SelectedItem = SelectItem(lb[box - 1], li.ID);
                    lb[box - 1].Focus();
                }
            }
        }

        private void LoadAsset(object sender, SelectionChangedEventArgs e)
        {
            if (Form == null) return;

            if (_listOpen) CloseList();

            if (_assetListViewModel.AllAssets.Any(i => i.IsSelected))
            {
                imgGrab.Visibility = Visibility.Visible;
            }
            else
            {
                imgGrab.Visibility = Visibility.Hidden;
            }

            SetButtons();

            var items = _assetListViewModel.AllAssets.Where(i => i.IsSelected).ToList();
            if (items.Count == 1)
            {
                var selected = items[0];
                listBox5.ScrollIntoView(selected);

                if (Keyboard.Modifiers == ModifierKeys.None)
                {
                    if (AssetTypeId == 0 || AssetTypeId == 12)
                    {
                        LoadAsset(selected.Value);
                    }
                    else
                    {
                        LoadAsset(selected.ID.ToString());
                    }
                }
            }
        }

        private void listBox5_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Form == null)
            {
                if (LoadedAsset is Conclusion)
                {
                    var p = e.GetPosition(listBox5);
                    var dp = (DependencyObject)listBox5.InputHitTest(p);
                    var li = GetListItem(dp);
                    if (li != null)
                    {
                        var con = LoadedAsset as Conclusion;
                        if (con.cat.IsEditing) con.addBulletToEnd(GetItems());
                    }
                }

                return;
            }

            if (!_assetListViewModel.AllAssets.Any(i => i.IsSelected)) return;

            var id = AssetTypeId == 12 ? ((ListItem)lb[4].SelectedItem).Value : lb[4].SelectedValue.ToString();// TODO: To ViewModel
            FullLoadAsset(id);
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Window1.setStatus("Accessing Data...");
            var cm = sender as ContextMenu;
            var menuItems = new Dictionary<string, MenuItem>();
            foreach (MenuItem item in cm.Items)
            {
                item.IsEnabled = false;
                menuItems.Add(item.Header.ToString(), item);
            }

            var xn = DataAccess.getData("ab_UpdateAsset", new[] {
                "@xml", GetAssetXml("context", TableNames[AssetTypeId]).OuterXml
            }, false);
            var mi = from item in xn.Elements() select item.Element("MenuItem").Value;
            foreach (var item in mi)
            {
                var split = item.Split('|');
                if (menuItems.ContainsKey(split[0]))
                {
                    menuItems[split[0]].IsEnabled = true;
                }

                if (split.Length > 1)
                {
                    menuItems[split[0]].CommandParameter = string.Join("|", split.Skip(1));
                }
            }

            if (_assetListViewModel.AllAssets.Any(x => x.IsSelected)) menuItems["Copy"].IsEnabled = true;

            Window1.clearStatus();
        }

        private void lb_DragOver(object sender, DragEventArgs e)
        {
            if (!Window1.CanMoveAssets) return;

            Window1.setStatus("");
            var listbox = sender as ListBox;
            var p = e.GetPosition(listbox);
            var dp = (DependencyObject)listbox.InputHitTest(p);
            var li = GetListItem(dp);
            if (li != null)
            {
                listbox.SelectedItem = li.Content;
                var s = "";
                for (var i = 0; i < 4; i++)
                {
                    s += (lb[i].SelectedValue == null ? "x" : lb[i].SelectedValue.ToString()) + " ";
                }
            }
            else
            {
                var sb = GetScrollbar(dp);
                if (sb != null)
                {
                    if (sb.ActualHeight - p.Y < 20) listbox.SelectedIndex++;
                    else if (p.Y < 20 && listbox.SelectedIndex > 0) listbox.SelectedIndex--;
                }
            }
        }

        private void lb_Drop(object sender, DragEventArgs e)
        {
            if (!Window1.CanMoveAssets) return;

            const string message = "You must select all available categories";
            var ret = lb[0].SelectedValue == null;
            switch (AssetTypeId)
            {
                case 2:
                case 3:
                case 4:
                    if (lb[1].SelectedValue == null) ret = true;
                    if (lb[2].SelectedValue == null) ret = true;
                    if (lb[3].SelectedValue == null) ret = true;
                    break;
                case 5:
                    if (lb[3].SelectedValue == null) ret = true;
                    break;
            }

            if (ret)
            {
                ErrorMessage(message);
                return;
            }

            Window1.setStatus("Updating Data...");
            var doc = GetAssetXml("move", TableNames[AssetTypeId], e.Data.GetData(typeof(string)).ToString());
            AddCategories(doc);
            var doIt = false;
            if (!Window1.DisableComments)
            {
                var pw = new PromptWindow(false)
                {
                    Owner = Form
                };
                Form.disableForm();
                var res = pw.ShowDialog();
                Form.enableForm();
                if (res == true)
                {
                    doc.DocumentElement.SetAttribute("comment", pw.Comment);
                }

                doIt = res == true;
            }

            if (Window1.DisableComments || doIt)
            {
                DataAccess.getData("ab_UpdateAsset", new[] {
                    "@xml", doc.OuterXml
                }, true);
                if(LoadedAsset != null && doc.DocumentElement.SelectSingleNode(string.Format("*[@id={0}]", LoadedAsset.AssetID)) != null)
                {
                    LoadedAsset.UpdateCategories(doc.DocumentElement);
                }
                Populate(null as ListBox);
            }

            Window1.setStatus("");
        }

        private void lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListBox l) || l.SelectedItem == null) return;

            var li = l.SelectedItem as ListItem;
            var idx = Grid.GetColumn(l);
            if (Window1.EditTranslation && !string.IsNullOrEmpty(li.Language))
            {
                tb[idx].Text = li.Language;
            }
            else
            {
                tb[idx].Text = li.Value;
            }
        }

        private void btnTitle_Click(object sender, RoutedEventArgs e)
        {
            var doc = GetAssetXml("title");
            doc.DocumentElement.AddAttribute("catid", (lb[1].SelectedItem as ListItem).ID.ToString());
            doc.DocumentElement.AddAttribute("CategoryTypeID", (lb[1].SelectedItem as ListItem).CategoryTypeID.ToString());
            Form.setConclusionTitle(doc);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            var box = GetBox(sender);
            if (lb[box - 1].SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("Must select an entry from the list to edit", "Error");
            }
            else if (lb[box - 1].SelectedIndex <= 0)
            {
                System.Windows.Forms.MessageBox.Show("Item is already at the top of the list", "Error");
            }
            else
            {
                ChangePriority(box, lb[box - 1].Items[lb[box - 1].SelectedIndex - 1] as ListItem,
                    lb[box - 1].SelectedItem as ListItem, (lb[box - 1].SelectedItem as ListItem).ID);
            }
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            var box = GetBox(sender);
            if (lb[box - 1].SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("Must select an entry from the list to edit", "Error");
            }
            else if (lb[box - 1].SelectedIndex >= lb[box - 1].Items.Count - 1)
            {
                System.Windows.Forms.MessageBox.Show("Item is already at the bottom of the list", "Error");
            }
            else
            {
                ChangePriority(box, lb[box - 1].SelectedItem as ListItem,
                    lb[box - 1].Items[lb[box - 1].SelectedIndex + 1] as ListItem,
                    (lb[box - 1].SelectedItem as ListItem).ID);
            }
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetButtons();
            var textBox = sender as TextBox;
            UpdateAdorner(textBox);
            if (textBox != null && textBox.Name.StartsWith("textBox"))
            {
                var boxId = int.Parse(textBox.Name.Substring(7));
                if (boxId < 5)
                {
                    FilterList(boxId);
                }
            }
        }

        private void imgGrab_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(listBox5, GetItems(), DragDropEffects.Move); // TODO: To ViewModel
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            SetAdorners();
        }

        private void listBox5_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clickedAsset = null;
            if (!(sender is ListBox listbox)) return;

            var p = e.GetPosition(listbox);
            var dp = (DependencyObject)listbox.InputHitTest(p);
            var li = GetListItem(dp);
            if (li != null && li.Content is ListItem)
            {
                _clickedAsset = li.Content as ListItem;
            }

            if (Keyboard.Modifiers != ModifierKeys.None || listbox.SelectedItems.Count == 1) return;

            if (li != null && listbox.SelectedItems.Contains(li.Content))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }
        }

        private void listBox5_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;
            if (_clickedAsset == null || Form == null) return;

            if (_listOpen) CloseList();

            imgGrab.Visibility = _assetListViewModel.AllAssets.Any(x => x.IsSelected) ? Visibility.Visible : Visibility.Hidden;

            SetButtons();
            listBox5.SelectedItem = _clickedAsset; // TODO: To ViewModel
        }

        private void ShowAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _showAll = true;
            Populate(null as ListBox);
        }

        private void Filter_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.Name.StartsWith("filter"))
            {
                var boxId = int.Parse(toggleButton.Name.Substring(6));
                if (boxId < 5)
                {
                    filter[boxId - 1] = (bool)toggleButton.IsChecked;
                    FilterList(boxId);
                }
            }

            SetAdorners();
        }

        private void ExecutedCopy(object sender, ExecutedRoutedEventArgs e)
        {
            Window1.window.Copy();
        }

        private void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listOpen) CloseList();
            var sel = sender as ListBox;
            if (sel.SelectedItem != null)
            {
                sel.ScrollIntoView(sel.SelectedItem);
                Populate(sel);
            }
        }

        private void listBox5_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !Window1.IsEditor && (AssetTypeId <= 5 && AssetTypeId > 0 || AssetTypeId == 11))
            {
                var p = e.GetPosition(listBox5);
                var li = GetListItem((DependencyObject)listBox5.InputHitTest(p)); // TODO: To ViewModel
                if (li != null)
                {
                    if (p.X > listBox5.ActualWidth - 20) return;
                    if (p.Y > listBox5.ActualHeight - 20) return;
                    DragDrop.DoDragDrop(listBox5, GetItems(), DragDropEffects.Move);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi != null && mi.Header.ToString() == "Open Visio")
            {
                var filename = mi.CommandParameter.ToString();
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    OpenVisio(filename);
                }));
                return;
            }
            var cp = mi?.CommandParameter.ToString().Split(',');
            if (cp[0] == "20")
            {
                Form.Copy();
                return;
            }
            if (IsEditing)
            {
                var dr = System.Windows.Forms.MessageBox.Show("There is an asset currently being edited. Do you want to save the changes?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!LoadedAsset.Save())
                        return;
                }
                else if (dr == System.Windows.Forms.DialogResult.Cancel) return;
            }
            Window1.setStatus("Accessing Data...");
            SaveCats();

            if (Form != null) Form.assetCanvas.Children.Clear();

            var selectedItem = _assetListViewModel.AllAssets.FirstOrDefault(x => x.IsSelected);
            if (selectedItem == null) return;

            if (Window1.SearchTranslation) Form.DisableSearchTranslation();
            var id = selectedItem.ID;
            var searchWord = "";
            if (int.Parse(cp[0]) < 15)
            {
                var prms = new[] {
                    "@boxid", cp[0],
                    "@assettypeid", AssetTypeId.ToString(),
                    "@searchword", id.ToString()
                };
                Populate(prms);
                textBox5.Text = "";
            }
            else
            {
                searchWord = prmentry[AssetTypeId + 3] + ":" + id;
                Populate(0, int.Parse(cp[1]), searchWord);
            }

            _AssetTypeId = int.Parse(cp[1]);
            textBox5.Text = searchWord;
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }

            Window1.RadioToggle(Form.assetGroup, _AssetTypeId);
            Window1.clearStatus();
        }

        #endregion Event Handlers

        #region Helper Methods

        private static int[] ResetSearch()
        {
            return Enumerable.Range(0, Atc).Select(f => 3).ToArray();
        }

        private T Cast<T>(T Typeholder, object x)
        {
            return (T)x;
        }

        private void SaveCats()
        {
            _cats[_AssetTypeId] = GetCats();
            _text[_AssetTypeId] = GetText();
            _open[_AssetTypeId] = Form != null && Form.assetCanvas.Children.Count > 0;
            _search[_AssetTypeId] = _searchTypeId;
            _lastBoxId = 0;
        }

        private void Populate(int boxId)
        {
            Populate(boxId, AssetTypeId, textBox5.Text);
        }

        private void Populate(ListBox sel)
        {
            var boxId = 0;
            int[] cats = { 0, 0, 0, 0 };
            for (var i = 0; i < lb.Length - 1; i++)
            {
                if (lb[i] == sel) boxId = i + 1;
                if (i == 0 && boxId == 1)
                {
                    lb[1].UnselectAll();
                    lb[2].UnselectAll();
                    lb[3].UnselectAll();
                }
                else if (i == 1 && boxId == 2) lb[2].UnselectAll();
                if (lb[i].SelectedItem != null) cats[i] = (lb[i].SelectedItem as ListItem).ID;
            }

            if (boxId == 0) boxId = _lastBoxId;

            _lastBoxId = boxId;
            Populate(boxId, _AssetTypeId, cats, textBox5.Text);
        }

        private void Populate(int boxId, int assetTypeId, string searchWord)
        {
            Populate(boxId, assetTypeId, 0, 0, 0, 0, searchWord);
        }

        private void Populate(int boxId, int assetTypeId, int[] cats, string searchWord)
        {
            Populate(boxId, assetTypeId, cats[0], cats[1], cats[2], cats[3], searchWord);
        }

        private void Populate(int boxId, int assetTypeId, int dataId, int catId, int subCatId, int cat2Id, string searchWord)
        {
            var parameters = getParameters(_noAssets, _searchTypeId, boxId, assetTypeId, dataId, catId, subCatId, cat2Id, ref searchWord);

            SetSearch(searchWord);
            if (_showAll)
            {
                SetShowAll(parameters, ref _showAll);
            }

            Populate(parameters.ToArray());
            SetButtons();
        }

        private void Populate(string[] paramaters)
        {
            IEnumerable<string> translationIds = null;
            if (Window1.SearchTranslation)
            {
                var oSearch = "";
                if (paramaters.Contains("@searchword")) oSearch = paramaters[Array.IndexOf(paramaters, "@searchword") + 1].Replace("%", "");
                paramaters = SetShowAll(paramaters.ToList(), ref _showAll);
                paramaters = ResetSearch(paramaters.ToList());
                var searchTranslation = DataAccess.searchLanguage(AssetTypeId, SearchTypeId, Window1.TranslationLanguage, oSearch);
                translationIds = from st in searchTranslation.SelectNodes("ID").OfType<XmlNode>() select st.InnerText;
            }

            var showCount = 100;
            if (paramaters.Contains("@count"))
            {
                if (int.TryParse(paramaters[Array.IndexOf(paramaters, "@count") + 1], out var tc) && tc > 0) showCount = tc;
            }

            var xe = GetAssets(paramaters);
            var xn = from x in xe.Elements() select x;
            var boxes = new HashSet<int>();
            foreach (var x in xn)
            {
                var box = GetInt(x, "BoxID");
                if (box < 1 || box > 5) continue;
                var priority = GetInt(x, "Priority");
                var catTypeId = GetInt(x, "CategoryTypeID");
                var notInUse = GetInt(x, "NotInUse");
                AuditItem ai = null;
                if (box == 5 && x.Element("AuditText") != null)
                {
                    ai = DataAccess.JsonDeSerialize<AuditItem>(x.Element("AuditText")?.Value);
                    ai.Date = DateTime.Parse(x.ElementValue("AuditDate"));
                    ai.User = x.ElementValue("AuditUser");
                }

                if (x.Element("ID") == null)
                {
                    if (box == 5) // listBox5 - ViewModel
                    {
                        _assetListViewModel.AllAssets.Clear();
                    }
                    else
                    {
                        lb[box - 1].Items.Clear();
                    }

                    boxes.Add(box);
                }
                else
                {
                    if (box < 5 || translationIds == null || translationIds.Contains(x.Element("ID").Value) || AssetTypeId == 0 && translationIds.Contains(string.Concat(x.Element("Description").Value.Split(System.IO.Path.GetInvalidFileNameChars())).TrimStart()))
                    {
                        var desc = x.Element("Description").Value;
                        if (desc == "") desc = "<-- blank -->";
                        if (Window1.MultiTextLanguage)
                        {
                            var item = new ListItem
                            {
                                ID = GetInt(x, "ID"),
                                Value = desc,
                                Priority = priority,
                                CategoryTypeID = catTypeId,
                                NotInUse = notInUse,
                                Audit = ai
                            };
                            if (box == 5)
                            {
                                _assetListViewModel.AllAssets.Add(item);
                            }
                            else
                            {
                                lb[box - 1].Items.Add(item);
                            }
                        }
                        else
                        {
                            var ct = AssetTypeId + ":" + box;
                            if (Window1.ShowTranslation && _catTrans.ContainsKey(ct))
                            {
                                var lang = DataAccess.getCategoryLanguage(_catTrans[ct], desc, Window1.TranslationLanguage);
                                var hidden = filter[box - 1] && Window1.SearchTranslation && lang.IndexOf(tb[box - 1].Text, StringComparison.OrdinalIgnoreCase) == -1
                                             || filter[box - 1] && !Window1.SearchTranslation && desc.IndexOf(tb[box - 1].Text, StringComparison.OrdinalIgnoreCase) == -1;
                                var item = new ListItem
                                {
                                    ID = GetInt(x, "ID"),
                                    Value = desc,
                                    Priority = priority,
                                    Language = lang,
                                    Hidden = hidden,
                                    CategoryTypeID = catTypeId,
                                    NotInUse = notInUse,
                                    Audit = ai
                                };
                                if (box == 5) // listBox5 -> ViewModel
                                {
                                    _assetListViewModel.AllAssets.Add(item);
                                }
                                else
                                {
                                    lb[box - 1].Items.Add(item);
                                }
                            }
                            else
                            {
                                var hidden = filter[box - 1] && desc.IndexOf(tb[box - 1].Text, StringComparison.OrdinalIgnoreCase) == -1;
                                var item = new ListItem
                                {
                                    ID = GetInt(x, "ID"),
                                    Value = desc,
                                    Priority = priority,
                                    Hidden = hidden,
                                    CategoryTypeID = catTypeId,
                                    NotInUse = notInUse,
                                    Audit = ai
                                };
                                if (box == 5) // listBox5 -> ViewModel
                                {
                                    _assetListViewModel.AllAssets.Add(item);
                                }
                                else
                                {
                                    lb[box - 1].Items.Add(item);
                                }
                            }
                        }
                    }
                }
            }

            if (Window1.MultiTextLanguage)
            {
                SetCategoryLanguage(boxes.Where(f => Window1.ShowTranslation && _catTrans.ContainsKey(AssetTypeId + ":" + f)));
            }

            if (_assetListViewModel.AllAssets.Count == showCount)
            {
                assetCount.Foreground = Brushes.Red;
                assetCount.Text = " - ???";
                imgWarning.Visibility = Visibility.Visible;
                ShowAll.Visibility = Visibility.Visible;
            }
            else
            {
                assetCount.Foreground = Brushes.Blue;
                if (!_assetListViewModel.AllAssets.Any()) assetCount.Text = "";
                else assetCount.Text = " - " + _assetListViewModel.AllAssets.Count;
                imgWarning.Visibility = Visibility.Hidden;
                ShowAll.Visibility = Visibility.Hidden;
            }

            if (_assetListViewModel.AllAssets.Any()) listBox5.ScrollIntoView(_assetListViewModel.AllAssets[0]);
        }

        private static string[] SetShowAll(List<string> parameters, ref bool showAll)
        {
            if (parameters.Contains("@count"))
            {
                parameters[parameters.IndexOf("@count") + 1] = "0";
            }
            else
            {
                parameters.AddRange(new[] { "@count", "0" });
            }

            showAll = false;
            return parameters.ToArray();
        }

        private static string[] ResetSearch(List<string> parameters)
        {
            if (parameters.Contains("@searchword"))
            {
                parameters[parameters.IndexOf("@searchword") + 1] = "";
            }
            else
            {
                parameters.AddRange(new[] { "@searchword", "" });
            }

            return parameters.ToArray();
        }

        private static XElement GetAssets(string[] parameters)
        {
            Window1.setStatus("Getting Assets...");
            var negSearch = false;
            var incSearch = false;
            string[] inc = null;
            var showAll = false;
            if (parameters.Any(f => f.Contains("***INC***"))) parameters = SetShowAll(parameters.ToList(), ref showAll);
            if (parameters.Any(f => f.Contains("***NEG***"))) parameters = SetShowAll(parameters.ToList(), ref showAll);

            var negprms = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Contains("***NEG***"))
                {
                    negprms[i] = parameters[i].Substring(parameters[i].IndexOf("***NEG***") + 9);
                    parameters[i] = parameters[i].Substring(0, parameters[i].IndexOf("***NEG***"));
                    if (parameters[i] == "") parameters[i - 1] = "";
                    negSearch = true;
                }
                else if (parameters[i].Contains("***INC***"))
                {
                    inc = parameters[i].Substring(parameters[i].IndexOf("***INC***") + 9).Split(',');
                    parameters[i] = parameters[i].Substring(0, parameters[i].IndexOf("***INC***"));
                    if (parameters[i] == "") parameters[i - 1] = "";
                    incSearch = true;
                }
                else negprms[i] = parameters[i];
            }

            var xe = DataAccess.getData("ab_getitems", parameters, false);
            var oi = -1;
            if ((oi = Array.IndexOf(parameters, "order")) > -1 && parameters.Length > oi + 1 && string.Compare(parameters[oi + 1], "%id%", true) == 0)
            {
                var assets = xe.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null);
                var ordered = assets.OrderBy(f => (int)f.Element("ID")).ToList();
                assets.Remove();
                xe.Add(ordered);
            }

            if (incSearch)
            {
                var negs = inc.Where(f => f.StartsWith("-")).Select(f => f.Substring(1)).ToArray();
                inc = inc.Where(f => !f.StartsWith("-") && !string.IsNullOrWhiteSpace(f)).ToArray();
                if (negs.Any())
                    xe.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null && negs.Contains(f.Element("ID").Value)).Remove();
                if (inc.Any())
                    xe.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null && !inc.Contains(f.Element("ID").Value)).Remove();
            }

            if (negSearch)
            {
                Window1.setStatus("Processing data...");
                var neg = DataAccess.getData("ab_getitems", negprms, false);
                var negids = neg.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null).Select(f => f.Element("ID").Value);
                xe.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null && negids.Contains(f.Element("ID").Value)).Remove();
            }

            Window1.setStatus("");
            return xe;
        }

        private static int GetInt(XElement node, string element)
        {
            return node.Element(element) == null ? 0 : int.Parse(node.Element(element).Value);
        }

        private string GetItems()
        {
            var ids = "";
            var s = "";
            foreach (ListItem item in listBox5.SelectedItems)
            {
                if (ids != "") ids += ",";
                if (s != "") s += "$$BREAK$$";
                ids += item.ID;
                s += item.Value;
            }

            ids = string.Format("{0} In (", _idColumn[AssetTypeId]) + ids + ")";
            if (Window1.McKesson_Mode)
            {
                s += "$$BREAK$$";
                var x = "";
                foreach (ListItem item in listBox5.SelectedItems)
                {
                    var key = "";
                    var xpathkeys = _properties.Where(f => f.Key.Contains(":XPath:")).Where(f =>
                    {
                        var sa = f.Key.Split(':');
                        var asset = DataAccess.getDataNode("ab_GetAsset", "@AssetTypeID", _AssetTypeId.ToString(), "@AssetID", (item as ListItem).ID.ToString());
                        if (asset.SelectSingleNode(sa[2]) != null) return true;
                        return false;
                    });

                    if (xpathkeys.Any()) key = xpathkeys.First().Key;
                    else if (listBox4.SelectedItem != null && _properties.ContainsKey(AssetTypeId + ":" + (listBox4.SelectedItem as ListItem).Value)) key = AssetTypeId + ":" + (listBox4.SelectedItem as ListItem).Value;
                    else if (_properties.ContainsKey(AssetTypeId.ToString())) key = AssetTypeId.ToString();

                    if (x != "") x += "$$XML$$";

                    if (key != "")
                    {
                        var assetType = ((AssetType)AssetTypeId).ToString();
                        var altAssetType = "Transfer";
                        var processed = "";

                        foreach (var split in _properties[key].Split(new[] { "$$ALT$$" }, StringSplitOptions.None))
                        {
                            if (processed != "") processed += "$$ALT$$";
                            var doc = new XmlDocument();
                            doc.LoadXml(split);
                            var xn = DataAccess.getDataNode("dsp_GetProperty", "@PropertyType", assetType, "@DataID", item.ID.ToString());
                            foreach (XmlAttribute attr in doc.DocumentElement.Attributes)
                            {
                                var def = xn.SelectSingleNode(string.Format("Table[PropertyName = '{0}']", attr.Name));
                                if (def != null)
                                {
                                    attr.Value = def["PropertyValue"].InnerText;
                                }
                            }
                            foreach (XmlNode node in xn.SelectNodes("Table"))
                            {
                                XmlNode propname = node["PropertyName"];
                                if (propname != null && !string.IsNullOrWhiteSpace(propname.InnerText) && doc.DocumentElement.Attributes[propname.InnerText] == null)
                                {
                                    doc.DocumentElement.Attributes.Append(doc.CreateAttribute(propname.InnerText)).Value = node["PropertyValue"].InnerText;
                                }
                            }

                            processed += doc.OuterXml;
                            assetType = altAssetType;
                        }

                        x += processed;
                    }
                }

                s += x;
            }

            s = "(" + s + ")";
            return ids + s + (listBox2.SelectedItem != null && (listBox2.SelectedItem as ListItem).Value == "Messages" ? "(Message)" : "");
        }

        private string SanitiseSearch(string searchWord)
        {
            if (searchWord.Contains(":"))
            {
                var newSearch = "";
                var seg = searchWord.Split(' ');
                foreach (var item in seg)
                {
                    if (item.Contains(':'))
                    {
                        var prmindex = prmentry.FindIndex(f => item.StartsWith(f, StringComparison.CurrentCultureIgnoreCase));
                        if (prmindex > -1)
                        {
                            continue;
                        }
                    }

                    if (newSearch != "") newSearch += " ";

                    newSearch += item;
                }
                searchWord = newSearch;
            }

            return searchWord;
        }

        private bool CanCreate()
        {
            switch ((AssetType)AssetTypeId)
            {
                case AssetType.Algo:
                    if (lb[0].SelectedItem == null)
                    {
                        ErrorMessage("You must select a valid Data Set");
                        return false;
                    }

                    break;
                case AssetType.Question:
                case AssetType.Answer:
                case AssetType.Conclusion:
                case AssetType.Map:
                case AssetType.ConclusionMap:
                    if (lb[0].SelectedItem == null || lb[1].SelectedItem == null || lb[2].SelectedItem == null || lb[3].SelectedItem == null)
                    {
                        ErrorMessage("You must select a valid Data Set, Category, Sub Category & Category 2");
                        return false;
                    }

                    break;
                case AssetType.Bullet:
                    if (lb[0].SelectedItem == null || lb[3].SelectedItem == null)
                    {
                        ErrorMessage("You must select a valid Data Set & Category 2");
                        return false;
                    }

                    break;
            }

            return true;
        }

        private static void ErrorMessage(string p)
        {
            System.Windows.Forms.MessageBox.Show(p);
        }

        private static bool ParseDeleteResults(XElement xn)
        {
            if (xn == null || xn.Name.LocalName.Contains("Error"))
            {
                throw new Exception("category failed, " + xn.Value);
            }

            if (xn.Descendants("Failure").Any())
            {

                var rr = from rf in xn.Descendants("Failure")
                         select rf;

                var s = "The asset could not be deleted because of the following reasons:\r\n\r\n";

                foreach (string re in rr)
                {
                    s += string.Concat("\t- ", re, "\r\n");
                }

                MessageBox.Show(s);
                return false;
            }

            return true;
        }

        private void UpdatePropertyCounts()
        {
            var t = new Task(() => RunAsync());
            t.Start();
        }

        private void RunAsync()
        {
            lock (_lockObj)
            {
                var id = AssetId;
                AssetBuilder.Classes.AssetType assetType = (AssetType)LoadedAsset.assetType;
                var searchPropertyType = (assetType == AssetBuilder.Classes.AssetType.Algo ? "[AT][lr][ga][on]%" : assetType.ToString()) + ":" + AssetId;
                var xn = DataAccess.getData("dsp_SearchProperties", "@PropertyType", searchPropertyType);
                var defaultData = DataAccess.getData("dsp_GetProperty", new[] { "@PropertyType", assetType.ToString(), "@DataID", AssetId.ToString() }, true);
                var defaultCount = defaultData.Elements().Count();
                var instanceCount = xn.Elements("Table").Select(f => f.Element("DataID").Value).Distinct().Count();

                if (id == AssetId)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        if (LoadedAsset != null && LoadedAsset.bs != null)
                        {
                            LoadedAsset.bs.PropertyCount = instanceCount.ToString();
                            if (defaultCount > 0) LoadedAsset.bs.DefaultProperties = true;
                        }
                    }));
                }
            }
        }

        private void AddRecentItem(string ID, string expert, string Search, string FromID)
        {
            var ri = new recentItem(_AssetTypeId, ID, expert, Search, FromID, this);
            for (var i = 0; i < Window1.RecentAssets.Items.Count; i++)
            {
                if (Window1.RecentAssets.Items[i] is recentItem)
                {
                    var il = Window1.RecentAssets.Items[i] as recentItem;
                    if (il.AssetID == ri.AssetID && il.AssetType == ri.AssetType && il.Search == ri.Search)
                    {
                        Window1.RecentAssets.Items.RemoveAt(i--);
                    }
                }
            }

            if (Window1.RecentAssets.Items.Count > 19) Window1.RecentAssets.Items.RemoveAt(Window1.RecentAssets.Items.Count - 1);

            Window1.RecentAssets.Items.Insert(0, ri);
        }

        private string[] GetText()
        {
            string[] catText = {
                             tb[0].Text,
                             tb[1].Text,
                             tb[2].Text,
                             tb[3].Text,
                             tb[4].Text
                         };
            return catText;
        }

        private static void InsertSelectedAssets(string tableName, XmlElement root, string stringData)
        {
            var s = stringData.Split(')')[0].Split('(')[1].Split(',');
            foreach (var item in s)
            {
                var table = root.AppendChild(root.OwnerDocument.CreateElement(tableName));
                var id = root.OwnerDocument.CreateAttribute("id");
                id.Value = item;
                table.Attributes.Append(id);
            }
        }

        private void InsertSelectedAssets(string tableName, XmlElement root)
        {
            foreach (var item in _assetListViewModel.AllAssets.Where(i => i.IsSelected))
            {
                if (AssetTypeId == 0 || AssetTypeId == 12)
                {
                    InsertAsset(tableName, root, item.Value);
                }
                else
                {
                    InsertAsset(tableName, root, item.ID.ToString());
                }
            }
        }

        private void InsertAllAssets(string tableName, XmlElement root)
        {
            foreach (var item in _assetListViewModel.AllAssets)
            {
                var table = root.AppendChild(root.OwnerDocument.CreateElement(tableName));
                var id = root.OwnerDocument.CreateAttribute("id");
                if (AssetTypeId == 0 || AssetTypeId == 12)
                {
                    id.Value = item.Value;
                }
                else
                {
                    id.Value = item.ID.ToString();
                }

                table.Attributes.Append(id);
            }
        }

        private static ListBoxItem GetListItem(DependencyObject dp)
        {
            if (dp == null) return null;

            var p = VisualTreeHelper.GetParent(dp);
            if (p == null || p is ListBoxItem) return p as ListBoxItem;

            return GetListItem(p);
        }

        private static ScrollBar GetScrollbar(DependencyObject dp)
        {
            var p = VisualTreeHelper.GetParent(dp);
            if (p == null || p is ScrollBar) return p as ScrollBar;

            return GetScrollbar(p);
        }

        private void AddCategories(XmlDocument doc)
        {
            if (lb[0].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("dataid")).Value = lb[0].SelectedValue.ToString();
            if (lb[1].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("catid")).Value = lb[1].SelectedValue.ToString();
            if (lb[2].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("subcatid")).Value = lb[2].SelectedValue.ToString();
            if (lb[3].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("cat2id")).Value = lb[3].SelectedValue.ToString();
        }

        private void ChangePriority(int box, ListItem moveUp, ListItem moveDown, int id)
        {
            var doc = GetAssetXml("edit");
            doc.DocumentElement.AddAttribute("boxid", box.ToString());
            doc.DocumentElement.AddAttribute("assettype", AssetTypeId.ToString());
            AddCategories(doc);

            var add = doc.DocumentElement.AddElement("Edit", moveDown.Value);
            add.AddAttribute("Priority", (moveDown.Priority - 1).ToString());
            add.AddAttribute("ID", moveDown.ID.ToString());

            var sadd = doc.DocumentElement.AddElement("Edit", moveUp.Value);
            sadd.AddAttribute("Priority", moveDown.Priority.ToString());
            sadd.AddAttribute("ID", moveUp.ID.ToString());

            var xn = DataAccess.getData("ab_UpdateAsset", new[] { "@xml", doc.OuterXml }, true);
            if (xn == null || xn.Name.LocalName.Contains("Error"))
            {
                throw new Exception("category failed, " + xn.Value);
            }

            Repopulate(box);
            lb[box - 1].SelectedItem = SelectItem(lb[box - 1], id);
            lb[box - 1].Focus();
        }

        private void FilterList(int boxId)
        {
            for (var i = 0; i < lb[boxId - 1].Items.Count; i++)
            {
                if (!(lb[boxId - 1].Items[i] is ListItem)) continue;

                var li = lb[boxId - 1].Items[i] as ListItem;
                if (li == null) continue;

                var selected = lb[boxId - 1].SelectedIndex == i;
                var ct = AssetTypeId + ":" + boxId;

                if (Window1.ShowTranslation && _catTrans.ContainsKey(ct))
                {
                    li.Hidden = !selected && (filter[boxId - 1] && Window1.SearchTranslation &&
                                              li.Language.IndexOf(tb[boxId - 1].Text,
                                                  StringComparison.OrdinalIgnoreCase) == -1
                                              || filter[boxId - 1] && !Window1.SearchTranslation &&
                                              li.Value.IndexOf(tb[boxId - 1].Text,
                                                  StringComparison.OrdinalIgnoreCase) == -1);
                }
                else
                {
                    li.Hidden = !selected && filter[boxId - 1] &&
                                li.Value.IndexOf(tb[boxId - 1].Text, StringComparison.OrdinalIgnoreCase) == -1;
                }
            }
        }

        #endregion Helper Methods
    }
}
