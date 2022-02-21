using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace AssetBuilder.TableGrid
{
    /// <summary>
    /// Interaction logic for AutoUpdateDataGrid.xaml
    /// </summary>
    public partial class AutoUpdateDataGrid : UserControl
    {


        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); SetXmlSource(); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AutoUpdateDataGrid), new PropertyMetadata(null, OnItemsSourcePropertyChanged));


        private static void OnItemsSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // AutocompleteTextBox source = d as AutocompleteTextBox;
            // Do something...
        }

        private void SetXmlSource()
        {
            if (!(ItemsSource is XmlNode)) return;
            XmlNode node = ItemsSource as XmlNode;
            data.AutoGenerateColumns = false;
            foreach (var item in node.SelectNodes("Definition/*").OfType<XmlNode>())
            {
                string s = item.Attributes["Name"].Value;
                string t = item.Attributes["Type"].Value;
                Binding b = new Binding() { XPath = s };
                DataGridColumn c;
                switch (t)
                {
                    case "System.Data.SqlTypes.SqlXml":
                        //b.Source = node;
                        DataTemplate dt = new DataTemplate();
                        c = new DataGridTemplateColumn() { Header = s, CellTemplate = (DataTemplate)data.FindResource("innerGrid") };

                        var o = (DataTemplate)((c as DataGridTemplateColumn).CellTemplate);
                        //DataGrid dg = (DataGrid)getChild<DataGrid>((DependencyObject));
                        break;
                    default:
                        c = new DataGridTextColumn() { Binding = b, Header = s };
                        break;
                }
                data.Columns.Add(c);
            }
            data.DataContext = node.SelectNodes("*");
            data.ItemsSource = node.SelectNodes("*");

        }

        public AutoUpdateDataGrid()
        {
            InitializeComponent();
        }
    }

    public class BoundTemplateColumn : DataGridTemplateColumn
    {
        public Binding binding { get; set; }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            var element = base.GenerateEditingElement(cell, dataItem);
            element.SetBinding(ContentPresenter.ContentProperty, binding);
            return element;
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var element = base.GenerateElement(cell, dataItem);
            element.SetBinding(ContentPresenter.ContentProperty, binding);
            return element;
        }
    }
}
