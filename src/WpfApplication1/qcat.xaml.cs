using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using AssetBuilder.Controls;
using System.Threading.Tasks;
using AssetBuilder.Classes;
using AssetBuilder.Controls.AssetControls;
using Visio = Microsoft.Office.Interop.Visio;
using Application = System.Windows.Application;
using Colors = System.Windows.Media.Colors;
using MenuItem = System.Windows.Controls.MenuItem;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for qcat.xaml
    /// </summary>
    public partial class qcat : UserControl
    {
        const int ATC = 16;
        int[][] cats = new int[ATC][];
        bool[] open = new bool[ATC];
        string[][] text = new string[ATC][];
        int[] search = resetSearch();
        string[] IDColumn = { "TITLEID", "ALGOID", "QUESTIONID", "ANSID", "RECID", "BPID", "TIMEID", "BUID", "", "", "", "MAPID", "TEXTID", "GROUPID", "", "CMAPID" };

        private static int[] resetSearch()
        {
            return Enumerable.Range(0, ATC).Select(f => 3).ToArray();
        }

        internal ListBox[] lb = new ListBox[5];
        internal TextBox[] tb = new TextBox[5];
        internal bool[] filter = new bool[5];
        internal Button[] ba = new Button[5];
        internal Button[] bd = new Button[5];
        internal Button[] bc = new Button[5];
        internal Button[] be = new Button[5];
        public Window1 form { get; set; }
        public XmlNode Defaults { get { return BuilderDefaults; } set { BuilderDefaults = value; } }
        public static XmlNode BuilderDefaults { get; set; }
        public static Dictionary<int, Dictionary<int, int>> AssetFlags { get; set; }
        public bool IsEditing { get; set; }
        public AssetControls.assetControl loadedAsset { get; set; }
        public string AssetID = "x";
        Intel intellisense = null;
        Dictionary<string, IntelModel>[] lists = null;
        GridLength[] savedGridWidths = new GridLength[5];

        static char[] comma = ",".ToCharArray();

        public void InitDicts()
        {
            lists = new Dictionary<string, IntelModel>[] {
                null,
                IntelListMakers.makeList("Regex:(?i)type:([0-9]+,)*$", Defaults, "cmbAssessment"),
                IntelListMakers.makeList("Regex:(?i)type:([0-9]+,)*$", Defaults, "cmbType"),
                IntelListMakers.makeList("Regex:(?i)type:([0-9]+,)*$", Defaults, "cmbAnsType"),
                new Dictionary<string, IntelModel> {
                    { "Regex:(?i)type:([0-9]+,)*$", new IntelItem[] {
                        new IntelItem { Value = "1", Display = (Window1.McKesson_Mode ? "Self Care" : "Silent") },
                        new IntelItem { Value = "2", Display = (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                        new IntelItem { Value = "3", Display = "Not " + (Window1.McKesson_Mode ? "Self Care" : "Silent") },
                        new IntelItem { Value = "4", Display = "Not " + (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                        new IntelItem { Value = "5", Display = "Neither " + (Window1.McKesson_Mode ? "Self Care" : "Silent") + " or " + (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                        new IntelItem { Value = "6", Display = "Both " + (Window1.McKesson_Mode ? "Self Care" : "Silent") + " and " + (Window1.McKesson_Mode ? "Watchout Condition" : "Information") },
                    }}
                },
                new Dictionary<string, IntelModel>(),
            };

            lists[2]["Regex:(?i)type:([0-9]+,)*$"].values.AddRange(Defaults.SelectNodes("*[*[1] = 'cmbHistory' and not(Exclude)]").OfType<XmlNode>().Select(f => new IntelItem { Value = (200 + int.Parse(f.ChildNodes[1].InnerText)).ToString(), Display = f.ChildNodes[2].InnerText }));
            lists[2]["Regex:(?i)type:([0-9]+,)*$"].values.AddRange(Defaults.SelectNodes("*[*[1] = 'cmbComments' and not(Exclude)]").OfType<XmlNode>().Select(f => new IntelItem { Value = (100 + int.Parse(f.ChildNodes[1].InnerText)).ToString(), Display = f.ChildNodes[2].InnerText }));

            foreach (var dict in lists)
                if (dict != null)
                {
                    foreach (var item in dict)
                    {
                        item.Value.endings = comma;
                        item.Value.AppendEnding = true;
                    }
                    dict.Add("Regex:(?i)date:$", new IntelItem[] {
                        new IntelItem { Value = DateTime.Now.ToString("yyyy-MM-dd"), Display = "Today" },
                        new IntelItem { Value = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), Display = "Yesterday" },
                        new IntelItem { Value = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd"), Display = "Last Week" },
                        new IntelItem { Value = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"), Display = "Last Month" },
                        new IntelItem { Value = DateTime.Now.ToString("yyyy-01-01"), Display = "This Year" },
                    });
                }
            _AssetTypeID = __AssetTypeID;
        }

        private int __AssetTypeID = -1;
        private int _AssetTypeID
        {
            get
            {
                return __AssetTypeID;
            }
            set
            {
                __AssetTypeID = value;
                if (form != null)
                {
                    if (__AssetTypeID == 15 || __AssetTypeID == 4 && Window1.AllowConclusionMap) form.rtbMap.Visibility = Visibility.Visible; else form.rtbMap.Visibility = Visibility.Collapsed;
                    if (__AssetTypeID == 6 || __AssetTypeID == 4) form.rtbTimes.Visibility = Visibility.Visible; else form.rtbTimes.Visibility = Visibility.Collapsed;
                    if (__AssetTypeID == 0 || __AssetTypeID == 1) form.rtbTitles.Visibility = Visibility.Visible; else form.rtbTitles.Visibility = Visibility.Collapsed;
                }
                if (intellisense != null) intellisense.Dispose();
                if (__AssetTypeID > 0 && __AssetTypeID < 6 && form != null)
                    intellisense = new Intel(textBox5, form.bubbleCanvas, lists[__AssetTypeID]);
                else
                    intellisense = null;
            }
        }
        public int AssetTypeID
        {
            get
            {
                return _AssetTypeID;
            }
            set
            {
                if (IsEditing)
                {
                    System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("There is another asset currently being edited. Do you want to save the changes?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (!loadedAsset.Save())
                            Window1.RadioToggle(form.assetGroup, _AssetTypeID);
                    }
                    else if (dr == System.Windows.Forms.DialogResult.No) IsEditing = false;
                    else if (dr == System.Windows.Forms.DialogResult.Cancel) Window1.RadioToggle(form.assetGroup, _AssetTypeID);
                }

                if (!IsEditing)
                {
                    AddContextMenus(value);
                    if (value == 0 && _AssetTypeID != 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            savedGridWidths[i] = categoryGrid.ColumnDefinitions[i].Width;
                            if (i == 4) categoryGrid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
                            else categoryGrid.ColumnDefinitions[i].Width = new GridLength(0);
                        }
                    }
                    if (_AssetTypeID == 0 && value != 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            categoryGrid.ColumnDefinitions[i].Width = savedGridWidths[i];
                        }
                    }
                    Window1.setStatus("Accessing Data...");
                    if (_AssetTypeID >= 0) saveCats();
                    if (form != null) form.assetCanvas.Children.Clear();
                    _AssetTypeID = Math.Abs(value);
                    for (int i = 0; i < 5; i++)
                    {
                        if (value >= 0 && text[_AssetTypeID] != null) tb[i].Text = text[_AssetTypeID][i];
                        else tb[i].Text = "";
                    }
                    if (timer != null)
                    {
                        timer.Stop();
                        timer = null;
                    }

                    _SearchTypeID = search[_AssetTypeID];
                    if (form != null) Window1.RadioToggle(form.searchGroup, _SearchTypeID);

                    if (value >= 0 && cats[_AssetTypeID] != null && cats[_AssetTypeID][4] > 0)
                    {
                        noAssets = true;
                        int topcat = -1;
                        for (int i = 0; i < 4; i++)
                        {
                            if (cats[_AssetTypeID][i] > 0 && i > topcat) topcat = i;
                        }
                        if (topcat == -1) noAssets = false;
                        populate(0);
                        for (int i = 0; i < 4; i++)
                        {
                            if (i == topcat) noAssets = false;
                            if (cats[_AssetTypeID][i] > 0) lb[i].SelectedValue = cats[_AssetTypeID][i];
                            else lb[i].UnselectAll();
                        }
                        noAssets = false;
                        if (cats[_AssetTypeID][4] == 0) lb[4].UnselectAll();
                        else
                        {
                            lb[4].SelectedValue = cats[_AssetTypeID][4];
                            if (open[_AssetTypeID] && lb[4].SelectedValue != null) loadAsset(lb[4].SelectedValue.ToString());
                        }
                    }
                    else
                        populate(0, _AssetTypeID, textBox5.Text);
                    Window1.setStatus("");
                }
            }
        }

        public void AddContextMenus(int value)
        {
            var CategoryTypes = Defaults.SelectNodes(string.Format("*[ContextMenu='{0}' and AssetTypeID='{1}']", "ListContext", value)).OfType<XmlNode>()
                .Select(f => new { ID = int.Parse(f["ID"].InnerText), Description = f["Description"].InnerText, BoxID = int.Parse(f["BoxID"].InnerText) });
            if (Window1.CategoryEnabled && CategoryTypes.Any())
            {
                for (int i = 0; i < 4; i++)
                {
                    var items = CategoryTypes.Where(f => f.BoxID == i + 1);
                    if (items.Any())
                    {
                        ContextMenu cm = new ContextMenu();
                        cm.Opened += cm_Loaded;
                        foreach (var item in items)
                        {
                            MenuItem m = new MenuItem() { Header = item.Description, CommandParameter = item };
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
                for (int i = 0; i < 4; i++)
                {
                    lb[i].ContextMenu = null;
                }
            }
        }

        void cm_Loaded(object sender, RoutedEventArgs e)
        {
            var cm = (ContextMenu)sender;
            var lb = (ListBox)cm.PlacementTarget;

            int cat = -1;

            if (lb.SelectedItems.Count > 0) cat = ((ListItem)lb.SelectedItem).CategoryTypeID;

            foreach (var m in cm.Items.OfType<MenuItem>())
            {
                var item = Cast(new { ID = 0, Description = "", BoxID = 0 }, m.CommandParameter);
                if (item.ID == cat) m.IsChecked = true; else m.IsChecked = false;
            }
        }

        void lb_Context_Click(object sender, RoutedEventArgs e)
        {
            var item = Cast(new { ID = 0, Description = "", BoxID = 0 }, ((MenuItem)sender).CommandParameter);
            var root = getAssetXml("setcategorytype");
            root.DocumentElement.Attributes.Append(root.CreateAttribute("AssetTypeID")).Value = AssetTypeID.ToString();
            root.DocumentElement.Attributes.Append(root.CreateAttribute("BoxID")).Value = item.BoxID.ToString();

            foreach (var listitem in lb[item.BoxID - 1].SelectedItems)
            {
                ListItem li = (ListItem)listitem;
                var cat = root.DocumentElement.AppendChild(root.CreateElement("Category"));
                cat.Attributes.Append(root.CreateAttribute("ID")).Value = li.ID.ToString();
                if (item.ID != 0) cat.Attributes.Append(root.CreateAttribute("CategoryTypeID")).Value = item.ID.ToString();
                li.CategoryTypeID = item.ID;
            }

            var xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                    "@xml", root.OuterXml
                }, true);
        }

        T Cast<T>(T Typeholder, object x)
        {
            return (T)x;
        }

        private void saveCats()
        {
            cats[_AssetTypeID] = getCats();
            text[_AssetTypeID] = getText();
            open[_AssetTypeID] = form != null && form.assetCanvas.Children.Count > 0;
            search[_AssetTypeID] = _SearchTypeID;
            lastboxid = 0;
        }

        public void clearCats()
        {
            cats = new int[ATC][];
            open = new bool[ATC];
            text = new string[ATC][];
            search = resetSearch();
            lastboxid = 0;
        }

        private int _SearchTypeID = 3;
        public int SearchTypeID
        {
            get
            {
                return _SearchTypeID;
            }
            set
            {
                _SearchTypeID = value;
                populate(null as ListBox);
            }
        }

        public bool IsDesignTime
        {
            get
            {
                return DesignerProperties.GetIsInDesignMode(this);
            }
        }

        public qcat()
        {
            InitializeComponent();
            lb = new ListBox[] { listBox1, listBox2, listBox3, listBox4, listBox5 };
            tb = new TextBox[] { textBox1, textBox2, textBox3, textBox4, textBox5 };
            filter = new bool[] { false, false, false, false, false };
            ba = new Button[] { btnAdd1, btnAdd2, btnAdd3, btnAdd4, btnAdd5 };
            bd = new Button[] { btnDelete1, btnDelete2, btnDelete3, btnDelete4, btnDelete5 };
            bc = new Button[] { btnClear1, btnClear2, btnClear3, btnClear4, btnClear5 };
            be = new Button[] { btnEdit1, btnEdit2, btnEdit3, btnEdit4, btnEdit5 };
            //if (!IsDesignTime) AssetTypeID = 2;
            IsEditing = false;
        }

        public void repopulate(int boxid)
        {
            //lb[boxid - 1].UnselectAll();
            if (boxid == 1) lastboxid = 0;
            if (boxid == 2) lastboxid = 1;
            if (boxid == 3)
                if (AssetTypeID == 6) lastboxid = 1; else lastboxid = 2;
            if (boxid == 4) lastboxid = 1;
            if (boxid == 5 || boxid == -1)
            {
                if (boxid == 5) form.assetCanvas.Children.Clear();
                if (AssetTypeID == 1) lastboxid = 1; else lastboxid = 4;
            }
            int id = 0;
            if (boxid == -1 && lb[4].SelectedItem != null) id = (int)lb[4].SelectedValue;

            populate(null as ListBox);
            if (id > 0)
            {
                loadedAsset = null;
                lb[4].SelectedItem = selectItem(lb[4], id);
            }
        }

        private void populate(int boxid)
        {
            populate(boxid, AssetTypeID, textBox5.Text);
        }

        private void populate(ListBox sel)
        {
            int boxid = 0;
            int[] cats = { 0, 0, 0, 0 };
            for (int i = 0; i < lb.Length - 1; i++)
            {
                if (lb[i] == sel) boxid = i + 1;
                if (i == 0 && boxid == 1)
                {
                    lb[1].UnselectAll();
                    lb[2].UnselectAll();
                    lb[3].UnselectAll();
                }
                else if (i == 1 && boxid == 2) lb[2].UnselectAll();
                if (lb[i].SelectedItem != null) cats[i] = (lb[i].SelectedItem as ListItem).ID;
            }
            if (boxid == 0) boxid = lastboxid;
            lastboxid = boxid;
            populate(boxid, _AssetTypeID, cats, textBox5.Text);
        }

        private void populate(int boxid, int assettypeid, string searchword)
        {
            populate(boxid, assettypeid, 0, 0, 0, 0, searchword);
        }

        private void populate(int boxid, int assettypeid, int[] cats, string searchword)
        {
            populate(boxid, assettypeid, cats[0], cats[1], cats[2], cats[3], searchword);
        }

        //private void populate(int boxid, int assettypeid, int dataid, int catid, int subcatid, int cat2id, string searchword)
        //{
        //    populate(boxid.ToString(), assettypeid.ToString(), dataid.ToString(), catid.ToString(), subcatid.ToString(), cat2id.ToString(), searchword);
        //}

        bool noAssets = false;
        public static string currentSearch = "";
        public static string currentSearchSQL = "";
        static List<string> prmentry = new List<string>(new string[] { "data", "cat1", "subcat", "cat2", "algo", "question", "answer", "conclusion", "bullet", "count", "date", "user", "type", "order" });
        static List<string> prmname = new List<string>(new string[] { "@dataid", "@catid", "@subcatid", "@cat2id", "@algoid", "@questionid", "@answerid", "@conclusionid", "@bulletid", "@count", "@date", "@user", "@type", "order" });

        private void populate(int boxid, int assettypeid, int dataid, int catid, int subcatid, int cat2id, string searchword)
        {
            List<string> prms = getParameters(noAssets, _SearchTypeID, boxid, assettypeid, dataid, catid, subcatid, cat2id, ref searchword);

            setSearch(searchword);

            if (showall)
                setShowAll(prms, ref showall);

            populate(prms.ToArray());
            setButtons();
        }

        public static List<string> getParameters(bool noAssets, int _SearchTypeID, int assettypeid, ref string searchword)
        {
            return getParameters(noAssets, _SearchTypeID, 0, assettypeid, 0, 0, 0, 0, ref searchword);
        }

        public static List<string> getParameters(bool noAssets, int _SearchTypeID, int boxid, int assettypeid, int dataid, int catid, int subcatid, int cat2id, ref string searchword)
        {
            while (searchword.IndexOf(" AND ", StringComparison.CurrentCulture) >= 0 && searchword.Length > searchword.IndexOf(" AND ", StringComparison.CurrentCulture) + 5)
            {
                searchword = string.Format("{0}%{1}",
                    searchword.Substring(0, searchword.IndexOf(" AND ", StringComparison.CurrentCulture)),
                    searchword.Substring(searchword.IndexOf(" AND ", StringComparison.CurrentCulture) + 5));
            }

            //string algoid = "0";
            object[] prmvalue = { dataid, catid, subcatid, cat2id, 0, 0, 0, 0, 0, 0, null, null, 0, "" };
            string[] prmtypes = { "int", "int", "int", "int", "ints", "int", "int", "int", "int", "int", "DateTime", "string", "ints", "string" };
            bool[] prminc = { dataid > 0, catid > 0, subcatid > 0, cat2id > 0, false, false, false, false, false, false, false, false, false, false };

            if (searchword.Contains(":"))
            {
                string newsearch = "";
                string[] seg = searchword.Split(' ');
                foreach (var item in seg)
                {
                    if (item.Contains(':'))
                    {
                        int prmindex = prmentry.FindIndex(f => item.StartsWith(f, StringComparison.CurrentCultureIgnoreCase));
                        if (prmindex > -1)
                        {
                            switch (prmtypes[prmindex])
                            {
                                case "int":
                                case "ints":
                                    string clause = item.Substring(item.IndexOf(':') + 1);
                                    bool lit = false;
                                    int aid = 0;
                                    if (prmtypes[prmindex] == "ints" && (clause.Contains('*') || clause.Contains(',')))
                                    {
                                        string[] list = { "", "" };
                                        foreach (var ta in clause.Split(','))
                                        {
                                            string talgoid = ta;
                                            if (talgoid.StartsWith("(") && talgoid.EndsWith(")")) lit = true; else lit = false;
                                            if (lit) talgoid = talgoid.Substring(1, talgoid.Length - 2);
                                            if (prmentry[prmindex] == "algo" && talgoid.EndsWith("*") && talgoid.Length > 1)
                                            {
                                                string tas = talgoid.Substring(0, talgoid.Length - 1);
                                                if (int.TryParse(tas, out aid))
                                                {
                                                    int index = aid >= 0 ? 0 : 1;
                                                    XmlNode xn = DataAccess.getDataNode("usp_getalgos", new string[] {
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
                                                int index = (aid >= 0 || lit) ? 0 : 1;
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
                                    //else if(prmentry[prmindex] == "algo" && clause.Contains(','))
                                    //{
                                    //    bool allParse = true;
                                    //    foreach (var talgoid in clause.Split(','))
                                    //    {
                                    //        if (!int.TryParse(talgoid, out aid))
                                    //        {
                                    //            allParse = false;
                                    //            break;
                                    //        }
                                    //    }
                                    //    if (allParse)
                                    //    {
                                    //        prminc[prmindex] = true;
                                    //        prmvalue[prmindex] = clause;
                                    //    }
                                    //}
                                    else
                                    {
                                        if (clause.StartsWith("(") && clause.EndsWith(")")) lit = true; else lit = false;
                                        if (lit) clause = clause.Substring(1, clause.Length - 2);
                                        if (int.TryParse(clause, out aid))
                                        {
                                            prminc[prmindex] = true;
                                            prmvalue[prmindex] = ((aid < 0 && !lit) ? "***NEG***" : "") + (lit ? aid : Math.Abs(aid));
                                        }
                                    }
                                    continue;
                                case "DateTime":
                                    DateTime dt = DateTime.MinValue;
                                    if (DateTime.TryParse(item.Substring(item.IndexOf(':') + 1), out dt))
                                    {
                                        prminc[prmindex] = true;
                                        prmvalue[prmindex] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    continue;
                                case "string":
                                    string s = item.Substring(item.IndexOf(':') + 1);
                                    if (s.Length > 0)
                                    {
                                        prminc[prmindex] = true;
                                        prmvalue[prmindex] = "%" + s + "%"; ;
                                    }
                                    continue;
                            }
                        }
                    }
                    if (newsearch != "") newsearch += " ";
                    newsearch += item;
                }
                searchword = newsearch;
            }

            //currentSearch = System.Text.RegularExpressions.Regex.Replace(searchword, @"(?:\\|\[)", @"\${0}");
            //currentSearch = searchword.Replace("[", "\\[");
            //currentSearch = currentSearch.Replace("\\", "\\\\");
            //currentSearch = currentSearch.Replace("^", "\\^");
            //currentSearch = currentSearch.Replace("$", "\\$");
            //currentSearch = currentSearch.Replace(".", "\\.");
            //currentSearch = currentSearch.Replace("|", "\\|");
            //currentSearch = currentSearch.Replace("?", "\\?");
            //currentSearch = currentSearch.Replace("*", "\\*");
            //currentSearch = currentSearch.Replace("+", "\\+");
            //currentSearch = currentSearch.Replace("(", "\\(");
            //currentSearch = currentSearch.Replace(")", "\\)");
            //currentSearch = currentSearch.Replace("%", "|");

            string sendsearch = "@@@###@@@";
            //if (noAssets) searchword = "@@@###@@@";
            if (!noAssets) sendsearch = ((_SearchTypeID & 1) == 1 ? "%" : "") + searchword + ((_SearchTypeID & 2) == 2 ? "%" : "");
            if (!searchword.StartsWith("%")) sendsearch = System.Text.RegularExpressions.Regex.Replace(sendsearch, @"(?:\[)", @"[${0}]");

            currentSearchSQL = sendsearch;

            List<string> prms = new List<string>(new string[] { "@boxid", boxid.ToString(), "@assettypeid", assettypeid.ToString(), "@searchword", sendsearch });
            for (int i = 0; i < prmentry.Count; i++)
            {
                if (prminc[i])
                {
                    prms.Add(prmname[i]);
                    prms.Add(prmvalue[i].ToString());
                }
            }
            return prms;
        }

        private static string[] setShowAll(List<string> prms, ref bool showall)
        {
            if (prms.Contains("@count")) prms[prms.IndexOf("@count") + 1] = "0";
            else prms.AddRange(new string[] { "@count", "0" });
            showall = false;
            return prms.ToArray();
        }

        private string[] resetSearch(List<string> prms)
        {
            if (prms.Contains("@searchword")) prms[prms.IndexOf("@searchword") + 1] = "";
            else prms.AddRange(new string[] { "@searchword", "" });
            return prms.ToArray();
        }

        public static void setSearch(string searchword)
        {
            if (searchword.StartsWith("Regex:")) currentSearch = searchword.Substring(6);
            else
            {
                if (searchword.StartsWith("%"))
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(searchword, @"(?:\%)", @"\w*");
                else
                {
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(searchword, @"(?:\\|\[|\^|\$|\.|\||\?|\*|\+|\(|\))", @"\${0}");
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(currentSearch, @"(?:\%)", @"(?s:.*?)");
                }
                currentSearch = System.Text.RegularExpressions.Regex.Replace(currentSearch, @"(?:[_])", @".{1}");
            }
        }

        public static void setSearch(string searchword, bool matchCase, bool wholeWord, bool useWildcards)
        {
            if (useWildcards)
            {
                currentSearch = System.Text.RegularExpressions.Regex.Replace(searchword, @"(?:\\|\[|\^|\$|\.|\||\+|\(|\)|\%)", @"\${0}");
                if (wholeWord)
                {
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(currentSearch, @"\?", @"\w");
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(currentSearch, @"\*", @"\w+?");
                }
                else
                {
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(currentSearch, @"\?", @".");
                    currentSearch = System.Text.RegularExpressions.Regex.Replace(currentSearch, @"\*", @".+?");
                }
            }
            else
                setSearch(searchword);
            if (wholeWord) currentSearch = @"\b" + currentSearch + @"\b";
            if (matchCase) currentSearch = @"(?-i)" + currentSearch;
        }

        public void setButtons(bool enabled)
        {
            for (int i = 0; i < 5; i++)
            {
                ba[i].IsEnabled = enabled;
                be[i].IsEnabled = enabled;
                bd[i].IsEnabled = enabled;
            }
            btnList5.IsEnabled = enabled;
        }

        public void setButtons()
        {
            if (!Window1.IsReviewerOrEditor)
            {
                setButtons(true);
            }
            if (form != null) if (AssetTypeID == 4 && !Window1.EditTranslation) btnEdit3.IsEnabled = false; else btnEdit3.IsEnabled = true;
            if (form != null) if (!Window1.IsReviewerOrEditor && AssetTypeID == 4 && lb[1].SelectedIndex > -1) btnTitle2.Visibility = Visibility.Visible; else btnTitle2.Visibility = Visibility.Collapsed;
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
            if (Window1.IsReviewerOrEditor || AssetTypeID == 0 || AssetTypeID == 13)
            {
                setButtons(false);
                if ((Window1.IsBuilderOrAdmin && Window1.EditTranslation) || Window1.IsEditor) btnEdit5.IsEnabled = true;
            }
            //for (int i = 0; i < 5; i++)
            //{
            //    ba[i].IsEnabled = tb[i].Text != "";
            //    bd[i].IsEnabled = tb[i].Text != "";
            //    bc[i].IsEnabled = tb[i].Text != "" || lb[i].SelectedIndex > -1;
            //    be[i].IsEnabled = tb[i].Text != "";
            //}
        }

        Dictionary<string, int> catTrans = new Dictionary<string, int>() {
            { "2:2", 6 },
            { "2:3", 7 },
            { "3:2", 6 },
            { "3:3", 7 },
            { "4:2", 8 },
            { "4:3", 9 },
            { "4:4", 10 },
        };
        Dictionary<string, string> catFields = new Dictionary<string, string>() {
            { "2:2", "Category" },
            { "2:3", "Subcategory" },
            { "3:2", "Category" },
            { "3:3", "Subcategory" },
            { "4:2", "Category" },
            { "4:3", "SubCat1" },
            { "4:4", "SubCat2" },
        };

        private void populate(string[] prms)
        {
            IEnumerable<string> tids = null;
            if (Window1.SearchTranslation)
            {
                string oSearch = "";
                if (prms.Contains("@searchword")) oSearch = prms[Array.IndexOf(prms, "@searchword") + 1].Replace("%", "");
                prms = setShowAll(prms.ToList(), ref showall);
                prms = resetSearch(prms.ToList());
                XmlNode searchTranslation = DataAccess.searchLanguage(AssetTypeID, SearchTypeID, Window1.TranslationLanguage, oSearch);
                tids = from st in searchTranslation.SelectNodes("ID").OfType<XmlNode>() select st.InnerText;
            }
            int showcount = 100;
            if (prms.Contains("@count"))
            {
                int tc = 0;
                if (int.TryParse(prms[Array.IndexOf(prms, "@count") + 1], out tc) && tc > 0) showcount = tc;
            }
            XElement xe = getAssets(prms);
            var xn = from x in xe.Elements() select x;
            HashSet<int> boxes = new HashSet<int>();
            foreach (XElement x in xn)
            {
                int box = getInt(x, "BoxID");
                if (box < 1 || box > 5) continue;
                int Priority = getInt(x, "Priority");
                int CatTypeID = getInt(x, "CategoryTypeID");
                int NotInUse = getInt(x, "NotInUse");
                AuditItem ai = null;
                if (box == 5 && x.Element("AuditText") != null)
                {
                    ai = DataAccess.JsonDeSerialize<AuditItem>(x.Element("AuditText")?.Value);
                    ai.Date = DateTime.Parse(x.ElementValue("AuditDate"));
                    ai.User = x.ElementValue("AuditUser");
                }
                if (x.Element("ID") == null)
                {
                    lb[box - 1].Items.Clear();
                    boxes.Add(box);
                }
                else
                {
                    if (box < 5 || tids == null || tids.Contains(x.Element("ID").Value) || (AssetTypeID == 0 && tids.Contains(string.Concat(x.Element("Description").Value.Split(System.IO.Path.GetInvalidFileNameChars())).TrimStart())))
                    {
                        string desc = x.Element("Description").Value;
                        if (desc == "") desc = "<-- blank -->";
                        if (Window1.MultiTextLanguage)
                            lb[box - 1].Items.Add(new ListItem { ID = getInt(x, "ID"), Value = desc, Priority = Priority, CategoryTypeID = CatTypeID, NotInUse = NotInUse, Audit = ai });
                        else
                        {
                            string ct = AssetTypeID + ":" + box;
                            if (Window1.ShowTranslation && catTrans.ContainsKey(ct))
                            {
                                string lang = DataAccess.getCategoryLanguage(catTrans[ct], desc, Window1.TranslationLanguage);
                                bool hidden = (filter[box - 1] && Window1.SearchTranslation && lang.IndexOf(tb[box - 1].Text, StringComparison.OrdinalIgnoreCase) == -1)
                                    || (filter[box - 1] && !Window1.SearchTranslation && desc.IndexOf(tb[box - 1].Text, StringComparison.OrdinalIgnoreCase) == -1);
                                lb[box - 1].Items.Add(new ListItem { ID = getInt(x, "ID"), Value = desc, Priority = Priority, Language = lang, Hidden = hidden, CategoryTypeID = CatTypeID, NotInUse = NotInUse, Audit = ai });
                            }
                            else
                            {
                                bool hidden = filter[box - 1] && desc.IndexOf(tb[box - 1].Text, StringComparison.OrdinalIgnoreCase) == -1;
                                lb[box - 1].Items.Add(new ListItem { ID = getInt(x, "ID"), Value = desc, Priority = Priority, Hidden = hidden, CategoryTypeID = CatTypeID, NotInUse = NotInUse, Audit = ai });
                            }
                        }
                    }
                }
            }
            if (Window1.MultiTextLanguage)
            {
                SetCategoryLanguage(boxes.Where(f => Window1.ShowTranslation && catTrans.ContainsKey(AssetTypeID + ":" + f)));
            }
            if (listBox5.Items.Count == showcount)
            {
                assetCount.Foreground = Brushes.Red;
                assetCount.Text = " - ???";
                imgWarning.Visibility = Visibility.Visible;
                ShowAll.Visibility = Visibility.Visible;
            }
            else
            {
                assetCount.Foreground = Brushes.Blue;
                if (listBox5.Items.Count == 0) assetCount.Text = "";
                else assetCount.Text = " - " + listBox5.Items.Count.ToString();
                imgWarning.Visibility = Visibility.Hidden;
                ShowAll.Visibility = Visibility.Hidden;
            }
            //if ((string)Window1.getStatus() != "Saving Record" && listBox5.Items.Count > 0) listBox5.ScrollIntoView(listBox5.Items[0]);
            if (listBox5.Items.Count > 0) listBox5.ScrollIntoView(listBox5.Items[0]);
        }

        private static XElement getAssets(string[] prms)
        {
            Window1.setStatus("Getting Assets...");
            bool negSearch = false;
            bool incSearch = false;
            string[] inc = null;
            bool showall = false;
            if (prms.Any(f => f.Contains("***INC***"))) prms = setShowAll(prms.ToList(), ref showall);
            if (prms.Any(f => f.Contains("***NEG***"))) prms = setShowAll(prms.ToList(), ref showall);
            string[] negprms = new string[prms.Length];
            for (int i = 0; i < prms.Length; i++)
            {
                if (prms[i].Contains("***NEG***"))
                {
                    negprms[i] = prms[i].Substring(prms[i].IndexOf("***NEG***") + 9);
                    prms[i] = prms[i].Substring(0, prms[i].IndexOf("***NEG***"));
                    if (prms[i] == "") prms[i - 1] = "";
                    negSearch = true;
                }
                else if (prms[i].Contains("***INC***"))
                {
                    inc = prms[i].Substring(prms[i].IndexOf("***INC***") + 9).Split(',');
                    prms[i] = prms[i].Substring(0, prms[i].IndexOf("***INC***"));
                    if (prms[i] == "") prms[i - 1] = "";
                    incSearch = true;
                }
                else negprms[i] = prms[i];
            }
            XElement xe = DataAccess.getData("ab_getitems", prms, false);
            int oi = -1;
            if ((oi = Array.IndexOf(prms, "order")) > -1 && prms.Length > oi + 1 && string.Compare(prms[oi + 1], "%id%", true) == 0)
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
                XElement neg = DataAccess.getData("ab_getitems", negprms, false);
                var negids = neg.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null).Select(f => f.Element("ID").Value);
                xe.Elements().Where(f => f.Element("BoxID").Value == "5" && f.Element("ID") != null && negids.Contains(f.Element("ID").Value)).Remove();
            }
            Window1.setStatus("");
            return xe;
        }

        private static int getInt(XElement node, string element)
        {
            if (node.Element(element) == null) return 0;
            return int.Parse(node.Element(element).Value);
        }

        int lastboxid = 0;

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listOpen) closeList();
            ListBox sel = sender as ListBox;
            if (sel.SelectedItem != null)
            {
                sel.ScrollIntoView(sel.SelectedItem);
                populate(sel);
            }
        }

        private void listBox5_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !Window1.IsEditor && ((AssetTypeID <= 5 && AssetTypeID > 0) || AssetTypeID == 11))
            {
                Point p = e.GetPosition(listBox5);
                ListBoxItem li = getListItem((DependencyObject)listBox5.InputHitTest(p));
                if (li != null)
                {
                    if (p.X > listBox5.ActualWidth - 20) return;
                    if (p.Y > listBox5.ActualHeight - 20) return;
                    DragDrop.DoDragDrop(listBox5, getItems(), DragDropEffects.Move);
                }
            }
        }

        Dictionary<string, string> properties = new Dictionary<string, string>()
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

        string getItems()
        {
            string ids = "";
            string s = "";
            foreach (ListItem item in listBox5.SelectedItems)
            {
                if (ids != "") ids += ",";
                if (s != "") s += "$$BREAK$$";
                ids += item.ID;
                s += item.Value;
            }
            ids = string.Format("{0} In (", IDColumn[AssetTypeID]) + ids + ")";
            if (Window1.McKesson_Mode)
            {
                s += "$$BREAK$$";
                string x = "";
                foreach (ListItem item in listBox5.SelectedItems)
                {
                    string key = "";
                    var xpathkeys = properties.Where(f => f.Key.Contains(":XPath:")).Where(f =>
                    {
                        string[] sa = f.Key.Split(':');
                        XmlNode asset = DataAccess.getDataNode("ab_GetAsset", "@AssetTypeID", _AssetTypeID.ToString(), "@AssetID", (item as ListItem).ID.ToString());
                        if (asset.SelectSingleNode(sa[2]) != null) return true;
                        return false;
                    });
                    if (xpathkeys.Any()) key = xpathkeys.First().Key;
                    else if (listBox4.SelectedItem != null && properties.ContainsKey(AssetTypeID + ":" + (listBox4.SelectedItem as ListItem).Value)) key = AssetTypeID + ":" + (listBox4.SelectedItem as ListItem).Value;
                    else if (properties.ContainsKey(AssetTypeID.ToString())) key = AssetTypeID.ToString();

                    if (x != "") x += "$$XML$$";
                    if (key != "")
                    {
                        string assettype = ((AssetBuilder.AssetControls.AssetType)AssetTypeID).ToString();
                        //x += properties[key];
                        string altassettype = "Transfer";
                        string processed = "";
                        foreach (var split in properties[key].Split(new string[] { "$$ALT$$" }, StringSplitOptions.None))
                        {
                            if (processed != "") processed += "$$ALT$$";
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(split);
                            XmlNode xn = DataAccess.getDataNode("dsp_GetProperty", "@PropertyType", assettype, "@DataID", item.ID.ToString());
                            foreach (XmlAttribute attr in doc.DocumentElement.Attributes)
                            {
                                XmlNode def = xn.SelectSingleNode(string.Format("Table[PropertyName = '{0}']", attr.Name));
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
                            assettype = altassettype;
                        }
                        x += processed;
                    }
                }
                s += x;
            }
            s = "(" + s + ")";
            return ids + s + (listBox2.SelectedItem != null && (listBox2.SelectedItem as ListItem).Value == "Messages" ? "(Message)" : "");
        }

        DispatcherTimer timer;
        Dictionary<string, TextAdorner> textAdorners = new Dictionary<string, TextAdorner>();

        public void clearAdorners()
        {
            if (listOpen) closeList();
            for (int i = 0; i < tb.Length; i++)
            {
                tb[i].clearAdornerLayer();
                if (textAdorners.ContainsKey(tb[i].Name)) textAdorners.Remove(tb[i].Name);
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
            for (int i = 0; i < tb.Length; i++)
            {
                AdornerLayer al = tb[i].clearAdornerLayer();
                if (al == null) continue;

                TextAdorner ta = new AssetBuilder.Controls.TextAdorner(tb[i], values[i], Colors.LightGray);
                if (textAdorners.ContainsKey(tb[i].Name)) textAdorners[tb[i].Name] = ta;
                else textAdorners.Add(tb[i].Name, ta);
                if (tb[i].Text == "") ta.Text = ""; else ta.Text = " ";
                al.Add(ta);
            }
        }

        public void updateAdorner(TextBox atb)
        {
            if (textAdorners.ContainsKey(atb.Name))
                if (atb.Text == "") textAdorners[atb.Name].Text = ""; else textAdorners[atb.Name].Text = " ";
        }

        private void textBox5_TextChanged(object sender, TextChangedEventArgs e)
        {
            setButtons();
            if (timer != null) timer.Stop();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
            updateAdorner(sender as TextBox);

            if (loadedAsset == null) return;

            string setsearch = sanitiseSearch((sender as TextBox).Text);
            setSearch(setsearch);
            foreach (var item in loadedAsset.TextChildren)
            {
                if (currentSearch == "" && !item.Key.EndsWith("Language"))
                    NLExtensions.clearAdornerLayer(item.Value);
                else if (item.Value.IsVisible)
                    NLExtensions.textBox_AdornAndValidate(item.Value, null);
            }
        }

        private string sanitiseSearch(string searchword)
        {
            if (searchword.Contains(":"))
            {
                string newsearch = "";
                string[] seg = searchword.Split(' ');
                foreach (var item in seg)
                {
                    if (item.Contains(':'))
                    {
                        int prmindex = prmentry.FindIndex(f => item.StartsWith(f, StringComparison.CurrentCultureIgnoreCase));
                        if (prmindex > -1)
                        {
                            continue;
                        }
                    }
                    if (newsearch != "") newsearch += " ";
                    newsearch += item;
                }
                searchword = newsearch;
            }

            return searchword;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            timer = null;
            populate(null as ListBox);
        }

        private static int getBox(object sender)
        {
            Button b = sender as Button;
            int box = int.Parse(b.CommandParameter.ToString());
            return box;
        }

        private void cancelClick(object sender, RoutedEventArgs e)
        {
            int box = getBox(sender);
            lb[box - 1].UnselectAll();
            tb[box - 1].Text = "";
            if (box == 1)
            {
                lastboxid = 0;
                populate(0);
            }
            else populate(lb[box - 1]);
        }

        private void addClick(object sender, RoutedEventArgs e)
        {
            int box = getBox(sender);
            if (box == 5)
            {
                addNew();

            }
            else if (tb[box - 1].Text != "")
            {
                XmlDocument doc = getAssetXml("add");
                addCategories(doc);
                doc.DocumentElement.AddAttribute("boxid", box.ToString());
                doc.DocumentElement.AddAttribute("assettype", AssetTypeID.ToString());
                XmlElement add = doc.DocumentElement.AddElement("Add", tb[box - 1].Text);
                int priority = 1;
                if (lb[box - 1].Items.Count > 0)
                    priority = ((lb[box - 1].Items[lb[box - 1].Items.Count - 1] as ListItem).Priority + 1);
                add.AddAttribute("Priority", priority.ToString());
                XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                    "@xml", doc.OuterXml
                }, true);
                if (xn == null || xn.Name.LocalName.Contains("Error"))
                {
                    throw new Exception("category failed, " + xn.Value);
                }
                repopulate(box);
                tb[box - 1].Text = "";
            }
        }

        bool canCreate()
        {
            switch (AssetTypeID)
            {
                case 1:
                    if (lb[0].SelectedItem == null)
                    {
                        errorMessage("You must select a valid Data Set");
                        return false;
                    }
                    break;
                case 2:
                case 3:
                case 4:
                case 11:
                case 15:
                    if (lb[0].SelectedItem == null || lb[1].SelectedItem == null || lb[2].SelectedItem == null || lb[3].SelectedItem == null)
                    {
                        errorMessage("You must select a valid Data Set, Category, Sub Category & Category 2");
                        return false;
                    }
                    break;
                case 5:
                    if (lb[0].SelectedItem == null || lb[3].SelectedItem == null)
                    {
                        errorMessage("You must select a valid Data Set & Category 2");
                        return false;
                    }
                    break;
            }
            return true;
        }

        public void addNew()
        {
            if (!canCreate()) return;
            resetTranslation();
            AssetControls.assetControl ac = loadAsset("new");
            if (ac != null)
            {
                if (textBox5.Text != "") ac.expert.InnerText = textBox5.Text;
                ac.setNew();
            }
        }

        public void resetTranslation()
        {
            if (Window1.EditTranslation || Window1.ShowTranslation)
            {
                form.rtbTranslation.IsChecked = false;
                form.rtbShowTranslation.IsChecked = false;
                form.Change_ShowTranslation(null, null);
            }
        }

        private void errorMessage(string p)
        {
            System.Windows.Forms.MessageBox.Show(p);
        }

        private void deleteClick(object sender, RoutedEventArgs e)
        {
            int box = getBox(sender);

            DeleteAssets(box);
        }

        public void DeleteAssets(int box)
        {
            if (lb[box - 1].SelectedIndex > -1)
            {
                for (int cc = box + 1; cc < 6; cc++)
                {
                    if (cc != 4 && lb[cc - 1].Items.Count > 0)
                    {
                        MessageBox.Show("Cannot delete, as this asset has child assets");
                        return;
                    }
                }

                XmlDocument doc = getAssetXml("delete");
                addCategories(doc);
                doc.DocumentElement.AddAttribute("boxid", box.ToString());
                doc.DocumentElement.AddAttribute("assettype", AssetTypeID.ToString());

                foreach (var item in lb[box - 1].SelectedItems)
                {
                    ListItem li = item as ListItem;
                    XmlElement add = doc.DocumentElement.AddElement("Delete", "");
                    if (AssetTypeID == 12) add.AddAttribute("id", li.Value);
                    else add.AddAttribute("id", li.ID.ToString());
                }

                List<string> prms = new List<string>(new string[] { "@xml", doc.OuterXml });
                if (AssetTypeID == 12) prms.AddRange(new string[] { "@AssetTypeID", "12" });

                XElement xn = DataAccess.getData("ab_UpdateAsset", prms.ToArray(), true);
                bool result = ParseDeleteResults(xn);
                if (result)
                {
                    MessageBoxResult mbr = MessageBox.Show("This will delete all assets selected permanently and they " +
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
                    }
                    else result = false;
                }
                if (!result) return;
                repopulate(box);
            }
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

                string s = "The asset could not be deleted because of the following reasons:\r\n\r\n";

                foreach (string re in rr)
                    s += string.Concat("\t- ", re, "\r\n");

                MessageBox.Show(s);
                return false;
            }
            return true;
        }

        int listWidth = 300;

        private void listClick(object sender, RoutedEventArgs e)
        {
            if (!canCreate()) return;

            if (form.listPanel.Visibility == Visibility.Visible)
            {
                closeList();
            }
            else
            {
                openList();
            }
        }

        bool listOpen = false;

        public void openList()
        {
            form.listTextBox.Text = "";
            TextAdorner ta = new TextAdorner(form.listTextBox, "Please enter each individual \nasset on a new line, then \nclick “Create Assets”.", Colors.LightGray);
            if (textAdorners.ContainsKey(form.listTextBox.Name)) textAdorners[form.listTextBox.Name] = ta;
            else textAdorners.Add(form.listTextBox.Name, ta);
            AdornerLayer al = form.listTextBox.clearAdornerLayer();
            al.Add(ta);

            form.listPanel.Visibility = Visibility.Visible;
            ThicknessAnimation da = new ThicknessAnimation();
            da.From = new Thickness(0);
            da.To = new Thickness(0, 0, listWidth, 0);
            da.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            this.BeginAnimation(MarginProperty, da);

            if (form.assetCanvas.Children.Count > 0)
            {
                DockPanel f = loadedAsset.AssetDockPanel;
                f.BeginAnimation(MarginProperty, da);
            }

            DoubleAnimation wa = new DoubleAnimation();
            wa.From = 0;
            wa.To = listWidth;
            wa.Duration = da.Duration;
            form.listPanel.BeginAnimation(WidthProperty, wa);

            listOpen = true;
        }

        public void closeList()
        {
            AdornerLayer al = form.listTextBox.clearAdornerLayer();
            ThicknessAnimation da = new ThicknessAnimation();
            da.To = new Thickness(0);
            da.From = new Thickness(0, 0, listWidth, 0);
            da.Duration = new Duration(TimeSpan.FromSeconds(0.3));

            if (form.assetCanvas.Children.Count > 0)
            {
                DockPanel f = loadedAsset.AssetDockPanel;
                f.BeginAnimation(MarginProperty, da);
            }


            this.BeginAnimation(MarginProperty, da);
            form.listPanel.Visibility = Visibility.Hidden;
            listOpen = false;
        }

        private void editClick(object sender, RoutedEventArgs e)
        {
            int box = getBox(sender);
            if (lb[box - 1].SelectedItem == null)
            {
                System.Windows.Forms.MessageBox.Show("Must select an entry from the list to edit", "Error");
                return;
            }
            if (box == 5)
            {
                if (loadedAsset == null) loadAsset(listBox5.SelectedValue.ToString());
                if (loadedAsset.asset["Table"]["Locked"].InnerText == "true") return;
                loadedAsset.goEdit();
            }
            else
            {
                ListItem li = lb[box - 1].SelectedItem as ListItem;
                string ct = AssetTypeID + ":" + box;
                if (Window1.EditTranslation && catTrans.ContainsKey(ct))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.AppendChild(doc.CreateElement("root"));
                    XmlElement add = doc.DocumentElement.AddElement(catFields[ct], tb[box - 1].Text);
                    DataAccess.setLanguage(catTrans[ct], li.Value, doc, Window1.TranslationLanguage);

                    repopulate(box);
                    lb[box - 1].SelectedItem = selectItem(lb[box - 1], li.ID);
                    lb[box - 1].Focus();
                }
                else if (tb[box - 1].Text != "")
                {
                    XmlDocument doc = getAssetXml("edit");
                    doc.DocumentElement.AddAttribute("boxid", box.ToString());
                    doc.DocumentElement.AddAttribute("assettype", AssetTypeID.ToString());
                    XmlElement add = doc.DocumentElement.AddElement("Edit", tb[box - 1].Text);

                    add.AddAttribute("Priority", li.Priority.ToString());
                    add.AddAttribute("ID", li.ID.ToString());
                    add.AddAttribute("OldValue", li.Value);

                    XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                        "@xml", doc.OuterXml
                    }, true);
                    if (xn == null || xn.Name.LocalName.Contains("Error"))
                    {
                        throw new Exception("category failed, " + xn.Value);
                    }

                    repopulate(box);
                    lb[box - 1].SelectedItem = selectItem(lb[box - 1], li.ID);
                    lb[box - 1].Focus();
                }
            }
        }

        //bool cancelloadAsset = false;

        private void loadAsset(object sender, SelectionChangedEventArgs e)
        {
            if (form == null) return;

            if (listOpen) closeList();
            if (listBox5.SelectedItems.Count > 0) imgGrab.Visibility = Visibility.Visible; else imgGrab.Visibility = Visibility.Hidden;
            setButtons();

            if (listBox5.SelectedItem != null && listBox5.SelectedItems.Count == 1)
            {
                listBox5.ScrollIntoView(listBox5.SelectedItem);
                if (Keyboard.Modifiers == ModifierKeys.None)
                    if (AssetTypeID == 0 || AssetTypeID == 12)
                        loadAsset((lb[4].SelectedItem as ListItem).Value.ToString());
                    else
                        loadAsset((lb[4].SelectedItem as ListItem).ID.ToString());
            }
        }

        public AssetControls.assetControl loadAsset(string ID)
        {
            return loadAsset(ID, "", "");
        }

        //bool loadingAsset = false;

        public AssetControls.assetControl loadAsset(string ID, string search, string fromID)
        {
            System.Diagnostics.Trace.WriteLine(string.Format("Starting loadAsset({0},{1},{2})", ID, search, fromID));
            Window1.window.HideBrowsers();
            if (ID == AssetID && loadedAsset != null && form.assetCanvas.Children.Count > 0) return loadedAsset;
            bool reedit = false;

            if (IsEditing)
            {
                if (Window1.AutoSave)
                {
                    if (!loadedAsset.Save()) return null;
                    reedit = true;
                }
                else
                {
                    System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("There is another asset currently being edited. Do you want to save the changes?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (!loadedAsset.Save())
                            return null;
                    }
                    else if (dr == System.Windows.Forms.DialogResult.Cancel) return null;
                }
            }

            XmlNode xe = DataAccess.getDataNode("ab_GetAsset", new string[] {
                "@AssetTypeID",_AssetTypeID.ToString(),
                "@AssetID",ID
            }, false);

            if (xe["Table"] == null) return null;
            //StringBuilder sb = new StringBuilder(xe.Transform("asset.xsl", null));

            AssetControls.assetControl ac = null;
            if (AssetTypeID == 0) ac = new AssetBuilder.AssetControls.Title(xe);
            if (AssetTypeID == 1) ac = new AssetBuilder.AssetControls.Algo(xe);
            if (AssetTypeID == 2) ac = new AssetBuilder.AssetControls.Question(xe);
            if (AssetTypeID == 3) ac = new AssetBuilder.AssetControls.Answer(xe);
            if (AssetTypeID == 4) ac = new AssetBuilder.AssetControls.Conclusion(xe);
            if (AssetTypeID == 5) ac = new AssetBuilder.AssetControls.Bullet(xe);
            if (AssetTypeID == 11) ac = new AssetBuilder.AssetControls.Map(xe);
            if (AssetTypeID == 12) ac = new AssetBuilder.AssetControls.TextAsset(xe);
            if (AssetTypeID == 13) ac = new AssetBuilder.AssetControls.Group(xe);
            if (AssetTypeID == 15) ac = new AssetBuilder.AssetControls.CMAP(xe);
            if (ac == null) return ac;

            if (ID != "new")
            {
                AddRecentItem(ID, ac.expert.InnerText, search, fromID);
            }

            ac.Margin = new Thickness(0, 0, 0, 0);
            loadedAsset = ac;
            AssetID = ID;
            ac.cat = this;
            form.assetCanvas.Children.Clear();
            form.assetCanvas.Children.Add(ac);
            if (loadedAsset != null && reedit) loadedAsset.goEdit();
            if (Window1.AllowProperties) updatePropertyCounts();
            return ac;
        }

        private void updatePropertyCounts()
        {
            Task t = new Task(() => runAsync());
            t.Start();
        }

        object lockobj = new object();

        private void runAsync()
        {
            lock (lockobj)
            {
                string id = AssetID;
                AssetControls.AssetType AssetType = loadedAsset.assetType;
                string SearchPropertyType = (AssetType == AssetControls.AssetType.Algo ? "[AT][lr][ga][on]%" : AssetType.ToString()) + ":" + AssetID;
                XElement xn = DataAccess.getData("dsp_SearchProperties", "@PropertyType", SearchPropertyType);
                XElement defaultData = DataAccess.getData("dsp_GetProperty", new string[] { "@PropertyType", AssetType.ToString(), "@DataID", AssetID.ToString() }, true);
                int defaultcount = defaultData.Elements().Count();
                int propcount = xn.Elements().Count();
                int instancecount = xn.Elements("Table").Select(f => f.Element("DataID").Value).Distinct().Count();

                if (id == AssetID)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        if (loadedAsset != null && loadedAsset.bs != null)
                        {
                            loadedAsset.bs.PropertyCount = instancecount.ToString();
                            if (defaultcount > 0) loadedAsset.bs.DefaultProperties = true;
                        }
                    }));
                }
            }
        }

        private void AddRecentItem(string ID, string expert, string Search, string FromID)
        {
            recentItem ri = new recentItem(_AssetTypeID, ID, expert, Search, FromID, this);
            for (int i = 0; i < Window1.RecentAssets.Items.Count; i++)
            {
                if (Window1.RecentAssets.Items[i] is recentItem)
                {
                    recentItem il = Window1.RecentAssets.Items[i] as recentItem;
                    if (il.AssetID == ri.AssetID && il.AssetType == ri.AssetType && il.Search == ri.Search)
                    {
                        Window1.RecentAssets.Items.RemoveAt(i--);
                    }
                }
            }
            if (Window1.RecentAssets.Items.Count > 19) Window1.RecentAssets.Items.RemoveAt(Window1.RecentAssets.Items.Count - 1);
            Window1.RecentAssets.Items.Insert(0, ri);
        }

        private void listBox5_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (form == null)
            {
                if (loadedAsset is AssetControls.Conclusion)
                {
                    Point p = e.GetPosition(listBox5);
                    DependencyObject dp = (DependencyObject)listBox5.InputHitTest(p);
                    ListBoxItem li = getListItem(dp);
                    if (li != null)
                    {
                        AssetControls.Conclusion con = loadedAsset as AssetControls.Conclusion;
                        if (con.cat.IsEditing) con.addBulletToEnd(getItems());
                    }
                }
                return;
            }
            if (lb[4].SelectedValue == null) return;
            string ID;
            if (AssetTypeID == 12)
            {
                ID = ((ListItem)lb[4].SelectedItem).Value;
            }
            else
            {
                ID = lb[4].SelectedValue.ToString();
            }
            fullLoadAsset(ID);
        }

        public void fullLoadAsset(string ID)
        {
            Window1.setStatus("Accessing Data...");
            AssetControls.assetControl ac = loadAsset(ID);
            if (ac == null)
            {
                Window1.setStatus("");
                return;
            }
            int topcat = -1;
            foreach (var item in ac.cats)
            {
                var cat = ac.asset["Table"][ac.cats[item.Key]];
                if (cat == null) continue;
                int cid = int.Parse(cat.InnerText);
                if ((lb[item.Key].SelectedItem == null || (lb[item.Key].SelectedItem as ListItem).ID != cid) && item.Key > topcat) topcat = item.Key;
            }
            noAssets = true;
            for (int i = 0; i < 4; i++)
            {
                if (i == topcat) noAssets = false;
                if (ac.cats.ContainsKey(i))
                {
                    var cat = ac.asset["Table"][ac.cats[i]];
                    if (cat == null) continue;
                    int cid = int.Parse(ac.asset["Table"][ac.cats[i]].InnerText);
                    if (lb[i].SelectedItem == null || (lb[i].SelectedItem as ListItem).ID != cid) lb[i].SelectedValue = cid;
                }
            }
            noAssets = false;
            int id;
            if (int.TryParse(ID, out id)) lb[4].SelectedValue = id;
            Window1.setStatus("");
        }

        public void LoadAssetFromList(string ID, string search, string fromID)
        {
            string txtsearch = search;
            Window1.setStatus("Accessing Data...");
            qcat.currentSearch = @"(?<![0-9])" + fromID + @"(?![0-9])";
            AssetControls.assetControl ac = loadAsset(ID, search, fromID);
            List<string> prms = new List<string>(new string[] { "@boxid", "0", "@assettypeid", _AssetTypeID.ToString(), "@searchword", search, "@count", "0" });
            if (search.StartsWith("@") && search.Contains(":"))
            {
                prms[5] = "";
                string[] newprms = search.Split(':');
                if (newprms.Length > 1)
                {
                    txtsearch = search.Replace("@", "").Replace("id", "");
                    prms.AddRange(new string[] { newprms[0], newprms[1] });
                }
            }
            populate(prms.ToArray());
            if (ac == null)
            {
                Window1.setStatus("");
                return;
            }
            textBox5.Text = txtsearch;
            lb[4].SelectedValue = int.Parse(ID);
            Window1.setStatus("");
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public int[] getCats()
        {
            int[] catids = {
                             lb[0].SelectedItem == null ? 0 : (lb[0].SelectedItem as ListItem).ID,
                             lb[1].SelectedItem == null ? 0 : (lb[1].SelectedItem as ListItem).ID,
                             lb[2].SelectedItem == null ? 0 : (lb[2].SelectedItem as ListItem).ID,
                             lb[3].SelectedItem == null ? 0 : (lb[3].SelectedItem as ListItem).ID,
                             lb[4].SelectedItem == null ? 0 : (lb[4].SelectedItem as ListItem).ID
                         };
            return catids;
        }

        private string[] getText()
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

        public XmlDocument getAssetXml(string command, string tableName)
        {
            XmlDocument doc;
            XmlElement root;
            CreateUpdateXml(command, out doc, out root);
            insertSelectedAssets(tableName, root);
            return doc;
        }

        public XmlDocument getAssetXml(string command)
        {
            XmlDocument doc;
            XmlElement root;
            CreateUpdateXml(command, out doc, out root);
            return doc;
        }

        public XmlDocument getAssetXml(string command, bool allListed)
        {
            XmlDocument doc;
            XmlElement root;
            CreateUpdateXml(command, out doc, out root);
            if (allListed) insertAllAssets(tables[AssetTypeID], root); else insertSelectedAssets(tables[AssetTypeID], root);
            return doc;
        }

        public XmlDocument getAssetXml(string command, string tableName, string stringData)
        {
            XmlDocument doc;
            XmlElement root;
            CreateUpdateXml(command, out doc, out root);
            insertSelectedAssets(tableName, root, stringData);
            return doc;
        }

        private void insertSelectedAssets(string tableName, XmlElement root, string stringData)
        {
            string[] s = stringData.Split(')')[0].Split('(')[1].Split(',');
            foreach (var item in s)
            {
                XmlNode table = root.AppendChild(root.OwnerDocument.CreateElement(tableName));
                XmlAttribute id = root.OwnerDocument.CreateAttribute("id");
                id.Value = item;
                table.Attributes.Append(id);
            }
        }

        public static void CreateUpdateXml(string command, out XmlDocument doc, out XmlElement root)
        {
            doc = new XmlDocument();
            root = doc.CreateElement("root");
            doc.AppendChild(root);
            root.Attributes.Append(doc.CreateAttribute("command")).Value = command;
        }

        private void insertSelectedAssets(string tableName, XmlElement root)
        {
            foreach (var item in listBox5.SelectedItems)
            {
                if (AssetTypeID == 0 || AssetTypeID == 12)
                    InsertAsset(tableName, root, (item as ListItem).Value.ToString());
                else
                    InsertAsset(tableName, root, (item as ListItem).ID.ToString());
            }
        }

        public void InsertAsset(string tableName, XmlElement root, string assetid)
        {
            XmlNode table = root.AppendChild(root.OwnerDocument.CreateElement(tableName));
            XmlAttribute id = root.OwnerDocument.CreateAttribute("id");
            id.Value = assetid;
            if (assetid == AssetID)
                table.Attributes.Append(root.OwnerDocument.CreateAttribute("open")).Value = "True";
            table.Attributes.Append(id);
        }

        private void insertAllAssets(string tableName, XmlElement root)
        {
            foreach (var item in listBox5.Items)
            {
                XmlNode table = root.AppendChild(root.OwnerDocument.CreateElement(tableName));
                XmlAttribute id = root.OwnerDocument.CreateAttribute("id");
                if (AssetTypeID == 0 || AssetTypeID == 12)
                    id.Value = (item as ListItem).Value.ToString();
                else
                    id.Value = (item as ListItem).ID.ToString();
                table.Attributes.Append(id);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mi = (sender as MenuItem);
            if (mi != null && mi.Header.ToString() == "Open Visio")
            {
                string filename = mi.CommandParameter.ToString();
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    OpenVisio(filename);
                }));
                return;
            }
            var cp = mi?.CommandParameter.ToString().Split(',');
            if (cp[0] == "20")
            {
                form.Copy();
                return;
            }
            if (IsEditing)
            {
                System.Windows.Forms.DialogResult dr = System.Windows.Forms.MessageBox.Show("There is an asset currently being edited. Do you want to save the changes?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    if (!loadedAsset.Save())
                        return;
                }
                else if (dr == System.Windows.Forms.DialogResult.Cancel) return;
            }
            Window1.setStatus("Accessing Data...");
            saveCats();
            if (form != null) form.assetCanvas.Children.Clear();
            if (listBox5.SelectedValue == null) return;
            int id = (int)listBox5.SelectedValue;

            if (Window1.SearchTranslation) form.DisableSearchTranslation();
            string search = "";
            if (int.Parse(cp[0]) < 15)
            {
                string[] prms = new string[] {
                    "@boxid", cp[0],
                    "@assettypeid", AssetTypeID.ToString(),
                    "@searchword", id.ToString()
                };
                populate(prms);
                textBox5.Text = "";
            }
            else
            {
                search = prmentry[AssetTypeID + 3] + ":" + id;
                populate(0, int.Parse(cp[1]), search);
            }

            _AssetTypeID = int.Parse(cp[1]);
            textBox5.Text = search;
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
            Window1.RadioToggle(form.assetGroup, _AssetTypeID);
            Window1.clearStatus();
        }

        public static void OpenVisio(string filename)
        {
            try
            {
                var vis = VisioInterface.GetVisio();
                if (vis == null) vis = new Visio.Application();
                vis.Documents.Open(filename);
                //Process.Start("Start Visio \"" + mi.CommandParameter.ToString() + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + filename, "Warning", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            return;
        }

        public readonly string[] tables = { "TITLE", "ALGO_START", "QUESTION", "ANSWER", "RECOMMENDATION", "BULLET", "BULLET_USE", "CATEGORY", "", "", "", "MAP", "TEXTASSET" };

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            Window1.setStatus("Accessing Data...");
            ContextMenu cm = sender as ContextMenu;
            Dictionary<string, MenuItem> MenuItems = new Dictionary<string, MenuItem>();
            foreach (MenuItem item in cm.Items)
            {
                item.IsEnabled = false;
                MenuItems.Add(item.Header.ToString(), item);
            }
            XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                "@xml", getAssetXml("context", tables[AssetTypeID]).OuterXml
            }, false);
            var mi = from item in xn.Elements() select item.Element("MenuItem").Value;
            foreach (var item in mi)
            {
                var split = item.Split('|');
                if (MenuItems.ContainsKey(split[0])) MenuItems[split[0]].IsEnabled = true;
                if (split.Length > 1) MenuItems[split[0]].CommandParameter = string.Join("|", split.Skip(1));
            }
            if (listBox5.SelectedItems.Count > 0) MenuItems["Copy"].IsEnabled = true;
            Window1.clearStatus();
        }

        private void lb_DragOver(object sender, DragEventArgs e)
        {
            if (!Window1.CanMoveAssets) return;
            Window1.setStatus("");
            ListBox listbox = sender as ListBox;
            Point p = e.GetPosition(listbox);
            DependencyObject dp = (DependencyObject)listbox.InputHitTest(p);
            ListBoxItem li = getListItem(dp);
            if (li != null)
            {
                listbox.SelectedItem = li.Content;
                string s = "";
                for (int i = 0; i < 4; i++)
                {
                    s += (lb[i].SelectedValue == null ? "x" : lb[i].SelectedValue.ToString()) + " ";
                }
                //Window1.setStatus(s);
            }
            else
            {
                ScrollBar sb = getScrollbar(dp);
                if (sb != null)
                {
                    if (sb.ActualHeight - p.Y < 20) listbox.SelectedIndex++;
                    else if (p.Y < 20 && listbox.SelectedIndex > 0) listbox.SelectedIndex--;
                    //listbox.Dispatcher.Invoke(Window1.EmptyDelegate, null);
                }
            }
        }

        ListBoxItem getListItem(DependencyObject dp)
        {
            if (dp == null) return null;
            DependencyObject p = VisualTreeHelper.GetParent(dp);
            if (p == null || p is ListBoxItem) return p as ListBoxItem;
            return getListItem(p);
        }

        ScrollBar getScrollbar(DependencyObject dp)
        {
            DependencyObject p = VisualTreeHelper.GetParent(dp);
            if (p == null || p is ScrollBar) return p as ScrollBar;
            return getScrollbar(p);
        }

        private void lb_Drop(object sender, DragEventArgs e)
        {
            if (!Window1.CanMoveAssets) return;
            string message = "You must select all available categories";
            bool ret = false;
            if (lb[0].SelectedValue == null) ret = true;
            switch (AssetTypeID)
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
                errorMessage(message);
                return;
            }
            Window1.setStatus("Updating Data...");
            XmlDocument doc = getAssetXml("move", tables[AssetTypeID], e.Data.GetData(typeof(string)).ToString());
            addCategories(doc);
            bool doit = false;
            if (!Window1.DisableComments)
            {
                PromptWindow pw = new PromptWindow(false);
                pw.Owner = form;
                form.disableForm();
                bool? res = pw.ShowDialog();
                form.enableForm();
                if (res == true)
                    doc.DocumentElement.SetAttribute("comment", pw.Comment);
                doit = res == true;
            }
            if (Window1.DisableComments || doit)
            {
                DataAccess.getData("ab_UpdateAsset", new string[] {
                    "@xml", doc.OuterXml
                }, true);
                populate(null as ListBox);
            }
            Window1.setStatus("");
        }

        private void addCategories(XmlDocument doc)
        {
            if (lb[0].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("dataid")).Value = lb[0].SelectedValue.ToString();
            if (lb[1].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("catid")).Value = lb[1].SelectedValue.ToString();
            if (lb[2].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("subcatid")).Value = lb[2].SelectedValue.ToString();
            if (lb[3].SelectedValue != null) doc.DocumentElement.Attributes.Append(doc.CreateAttribute("cat2id")).Value = lb[3].SelectedValue.ToString();
        }

        internal object selectItem(ListBox l, int p)
        {
            foreach (var item in l.Items)
            {
                if (item is ListItem && (item as ListItem).ID == p)
                {
                    return item;
                }
            }
            return null;
        }

        private void lb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox l = sender as ListBox;
            if (l.SelectedItem == null) return;
            ListItem li = l.SelectedItem as ListItem;
            int idx = Grid.GetColumn(l);
            if (Window1.EditTranslation && !string.IsNullOrEmpty(li.Language))
            {
                tb[idx].Text = li.Language;
            }
            else
                tb[idx].Text = li.Value;
        }

        private void btnTitle_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = getAssetXml("title");
            doc.DocumentElement.AddAttribute("catid", (lb[1].SelectedItem as ListItem).ID.ToString());
            doc.DocumentElement.AddAttribute("CategoryTypeID", (lb[1].SelectedItem as ListItem).CategoryTypeID.ToString());
            form.setConclusionTitle(doc);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int box = getBox(sender);
            if (lb[box - 1].SelectedItem == null)
                System.Windows.Forms.MessageBox.Show("Must select an entry from the list to edit", "Error");
            else if (lb[box - 1].SelectedIndex <= 0)
                System.Windows.Forms.MessageBox.Show("Item is already at the top of the list", "Error");
            else
                changePriority(box, lb[box - 1].Items[lb[box - 1].SelectedIndex - 1] as ListItem, lb[box - 1].SelectedItem as ListItem, (lb[box - 1].SelectedItem as ListItem).ID);
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int box = getBox(sender);
            if (lb[box - 1].SelectedItem == null)
                System.Windows.Forms.MessageBox.Show("Must select an entry from the list to edit", "Error");
            else if (lb[box - 1].SelectedIndex >= lb[box - 1].Items.Count - 1)
                System.Windows.Forms.MessageBox.Show("Item is already at the bottom of the list", "Error");
            else
                changePriority(box, lb[box - 1].SelectedItem as ListItem, lb[box - 1].Items[lb[box - 1].SelectedIndex + 1] as ListItem, (lb[box - 1].SelectedItem as ListItem).ID);
        }

        private void changePriority(int box, ListItem moveUp, ListItem moveDown, int id)
        {
            XmlDocument doc = getAssetXml("edit");
            doc.DocumentElement.AddAttribute("boxid", box.ToString());
            doc.DocumentElement.AddAttribute("assettype", AssetTypeID.ToString());
            addCategories(doc);

            XmlElement add = doc.DocumentElement.AddElement("Edit", moveDown.Value);
            add.AddAttribute("Priority", (moveDown.Priority - 1).ToString());
            add.AddAttribute("ID", moveDown.ID.ToString());

            XmlElement sadd = doc.DocumentElement.AddElement("Edit", moveUp.Value);
            sadd.AddAttribute("Priority", moveDown.Priority.ToString());
            sadd.AddAttribute("ID", moveUp.ID.ToString());

            XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                "@xml", doc.OuterXml
            }, true);
            if (xn == null || xn.Name.LocalName.Contains("Error"))
            {
                throw new Exception("category failed, " + xn.Value);
            }

            repopulate(box);
            lb[box - 1].SelectedItem = selectItem(lb[box - 1], id);
            lb[box - 1].Focus();
        }

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            setButtons();
            TextBox tb = sender as TextBox;
            updateAdorner(tb);
            if (tb.Name.StartsWith("textBox"))
            {
                int boxid = int.Parse(tb.Name.Substring(7));
                if (boxid < 5)
                {
                    FilterList(boxid);
                }
            }
        }

        private void FilterList(int boxid)
        {
            for (int i = 0; i < lb[boxid - 1].Items.Count; i++)
            {
                if (lb[boxid - 1].Items[i] is ListItem)
                {
                    ListItem li = lb[boxid - 1].Items[i] as ListItem;
                    bool selected = lb[boxid - 1].SelectedIndex == i;
                    string ct = AssetTypeID + ":" + boxid;

                    if (Window1.ShowTranslation && catTrans.ContainsKey(ct))
                        li.Hidden = !selected && ((filter[boxid - 1] && Window1.SearchTranslation && li.Language.IndexOf(tb[boxid - 1].Text, StringComparison.OrdinalIgnoreCase) == -1)
                            || (filter[boxid - 1] && !Window1.SearchTranslation && li.Value.IndexOf(tb[boxid - 1].Text, StringComparison.OrdinalIgnoreCase) == -1));
                    else
                        li.Hidden = !selected && filter[boxid - 1] && li.Value.IndexOf(tb[boxid - 1].Text, StringComparison.OrdinalIgnoreCase) == -1;
                }
            }
        }

        private void imgGrab_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(listBox5, getItems(), DragDropEffects.Move);
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            SetAdorners();
        }

        ListItem clickedAsset = null;

        private void listBox5_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            clickedAsset = null;
            ListBox listbox = sender as ListBox;
            Point p = e.GetPosition(listbox);
            DependencyObject dp = (DependencyObject)listbox.InputHitTest(p);
            ListBoxItem li = getListItem(dp);
            if (li != null && li.Content is ListItem) clickedAsset = li.Content as ListItem;
            if (Keyboard.Modifiers != ModifierKeys.None || listbox.SelectedItems.Count == 1) return;
            if (li != null && listbox.SelectedItems.Contains(li.Content)) e.Handled = true; else e.Handled = false;
        }

        private void listBox5_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (clickedAsset == null || form == null) return;
            if (listOpen) closeList();
            if (listBox5.SelectedItems.Count > 0) imgGrab.Visibility = Visibility.Visible; else imgGrab.Visibility = Visibility.Hidden;
            setButtons();
            //if (e.AddedItems.Count == 0) return;
            //if (lb[4].SelectedValue == null || cancelloadAsset) return;
            string ID = ((AssetTypeID == 0 || AssetTypeID == 12) ? clickedAsset.Value : clickedAsset.ID.ToString());
            AssetControls.assetControl ac = loadAsset(ID);
            //listBox5.SelectedItems.Clear();
            listBox5.SelectedItem = clickedAsset;
        }

        bool showall = false;

        private void ShowAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            showall = true;
            populate(null as ListBox);
        }

        private void filter_CheckedChanged(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            if (tb.Name.StartsWith("filter"))
            {
                int boxid = int.Parse(tb.Name.Substring(6));
                if (boxid < 5)
                {
                    filter[boxid - 1] = (bool)tb.IsChecked;
                    FilterList(boxid);
                }
            }
            SetAdorners();
        }

        internal void SetCategoryLanguage()
        {
            string keystart = AssetTypeID + ":";
            if (Window1.MultiTextLanguage)
            {
                SetCategoryLanguage(catTrans.Where(f => f.Key.StartsWith(keystart)).Select(f => int.Parse(f.Key.Substring(keystart.Length))));
                return;
            }
            foreach (var item in catTrans.Where(f => f.Key.StartsWith(keystart)))
            {
                int box = int.Parse(item.Key.Substring(keystart.Length));
                int ct = item.Value;
                foreach (ListItem li in lb[box - 1].Items)
                {
                    li.Language = Window1.ShowTranslation ? DataAccess.getCategoryLanguage(item.Value, li.Value, Window1.TranslationLanguage) : "";
                }
            }
        }

        internal void SetCategoryLanguage(IEnumerable<int> boxes)
        {
            int box = 0;
            var assets = boxes.SelectMany(f => lb[(box = f) - 1].Items.OfType<ListItem>()).Select(f => new { Box = box, CT = catTrans[AssetTypeID + ":" + box], Item = f });
            XDocument doc = new XDocument();
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
                XmlNode trans = DataAccess.getLanguage(int.MaxValue, doc.ToString(), Window1.TranslationLanguage);
                foreach (XmlNode item in trans.SelectNodes("Asset[@Language]"))
                {
                    int sBox = int.Parse(item.Attributes["Box"].Value);
                    int ID = int.Parse(item.Attributes["ID"].Value);
                    assets.First(f => f.Box == sBox && f.Item.ID == ID).Item.Language = item.Attributes["Language"].Value;
                }
            }
        }

        private void ExecutedCopy(object sender, ExecutedRoutedEventArgs e)
        {
            Window1.window.Copy();
        }

        private void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }

    class ListItem : INotifyPropertyChanged
    {
        public string ToolTip
        {
            get
            {
                if (Audit == null) return ID.ToString();
                else return $"{ID}\n{Audit}";
            }
        }
        public int ID { get; set; }
        public int NotInUse { get; set; }

        public bool IsFlagged { get { return Flags > 0; } }

        public string IconImage { get; set; } = "/images/flag.png";

        public int Flags
        {
            get
            {
                if (qcat.AssetFlags == null) return 0;
                var typeid = Window1.window.qcat1.AssetTypeID;
                var flags = qcat.AssetFlags.ContainsKey(typeid) ? (qcat.AssetFlags[typeid].ContainsKey(ID) ? qcat.AssetFlags[typeid][ID] : 0) : 0;
                if (flags.In(1, 2, 3))
                {
                    IconImage = $"/images/{flags}.png";
                    //NotifyPropertyChanged("IconImage");
                }
                return flags;
            }
        }

        private int _categoryTypeId;
        public int CategoryTypeID
        {
            get { return _categoryTypeId; }
            set
            {
                _categoryTypeId = value;

                NotifyPropertyChanged("CategoryTypeID");
            }
        }

        private AuditItem _audit;
        public AuditItem Audit
        {
            get { return _audit; }
            set
            {
                _audit = value;
                NotifyPropertyChanged("Audit");
                NotifyPropertyChanged("ToolTip");
            }
        }

        public string MultID { get; set; }
        public string Value { get; set; }
        public int Priority { get; set; }
        public string _Language;
        public string Language
        {
            get { return _Language; }
            set
            {
                if (_Language != value)
                {
                    if (!string.IsNullOrEmpty(_Language))
                    {
                        _Language = "";
                        NotifyPropertyChanged("HasLanguage");
                    }
                    _Language = value;
                    NotifyPropertyChanged("HasLanguage");
                }
            }
        }
        public bool HasLanguage { get { return !string.IsNullOrEmpty(Language); } }
        private bool _Hidden;
        public bool Hidden
        {
            get { return _Hidden; }
            set { _Hidden = value; NotifyPropertyChanged("Hidden"); }
        }

        public object Content { get { return Value; } }

        public override string ToString()
        {
            return Value;
        }

        public string ToCopyString()
        {
            return ID + "\t" + Value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
