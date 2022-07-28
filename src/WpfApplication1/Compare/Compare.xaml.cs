using AssetBuilder.Controls;
using AssetBuilder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using ListItem = AssetBuilder.ViewModels.ListItem;

namespace AssetBuilder.Reports
{
    /// <summary>
    /// Interaction logic for Compare.xaml
    /// </summary>
    public partial class Compare : UserControl
    {
        private ToggleButton toggleButton;
        Func<bool, TextBlock> disable;
        Action disableAction;
        XDocument SourceXml;
        XDocument TargetXml;
        Action enable;
        AlgoLoader Loader;
        string pageid;

        public Compare(ToggleButton button, AlgoLoader loader)
        {
            InitializeComponent();
            var data = qcat.BuilderDefaults.SelectNodes("*[EnvironmentType='WebBuilder']").OfType<XmlNode>().Select(f => new ListItem { Value = f["EnvironmentName"].InnerText, MultID = f["EnvironmentUrl"].InnerText });

            Source.ItemsSource = data;
            Target.ItemsSource = data;
            LogicSource.ItemsSource = data;
            LogicTarget.ItemsSource = data;
            toggleButton = button;
            loader.AlgoSelectionChanged += Loader_AlgoSelectionChanged;
            disable = loader.disableForm;
            disableAction = delegate () { var tb = disable(false); tb.Text = "Running..."; };
            enable = loader.enableForm;
            Loader = loader;
            //output = wb;
        }

        private void Loader_AlgoSelectionChanged(object sender, Classes.AlgoSelectionChangedEventArgs e)
        {
            string sels = (e.Algos ?? "");
            if (sels != "" && !string.IsNullOrWhiteSpace(e.Assets)) sels += ",";
            sels += (e.Assets ?? "");
            selections.Text = sels;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Panel).Children.Clear();
            toggleButton.IsChecked = false;
        }


        private string ExtractAssets(string assets)
        {
            //if (assets == null) return "";
            int id = 0;
            var algos = string.Join(",", assets.Split(',').Where(f => int.TryParse(f.Trim(), out id)).Select(f => id));
            if (algos == "") algos = null;
            string a = "";
            char[] c = { 'S', 'Q', 'A', 'C', 'B' };
            string[] t = { "ALGO_START", "QUESTION", "ANSWER", "RECOMMENDATION", "BULLET" };
            for (int x = 0; x < c.Length; x++)
            {
                foreach (var i in assets.Split(',').Select(f => f.Trim()).Where(f => f.StartsWith(c[x].ToString())).Select(f => f.Substring(1)))
                    a += $"<{t[x]} id=\"{i}\" />";
            }
            var ip = chkprops.IsChecked == true ? " IncludeProperties=\"true\"" : "";
            if (a + ip != "") a = $"&args=@Assets&args=<root{ip}>{a}</root>";
            return algos + a;
        }

        private async void LoadData(object sender, RoutedEventArgs e)
        {
            await LoadData(sender);
        }

