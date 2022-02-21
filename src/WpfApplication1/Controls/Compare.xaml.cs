using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using StringCompare;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for Compare.xaml
    /// </summary>
    public partial class Compare : UserControl
    {
        private int typeid;

        public static Regex _defaultRegex = new Regex(@"(?<=[^a-zA-Z]|^)([a-zA-Z]+?)(?: |(?=[^a-zA-Z])|$)|(?<= |[a-zA-Z]|^)([^a-zA-Z ]+?)(?: |(?=[a-zA-Z])|$)", RegexOptions.Multiline);
        public static Comparer<Match> _defaultComp = Comparer<Match>.Create((x, y) => x == null ? (y == null ? 0 : -1) : x.Value.Trim().CompareTo(y?.Value.Trim()));
        IList<DifferenceSets<Match>> Changes;

        private Compare()
        {
            InitializeComponent();
            typeid = Window1.window.qcat1.AssetTypeID;
            var match = AssetType.Items.OfType<ComboBoxItem>().FirstOrDefault(f => f.Tag.ToString() == typeid.ToString());
            if (match != null) AssetType.SelectedItem = match;
            AssetID.Text = Window1.window.qcat1.AssetID;

            Past.SizeChanged += ReAdorn;
            Current.SizeChanged += ReAdorn;
            Past.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(ReAdorn));
            Current.AddHandler(ScrollViewer.ScrollChangedEvent, new RoutedEventHandler(ReAdorn));

        }

        public static async Task<Compare> Create()
        {
            var c = new Compare();
            await c.Populate_OnClick(null, null);
            return c;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
            Window1.window.enableForm();
        }

        private Task Populate_OnClick(object sender, RoutedEventArgs e)
        {
            var cmd = Window1.window.qcat1.getAssetXml("assethistory", Window1.window.qcat1.tables[typeid]);
            var res = DataAccess.getData("ab_GetOtherData", "@xml", cmd.OuterXml);

            Versions.ItemsSource = res.Elements("Table").Select(f => new VersionItem(f));
            return Task.CompletedTask;
        }

        private void VersionsSelected(object sender, SelectionChangedEventArgs e)
        {
            if (Versions.SelectedItems.Count == 2)
            {
                Past.Text = CollateXElement((Versions.SelectedItems[0] as VersionItem).Source);
                Current.Text = CollateXElement((Versions.SelectedItems[1] as VersionItem).Source);
                var w1 = _defaultRegex.Matches(Past.Text).OfType<Match>().ToArray();
                var w2 = _defaultRegex.Matches(Current.Text).OfType<Match>().ToArray();
                Changes = StringCompare.LD.GetChangeSets(w1, w2, _defaultComp);
                Adorn();
            }
        }

        public void ReAdorn(object sender, RoutedEventArgs e)
        {
            Adorn();
        }


        bool adorning = false;
        void Adorn()
        {
            if (adorning || Changes == null) return;
            adorning = true;
            AdornerLayer fal = Past.clearAdornerLayer();
            AdornerLayer sal = Current.clearAdornerLayer();

            foreach (var item in Changes)
            {
                bool ov = false;
                if (item.OldValues != null)
                {
                    ov = true;
                    bool nv = item.NewValues != null;
                    HighLightAdorner ha = new HighLightAdorner(Past, item.OldValues.First().Index, item.OldValues.Sum(f => f.Length), nv ? "edit" : "delete");
                    fal.Add(ha);
                }
                if (item.NewValues != null)
                {
                    HighLightAdorner ha = new HighLightAdorner(Current, item.NewValues.First().Index, item.NewValues.Sum(f => f.Length), ov ? "edit" : "add");
                    sal.Add(ha);
                }
            }
            adorning = false;
        }

        string CollateXElement(XElement source)
        {
            string output = "";
            foreach (var elem in source.Elements())
            {
                output += $"{elem.Name}: {elem.Value}\r\n\r\n";
            }
            return output;
        }
    }

    class VersionItem
    {
        public XElement Source { get; set; }
        public DateTime Version { get; set; }

        public VersionItem(XElement source)
        {
            Source = source;
            DateTime dt;
            if (DateTime.TryParse(Source.ElementValue("DateTimeStamp"), out dt)) Version = dt;
        }

        public override string ToString()
        {
            return Version.ToString("MMM d, yyyy HH:mm");
        }
    }
}
