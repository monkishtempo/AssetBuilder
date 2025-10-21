using AssetBuilder.Classes;
using AssetBuilder.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RadioButton = System.Windows.Controls.RadioButton;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

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
            var segments = string.Join($"{(char)7}", Parameters.Select(f => f.Text));
            var query = "";

            if (Loader == null) return;

            Loader.PageID = Guid.NewGuid().ToString();
            if (textAssetBloat.IsChecked == true) query += "?TextAsset=Bloat";

            var content = "";
            //if(source.Length + segments.Length + query.Length <= 260)
            //    content = (source + segments + query).GetContent<string>();
            //else
                content = new { segments = segments }.PostObject<string>(source + query, new[] {("Content-Type", "application/json")});
            if (ReportType.StartsWith("/csv") || ReportType.StartsWith("/file"))
            {
                Loader.ScriptText = content;
            }
            else if (ReportType.StartsWith("/xml"))
            {
                var doc = new XmlDocument();
                doc.LoadXml(content);
                var footer = GetFooter(doc);
                if (footer != null) doc.DocumentElement?.AppendChild(footer);

                Loader.ScriptHtml = $"<!-- PageID='{Loader.PageID}' -->\n" + doc.DocumentElement.Transform("Xml.xsl", null);
            }
            else
            {
                content = content.Replace("@GENERATED@", $"Generated: {DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}");
                content = content.Replace("@AUTHOR@", $"Author: {Environment.UserName}");
                content = content.Replace("@REPORT@", $"Report: {Report.TrimStart('$')}");
                Loader.ScriptHtml = (string.IsNullOrEmpty(ReportType) ? $"<!-- PageID='{Loader.PageID}' -->\n" : "") + content;
            }

            Loader.enableForm();
        }

        private XmlElement GetFooter(XmlDocument doc)
        {
            var footer = doc.CreateElement("footer");
            footer.InnerXml = $"<generated>{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}</generated>" +
                              $"<author>{Environment.UserName}</author>" +
                              $"<report>{Report.TrimStart('$')}</report>";
            return footer;
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