        private async Task<string> LoadData(object sender, bool enableForm = true)
        {
            DateTime dt;
            //output.Visibility = Visibility.Collapsed;
            var cp = (sender as ButtonBase).CommandParameter.ToString();
            var extraparamname = "Language";
            var extraparamvalue = "";
            var stub = Settings.Default.WebService;
            if (cp == "Source") extraparamvalue = SourceLanguage.Text;
            else if (cp == "Target") extraparamvalue = TargetLanguage.Text;
            if (DateTime.TryParse(extraparamvalue, out dt)) extraparamname = "@Date";
            if (cp == "Source" && Source.SelectedValue != null) stub = (Source.SelectedValue as ListItem).MultID;
            else if (cp == "Target" && Target.SelectedValue != null) stub = (Target.SelectedValue as ListItem).MultID;
            if (!stub.EndsWith("/")) stub += '/';
            var assets = ExtractAssets(selections.Text);
            var url = $"{stub}getData?procedure=ab_Report&args=Timeout&args=600&args={extraparamname}&args={extraparamvalue}&args=@ReportType&args=AssetReport&args=@Algos&args={assets}";
            DateTime then = DateTime.Now;
            using (var wc = new WebClient() { Encoding = Encoding.UTF8 })
            {
                var tb = disable(false);
                var uri = new Uri(url.Split('?')[0]);
                var prms = string.Join("?", url.Split('?').Skip(1));
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                tb.Text = $"Loading {cp} data";
                var task = await wc.UploadStringTaskAsync(uri, prms);
                { 
                    tb.Text = "Ran to Completion";
                    //Loader.ScriptText = f.Status.ToString();
                    //output.NavigateToString(f.Status.ToString());
                    if (enableForm) enable();
                    //output.Visibility = Visibility.Visible;
                    XDocument res = XDocument.Parse(task);
                    if (cp == "Source") SourceXml = res;
                    else if (cp == "Target") TargetXml = res;
                    DataAccess.AddLastCommand(url, res.Root.GetXmlNode(), (DateTime.Now - then));
                }
                return task;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Loader.SaveScript();
        }

        string content;

        private async void Run_Click(object sender, RoutedEventArgs e)
        {
            var preTitle = "Asset Compare Report";
            var disposition = (sender as Button)?.CommandParameter?.ToString() == "Dispositions";
            if (disposition) preTitle = "Disposition Change Report";

            var silent = (sender as Button)?.CommandParameter?.ToString() == "Silent";
            if (silent) preTitle = "Silent Conclusion Comparison Report";

            var reportResults = new List<Task<string>>();
            if (SourceXml == null) reportResults.Add(LoadData(SourceButton, false));
            if (TargetXml == null) reportResults.Add(LoadData(TargetButton, false));

            //Task.WaitAll(reportResults.ToArray());
            try
            {
                var prms = new Dictionary<string, string>
                {
                    { "TL", TargetLanguage.Text },
                    { "T", (Target.SelectedValue as ListItem)?.Value ?? "" },
                    { "SL", SourceLanguage.Text },
                    { "S", (Source.SelectedValue as ListItem)?.Value ?? new Uri(Settings.Default.WebService).Host },
                    { "Title", "" },
                    { "Subtitle", "" },
                };
                InputBox ib = new InputBox("Enter the Title and Subtitle.", "Input required", new[] { "Title", "Subtitle" }, WindowStartupLocation.CenterOwner, new[] { InputBoxValidate.Required, InputBoxValidate.None });
                ib[0] = GetTitle(prms, preTitle);
                ib.Owner = Loader;
                //ib.btnOK.IsEnabled = false;
                //Task.WhenAll(reportResults.ToArray()).ContinueWith(f => ib.btnOK.IsEnabled = true);
                ib.ShowDialog();
                if (!ib.DialogResult.HasValue || !ib.DialogResult.Value)
                {
                    enable();
                    return;
                }

                await Task.WhenAll(reportResults.ToArray());
                //await Task.Run (() => { Task.WaitAll(reportResults.ToArray()); });
                prms["Title"] = ib[0];
                prms["Subtitle"] = ib[1];
                if (disposition) {
                    DispositionChange dc = new DispositionChange() { prms = prms, Owner = Loader, Folder = @"Reports\DispositionChange" };
                    dc.rp = dc.GetPayLoad(TargetXml, SourceXml);
                    dc.Started += delegate (object obj, EventArgs ea) { disableAction(); };
                    dc.Completed += delegate (object obj, CompletedEventArgs ea)
                    {
                        content = ea.Content;
                        pageid = ea.UniqueID;
                        Loader.ScriptHtml = content;
                        Loader.PageID = pageid;
                        enable();
                    };
                    dc.RunReport();
                }
                else
                {
                    CompareReport cr = new CompareReport(prms, Loader) { Folder = @"Compare\Content", AddCommentColumn = chkComments.IsChecked ?? false };
                    cr.Started += delegate (object obj, EventArgs ea)
                    {
                        disableAction();
                    };
                    cr.Completed += delegate (object obj, CompletedEventArgs ea)
                    {
                        content = ea.Content;
                        pageid = ea.UniqueID;
                        Loader.ScriptHtml = content;
                        Loader.PageID = pageid;
                        //output.NavigateToString(content);
                        enable();
                    };
                    BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                    bw.RunWorkerCompleted += delegate (object worker, RunWorkerCompletedEventArgs ec)
                    {
                        cr.RunReport();
                    };
                    bw.DoWork += delegate (object worker, DoWorkEventArgs ew)
                    {
                        cr.rp = cr.GetComparePayload(prms, TargetXml, SourceXml, worker as BackgroundWorker, silent);
                    };
                    Progress p = new Progress(bw);
                    bw.ProgressChanged += delegate (object pc, ProgressChangedEventArgs ep)
                    {
                        p.pbStatus.Value = ep.ProgressPercentage;
                    };
                    p.Owner = Loader;
                    p.ShowDialog();
                }
            }
            catch(Exception ex)
            {
                Loader.ScriptText = ex.ToString();
                enable();
                throw (ex);
            }
        }

        private void Dc_Started(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private string GetTitle(Dictionary<string, string> prms, string v)
        {
            v += " : ";
            if (!string.IsNullOrEmpty(prms["T"])) v += $" {prms["T"]}";
            if (!string.IsNullOrEmpty(prms["TL"])) v += $" ({prms["TL"]})";
            if (!string.IsNullOrEmpty(prms["T"]) || !string.IsNullOrEmpty(prms["TL"])) v += " -";
            if (!string.IsNullOrEmpty(prms["S"])) v += $" {prms["S"]}";
            if (!string.IsNullOrEmpty(prms["SL"])) v += $" ({prms["SL"]})";
            return v;
        }

        private void Selections_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceXml = null;
            TargetXml = null;
        }

        private void TargetLanguage_TextChanged(object sender, TextChangedEventArgs e)
        {
            TargetXml = null;
        }

        private void SourceLanguage_TextChanged(object sender, TextChangedEventArgs e)
        {
            SourceXml = null;
        }

        private void Target_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TargetXml = null;
        }

