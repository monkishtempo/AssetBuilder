using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for AlgoStatusList.xaml
    /// </summary>
    public partial class AlgoStatusList : UserControl
    {
        public AlgoStatusList()
        {
            InitializeComponent();
        }

        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // TODO: Set these as defaults, allow overriding by parent

            // Need to know the names, or do by index (no index information?) -
            // Number of 'stages' may vary, names may vary...
            // Should go from green...amber...red with intermediate colours.
            switch (e.Column.Header.ToString().ToLowerInvariant())
            {
                case "id":
                    e.Column.Width = 40;
                    break;
                case "name":
                    e.Column.Width = 150;
                    break;
                case "authoring":
                    e.Column.CellStyle = new Style(typeof(DataGridCell));
                    e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(209, 231, 221))));
                    e.Column.Width = 200;
                    break;
                case "training":
                    e.Column.CellStyle = new Style(typeof(DataGridCell));
                    e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(207, 226, 255))));
                    e.Column.Width = 200;
                    break;
                case "test":
                    e.Column.CellStyle = new Style(typeof(DataGridCell));
                    e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(207, 244, 252))));
                    e.Column.Width = 200;
                    break;
                case "health":
                    e.Column.CellStyle = new Style(typeof(DataGridCell));
                    e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(255, 243, 205))));
                    e.Column.Width = 200;
                    break;
                case "release":
                    e.Column.CellStyle = new Style(typeof(DataGridCell));
                    e.Column.CellStyle.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Color.FromRgb(248, 215, 218))));
                    e.Column.Width = 200;
                    break;
            }

            if (e.Column is DataGridTextColumn textColumn)
            {
                var style = new Style(typeof(TextBlock), textColumn.ElementStyle);
                style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
                textColumn.ElementStyle = style;
            }
        }
    }
}
