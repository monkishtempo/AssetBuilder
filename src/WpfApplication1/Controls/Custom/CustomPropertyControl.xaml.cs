using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for CustomPropertyControl.xaml
    /// </summary>
    public partial class CustomPropertyControl : UserControl
    {
        string Method;
        string PropertyName;
        public bool Cancelled { get; set; }
        public string Value { get; set; }
        public CustomPropertyControl(string method, string propertyName, string value)
        {
            InitializeComponent();
            Method = method;
            Value = value;
            PropertyName = propertyName;
            Cancelled = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            btnCancel.Focus();
            var method = this.GetType().GetMethod(Method + "Method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null) method.Invoke(this, null);
        }

        void XmlCategoryPickerMethod()
        {
            if (Properties.CustomProperties.ContainsKey(PropertyName + "Data"))
            {
                XDocument doc = XDocument.Parse(Properties.CustomProperties[PropertyName + "Data"]);
                XmlCategoryPicker.Visibility = Visibility.Visible;
                Custom.CheckableItem data = new Custom.CheckableItem(null, null);
                var existingValue = DataAccess.JsonDeSerialize<List<Category>>(Value);
                AddNodes(data, (data.Children = new ObservableCollection<Custom.CheckableItem>()), doc.Element("Categories"), existingValue);
                XmlCategoryPicker.ItemsSource = data.Children;
            }

        }

        void AddNodes(Custom.CheckableItem parent, ObservableCollection<Custom.CheckableItem> items, XElement cat, List<Category> values)
        {
            foreach (var item in cat.Elements("Category"))
            {
                var tvi = new Custom.CheckableItem(parent.Owner ?? parent, parent) { Header = item.Attribute("label").Value };
                var match = values?.Where(f => f.Name == tvi.Header);
                List<Category> subValues = null;
                if (match != null && match.Any())
                {
                    tvi.IsChecked = true;
                    subValues = match.First().Categories;
                }
                tvi.PropertyChanged += Data_CollectionChanged;
                items.Add(tvi);
                AddNodes(tvi, (tvi.Children = new ObservableCollection<Custom.CheckableItem>()), item, subValues);
            }
        }

        private void Data_CollectionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var c = (sender as Custom.CheckableItem);
            if (c.IsChecked)
                c.Parent.IsChecked = true;
            else
                foreach (var item in c.Children)
                {
                    item.IsChecked = false;
                }
            XmlCategoryPicker.ItemsSource = c.Owner.Children;
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            var cats = GetCategoies(XmlCategoryPicker.ItemsSource as ObservableCollection<Custom.CheckableItem>);
            Value = DataAccess.JsonSerialize(cats);
        }

        List<Category> GetCategoies(ObservableCollection<Custom.CheckableItem> checks)
        {
            List<Category> cats = null;
            foreach (var item in checks)
            {
                if (item.IsChecked)
                {
                    if (cats == null) cats = new List<Category>();
                    Category cat = new Category { Name = item.Header };
                    cats.Add(cat);
                    cat.Categories = GetCategoies(item.Children);
                }
            }

            return cats;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancelled = true;
            (Parent as Panel).Children.Clear();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Panel).Children.Clear();
        }
    }

    public class Category
    {
        public string Name { get; set; }
        public List<Category> Categories { get; set; }
    }
}