        private void Source_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            chkComments.IsChecked = Source.SelectedValue == null || string.Compare((Source.SelectedValue as ListItem).MultID, Settings.Default.WebService, true) == 0;
            chkComments.Visibility = (chkComments.IsChecked ?? false) ? Visibility.Visible : Visibility.Hidden;
            SourceXml = null;
        }

        private void TraversalCompare_Click(object sender, RoutedEventArgs e)
        {
            Guid LeftTraversalID;
            Guid RightTraversalID;
            if (!Guid.TryParse(Traversal1.Text, out LeftTraversalID) || !Guid.TryParse(Traversal2.Text, out RightTraversalID)) MessageBox.Show("Please enter two valid TraversalIDs");
            else
            {
                var left = new Uri(new Uri(Settings.Default.WebService), "TraversalService/SummaryCompare/" + LeftTraversalID.ToString()).AbsoluteUri;
                var right = new Uri(new Uri(Settings.Default.WebService), "TraversalService/SummaryCompare/" + RightTraversalID.ToString()).AbsoluteUri;
                StartUrlCompare(left, right);
            }
        }

        private void UrlCompare_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(Url1.Text) || string.IsNullOrWhiteSpace(Url2.Text)) MessageBox.Show("Please enter two valid TraversalIDs");
            else StartUrlCompare(Url1.Text, Url2.Text);
        }

        private async void StartUrlCompare(string left, string right)
        {
            List<Task<string>> reportResults = new List<Task<string>>();
            if (!string.IsNullOrWhiteSpace(left)) reportResults.Add(Task.Run(() => Extension.GetWebRequest(left)));
            if (!string.IsNullOrWhiteSpace(right)) reportResults.Add(Task.Run(() => Extension.GetWebRequest(right)));
            await Task.WhenAll(reportResults.ToArray());
            var prms = new Dictionary<string, string>
                {
                    { "TL", "" },
                    { "T", left },
                    { "SL", "" },
                    { "S", right },
                    { "Title", "Url Comparison" },
                    { "Subtitle", "" },
                };
            CompareReport cr = new CompareReport(prms, Loader) { Folder = @"Compare\Url" };
            cr.Started += delegate (object obj, EventArgs ea)
            {
                disableAction();
            };
            cr.Completed += delegate (object obj, CompletedEventArgs ea)
            {
                content = ea.Content;
                pageid = ea.UniqueID;
                Loader.ScriptHtml = content;
                Loader.PageID = pageid;
                //output.NavigateToString(content);
                enable();
            };
            cr.rp = cr.GetUrlComparePayload(reportResults[0].Result, reportResults[1].Result);
            cr.RunReport();
        }

        private void chkprops_Checked(object sender, RoutedEventArgs e)
        {
            SourceXml = null;
            TargetXml = null;
        }

        private void LogicSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void LogicTarget_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void LogicCompare_Click(object sender, RoutedEventArgs e)
        {

            if (LogicSource.SelectedValue != null && LogicTarget.SelectedValue != null && !string.IsNullOrWhiteSpace(MotherAlgoID.Text) && MotherAlgoID.Text.Split(',').All(f => int.TryParse(f, out var _)))
            {
                try
                {
                    disableAction();
                    var s = (LogicSource.SelectedValue as ListItem);
                    var t = (LogicTarget.SelectedValue as ListItem);
                    var sourceUrl = s.MultID.ToLower().Replace("data.asmx", "TraversalService/");
                    var targetUrl = t.MultID.ToLower().Replace("data.asmx", "TraversalService/");

                    AssetBuilder.Reports.LogicCompare lc = new AssetBuilder.Reports.LogicCompare(sourceUrl, targetUrl, MotherAlgoID.Text, LimitAlgos.Text);
                    lc.Title = $"Logic Compare Report : {s.Value} - {t.Value}";
                    var report = await lc.GetReport();
                    Loader.PageID = lc.PageID.ToString();
                    Loader.ScriptHtml = report;
                }
                catch(Exception ex)
                {
                    Loader.ScriptText = ex.ToString();
                    throw (ex);
                }
                finally
                {
                    enable();
                }
            }
        }
    }
}
