using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace XmlTreeView
{
    /// <summary>
    /// Interaction logic for AssetBuilderPropertiesEditor.xaml
    /// </summary>
    public partial class AssetBuilderPropertiesEditor : UserControl
    {
        int _MaxID = 0;
        private static Dictionary<string, string> _CustomProperties;
        EditableTreeView tview = null;

        public event EventHandler Closed;

        public AssetBuilderPropertiesEditor()
        {
            InitializeComponent();
            var data = AssetBuilder.DataAccess.getData("dsp_GetProperty", new string[] {
                "@PropertyType", "AssetBuilder",
                "@DataID", "Properties"
            });
            //var data = "http://aph.expert-24.net/webbuilder/data.asmx/getData?procedure=dsp_GetProperty&args=@PropertyType&args=AssetBuilder&args=@DataID&args=Properties".GetXmlExternal();

            _CustomProperties = new Dictionary<string, string>();
            foreach (var item in data.Elements("Table"))
            {
                var name = item.Element("PropertyName").Value;
                var value = item.Element("PropertyValue").Value;
                if (!_CustomProperties.ContainsKey(name)) _CustomProperties.Add(name, value);
                else _CustomProperties[name] = value;
            }

            UpdateForm();
        }

        private void UpdateForm()
        {
            foreach (var item in _CustomProperties.Where(f => f.Value != "XmlCategoryPicker"))
            {
                var et = EditableTextBlock.CreateReadOnlyKey(item.Key);
                et.PropertyChanged += Et_PropertyChanged;
                et.Changed += Et_KeyChanged;
                et.Selected += Et_KeySelected;
                properties.Children.Add(et);
            }
        }

        private void Et_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                var s = sender as EditableTextBlock;
                foreach (var item in properties.Children)
                {
                    if (item is EditableTextBlock)
                    {
                        var etb = item as EditableTextBlock;
                        if (etb != s && etb.IsSelected == true) etb.DeSelect();
                    }
                }
            }
        }

        private void Et_KeySelected(object sender, EventArgs e)
        {
            EditableTextBlock et = sender as EditableTextBlock;
            value.Children.Clear();
            string sub;
            if (et.Key.EndsWith("Data") && _CustomProperties.ContainsKey(sub = et.Key.Substring(0, et.Key.Length - 4)) && _CustomProperties[sub] == "XmlCategoryPicker")
            {
                var l = new ObservableCollection<TreeViewSource>();
                var parents = new Dictionary<string, ObservableCollection<TreeViewSource>>();
                var root = Guid.NewGuid().ToString();
                parents.Add(root, l);
                XDocument doc = XDocument.Parse(_CustomProperties[et.Key]);
                XmlReaderSettings settings = new XmlReaderSettings { IgnoreWhitespace = true, IgnoreComments = true, IgnoreProcessingInstructions = true };
                var maxId = 0;
                using (XmlReader cats = XmlReader.Create(new StringReader(_CustomProperties[et.Key]), settings))
                {
                    while (cats.ReadToFollowing("Category"))
                    {
                        int i;
                        var id = 0;
                        if (int.TryParse(cats.GetAttribute("id"), out i)) id = i;
                        if (id > maxId) maxId = id;
                        var description = cats.GetAttribute("description");
                        var apId = cats.GetAttribute("apId");
                        var label = cats.GetAttribute("label");
                        var pid = cats.GetAttribute("parentCategoryApId");
                        ObservableCollection<TreeViewSource> parent;
                        if (cats.Depth == 1) parent = l;
                        else
                        {
                            parent = parents[pid];
                        }

                        var tv = new TreeViewSource(apId, label) { id = id, description = description };
                        parents.Add(apId, tv.Children);
                        parent.Add(tv);
                        //var etv = EditableTextBlock.CreateValue(key, label);
                        //etv.Margin = new Thickness(cats.Depth * 20, 0, 0, 0);
                        //value.Children.Add(etv); 
                    }
                    tview = new EditableTreeView() { Key = et.Key };
                    tview.ItemsSource = l;
                    value.Children.Add(tview);
                    buttons.Visibility = Visibility.Visible;
                }
                _MaxID = maxId;
            }
            else
            {
                tview = null;
                buttons.Visibility = Visibility.Collapsed;
                var etv = EditableTextBlock.CreateValue(et.Key, _CustomProperties[et.Key]);
                etv.Changed += Et_ValueChanged;
                value.Children.Add(etv);
            }
        }

        private void Et_ValueChanged(object sender, EventArgs e)
        {
            EditableTextBlock et = sender as EditableTextBlock;
            if (et.Value != et.Text && _CustomProperties.ContainsKey(et.Key))
            {
                et.Value = et.Text;
                _CustomProperties[et.Key] = et.Value;
                AssetBuilder.DataAccess.SetProperty("AssetBuilder", "Properties", et.Key, et.Value);
                animateOpacity(message, $"Set Value of {et.Key} to \"{et.Value}\"");
                if(AssetBuilder.Controls.Properties.CustomProperties.ContainsKey(et.Key))
                    AssetBuilder.Controls.Properties.CustomProperties[et.Key] = et.Value;
            }
        }

        private void animateOpacity(Label message, string v)
        {
            DoubleAnimation da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(3)));
            message.Content = v;
            message.BeginAnimation(Label.OpacityProperty, da);
        }

        private void Et_KeyChanged(object sender, EventArgs e)
        {
            EditableTextBlock et = sender as EditableTextBlock;
            if (et.Key != et.Text && _CustomProperties.ContainsKey(et.Text)) et.Text = et.Key;
            else if (et.Key != et.Text)
            {
                _CustomProperties.Remove(et.Key);
                et.Key = et.Text;
                _CustomProperties.Add(et.Key, et.Value);
            }
        }

        public static TreeViewItem FindTviFromObjectRecursive(ItemsControl ic, object o)
        {
            if (ic == null) return null;
            //Search for the object model in first level children (recursively)
            TreeViewItem tvi = ic.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
            if (tvi != null) return tvi;
            //Loop through user object models
            foreach (object i in ic.Items)
            {
                //Get the TreeViewItem associated with the iterated object model
                TreeViewItem tvi2 = ic.ItemContainerGenerator.ContainerFromItem(i) as TreeViewItem;
                tvi = FindTviFromObjectRecursive(tvi2, o);
                if (tvi != null) return tvi;
            }
            return null;
        }

        private TreeViewItem GetItem(ItemCollection items, object find)
        {
            foreach (var item in items)
            {
                //if (item == find) return items.;
            }
            return null;
        }

        private ObservableCollection<TreeViewSource> GetParent(ObservableCollection<TreeViewSource> data, TreeViewSource item)
        {
            foreach (var child in data)
            {
                if (child == item)
                {
                    return data;
                }
                else
                {
                    var parent = GetParent(child.Children, item);
                    if (parent != null) return parent;
                }
            }
            return null;
        }

        private void Move(bool up)
        {
            if (tview != null)
            {
                var ud = up ? -1 : 1;
                var data = (ObservableCollection<TreeViewSource>)tview.TreeView.ItemsSource;
                var item = (TreeViewSource)tview.SelectedItem;
                var parent = GetParent(data, item);
                var index = parent.IndexOf(item);
                var ti = index + ud;
                if (ti >= 0 && ti <= parent.Count - 1)
                {
                    var t1 = parent[ti];
                    var t2 = parent[index];
                    parent.Remove(t1);
                    parent.Remove(t2);
                    parent.Insert(up ? ti : index, up ? t1 : t2);
                    parent.Insert(up ? ti : index, up ? t2 : t1);
                    var tvi = FindTviFromObjectRecursive(tview.TreeView, parent[ti]);
                    if (tvi != null) tvi.IsSelected = true;
                }
            }
        }

        private void AddChildItem(object sender, RoutedEventArgs e)
        {
            AddItem(false);
        }
        private void AddSiblingItem(object sender, RoutedEventArgs e)
        {
            AddItem(true);
        }
        private void AddItem(bool sibling)
        {
            if (tview != null)
            {
                var data = (ObservableCollection<TreeViewSource>)tview.TreeView.ItemsSource;
                var item = (TreeViewSource)tview.SelectedItem;
                var parent = sibling ? GetParent(data, item) : item.Children;
                var index = sibling ? parent.IndexOf(item) : -1;
                if (index == parent.Count - 1) parent.Add(new TreeViewSource("", "new item")); else parent.Insert(index + 1, new TreeViewSource("", "new item"));
                if (!sibling)
                {
                    var tvi = FindTviFromObjectRecursive(tview.TreeView, item);
                    if (tvi != null) tvi.IsExpanded = true;
                }
            }
        }
        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            var data = (ObservableCollection<TreeViewSource>)tview.TreeView.ItemsSource;
            var item = (TreeViewSource)tview.SelectedItem;
            var parent = GetParent(data, item);
            parent.Remove(item);
        }
        private void MoveUp(object sender, RoutedEventArgs e)
        {
            Move(true);
        }
        private void MoveDown(object sender, RoutedEventArgs e)
        {
            Move(false);
        }

        private void SaveCategories(object sender, RoutedEventArgs e)
        {
            var data = (ObservableCollection<TreeViewSource>)tview.TreeView.ItemsSource;
            XDocument doc = new XDocument();
            var root = new XElement("Categories");
            doc.Add(root);
            int id = _MaxID;
            AddBranches(root, data, Guid.Empty, ref id);
            if(_CustomProperties.ContainsKey(tview.Key)) _CustomProperties[tview.Key] = doc.ToString();
            AssetBuilder.DataAccess.SetProperty("AssetBuilder", "Properties", tview.Key, _CustomProperties[tview.Key]);
            animateOpacity(message, $"Updated {tview.Key}");
            if (AssetBuilder.Controls.Properties.CustomProperties.ContainsKey(tview.Key))
                AssetBuilder.Controls.Properties.CustomProperties[tview.Key] = _CustomProperties[tview.Key];
        }

        private void AddBranches(XElement elem, ObservableCollection<TreeViewSource> items, Guid parentId, ref int id)
        {
            foreach (var item in items)
            {
                var newid = item.apId;
                var child = new XElement("Category", new XAttribute("id", item.id == 0 ? ++id : item.id), new XAttribute("apId", newid.ToString().ToUpper()), new XAttribute("label", item.Category), new XAttribute("description", item.description ?? ""));
                elem.Add(child);
                if (parentId != Guid.Empty) child.Add(new XAttribute("parentCategoryApId", parentId.ToString().ToUpper()));
                AddBranches(child, item.Children, newid, ref id);
            }
        }

        private void CloseForm(object sender, RoutedEventArgs e)
        {
            (Parent as Panel)?.Children.Remove(this);
            Closed?.Invoke(this, new EventArgs());
        }
    }
}
