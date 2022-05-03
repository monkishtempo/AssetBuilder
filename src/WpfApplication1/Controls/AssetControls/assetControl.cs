using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using System.Xml.Linq;
using AssetBuilder.Controls;
using Visio = Microsoft.Office.Interop.Visio;
using System.Collections.ObjectModel;
using WpfXmlGrid;
using System.Reflection;
using Diva.Controls.Simple;
using StringCompare;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssetBuilder.Extensions;

namespace AssetBuilder.AssetControls
{

    public class assetControl : UserControl
    {
        public qcat cat;
        internal XmlNode asset;
        internal XmlNode originalAsset;
        internal XmlNode translation;
        internal XmlNode originalTranslation;
        internal XmlNode expert;
        internal XElement properties;
        internal string[] newScript = { "First select the answer type." };
        internal string[] scriptControls = { "cmbAnsType" };
        internal AssetType assetType { get; set; }
        internal string tableName;
        internal Dictionary<string, TextBox> TextChildren = new Dictionary<string, TextBox>();
        internal Dictionary<string, DataGrid> GridChildren = new Dictionary<string, DataGrid>();
        internal List<TextBox> SpellChildren = new List<TextBox>();
        internal List<TextBox> LanguageChildren = new List<TextBox>();
        internal Dictionary<string, ComboBox> ComboChildren = new Dictionary<string, ComboBox>();
        internal Dictionary<string, CheckBox> CheckChildren = new Dictionary<string, CheckBox>();
        internal Dictionary<string, Button> ButtonChildren = new Dictionary<string, Button>();
        internal ButtonStrip bs;
        internal usageControl usage;
        //internal nlwControl nlw;
        internal findControl find;
        internal Dictionary<int, string> cats = new Dictionary<int, string>();
        internal XElement algos;
        internal Dictionary<string, IList<DifferenceSets<Match>>> Changes;
        private Image CompareLanguage;

        public DockPanel AssetDockPanel { get; set; }

        public int? AssetID
        {
            get
            {
                int assetid;
                XmlNode idcolumn;
                if (asset != null && (idcolumn = asset.SelectSingleNode("Table/*")) != null &&
                    int.TryParse(idcolumn.InnerText, out assetid)) return assetid;
                return null;
            }
        }

        public bool EditMode
        {
            get { return bs.btnEdit.Visibility != Visibility.Visible; }
        }

        public bool IsValid
        {
            get
            {
                bool valid = true;
                foreach (var item in SpellChildren)
                {
                    if (!item.validateTextBox()) return false;
                }
                return valid;
            }
        }

        public bool IsValidLanguage
        {
            get
            {
                bool valid = true;
                foreach (var item in LanguageChildren)
                {
                    if (!item.validateTextBox()) return false;
                }
                return valid;
            }
        }

        public bool IsDesignTime
        {
            get { return DesignerProperties.GetIsInDesignMode(this); }
        }

