using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using AssetBuilder.AssetControls;
using AssetBuilder.Classes;
using AssetBuilder.Properties;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class Properties : ABWindow
    {
        private static Dictionary<string, string> _CustomProperties;
        public static Dictionary<string, string> CustomProperties { get { if (Settings.Default.WebService != PropertyUrl) UpdateProperties(); return _CustomProperties; } }
        private static string PropertyUrl = "";
        private static object lockObj = new object();

        private static void UpdateProperties()
        {
            lock (lockObj)
            {
                PropertyUrl = Settings.Default.WebService;
                _CustomProperties = new Dictionary<string, string>();
                var customProps = DataAccess.getData("dsp_GetProperty", new string[] {
                    "@PropertyType", "AssetBuilder",
                    "@DataID", "Properties"
                });

                foreach (var item in customProps.Elements("Table"))
                {
                    var name = item.Element("PropertyName").Value;
                    var value = item.Element("PropertyValue").Value;
                    if (!_CustomProperties.ContainsKey(name)) _CustomProperties.Add(name, value);
                    else _CustomProperties[name] = value;
                }
            }
        }

        static Properties()
        {
            UpdateProperties();
        }

        SolidColorBrush highlight = new SolidColorBrush(Color.FromArgb(255, 225, 204, 0));
        XmlGrid.PropertiesData PropData;
        AssetType AssetType;
        string AssetID;
        bool suspendevents = false;
        static Dictionary<string, Properties> openWindows = new Dictionary<string, Properties>();

        public string PropertyType
        {
            get
            {
                return AssetType + ":" + AssetID;
            }
        }

        public string SearchPropertyType
        {
            get
            {
                return (AssetType == AssetType.Algo ? "[AT][lr][ga][on]%" : AssetType.ToString()) + ":" + AssetID;
            }
        }

        public static Properties CreateProperties(AssetType assettype, string assetid, bool loadDefault = true)
        {
            if (openWindows.ContainsKey(assettype + ":" + assetid))
            {
                var pi = openWindows[assettype + ":" + assetid];
                pi.Activate();
                return pi;
            }
            var p = new Properties(assettype, assetid, loadDefault);
            if (assettype.NotIn(AssetType.Algo, AssetType.Question, AssetType.Answer, AssetType.Conclusion)) { p.btnNewInstance.IsEnabled = false; }
            return p;
        }

        private Properties(AssetType assettype, string assetid, bool loadDefault = true)
        {
            if (Settings.Default.WebService != PropertyUrl) UpdateProperties();
            InitializeComponent();
            AssetID = assetid;
            AssetType = assettype;
            openWindows.Add(PropertyType, this);
            if (loadDefault)
            {
                Type.Text = AssetType.ToString();
                DataID.Text = assetid.ToString();
            }
            PropData = new XmlGrid.PropertiesData(datagrid, "<Properties />") { Search = qcat.CurrentSearch };
            if (Window1.IsReviewerOrTranslator)
            {
                PropData.Properties.AllowEdit = false;
                PropData.Properties.AllowNew = false;
                PropData.Properties.AllowRemove = false;
            }
            else
            {
                PropData.Properties.ItemRemoved += Properties_ItemRemoved;
                PropData.Properties.ListChanged += Properties_ListChanged;
                datagrid.CellBeginEdit += Datagrid_CellBeginEdit;
            }
            XElement xn = DataAccess.getData("dsp_SearchProperties", "@PropertyType", SearchPropertyType);
            XElement defaultData = DataAccess.getData("dsp_GetProperty", new string[] { "@PropertyType", AssetType.ToString(), "@DataID", assetid.ToString() }, true);
            var algos = xn.Elements("Table").Select(f => new { AlgoID = f.Element("AlgoID").Value, Algo = f.Element("Algo_Name").Value }).Distinct();
            var nodes = xn.Elements("Table").Select(f => new { AlgoID = f.Element("AlgoID").Value, PropertyType = f.Element("PropertyType").Value, DataID = f.Element("DataID").Value }).Distinct();
            Regex regex = new Regex(qcat.CurrentSearch, RegexOptions.IgnoreCase);

            var matches = xn.Elements("Table")
                .Where(f => !string.IsNullOrWhiteSpace(qcat.CurrentSearch) && (regex.IsMatch(f.Element("PropertyName").Value) || regex.IsMatch(f.Element("PropertyValue").Value)))
                .Select(f => new { AlgoID = f.Element("AlgoID").Value, PropertyType = f.Element("PropertyType").Value, DataID = f.Element("DataID").Value }).Distinct();
            var defaultmatches = defaultData.Elements("Table")
                .Any(f => !string.IsNullOrWhiteSpace(qcat.CurrentSearch) && (regex.IsMatch(f.Element("PropertyName").Value) || regex.IsMatch(f.Element("PropertyValue").Value)));

            TreeViewItem dfault = new TreeViewItem() { Header = "Default", Tag = Tuple.Create(AssetType.ToString(), AssetID.ToString()) };
            dfault.Selected += tNode_Selected;
            usage.Items.Add(dfault);
            if (defaultmatches) dfault.Background = highlight;
            if (assettype == AssetType.Algo)
            {
                XElement transferData = DataAccess.getData("dsp_GetProperty", new string[] { "@PropertyType", "Transfer", "@DataID", assetid.ToString() }, true);
                var transfermatches = defaultData.Elements("Table")
                    .Any(f => !string.IsNullOrWhiteSpace(qcat.CurrentSearch) && (regex.IsMatch(f.Element("PropertyName").Value) || regex.IsMatch(f.Element("PropertyValue").Value)));

                TreeViewItem transfer = new TreeViewItem() { Header = "Transfer", Tag = Tuple.Create("Transfer", AssetID.ToString()) };
                transfer.Selected += tNode_Selected;
                usage.Items.Add(transfer);
                if (transfermatches) transfer.Background = highlight;
            }

            foreach (var algo in algos)
            {
                TreeViewItem tAlgo = new TreeViewItem() { Header = algo.Algo, IsExpanded = true };
                foreach (var item in nodes.Where(f => f.AlgoID == algo.AlgoID))
                {
                    TreeViewItem tNode = new TreeViewItem() { Header = item.DataID, Tag = Tuple.Create(item.PropertyType, item.DataID) };
                    tNode.Selected += tNode_Selected;
                    tAlgo.Items.Add(tNode);
                    if (matches.Any(f => f.PropertyType == item.PropertyType && f.DataID == item.DataID)) tNode.Background = highlight;
                }
                usage.Items.Add(tAlgo);
            }
            if (loadDefault) Populate(defaultData);
            //Exec dsp_SearchProperties @PropertyType = 'Question:7486', UserName = 'e24',  Password = '********'
        }

        private void windowsFormsHost_GotFocus(object sender, EventArgs e)
        {
            CustomEntry.Children.Clear();
        }

        private void Datagrid_CellBeginEdit(object sender, System.Windows.Forms.DataGridViewCellCancelEventArgs e)
        {
            var dgv = (sender as System.Windows.Forms.DataGridView);
            var cell = dgv.CurrentCell;
            var ci = cell.ColumnIndex;
            var ri = cell.RowIndex;
            if (ci == 1 && dgv[0, ri].Value == null) e.Cancel = true;
            else if (ci == 1 && dgv[0, ri].Value != null && _CustomProperties.ContainsKey(dgv[0, ri].Value.ToString()))
            {
                e.Cancel = true;
                var propertyName = dgv[0, ri].Value.ToString();
                var method = _CustomProperties[propertyName];
                var cp = new CustomPropertyControl(method, propertyName, cell.Value.ToString());
                cp.Tag = cell;
                CustomEntry.Children.Add(cp);
                FocusManager.SetFocusedElement(cp, null);
                Keyboard.ClearFocus();
                cp.Unloaded += cp_Unloaded;
            }
        }

        private void cp_Unloaded(object sender, RoutedEventArgs e)
        {
            var cp = (sender as CustomPropertyControl);
            if (cp.Cancelled) return;
            var cell = (cp.Tag as System.Windows.Forms.DataGridViewCell);
            cell.Value = cp.Value;
            CustomEntry.Children.Remove(cp);
            cp = null;
        }

        void Properties_ItemRemoved(object sender, ExpertData.ItemRemovedEventArgs<XmlGrid.Attribute> e)
        {
            if (!suspendevents)
            {
                SetProperty(Type.Text, DataID.Text, e.Item.Key, "null");
            }
        }



        void Properties_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
        {
            if (!suspendevents && (e.ListChangedType == System.ComponentModel.ListChangedType.ItemChanged || e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded))
            {
                SetProperty(Type.Text, DataID.Text, PropData.Properties[e.NewIndex].Key, PropData.Properties[e.NewIndex].Value);
            }
        }

        void SetProperty(string type, string dataid, string name, string value)
        {
            if (name != null && !Window1.IsReviewerOrTranslator)
                DataAccess.SetProperty(type, dataid, name, value);
        }

        void tNode_Selected(object sender, RoutedEventArgs e)
        {
            datagrid.EndEdit();
            var data = (sender as TreeViewItem).Tag as Tuple<string, string>;
            Type.Text = data.Item1;
            DataID.Text = data.Item2;
            Populate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            datagrid.Columns[0].Width = 160;
            datagrid.Columns[0].AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    col1.InvalidateVisual();
                }));
        }

        public void Populate(XElement data = null)
        {
            suspendevents = true;
            if (data == null) data = DataAccess.getData("dsp_GetProperty", new string[] { "@PropertyType", Type.Text, "@DataID", DataID.Text }, true);

            btnDefault.IsEnabled = Type.Text.Contains(':');

            GenerateTitle();

            //XmlDocument doc = Properties.Xml;
            PropData.Properties.Clear();
            foreach (var property in data.Elements("Table"))
            {
                PropData.Properties.Add(new XmlGrid.Attribute { Properties = PropData, Key = property.Element("PropertyName").Value, Value = property.Element("PropertyValue").Value });
            }
            suspendevents = false;
        }

        private void GenerateTitle()
        {
            string[] t = Type.Text.Split(':');
            string[] d = DataID.Text.Split(':');
            string type = t[0];
            string dataid = t.Length > 1 ? t[1] : d[0];
            string algoid = d.Length > 1 ? d[0] : "";
            string nodeid = d.Length > 1 ? d[1] : "";
            string title = string.Format("Properties for {0} {1}", type, dataid);
            if (!string.IsNullOrWhiteSpace(algoid) && !string.IsNullOrWhiteSpace(nodeid))
                title += string.Format(", Algo {0}, Node {1}", algoid, nodeid);
            this.Title = title;
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            datagrid.EndEdit();

            string[] s = Type.Text.Split(':');
            if (s.Length != 2) return;
            foreach (var item in PropData.Properties)
            {
                SetProperty(s[0], s[1], item.Key, item.Value);
            }
        }

        private void btnScript_Click(object sender, RoutedEventArgs e)
        {
            datagrid.EndEdit();

            var template = $@"If Not Exists(Select 1 From diva_properties Where PropertyType = '{Type.Text}' And DataID = '{DataID.Text}' And PropertyName = '{{0}}')
	Insert Into diva_properties (PropertyType, DataID, PropertyName, PropertyValue) Values('{Type.Text}', '{DataID.Text}', '{{0}}', '{{1}}')
Else If Not Exists (Select 1 From diva_properties Where PropertyType = '{Type.Text}' And DataID = '{DataID.Text}' And PropertyName = '{{0}}' And PropertyValue = '{{1}}')
	Update diva_properties Set [PropertyValue]='{{1}}' Where PropertyType = '{Type.Text}' And DataID = '{DataID.Text}' And PropertyName = '{{0}}'
";
            var delete = $"Delete diva_Properties Where PropertyType = '{Type.Text}' And DataID = '{DataID.Text}' And PropertyName Not In ({{0}})\r\n";
            var script = "";
            var keys = "";
            foreach (var item in PropData.Properties)
            {
                if (item.Key == null || item.Value == null) continue;
                if (keys != "") keys += ",";
                keys += $"'{item.Key}'";
                script += string.Format(template, item.Key, item.Value.Replace("'", "''"));
            }
            script = string.Format(delete, keys == "" ? "''" : keys) + script;
            AlgoLoader.SaveScript(script);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            datagrid.EndEdit();

            InputBox ib = new InputBox("Enter the AlgoID and NodeID.", "Input required", new[] { "AlgoID", "NodeID" }, WindowStartupLocation.CenterOwner, new[] { InputBoxValidate.Int, InputBoxValidate.Int });
            ib.Owner = this;
            ib.ShowDialog();
            if (ib.DialogResult.HasValue && ib.DialogResult.Value)
            {
                var aid = ib.Texts[0];
                var nid = ib.Texts[1];
                var xn = DataAccess.getData("ab_GetAsset", new[] { "@AssetTypeID", "1", "@AssetID", aid });
                var an = xn.Element("Table")?.Element("Algo_Name")?.Value;
                if (an != null)
                {
                    TreeViewItem tAlgo;
                    var match = usage.Items.OfType<TreeViewItem>().Where(f => f.Header.ToString() == an);
                    if (match.Any()) tAlgo = match.First();
                    else
                    {
                        tAlgo = new TreeViewItem() { Header = an, IsExpanded = true };
                        usage.Items.Add(tAlgo);
                    }

                    match = tAlgo.Items.OfType<TreeViewItem>().Where(f => f.Header.ToString() == $"{aid}:{nid}");
                    if (!match.Any())
                    {
                        TreeViewItem tNode = new TreeViewItem() { Header = $"{aid}:{nid}", Tag = Tuple.Create($"{AssetType}:{AssetID}", $"{aid}:{nid}") };
                        tNode.Selected += tNode_Selected;
                        tAlgo.Items.Add(tNode);
                    }
                }

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (openWindows.ContainsKey(PropertyType)) openWindows.Remove(PropertyType);
            datagrid.EndEdit();
        }
    }
}
