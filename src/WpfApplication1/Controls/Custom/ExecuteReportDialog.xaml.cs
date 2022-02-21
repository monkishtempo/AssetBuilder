using AssetBuilder.Classes;
using AssetBuilder.Properties;
using System;
using System.Collections.Generic;
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
using System.Xml;

namespace AssetBuilder.Controls.Custom
{
    /// <summary>
    /// Interaction logic for ExecuteReportDialog.xaml
    /// </summary>
    public partial class ExecuteReportDialog : UserControl
    {
        public AlgoLoader Loader { get; set; }
        public List<TextBox> Parameters { get; set; } = new List<TextBox>();
        public string Report { get; set; }
        public string ReportType { get; set; }
        public Dictionary<string, RadioButton> RadioButtons { get; set; }

        public ExecuteReportDialog(JNode parameters)
        {
            InitializeComponent();
            RadioButtons = ControlTree.getChildren<RadioButton>((DependencyObject)mainGrid).ToDictionary(f => f.CommandParameter.ToString());
            Report = parameters["report"].Value;
            ReportType = parameters["type"].Value ?? "";
            if (RadioButtons.ContainsKey(ReportType.Replace("html", ""))) RadioButtons[ReportType.Replace("html", "")].IsChecked = true;
            if (!string.IsNullOrWhiteSpace(ReportType)) ReportType = "/" + ReportType;
            ParameterLabels.Children.Clear();
            ParameterFields.Children.Clear();
            foreach (var item in parameters["parameters"])
            {
                ParameterLabels.Children.Add(new TextBlock { Text = item, Height = 24, FontSize = 16, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 5, 10, 5) });
                var tb = new TextBox { Height = 24, FontSize = 16, Margin = new Thickness(0, 5, 0, 5) };
                Parameters.Add(tb);
                ParameterFields.Children.Add(tb);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Loader != null) Loader.enableForm();
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            ReportType = RadioButtons.FirstOrDefault(f => f.Value.IsChecked == true).Key;
            if (!string.IsNullOrWhiteSpace(ReportType)) ReportType = "/" + ReportType;
            var source = new Uri(new Uri(Settings.Default.WebService), $"TraversalService/TableOutput/{Report}{ReportType}").AbsoluteUri;
            foreach (var item in Parameters)
            {
                source += $"/{item.Text}";
            }
            if (Loader != null)
            {
                Loader.PageID = Guid.NewGuid().ToString();
                if (textAssetBloat.IsChecked == true) source += "?TextAsset=Bloat";
                var content = source.GetContent<string>();
                if (ReportType.StartsWith("/csv") || ReportType.StartsWith("/file"))
                {
                    Loader.ScriptText = content;
                }
                else if (ReportType.StartsWith("/xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(content);
                    Loader.ScriptHtml = $"<!-- PageID='{Loader.PageID}' -->\n" + doc.DocumentElement.Transform("Xml.xsl", null);
                }
                else
                    Loader.ScriptHtml = (string.IsNullOrEmpty(ReportType) ? $"<!-- PageID='{Loader.PageID}' -->\n" : "") + content;
                Loader.enableForm();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus();
            if (Parameters.Count > 0) Parameters[0].Focus();
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Return) && Parameters.All(f => !string.IsNullOrWhiteSpace(f.Text)))
            {
                Execute_Click(this, null);
            }
            else if (e.Key == Key.Escape)
            {
                Cancel_Click(this, null);
            }
        }
    }
}