        static assetControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(assetControl),
                new FrameworkPropertyMetadata(typeof(assetControl)));
        }

        public assetControl()
        {
            IntelListMakers.clearCache();
        }

        public assetControl(XmlNode _asset)
        {
            NLExtensions.errors.Clear();
            this.asset = _asset;
            properties = null;
            updateTranslation();
        }

        Dictionary<string, string[]> emptyLanguageFields = new Dictionary<string, string[]>();

        private void updateTranslation()
        {
            if (Window1.ShowTranslation)
            {
                this.translation = DataAccess.getLanguage(asset, Window1.TranslationLanguage);
                setLanguageDataContext();
                if (CompareLanguage != null) CompareLanguage.Visibility = Visibility.Visible;
            }
            else if (CompareLanguage != null) CompareLanguage.Visibility = Visibility.Hidden;
        }

        private void setLanguageDataContext()
        {
            foreach (TextBox item in LanguageChildren)
            {
                item.DataContext = this.translation;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (IsDesignTime) return;
            bs = this.GetTemplateChild("buttonStrip") as ButtonStrip;
            usage = this.GetTemplateChild("usageDockPanel") as usageControl;
            usage.Close += new RoutedEventHandler(bs_Usage);
            usage.AssetUsage += new RoutedEventHandler(usage_AssetUsage);
            usage.AlgoUsage += new RoutedEventHandler(usage_AlgoUsage);
            usage.LanguageUsage += new RoutedEventHandler(usage_LanguageUsage);
            usage.PropertiesUsage += new RoutedEventHandler(usage_PropertiesUsage);
            usage.AssetID = asset.SelectSingleNode("Table/*[1]").InnerText;
            if (assetType == AssetType.Algo || assetType == AssetType.Bullet) usage.EnableAssetSearch = false;
            //nlw = this.GetTemplateChild("nlwDockPanel") as nlwControl;
            //nlw.Close += new RoutedEventHandler(bs_NLW);
            find = this.GetTemplateChild("findDockPanel") as findControl;
            find.Close += new RoutedEventHandler(bs_Find);
            bs.ApplyTemplate();
            bs.asset = this;
            bs.NaturalLanguage += new RoutedEventHandler(bs_NaturalLanguage);
            bs.Delete += new RoutedEventHandler(bs_Delete);
            bs.Edit += new RoutedEventHandler(btnstrpEdit_Edit);
            bs.Cancel += new RoutedEventHandler(bs_Cancel);
            bs.Close += new RoutedEventHandler(bs_Close);
            bs.Save += new RoutedEventHandler(bs_Save);
            bs.Add += new RoutedEventHandler(bs_Add);
            bs.Usage += new RoutedEventHandler(bs_Usage);
            //bs.NLW += new RoutedEventHandler(bs_NLW);
            bs.Find += new RoutedEventHandler(bs_Find);
            bs.Duplicate += new RoutedEventHandler(bs_Duplicate);
            bs.DeriveAnswer += new RoutedEventHandler(bs_DeriveAnswer);
            bs.DeriveQuestion += new RoutedEventHandler(bs_DeriveQuestion);
            bs.CreateAnswer += new RoutedEventHandler(bs_CreateAnswer);
            bs.Properties += new RoutedEventHandler(bs_Properties);
            bs.Audit += new RoutedEventHandler(bs_Audit);

            CompareLanguage = this.GetTemplateChild("CompareLanguage") as Image;
            if(CompareLanguage != null)
            {
                CompareLanguage.Visibility = Window1.ShowTranslation ? Visibility.Visible : Visibility.Hidden;
                CompareLanguage.MouseDown += delegate
                {
                    Changes = new Dictionary<string, IList<DifferenceSets<Match>>>();
                    foreach (var item in TextChildren)
                    {
                        NLExtensions.validateTextBox(item.Value);
                    }
                };
            }

            DockPanel dp = this.GetTemplateChild("mainPanel") as DockPanel;
            AssetDockPanel = this.GetTemplateChild("assetDockPanel") as DockPanel;
            iterate(dp.Children);
            setButtons(false);
            this.DataContext = asset["Table"];
        }

        private void bs_Audit(object sender, RoutedEventArgs e)
        {
            var a = new AuditTrail();
            cat.form.disableForm();
            a.SetValue(Canvas.LeftProperty, (cat.form.Width - a.Width) / 2);
            a.SetValue(Canvas.TopProperty, 100d);
            cat.form.bubbleCanvas.Children.Add(a);
            a.Cancel.Click += delegate (object o, RoutedEventArgs args)
            {
                cat.form.bubbleCanvas.Children.Clear();
                cat.form.enableForm();
            };
            a.OK.Click += delegate (object o, RoutedEventArgs args)
            {
                var ai = a.getAuditItem();
                ai.Date = DateTime.Now;
                ai.User = Window1.UserName;
                DataAccess.getData("dsp_InsertAudit",
                    "@AuditType", "Approval",
                    "@AuditObject", assetType.ToString(),
                    "@AuditDataID", AssetID.ToString(),
                    "@AuditText", DataAccess.JsonSerialize(ai)
                );
                var li = cat.listBox5.SelectedItem as ListItem;
                if (li != null) li.Audit = ai;
                cat.form.bubbleCanvas.Children.Clear();
                cat.form.enableForm();
            };
        }

        private void setLocked()
        {
            if (asset["Table"]["Locked"] != null && asset["Table"]["Locked"].InnerText == "true")
            {
                if (!Window1.EditTranslation) bs.btnEdit.IsEnabled = false;
                //bs.btnNLW.IsEnabled = false;
                bs.btnDelete.IsEnabled = false;
            }
            if ((assetType == AssetType.Question && asset["Table"]["QuestTypeID"].InnerText == "9") ||
                (assetType == AssetType.Answer && (asset["Table"]["AnsTypeID"].InnerText == "13" ||
                    asset["Table"]["AnsTypeID"].InnerText == "14")))
            {
                bs.btnEdit.IsEnabled = false;
                //bs.btnNLW.IsEnabled = false;
                bs.btnDuplicate.IsEnabled = false;
            }
        }

        void usage_AssetUsage(object sender, RoutedEventArgs e)
        {
            usage.UsageMode = UsageMode.AssetMode;
            UsagePopulate();
        }

        void usage_AlgoUsage(object sender, RoutedEventArgs e)
        {
            usage.UsageMode = UsageMode.UsageMode;
            UsagePopulate();
        }

        void usage_LanguageUsage(object sender, RoutedEventArgs e)
        {
            usage.UsageMode = UsageMode.LanguageMode;
            UsagePopulate();
        }

        void usage_PropertiesUsage(object sender, RoutedEventArgs e)
        {
            usage.UsageMode = UsageMode.PropertiesMode;
            UsagePopulate();
        }

        void bs_NaturalLanguage(object sender, RoutedEventArgs e)
        {
            bs.btnNaturalLanguage.IsEnabled = false;

            foreach (var item in TextChildren)
            {
                if ((bool)item.Value.GetValue(SpellCheck.IsEnabledProperty))
                {
                    item.Value.Background = Brushes.LavenderBlush;
                    item.Value.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(NL_MouseUp);
                }
            }

            Window1.setStatus("Please select the field to test natural language");
        }

        void removeNLEvents()
        {
            foreach (var item in TextChildren)
            {
                if ((bool)item.Value.GetValue(SpellCheck.IsEnabledProperty))
                {
                    if (!item.Value.IsReadOnly)
                        item.Value.SetValue(BackgroundProperty, FindResource("Editable"));
                    else
                        item.Value.SetValue(BackgroundProperty, FindResource("ReadOnly"));
                    item.Value.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(NL_MouseUp);
                }
            }
        }

        void NL_MouseUp(object sender, MouseButtonEventArgs e)
        {
            removeNLEvents();
            bs.btnNaturalLanguage.IsEnabled = true;
            NLTest nl = new NLTest(sender as TextBox);
        }

        public virtual void Refresh()
        {
            updateTranslation();
            setButtons(EditMode);
        }

        void bs_Find(object sender, RoutedEventArgs e)
        {
            bool ret = toggleRightControl(AssetDockPanel, find, find.findDockPanel, 300);
            //bs.btnNLW.IsEnabled = !ret;
            if (ret)
            {
                find.findReplace.init(this);
            }
            else
            {
            }
        }

        void bs_Delete(object sender, RoutedEventArgs e)
        {
            cat.DeleteAssets(5);
        }

        public static void fixBoolean(XmlNode asset, string field)
        {
            string bl = asset["Table"][field].InnerText;
            if (bl == "-1") bl = "True";
            if (bl == "0") bl = "False";
            bool blval = false;
            if (bool.TryParse(bl, out blval))
                asset["Table"][field].InnerText = blval.ToString();
        }

        public static void fixDate(XmlNode asset, string field)
        {
            string bl = asset["Table"][field].InnerText;
            DateTime blval = DateTime.MinValue;
            if (DateTime.TryParse(bl, out blval))
                asset["Table"][field].InnerText = blval.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static void RunUpdate(XmlDocument doc)
        {
            Window1.setStatus("Accessing Data...");
            XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                "@xml", doc.OuterXml
            }, true);

            var node = from item in xn.DescendantsAndSelf()
                       where item.Name.LocalName == "Error" || item.Name.LocalName == "Info"
                       select item;
            if (node.Any()) System.Windows.Forms.MessageBox.Show(node.First().Value, node.First().Name.ToString());
            Window1.setStatus("");
        }

        protected Button GetTemplateButton(string name)
        {
            return this.GetTemplateChild(name) as Button;
        }

        void bs_CreateAnswer(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = cat.getAssetXml("derive", "COPYQUESTION");
            RunUpdate(doc);
        }

        private void bs_Properties(object sender, RoutedEventArgs e)
        {
            var p = Controls.Properties.CreateProperties((AssetType)cat.AssetTypeID, AssetID.Value.ToString());
            p.Type.Text = ((AssetType)cat.AssetTypeID).ToString();
            p.DataID.Text = asset.SelectSingleNode("Table/*").InnerText;
            p.Populate();
            p.Show();
        }

        void bs_DeriveQuestion(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = cat.getAssetXml("derive", "VALUEQUESTION");
            RunUpdate(doc);
        }

        void bs_DeriveAnswer(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = cat.getAssetXml("derive", tableName);
            RunUpdate(doc);
        }

        //void bs_NLW(object sender, RoutedEventArgs e)
        //{
        //    //DockPanel f = this.GetTemplateChild("assetDockPanel") as DockPanel;
        //    if (AssetDockPanel == null || nlw == null) return;
        //    bool ret = toggleRightControl(AssetDockPanel, nlw, nlw.nlwDockPanel, 500);
        //    bs.btnFind.IsEnabled = !ret;
        //    if (ret)
        //    {
        //        nlw.SetUpTextPredictors();
        //        nlw.nlwc.InsertNLW += new RoutedEventHandler(nlwc_InsertNLW);
        //    }
        //    else
        //        nlw.nlwc.InsertNLW -= new RoutedEventHandler(nlwc_InsertNLW);
        //}

        private bool toggleRightControl(DockPanel f, Control control, DockPanel panel, int width)
        {
            if (control.Visibility == Visibility.Visible)
            {
                ThicknessAnimation da = new ThicknessAnimation();
                da.To = new Thickness(0);
                da.From = new Thickness(0, 0, width, 0);
                da.Duration = new Duration(TimeSpan.FromSeconds(0.3));

                f.BeginAnimation(MarginProperty, da);
                control.Visibility = Visibility.Hidden;
                return false;
            }
            else
            {
                control.Visibility = Visibility.Visible;
                ThicknessAnimation da = new ThicknessAnimation();
                da.From = new Thickness(0);
                da.To = new Thickness(0, 0, width, 0);
                da.Duration = new Duration(TimeSpan.FromSeconds(0.3));
                da.Completed += new EventHandler(da_Completed);
                f.BeginAnimation(MarginProperty, da);

                DoubleAnimation wa = new DoubleAnimation();
                wa.From = 0;
                wa.To = width;
                wa.Duration = da.Duration;
                panel.BeginAnimation(WidthProperty, wa);
                return true;
            }
        }

        void da_Completed(object sender, EventArgs e)
        {
            //nlw.nlwc.txtInsert.CaretPosition = nlw.nlwc.txtInsert.Document.ContentEnd;
        }

        //void nlwc_InsertNLW(object sender, RoutedEventArgs e)
        //{
        //    var text = (e as NaturalLanguageWizard.NLWControl.InsertNLWEventArgs).Text;

        //    //Clipboard.SetText(text);            

        //    //ApplicationCommands.Paste.Execute(null, null); // pastes to element with Keyboard Focus
        //    System.Windows.IInputElement ie = FocusManager.GetFocusedElement(cat.form);

        //    if (ie is TextBox)
        //    {
        //        TextBox lt = ie as TextBox;
        //        if (!lt.AcceptsReturn) text = text.Replace("\n", "").Replace("\r", "");
        //        string s = lt.Text;
        //        int pos = lt.SelectionStart;
        //        lt.Text = s.Substring(0, lt.SelectionStart) + text + s.Substring(lt.SelectionStart + lt.SelectionLength);
        //        lt.SelectionStart = pos + text.Length;
        //        lt.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        //    }
        //    else
        //        MessageBox.Show("Please place the cursor in the textbox at the point you wish to insert the natural language and try again.", "Cannot insert natural language.", MessageBoxButton.OK, MessageBoxImage.Stop);
        //}

        void bs_Duplicate(object sender, RoutedEventArgs e)
        {
            if (Window1.AllowProperties)
                properties = DataAccess.getData("dsp_GetProperty", new string[] { "@PropertyType", assetType.ToString(), "@DataID", AssetID.ToString() }, true);
            cat.resetTranslation();
            originalAsset = asset.Clone();
            setButtons(true);
            if (assetType == AssetType.Conclusion)
            {
                Conclusion c = this as Conclusion;
                c.cleanBullets();
            }
            XmlNode idcolumn = asset.SelectSingleNode("Table/*");
            idcolumn.InnerText = "new";
        }

        void bs_Usage(object sender, RoutedEventArgs e)
        {
            //DockPanel f = this.GetTemplateChild("assetDockPanel") as DockPanel;
            if (AssetDockPanel == null) return; // || nlw == null) return;
            bool ret = toggleRightControl(AssetDockPanel, usage, usage.usageDockPanel, 400);
            if (ret)
            {
                usage.Populate(usage.usageTabs.SelectedItem as TabItem);
            }
        }

        private void UsagePopulate()
        {
            ContextMenu menu = new System.Windows.Controls.ContextMenu();
            if (Window1.IsBuilderOrAdmin && Window1.AllowProperties)
            {
                MenuItem mi = new MenuItem { Header = "_Open Properties", CommandParameter = "1", InputGestureText = "Ctrl+O" };
                mi.Click += mi_Click;
                menu.Items.Add(mi);
            }
            MenuItem gx = new MenuItem { Header = "_Get Xml", CommandParameter = "1", InputGestureText = "Ctrl+G" };
            gx.Click += gx_Click;
            menu.Items.Add(gx);
            MenuItem rn = new MenuItem { Header = "_Render Node", CommandParameter = "1", InputGestureText = "Ctrl+R" };
            rn.Click += rn_Click;
            menu.Items.Add(rn);

            //MenuItem vx = new MenuItem { Header = "Open _Visio", CommandParameter = "1", InputGestureText = "Ctrl+V" };
            //vx.Click += vx_Click;
            //menu.Items.Add(vx);

            TreeView tv = usage.usageTreeView;
            tv.Items.Clear();
            if (usage.IsLanguageMode)
            {
                XElement xn = DataAccess.searchLanguage((int)assetType, 0, "*", usage.AssetID).GetXElement();

                var nodes = from item in xn.Elements("ID")
                            where item.Value != null
                            orderby item.Value
                            select item.Value;

                foreach (var item in nodes)
                {
                    TreeViewItem tvi = new TreeViewItem { Header = item };
                    tvi.MouseDoubleClick += new MouseButtonEventHandler(tvi_MouseDoubleClick);
                    tv.Items.Add(tvi);
                }
            }
            else if (usage.IsAssetMode)
            {
                for (int i = 1; i <= 5; i++)
                {
                    string search = usage.AssetID;
                    if (assetType == AssetType.Question) search = "%{q_" + search + "[^0-9]%";
                    if (assetType == AssetType.Answer) search = "%|" + search + "[^0-9]%";
                    if (assetType == AssetType.Conclusion) search = "%{c_" + search + "[^0-9]%";
                    if (assetType == AssetType.TextAsset) search = "%~T" + search + ".%#%~%";
                    XElement xn = DataAccess.getData("ab_getitems", new string[] { "@boxid", "5", "@assettypeid", i.ToString(), "@searchword", search }, false);

                    var nodes = from item in xn.Elements()
                                where item.Element("ID") != null
                                select item;

                    TreeViewItem currentType = null;
                    if (nodes.Any())
                    {
                        currentType = new TreeViewItem();
                        int count = nodes.Count();
                        string suff = string.Format("{0} ({1})", count > 1 ? "s" : "", count);
                        currentType.Header = ((AssetType)i).ToString() + suff;
                        tv.Items.Add(currentType);
                    }

                    if (currentType != null)
                        foreach (var item in nodes)
                        {
                            string id = item.Element("ID").Value;
                            string desc = item.Element("Description").Value;
                            TextBlock tb = formattedTextBlock(id, desc, 0, greenBrush);
                            Label l = new Label { Content = tb, Padding = new Thickness(0), Margin = new Thickness(0) };
                            l.Tag = new { ID = id, assetType = i, Search = search, FromID = usage.AssetID };
                            l.MouseDoubleClick += new MouseButtonEventHandler(l_LoadAsset);
                            currentType.Items.Add(l);
                        }
                }
            }
            else if (usage.IsPropertiesMode)
            {
                var props = DataAccess.getData("dsp_SearchProperties", new string[] { "@PropertyType", (assetType == AssetType.Algo ? "[AT][lr][ga][on]%" : assetType.ToString()) + ":" + usage.AssetID }, true);

                var nodes = from item in props.Elements()
                            group item by new { PropertyType = item.Element("PropertyType").Value, DataID = item.Element("DataID").Value, VisioID = item.Element("VisioID").Value, AlgoID = item.Element("AlgoID").Value, Algo = item.Element("Algo_Name").Value } into g
                            select new { PropertyType = g.Key.PropertyType, DataID = g.Key.DataID, VisioID = g.Key.VisioID, AlgoID = g.Key.AlgoID, Algo = g.Key.Algo, Count = g.Count() };

                TreeViewItem currentAlgo = null;
                string algo = "";
                string name = "";
                int count = 0;

                foreach (var item in nodes)
                {
                    string[] ds = item.DataID.Split(':');
                    string ta = item.Algo;
                    string algoid = item.AlgoID;
                    string nodeid = ds.Length > 1 ? ds[1] : "";
                    if (algoid != algo || name != ta || currentAlgo == null)
                    {
                        count = 0;
                        algo = algoid;
                        name = ta;
                        currentAlgo = new TreeViewItem();
                        tv.Items.Add(currentAlgo);
                    }

                    currentAlgo.Header = formattedTextBlock(algoid, ta, ++count, blueBrush);
                    TextBlock tb = formattedTextBlock(nodeid, item.VisioID + " (" + item.Count + ")", 0, redBrush);
                    Label l = new Label { Content = tb, Padding = new Thickness(0), Margin = new Thickness(0) };

                    l.Tag = new usageShape { AlgoID = algoid, NodeID = nodeid, ShapeName = item.VisioID, AlgoName = ta, PropertyType = item.PropertyType, DataID = item.DataID };
                    l.MouseDoubleClick += new MouseButtonEventHandler(l_MouseDoubleClick);

                    l.ContextMenu = menu;

                    currentAlgo.Items.Add(l);
                }
            }
            else
            {
                XmlDocument doc = cat.getAssetXml("usage", tableName);
                if (doc.SelectSingleNode(string.Format("/root/{0}[@id={1}]", tableName, usage.ConcatID ?? usage.AssetID)) == null)
                    cat.InsertAsset(tableName, doc.DocumentElement, usage.AssetID);

                XElement xn = DataAccess.getData("ab_UpdateAsset", new string[] {
                    "@xml", doc.OuterXml
                }, false);

                var nodes = from item in xn.Elements()
                            select item;

                TreeViewItem currentAlgo = null;
                string algo = "";
                string name = "";
                int count = 0;

                foreach (var item in nodes)
                {
                    string ta = item.Element("Algo_Name").Value;
                    string algoid = item.Element("AlgoID").Value;
                    string nodeid = item.Element("NodeID").Value;
                    if (algoid != algo || name != ta || currentAlgo == null)
                    {
                        count = 0;
                        algo = algoid;
                        name = ta;
                        currentAlgo = new TreeViewItem();
                        tv.Items.Add(currentAlgo);
                    }
                    bool bulletasset = item.Name == "Table1";
                    currentAlgo.Header = formattedTextBlock(algoid, ta, ++count, bulletasset ? greenBrush : blueBrush);
                    TextBlock tb = formattedTextBlock(nodeid, item.Element("UserName").Value, 0, bulletasset ? greenBrush : redBrush);
                    Label l = new Label { Content = tb, Padding = new Thickness(0), Margin = new Thickness(0) };
                    if (bulletasset)
                    {
                        l.Tag = new { ID = nodeid, assetType = 4, Search = "@bulletid:" + algoid, FromID = algoid };
                        l.MouseDoubleClick += new MouseButtonEventHandler(l_LoadAsset);
                    }
                    else
                    {
                        string shapename = item.Element("UserName").Value;
                        if (shapename.Contains(" (")) shapename = shapename.Substring(0, shapename.IndexOf(" ("));
                        string instanceAssetType = item.Element("UserName").Value.Contains("Transfer") ? "Transfer" : assetType.ToString();
                        l.Tag = new usageShape { AlgoID = algoid, NodeID = nodeid, ShapeName = shapename, AlgoName = ta, PropertyType = instanceAssetType + ":" + AssetID, DataID = algoid + ":" + nodeid };
                        l.MouseDoubleClick += new MouseButtonEventHandler(l_MouseDoubleClick);
                        l.ContextMenu = menu;
                    }
                    currentAlgo.Items.Add(l);
                }
            }
        }

        private void rn_Click(object sender, RoutedEventArgs e)
        {
            var us = (usageShape)((Label)((System.Windows.Controls.ContextMenu)((sender as MenuItem).Parent)).PlacementTarget).Tag;
            Window1.window.rtbTraversalClient.IsChecked = true;
            Window1.window.Start_TraversalClient(Tuple.Create(us.AlgoID, us.NodeID));
        }

        void mi_Click(object sender, RoutedEventArgs e)
        {
            var l = (Label)((System.Windows.Controls.ContextMenu)((sender as MenuItem).Parent)).PlacementTarget;
            var us = l.Tag as usageShape;
            var p = Controls.Properties.CreateProperties((AssetType)cat.AssetTypeID, us.PropertyType.Split(':')[1], false);
            p.Type.Text = us.PropertyType;
            p.DataID.Text = us.DataID;
            p.Populate();
            p.Show();
        }

        void gx_Click(object sender, RoutedEventArgs e)
        {
            Label l = (Label)((System.Windows.Controls.ContextMenu)((sender as MenuItem).Parent)).PlacementTarget;
            usageShape us = l.Tag as usageShape;
            GetXml(us.AlgoID);
        }

        void tvi_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Window1.TranslationLanguage = (sender as TreeViewItem).Header.ToString();
            cat.form.ForceLanguage((sender as TreeViewItem).Header.ToString());
        }

        void l_LoadAsset(object sender, MouseButtonEventArgs e)
        {
            Label l = sender as Label;
            var assetToLoad = Cast(l.Tag, new { ID = "", assetType = 1, Search = "", FromID = "" });
            cat.AssetTypeID = assetToLoad.assetType;
            Window1.RadioToggle(cat.form.assetGroup, cat.AssetTypeID);
            cat.LoadAssetFromList(assetToLoad.ID, assetToLoad.Search, assetToLoad.FromID);
        }

        T Cast<T>(object o, T type)
        {
            return (T)o;
        }

        void l_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            usageShape us = (usageShape)(sender as Label).Tag;
            VisioFindorLoad(us, ref algos, this);
        }

        public static void VisioFindorLoad(usageShape us, ref XElement algos, assetControl @this)
        {
            bool visioFound;
            bool shpfound = HighlightVisioShape(us, out visioFound, @this);
            if (shpfound) return;
            if (visioFound)
                System.Windows.Forms.MessageBox.Show("Asset not found", "Problem", System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            else
            {
                if (algos == null)
                    algos = DataAccess.getData("ab_UpdateAsset", new string[] { "@xml", "<root command=\"algos\" />" }, true);
                XElement fn =
                    algos
                        .Elements("Table")
                        .FirstOrDefault(f => f.Element("AlgoID") != null && f.Element("AlgoID").Value == us.AlgoID);
                if (Window1.IsBuilderOrAdmin && fn != null && fn.Element("FileName") != null)
                {
                    string algoname = "Visio";
                    if (fn.Element("Title") != null) algoname = string.Format("'{0}'", fn.Element("Title").Value);
                    string machinename = "";
                    if (fn.Element("MachineName") != null) machinename = fn.Element("MachineName").Value;
                    string filename = fn.Element("FileName").Value;
                    string message = string.Format("This was last loaded from {1}\n   {0}", filename, machinename);

                    List<string> buttons = new List<string>() { "Load from File", "Cancel" };
                    message += "\n\nDo you want to try to open the file";
                    if (Window1.UserLevel == UserSecurityLevel.Admin || Window1.Security == SecurityContext.Open)
                    {
                        message +=
                            "\n   or copy the algorithm XML to the clipboard\n      (for regenerating in Visio via [Convert])";
                        buttons.Insert(1, "Copy Xml to Clipboard");
                    }
                    message += "?";
                    string result = Diva.Controls.Simple.CustomMessageBox.Show(message, algoname, buttons.ToArray(),
                        "Load from File", "Cancel");
                    if (result == "Copy Xml to Clipboard")
                    {
                        GetXml(us.AlgoID);
                    }
                    else if (result == "Load from File")
                    {
                        AlgoLoader.LoadAlgoFromFile(machinename, filename);
                    }
                }
                else
                    System.Windows.Forms.MessageBox.Show("Visio document not open", "Problem",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
        }

        private static void GetXml(string AlgoID)
        {
            string sql = DataAccess.GetDataService().generateSQL("(" + AlgoID + ")", true, "");
            Clipboard.SetText(AlgoLoader.ExtractXml(sql));
            System.Windows.Forms.MessageBox.Show("Xml copied to clipboard", "Message", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        public static bool HighlightVisioShape(usageShape us, out bool visioFound, assetControl assetcontrol)
        {
            Visio.Shape shp;
            visioFound = false;

            Visio.Application vis = Classes.VisioInterface.GetVisio();
            if (vis != null)
            {
                foreach (Visio.Window win in vis.Windows)
                    foreach (Visio.Page pge in win.Document.Pages)
                        if (pge.Shapes.Count > 0)
                        {
                            try { shp = pge.Shapes["Algo Start"]; }
                            catch { shp = null; }
                            if (shp != null)
                            {
                                string mAlgoID;
                                if (shp.get_CellExists("Prop.AlgoID", 0) == 0)
                                    mAlgoID = shp.get_Cells("Prop.ID").Formula;
                                else
                                    mAlgoID = shp.get_Cells("Prop.AlgoID").Formula;
                                if (assetcontrol != null && mAlgoID != us.AlgoID && assetcontrol.assetType == AssetType.Algo && shp.get_CellExists("Prop.Description", 0) != 0)
                                {
                                    if ("\"" + us.AlgoName + "\"" == shp.get_Cells("Prop.Description").Formula) mAlgoID = us.AlgoID;
                                }
                                if (mAlgoID == us.AlgoID)
                                {
                                    visioFound = true;
                                    try
                                    {
                                        if (assetcontrol is Bullet || string.IsNullOrWhiteSpace(us.ShapeName))
                                            shp = pge.Shapes.ItemFromID[int.Parse(us.NodeID)];
                                        else
                                            shp = pge.Shapes[us.ShapeName];
                                    }
                                    catch { shp = null; }
                                    if (shp != null)
                                    {
                                        win.Activate();
                                        win.Page = pge.Name;
                                        win.Select(shp, 258);
                                        double X = shp.get_Cells("PinX").ResultIU;
                                        double Y = shp.get_Cells("PinY").ResultIU;

                                        win.ScrollViewTo(X, Y);
                                        vis = null;
                                        return true;
                                    }
                                }
                            }
                        }
            }
            vis = null;
            return false;
        }

        SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        static SolidColorBrush blackBrush = new SolidColorBrush(Colors.Black);

        private static TextBlock formattedTextBlock(string id, string text, int count, Brush idBrush)
        {
            TextBlock tb = new TextBlock();
            tb.SetValue(ForegroundProperty, blackBrush);
            TextBlock idtb = new TextBlock();
            idtb.Text = id;
            idtb.TextAlignment = TextAlignment.Right;
            idtb.SetValue(ForegroundProperty, idBrush);
            idtb.Width = 40;
            TextBlock texttb = new TextBlock();
            texttb.Text = text;
            tb.Inlines.Add(idtb);
            tb.Inlines.Add(") ");
            tb.Inlines.Add(texttb);
            if (count > 0) tb.Inlines.Add(string.Format(" ({0})", count));
            return tb;
        }

        void bs_Add(object sender, RoutedEventArgs e)
        {
            cat.addNew();
        }

        void bs_Save(object sender, RoutedEventArgs e)
        {
            Save();
        }

        public virtual bool Save(bool toSaas = true)
        {
            if (Window1.EditTranslation)
            {
                if (!IsValidLanguage)
                {
                    Window1.setStatus("Validation Error!");
                    return false;
                }
                Window1.setStatus("Saving Record");
                XmlNode tres = DataAccess.setLanguage(asset, translation, Window1.TranslationLanguage);
                foreach (XmlNode item in tres.ChildNodes)
                {
                    TextBox t = LanguageChildren.Find(f => BindingOperations.GetBinding(f, TextBox.TextProperty).XPath == item.Name);
                    if (t != null)
                    {
                        SetTooltip(t);
                        if (item.InnerText == "") t.ClearValue(TextBox.BorderBrushProperty);
                        else
                        {
                            t.SetValue(TextBox.BorderBrushProperty, FindResource(item.InnerText));
                            t.ToolTip += " - " + item.InnerText;
                        }
                    }
                }
                setButtons(false);
                Window1.setStatus("");
                return true;
            }
            if (!IsValid)
            {
                Window1.setStatus("Validation Error!");
                return false;
            }
            if (Window1.DisableComments || asset["Table"]["comment"] == null) return SaveAfterPrompt();
            XmlNode idcolumn = asset.SelectSingleNode("Table/*");
            PromptWindow pw = new PromptWindow(idcolumn.InnerText == "new");
            pw.Owner = cat.form;
            cat.form.disableForm();
            bool? res = pw.ShowDialog();
            cat.form.enableForm();
            if (res == true)
            {
                asset["Table"]["comment"].InnerText = pw.Comment;
                return SaveAfterPrompt(toSaas);
            }
            return false;
        }

        protected int newID;

        private bool SaveAfterPrompt(bool toSaas = true)
        {
            if (Keyboard.FocusedElement is TextBox) (Keyboard.FocusedElement as TextBox)?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            newID = 0;
            Window1.setStatus("Saving Record");
            List<string> prms = new List<string>(new string[] { "@xml", asset.OuterXml });
            if (assetType == AssetType.TextAsset) prms.AddRange(new string[] { "@AssetTypeID", "12" });
            XmlNode xn = DataAccess.getDataNode("ab_updateasset", prms.ToArray(), true);
            if (xn.Name == "Error")
            {
                CustomMessageBox.Show(xn.InnerText, "Something went wrong", new[] { "OK" });
                Window1.setStatus("");
                return false;
            }
            setButtons(false);
            XmlNode idcolumn = asset.SelectSingleNode("Table/*");
            if (!Window1.EnableLanguageInheritance && cat.form.AlternateLanguages.Count > 0 && idcolumn.InnerText != "new")
            {
                Dictionary<string, List<string>> tickedFields = new Dictionary<string, List<string>>();
                foreach (var item in inheritList)
                {
                    IEnumerable<string> list = item.Value.getUncheckedLanguages();
                    foreach (var l in list)
                    {
                        if (!tickedFields.ContainsKey(l)) tickedFields.Add(l, new List<string>());
                        tickedFields[l].Add(item.Value.Path);
                    }
                }
                var changes = originalAsset.SelectNodes("Table/*").OfType<XmlNode>().Where(f => asset["Table"][f.Name].InnerText != f.InnerText);
                var tf = translationLookup.Where(f => f.Key.Item1 == assetType).ToDictionary(f => f.Key.Item2, f => f.Value);
                foreach (var language in cat.form.AlternateLanguages)
                {
                    XmlNode xlang = DataAccess.getLanguage(asset, language);
                    var inherit = xlang.SelectNodes("*").OfType<XmlNode>()
                        .Where(f => f.InnerText == "").Select(f => f.Name)
                        .Where(f => changes.Any(x => tf.ContainsKey(f) ? x.Name == tf[f] : x.Name == f))
                        .Where(f => tickedFields.ContainsKey(language) && tickedFields[language].Contains(f))
                        .Select(f => new { Name = f, Value = changes.First(x => x.Name == (tf.ContainsKey(f) ? tf[f] : f)).InnerText });
                    XDocument doc = new XDocument(new XElement("root"));
                    bool savelanguage = false;
                    foreach (var item in inherit)
                    {
                        savelanguage = true;
                        doc.Element("root").Add(new XElement(item.Name, item.Value == "" ? "*" : item.Value));
                    }
                    if (savelanguage) DataAccess.setLanguage(asset, DataAccess.GetXmlNode(doc.Element("root")), language);
                }
            }
            originalAsset = asset;
            if (assetType == AssetType.TextAsset) idcolumn.InnerText = xn.SelectSingleNode("Table/*").InnerText;
            if (idcolumn.InnerText == "new" && xn["Table"] != null && xn["Table"]["ID"] != null)
            {
                newID = int.Parse(xn.SelectSingleNode("Table/ID").InnerText);
                if (properties != null)
                {
                    var props = properties.Elements("Table").Where(f => f.Element("PropertyType") != null && f.Element("DataID") != null && f.Element("PropertyName") != null && f.Element("PropertyValue") != null).Select(f => new
                    {
                        PropertyType = f.Element("PropertyType").Value,
                        DataID = f.Element("DataID").Value,
                        PropertyName = f.Element("PropertyName").Value,
                        PropertyValue = f.Element("PropertyValue").Value
                    });
                    foreach (var item in props)
                    {
                        DataAccess.SetProperty(item.PropertyType, newID.ToString(), item.PropertyName, item.PropertyValue);
                    }
                }
                //SetValue(ListBox.SelectedValueProperty, int.Parse(xn.SelectSingleNode("Table/ID").InnerText);
            }
            if (!(this is Conclusion)) reloadAsset(idcolumn.InnerText);
            Window1.setStatus("");
            properties = null;
            if(toSaas) SaveToSaas();
            return xn.Name != "Error";
        }

        public void SaveToSaas()
        {
            var saasId = AssetID ?? newID;
            if (saasId > 0 && Window1.AllowSaaSIntegration && assetType.In(AssetType.Algo, AssetType.Question, AssetType.Answer, AssetType.Conclusion, AssetType.Bullet, AssetType.ConclusionMap)) Task.Run(() => SaaS.Instance.SaveAssetToSaas(assetType.ToString(), saasId));
        }

        //public async Task SaveToSaas()
        //{
        //    await Task.CompletedTask;
        //    var at = assetType.ToString();
        //    var token = SaaS.Instance.token;
        //    var content = new Uri(Properties.Settings.Default.SaaSEndpoint);
        //    var endpoint = $"TraversalService/TableOutput/Asset_{assetType}/json/object/{AssetID}?TextAsset=Bloat";
        //    var url = new Uri(new Uri(Properties.Settings.Default.WebService), endpoint).AbsoluteUri;
        //    var then = DateTime.Now;
        //    var a = url.GetContent<JNode>();
        //    DataAccess.AddLastCommand(url, a, then - DateTime.Now);
        //    var p = new Uri(content, $"api/v1/{Properties.Settings.Default.ClientID}/{at}s/{AssetID}").AbsoluteUri;
        //    var headers = new[] { ("Content-Type", "application/json"), ("Authorization", $"Bearer {token}"), };
        //    then = DateTime.Now;
        //    var put = a[at.ToLower()].ToJson().PostObject<JNode>(p, headers, "PUT");
        //    DataAccess.AddLastCommand(p, put, then - DateTime.Now);
        //}

        public void reloadAsset(string ID)
        {
            cat.repopulate(-1);
            if (ID == "new") cat.lb[4].SelectedItem = cat.selectItem(cat.lb[4], newID);
            else if (cat.listBox5.SelectedIndex == -1)
            {
                cat.loadedAsset = null;
                cat.loadAsset(ID);
            }
        }

        void bs_Close(object sender, RoutedEventArgs e)
        {
            NLExtensions.errors.Clear();
            cat.form.assetCanvas.Children.Clear();
            cat.loadedAsset = null;
            cat.listBox5.SelectedItem = null;
        }

        void bs_Cancel(object sender, RoutedEventArgs e)
        {
            Cancel();
        }

        public virtual void Cancel()
        {
            properties = null;
            setButtons(false);
            XmlNode idcolumn = originalAsset.SelectSingleNode("Table/*");
            if (idcolumn.InnerText == "new")
            {
                cat.form.assetCanvas.Children.Clear();
                return;
            }
            asset = originalAsset;
            if (Window1.ShowTranslation)
            {
                foreach (var item in LanguageChildren)
                {
                    if (string.IsNullOrEmpty(item.Text))
                        NLExtensions.textBox_AdornAndValidate(item, null);
                }
            }
            if (Window1.EditTranslation)
            {
                translation = originalTranslation;
                setLanguageDataContext();
            }
            this.DataContext = asset["Table"];
        }

        void btnstrpEdit_Edit(object sender, RoutedEventArgs e)
        {
            goEdit();
        }

        public virtual void goEdit()
        {
            if (bs == null) ApplyTemplate();
            originalAsset = asset.Clone();
            if (Window1.EditTranslation) originalTranslation = translation.Clone();
            setButtons(true);

            if (!Window1.EditTranslation) updateLanguageInheritance();
        }

        private void updateLanguageInheritance()
        {
            emptyLanguageFields.Clear();
            XmlNode thislanguage = null;
            XmlNode idcolumn = asset.SelectSingleNode("Table/*");
            if (!Window1.EnableLanguageInheritance && Window1.window.AlternateLanguages.Count > 0 && idcolumn.InnerText != "new")
            {
                foreach (var language in Window1.window.AlternateLanguages)
                {
                    XmlNode xlang = DataAccess.getLanguage(asset, language);
                    if (Window1.TranslationLanguage == language) thislanguage = xlang;
                    var inherit = xlang.SelectNodes("*").OfType<XmlNode>().Where(f => f.InnerText == "").Select(f => f.Name);
                    if (inherit.Any()) emptyLanguageFields.Add(language, inherit.ToArray());
                }
                foreach (var item in inheritList)
                {
                    string lookup = textPaths[item.Key.Name];
                    var tf = translationLookup.Where(f => f.Key.Item1 == assetType && f.Value == lookup).Select(f => f.Key.Item2);
                    if (tf.Any()) lookup = tf.First();
                    if (emptyLanguageFields.Any(f => f.Value.Contains(lookup)))
                    {
                        var languages = new ObservableCollection<string>(emptyLanguageFields.Where(f => f.Value.Contains(lookup)).Select(f => f.Key).Distinct());
                        item.Value.List = languages;
                    }
                }
            }
        }

        void setButtons(bool IsEditMode)
        {
            removeNLEvents();

            cat.IsEditing = IsEditMode;
            foreach (var item in TextChildren)
            {
                item.Value.IsReadOnly = !(IsEditMode && (Window1.EditTranslation == LanguageChildren.Contains(item.Value)));
                if (Window1.IsEditor && item.Value.Name.EndsWith("Expert")) item.Value.IsReadOnly = true;
                if (!item.Value.IsReadOnly)
                    item.Value.SetValue(BackgroundProperty, FindResource("Editable"));
                else
                    item.Value.SetValue(BackgroundProperty, FindResource("ReadOnly"));
                //if (LanguageChildren.Contains(item.Value))
                //    if (Window1.ShowTranslation)
                //        item.Value.SetValue(TextBox.VisibilityProperty, Visibility.Visible);
                //    else
                //        item.Value.SetValue(TextBox.VisibilityProperty, Visibility.Collapsed);
            }
            foreach (var item in CheckChildren)
            {
                item.Value.IsEnabled = IsEditMode && !Window1.EditTranslation;
            }
            foreach (var item in ComboChildren)
            {
                item.Value.IsEnabled = IsEditMode && !Window1.EditTranslation;
            }
            foreach (var item in ButtonChildren)
            {
                if (!item.Key.StartsWith("ebtn"))
                    item.Value.IsEnabled = IsEditMode && !Window1.EditTranslation;
            }
            foreach (var item in GridChildren)
            {
                item.Value.IsReadOnly = !(IsEditMode && !Window1.EditTranslation);
            }

            if (!IsEditMode)
            {
                foreach (var item in inheritList)
                {
                    item.Value.SetValue(VisibilityProperty, Visibility.Collapsed);
                }
                //if (nlw.Visibility == Visibility.Visible)
                //{
                //    //bs_NLW(this, null);
                //}
                if (find.Visibility == Visibility.Visible)
                {
                    bs_Find(this, null);
                }
            }

            else
            {
                if (usage.Visibility == Visibility.Visible)
                {
                    bs_Usage(this, null);
                }
            }

            bs.setButtons(IsEditMode);
            setLocked();
        }

        Dictionary<Tuple<AssetType, string>, string> translationLookup = new Dictionary<Tuple<AssetType, string>, string>()
        {
            { Tuple.Create<AssetType, string>(AssetType.Answer, "Lay_Statement"), "Lay_Answer" },
            { Tuple.Create<AssetType, string>(AssetType.Answer, "Answer"), "Answer_Text" },
            { Tuple.Create<AssetType, string>(AssetType.Conclusion, "Lay_Statement"), "Lay_Condition" },
        };

        Dictionary<string, string> textPaths = new Dictionary<string, string>();
        Dictionary<TextBox, inherits> inheritList = new Dictionary<TextBox, inherits>();

        void iterate(UIElementCollection vc)
        {
            Thickness tbt = new Thickness(2, 4, 2, 2);
            Thickness cbt = new Thickness(5, 5, 2, 2);
            foreach (UIElement item in vc)
            {
                if (item is TextBox tb)
                {
                    DataObject.AddPastingHandler(tb, OnPaste);

                    tb.SetValue(PaddingProperty, tbt);
                    tb.SetValue(LanguageProperty, Window1.DefaultLanguage);
                    tb.SetValue(BackgroundProperty, FindResource("ReadOnly"));
                    if (tb.Name.EndsWith("Language"))
                    {
                        tb.DataContext = translation;
                        LanguageChildren.Add(tb);
                    }

                    Binding b = BindingOperations.GetBinding(tb, TextBox.TextProperty);
                    if (cat.Defaults != null && b != null && b.XPath != null)
                    {
                        XmlNode len;
                        Tuple<AssetType, string> tup = Tuple.Create(assetType, b.XPath);
                        if (translationLookup.ContainsKey(tup))
                            len = cat.Defaults.SelectSingleNode(string.Format("Table[Table='{0}' and Column='{1}']", tableName, translationLookup[tup]));
                        else
                            len = cat.Defaults.SelectSingleNode(string.Format("Table[Table='{0}' and Column='{1}']", tableName, b.XPath));
                        if (len != null)
                        {
                            if (tb.AcceptsReturn)
                            {
                                asset["Table"][b.XPath].InnerText = asset["Table"][b.XPath].InnerText.Replace("\r\n", "\n").Replace("\n", "\r\n");
                            }
                            if (!tb.Name.EndsWith("Language"))
                            {
                                if (len["Type"] != null && (len["Type"].InnerText == "text" || (len["Type"].InnerText == "varchar" && len["Length"] != null && len["Length"].InnerText == "-1")))
                                    tb.MaxLength = tb.Name.StartsWith("txtMoreDetail") ? 8000 : 4000;
                                else if (len["Precision"] != null && len["Type"] != null && len["Type"].InnerText != "datetime" && len["Type"].InnerText != "int")
                                    tb.MaxLength = int.Parse(len["Precision"].InnerText);
                                //else if (len["Type"] != null && len["Type"].InnerText != "datetime")
                                //    tb.Text = tb.Text.Substring(0, tb.Text.IndexOf('+'));
                            }
                        }
                        SetTooltip(tb);
                    }
                    if (b != null && b.XPath != null && !tb.Name.EndsWith("Language")) textPaths.Add(tb.Name, b.XPath);
                    inherits ih = (inherits)GetTemplateChild("list" + tb.Name.Substring(3));
                    if (ih != null)
                    {
                        string ihPath = b.XPath;
                        var ihPathLookup = translationLookup.Where(f => f.Key.Item1 == assetType && f.Value == ihPath).Select(f => f.Key.Item2);
                        if (ihPathLookup.Any()) ihPath = ihPathLookup.First();
                        ih.Path = ihPath;
                        if (b.XPath != null) ih.OriginalText = asset["Table"][b.XPath].InnerText;
                        ih.TemplateName = tb.Name + "Language";
                        inheritList.Add(tb, ih);
                    }
                    TextChildren.Add(tb.Name, tb);
                    if ((bool)tb.GetValue(SpellCheck.IsEnabledProperty))
                    {
                        SpellChildren.Add(tb);
                        //if (!System.IO.File.Exists("custom.lex")) System.IO.File.Create("custom.lex").Close();
                        //tb.SpellCheck.CustomDictionaries.Add(new Uri("custom.lex", UriKind.Relative));
                        tb.PreviewDragEnter += new DragEventHandler(tb_PreviewDragEnter);
                        tb.PreviewDragOver += new DragEventHandler(tb_PreviewDragEnter);
                        tb.PreviewDrop += new DragEventHandler(tb_PreviewDrop);

                        Intel lb = new Intel(tb, cat.form.bubbleCanvas, IntelListMakers.mylistmakers);
                    }
                    if ((bool)tb.GetValue(SpellCheck.IsEnabledProperty) || tb.Name.EndsWith("Language"))
                    {
                        tb.SelectionChanged += new RoutedEventHandler(AssetBuilder.Controls.NLExtensions.textBox_AdornAndValidate);
                        tb.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(AssetBuilder.Controls.NLExtensions.textBox_ReAdorn));
                        if (tb.Name.EndsWith("Language") && tb.Text == "") tb.Loaded += new RoutedEventHandler(NLExtensions.textBox_AdornAndValidate);
                    }
                    if (Window1.DisableSpelling) tb.SpellCheck.IsEnabled = false;
                    tb.TextChanged += new TextChangedEventHandler(tb_TextChanged);
                    tb.MouseDoubleClick += new MouseButtonEventHandler(tb_MouseDoubleClick);
                    tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                    tb.LostFocus += new RoutedEventHandler(tb_LostFocus);
                    tb.IsVisibleChanged += new DependencyPropertyChangedEventHandler(tb_IsVisibleChanged);
                }
                if (item is CheckBox)
                {
                    CheckBox cb = item as CheckBox;
                    CheckChildren.Add(cb.Name, cb);
                }
                if (item is ComboBox)
                {
                    ComboBox cb = item as ComboBox;
                    cb.SetValue(PaddingProperty, cbt);
                    if (cat.Defaults != null)
                    {
                        XmlNodeList xnl = cat.Defaults.SelectNodes(string.Format("*[ComboBox='{0}']", cb.Name));
                        if (xnl.Count > 0)
                        {
                            if (cb.Name != "cmbMultiplier" && cb.Name != "cmbMultiplierText")
                                foreach (XmlNode xn in xnl)
                                {
                                    cb.Items.Add(new ListItem { ID = int.Parse(xn.ChildNodes[1].InnerText), Value = xn.ChildNodes[2].InnerText });
                                }
                            else
                                foreach (XmlNode xn in xnl)
                                    cb.Items.Add(new ListItem { MultID = xn.ChildNodes[1].InnerText, Value = xn.ChildNodes[2].InnerText });
                        }
                    }
                    ComboChildren.Add(cb.Name, cb);
                    cb.SelectionChanged += new SelectionChangedEventHandler(cb_SelectionChanged);
                }
                if (item is DataGrid)
                {
                    DataGrid dg = item as DataGrid;
                    Type T = Type.GetType(dg.Tag.ToString());
                    MethodInfo mi = typeof(assetControl).GetMethod("AssociateGrid", BindingFlags.Instance | BindingFlags.NonPublic);
                    mi.MakeGenericMethod(T).Invoke(this, new object[] { dg });
                    //AssociateGrid<T>(dg);
                    //switch (dg.Tag.ToString())
                    //{
                    //    case "QuestionAnswer":
                    //        AssociateGrid<QuestionAnswer>(dg);
                    //        break;
                    //    case "QuestionAnswerValue":
                    //        AssociateGrid<QuestionAnswerValue>(dg);
                    //        break;
                    //}
                    GridChildren.Add(dg.Name, dg);
                }
                if (item is Button)
                {
                    Button b = item as Button;
                    ButtonChildren.Add(b.Name, b);
                }
                if (item is Panel)
                {
                    iterate((item as Panel).Children);
                }
            }
        }

        /// <summary>
        /// Intercept the pasting of text into text boxes.
        /// Apply conversion of selected Unicode characters.
        /// </summary>
        /// <param name="sender">The object that is being pasted into</param>
        /// <param name="e">The pasted data object event arguments</param>
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!(sender is TextBox tb)) return;

            var sourceTextFormat = DataFormats.UnicodeText;
            var isText = e.SourceDataObject.GetDataPresent(sourceTextFormat, true);
            if (isText)
            {
                var input = e.DataObject.GetData(sourceTextFormat) as string;
                var result = input.ReplaceChars();

                if (string.IsNullOrWhiteSpace(result)) return;

                var start = tb.SelectionStart;
                var length = tb.SelectionLength;
                var caret = tb.CaretIndex;

                var text = tb.Text.Substring(0, start);
                text += tb.Text.Substring(start + length);

                var newText = text.Substring(0, tb.CaretIndex) + result;
                newText += text.Substring(caret);
                tb.Text = newText;
                tb.CaretIndex = caret + result.Length;

                e.CancelCommand();
            }
        }

        private void AssociateGrid<T>(DataGrid dg) where T : INotifyPropertyChanged, new()
        {
            asset["Table"][dg.Name].InnerXml = asset["Table"][dg.Name].InnerXml.XmlDecode();
            WpfXmlGrid<T> grid = new WpfXmlGrid<T>(dg, asset["Table"][dg.Name].InnerXml, dg.Name);
            grid.Data.ListChanged += delegate (object sender, ListChangedEventArgs e) { asset["Table"][dg.Name].InnerXml = grid.GetXml().ToString(); };
        }

        void Data_ListChanged(object sender, ListChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void tb_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.IsVisible)
                NLExtensions.textBox_AdornAndValidate(tb, null);
            else
                tb.clearAdornerLayer();
        }

        void tb_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop"))
            {
                string[] data = (string[])e.Data.GetData("FileDrop");
                string input = "";
                foreach (var item in data)
                {
                    string file = item.Substring(item.LastIndexOf('\\') + 1);
                    if (input != "") input += Environment.NewLine;
                    input += string.Format("~I{0}#~", file);
                }
                insertText(sender as TextBox, input);
            }
            if (e.Data.GetDataPresent(typeof(string)))
            {
                string data = (string)e.Data.GetData(typeof(string));
                if (data.StartsWith("ALGOID In ") || data.StartsWith("QUESTIONID In ") || data.StartsWith("ANSID In ") || data.StartsWith("RECID In ") || data.StartsWith("BPID In "))
                {
                    e.Handled = true;
                }
            }
            else if (e.Data.GetFormats()[0] == "Visio 11.0 Shapes")
            {
                if (e.Data.GetDataPresent("Visio 11.0 Shapes", true))
                {
                    Visio.Application app = Classes.VisioInterface.GetVisio();
                    Visio.Selection sel = app.ActiveWindow.Selection;
                    foreach (Visio.Shape shp in sel)
                    {
                        if (shp.get_CellExists("Prop.ID", 0) != 0)
                        {
                            insertText(sender as TextBox, shp.get_Cells("Prop.ID").Formula + (sel.Count > 1 ? "|" : ""));
                        }
                    }
                }
            }
        }

        private static void insertText(TextBox t, string si)
        {
            int ss = t.SelectionStart; int se = t.SelectionLength + ss;
            string s = t.Text;
            t.Text = s.Substring(0, ss) + si + s.Substring(se);
            t.SelectionStart = ss + si.Length;
            t.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        void tb_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if ((e.Data.GetDataPresent("FileDrop") || e.Data.GetFormats()[0] == "Visio 11.0 Shapes") && !(sender as TextBox).IsReadOnly) e.Handled = true;
        }

        public TextBox lastTextBox;

        void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            lastTextBox = sender as TextBox;
        }

        void cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (assetType == AssetType.Question && cb.Name == "cmbType" && cb.SelectedItem != null)
            {
                int value = (cb.SelectedItem as ListItem).ID;
                if ((value == 4 || value == 5) && !Window1.IsReviewerOrEditor)
                    bs.btnDeriveQuestion.IsEnabled = true;
                else
                    bs.btnDeriveQuestion.IsEnabled = false;
            }

            //            XmlDocument doc = new XmlDocument();
            //            doc.LoadXml(@"<NewDataSet>
            //    <Table8>
            //        <Object>cmbType</Object>
            //        <Exclude>pnlHistory</Exclude>
            //        <Values>10,11</Values>
            //    </Table8>
            //    <Table8>
            //        <Object>cmbType</Object>
            //        <Exclude>pnlComments</Exclude>
            //        <Values>10,11</Values>
            //    </Table8>
            //</NewDataSet>");

            if (cb.SelectedItem != null)
            {
                XmlNodeList xnl = cat.Defaults.SelectNodes(string.Format("*[Object='{0}']", cb.Name));
                int value = (cb.SelectedItem as ListItem).ID;
                foreach (XmlNode item in xnl)
                {
                    string[] values = item["Values"].InnerText.Split(',');
                    FrameworkElement fe = this.GetTemplateChild(item["Exclude"].InnerText) as FrameworkElement;
                    if (fe == null) continue;
                    if (Window1.CollapseDisabled)
                        if (values.Contains(value.ToString())) fe.Visibility = Visibility.Collapsed; else fe.Visibility = Visibility.Visible;
                    else
                        if (values.Contains(value.ToString())) fe.IsEnabled = false; else fe.IsEnabled = true;
                }
            }
        }

        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (bs.btnEdit.Visibility == Visibility.Visible) System.Media.SystemSounds.Beep.Play();
        }

        void tb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                NLTest nl = new NLTest(sender as TextBox);
                //nl.NLText = (sender as TextBox).Text;
                //nl.ShowDialog();
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                TextBox tb = sender as TextBox;
                int s = tb.SelectionStart;
                int idOut;
                if (tb.Name == "txtKeywords" && int.TryParse(tb.SelectedText, out idOut))
                    OpenPopup(idOut.ToString(), 4);
                else if (s > 0 && tb.SelectionLength > 0)
                {
                    char c = tb.Text[s - 1];
                    if (c == '{' || c == '|')
                    {
                        string id = tb.SelectedText;
                        if (c == '{') id = tb.SelectedText.Substring(2);
                        int i_id = 0;
                        if (int.TryParse(id, out i_id) && id.Length > 0)
                        {
                            int at = 0;
                            if (c == '|') at = 3;
                            else if (tb.SelectedText[0] == 'q') at = 2;
                            else if (tb.SelectedText[0] == 'c') at = 4;
                            if (at != 0)
                            {
                                //cat.AssetTypeID = -at;
                                //Window1.RadioToggle(cat.form.assetGroup, cat.AssetTypeID);
                                //cat.fullLoadAsset(id);
                                OpenPopup(id, at);
                            }
                        }
                    }
                    if (tb.Text.Length > tb.SelectionLength + s && c == '<' && tb.Text[s + tb.SelectionLength] == '>')
                    {
                        XElement variables = DataAccess.getData("ab_TableEdit", new string[] {
                            "@TableName", "Variables",
                            "@xml", "<root command=\"get\" />",
                        }, false);
                        var types = variables.Elements().Where(f => f.Element("value_Variable").Value == tb.SelectedText && f.Element("value_UserType") != null).Select(f => f.Element("value_UserType").Value);
                        var values = variables.Elements().Where(f => f.Element("value_Variable").Value == tb.SelectedText).Select(f => new { Type = f.Element("value_UserType") == null ? "Default" : f.Element("value_UserType").Value, Value = f.Element("value_Value").Value });
                        Dictionary<string, string> typeLookup = new Dictionary<string, string>() { { "Default", "Default" } };
                        foreach (var item in types)
                        {
                            if (!typeLookup.ContainsKey(item))
                            {
                                XmlNode xe = DataAccess.getDataNode("ab_GetAsset", new string[] {
                                    "@AssetTypeID", "3",
                                    "@AssetID", item
                                }, false);
                                typeLookup.Add(item, xe["Table"]["Clinical_Answer"].InnerText);
                            }
                        }
                        string content = tb.SelectedText;
                        foreach (var item in values)
                        {
                            if (content != "") content += "\n";
                            content += string.Format("{0} - {1}", typeLookup[item.Type], item.Value);
                        }
                        if (values.Any())
                        {
                            richPopup p = new richPopup(content, false);
                            cat.form.assetCanvas.Children.Add(p);
                            p.IsOpen = true;
                        }
                    }
                }
            }
        }

        private void OpenPopup(string id, int at)
        {
            XmlNode xe = DataAccess.getDataNode("ab_GetAsset", new string[] {
                                    "@AssetTypeID", at.ToString(),
                                    "@AssetID", id
                                }, false);
            string t = string.Format("{0} {1}) {2}{3}{4}", (AssetType)at, id,
                at == 2 ? xe["Table"]["Clinical_Statement"].InnerText : "",
                at == 3 ? xe["Table"]["Clinical_Answer"].InnerText : "",
                at == 4 ? xe["Table"]["Possible_Condition"].InnerText : ""
                );
            richPopup p = new richPopup(t);
            p.OpenAsset += delegate
            {
                cat.AssetTypeID = -at;
                Window1.RadioToggle(cat.form.assetGroup, cat.AssetTypeID);
                cat.fullLoadAsset(id);
            };
            cat.form.assetCanvas.Children.Add(p);
            p.IsOpen = true;
        }

        void p_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Popup p = (sender as Popup);
            p.IsOpen = false;
            p = null;
        }

        static SolidColorBrush blue = new SolidColorBrush(Color.FromRgb(236, 236, 255));

        void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            string suffix = "";
            if (!tb.IsReadOnly && !Window1.EnableLanguageInheritance && Window1.window.AlternateLanguages.Count > 0)
            {
                inherits ih = null;
                if (inheritList.ContainsKey(tb)) ih = inheritList[tb];
                if (originalAsset != null && ih != null && tb.Text != originalAsset["Table"][textPaths[tb.Name]].InnerText)
                {
                    //string lookup = textPaths[tb.Name];
                    //var tf = translationLookup.Where(f => f.Key.Item1 == assetType && f.Value == lookup).Select(f => f.Key.Item2);
                    //if (tf.Any()) lookup = tf.First();
                    //if (emptyLanguageFields.Any(f => f.Value.Contains(lookup)))
                    if (ih.List != null && ih.List.Count > 0)
                    {
                        ih.Visibility = Visibility.Visible;
                        tb.SetValue(TextBox.BackgroundProperty, blue);
                        //var languages = emptyLanguageFields.Where(f => f.Value.Contains(lookup)).Select(f => f.Key).Distinct();
                        var languages = ih.List;
                        suffix = "\n\nOriginal value will be applied to the following languages";
                        foreach (var item in languages)
                        {
                            suffix += string.Format("\n{0}", item);
                        }
                    }
                }
                else if (ih != null)
                {
                    ih.Visibility = Visibility.Collapsed;
                    tb.SetValue(TextBox.BackgroundProperty, FindResource("Editable"));
                }
            }
            if (!tb.IsReadOnly && !tb.Name.EndsWith("Language"))
            {
                TextBox ltb = (TextBox)tb.FindName(tb.Name + "Language");
                if (ltb != null && ltb.Text == "") NLExtensions.textBox_AdornAndValidate(ltb, null);
            }
            int selstart = tb.SelectionStart;
            int sellength = tb.SelectionLength;
            string[] s = { tb.Text.Substring(0, selstart), tb.Text.Substring(selstart, sellength), tb.Text.Substring(selstart + sellength) };
            int[] len = { s[0].Length, s[1].Length, s[2].Length };
            s[0] = s[0].Replace("\r\n", "\n").Replace("\n", "\r\n");
            s[1] = s[1].Replace("\r\n", "\n").Replace("\n", "\r\n");
            s[2] = s[2].Replace("\r\n", "\n").Replace("\n", "\r\n");
            string ss = string.Concat(s);
            if (ss.Length > tb.MaxLength && tb.MaxLength > 0) ss = ss.Substring(0, tb.MaxLength);
            if (ss != tb.Text)
            {
                tb.Text = ss;
                tb.SelectionStart = selstart + (s[0].Length - len[0]);
                tb.SelectionLength = sellength + (s[1].Length - len[1]);
            }
            SetTooltip(tb, suffix);
        }

        private static void SetTooltip(TextBox tb, string suffix = "")
        {
            if (tb.MaxLength == 0)
                tb.ToolTip = string.Format("{0} characters{1}", tb.Text.Length, suffix);
            else
                tb.ToolTip = string.Format("{0} out of {1} characters{2}", tb.Text.Length, tb.MaxLength, suffix);
        }

        public static void setNew(XmlNode asset, qcat cat)
        {
            XmlNode idcolumn = asset.SelectSingleNode("Table/*");
            XmlNode a = asset["Table"];
            idcolumn.InnerText = "new";
            int[] catids = cat.getCats();

            //foreach (var item in cats)
            //{
            //    a[item.Value].InnerText = catids[item.Key].ToString();
            //}
            switch (idcolumn.Name)
            {
                case "AlgoID":
                    a["AgeID"].InnerText = catids[0].ToString();
                    break;
                case "QuestionID":
                case "AnsID":
                    a["AgeID"].InnerText = catids[0].ToString();
                    a["HistID"].InnerText = catids[1].ToString();
                    a["Hist_SubID"].InnerText = catids[2].ToString();
                    a["BodyID"].InnerText = catids[3].ToString();
                    break;
                case "RecID":
                    a["AgeID"].InnerText = catids[0].ToString();
                    a["RecTypeID"].InnerText = catids[1].ToString();
                    a["TimeID"].InnerText = catids[2].ToString();
                    a["ProviderID"].InnerText = catids[3].ToString();
                    break;
                case "BPID":
                    a["AGEID"].InnerText = catids[0].ToString();
                    a["BodyID"].InnerText = catids[3].ToString();
                    break;
                case "MapID":
                case "CMapID":
                    a["DataID"].InnerText = catids[0].ToString();
                    a["CatID"].InnerText = catids[1].ToString();
                    a["SubCatID"].InnerText = catids[2].ToString();
                    a["Cat2ID"].InnerText = catids[3].ToString();
                    break;
            }
        }

        internal void setNew()
        {
            setNew(asset, cat);
            goEdit();

            //Bubble b = new Bubble();
            //b.content.Text = newScript[0];
            //Point p = cat.form.assetCanvas.TranslatePoint(new Point(170, 0), cat.form.bubbleCanvas);
            //b.SetValue(Canvas.LeftProperty, p.X);
            //b.SetValue(Canvas.TopProperty, p.Y);
            //cat.form.bubbleCanvas.Children.Add(b);
        }
    }

    public enum AssetType
    {
        AssetBuilder = -1,
        Title = 0,
        Algo,
        Question,
        Answer,
        Conclusion,
        Bullet,
        ConclusionCategory,
        Bullet_Use,
        Map = 11,
        TextAsset = 12,
        Group = 13,
        ConclusionMap = 15
    }

    public class usageShape
    {
        public string AlgoID { get; set; }
        public string NodeID { get; set; }
        public string ShapeName { get; set; }
        public string AlgoName { get; set; }
        public string PropertyType { get; set; }
        public string DataID { get; set; }
    }
}
