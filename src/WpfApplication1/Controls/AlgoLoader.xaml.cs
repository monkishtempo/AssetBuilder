using AssetBuilder.Classes;
using AssetBuilder.Properties;
using AssetBuilder.Reports;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Microsoft.Windows.Controls.Ribbon;
using mshtml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using AssetBuilder.ViewModels;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for AlgoLoader.xaml
    /// </summary>
    public partial class AlgoLoader : ABRibbonWindow
    {
        private const string ExportError = "An error occurred during Export. Export cannot continue.\r\n\r\nPlease contact Toolkit support.";

        private AlgoReleaseStatusCollectionViewModel _algoStatusViewModel;

        public static AlgoLoader AlgoLoaderForm = null;
        public static XmlNode KeyWordXml;
        LoadedAlgos loadedAlgos = null;
        public string PageID;

        public string Url
        {
            set
            {
                Loaded += delegate (object sender, RoutedEventArgs e)
                {
                    ScriptAssetsFromUrl(value);
                };
            }
        }

        private void ScriptAssetsFromUrl(string value)
        {
            string s;
            if (value.StartsWith("http"))
                s = Extension.GetWebRequest(value);
            else
                s = _ScriptText;
            //Dictionary<string, List<Tuple<string, string>>> inputs = new Dictionary<string, List<Tuple<string, string>>>();
            var ms = Regex.Matches(s, @"(?:<!-- Script : )(?<class>.*ID)(?: In )(?:[(])(?<id>[0-9]*)(?:[)])(?:[(])(?<text>.*)(?:[)])(?: -->)");
            lstAssets.Items.Clear();
            foreach (Match m in ms)
            {
                //var c = m.Groups["class"].Value;
                //if (!inputs.ContainsKey(c)) inputs.Add(c, new List<Tuple<string, string>>);
                //inputs[c].Add(Tuple.Create(m.Groups["ID"].Value, m.Groups["text"].Value));
                var type = assetTypes[m.Groups["class"].Value];
                lstAssets.Items.Add(new LoadedAsset { AssetID = int.Parse(m.Groups["id"].Value), Type = type, Text = m.Groups["text"].Value });
            }
            rbtScriptAssets.IsChecked = true;
            ScriptAssets_Click(null, null);
            updateCompare();
            setButtons();
            GenerateExport(false);
        }

        public bool IsDesignTime => DesignerProperties.GetIsInDesignMode(this);

        //private string WebBrowserTextTemplate = @"<!DOCTYPE html><html><head><meta http-equiv=""Content-Type"" content=""text/plain;charset=UTF-8""></head><body style=""white-space:pre-wrap;"">{0}</body></html>";
        //private string WebBrowserHtmlTemplate = @"<!DOCTYPE html><html><head><meta http-equiv=""Content-Type"" content=""text/html;charset=UTF-8""></head><body>{0}</body></html>";
        private string CloseIcon = @"<i class=""w3-right fa fa-close w3-text-red""></i>";

        private string _ScriptText;
        public string ScriptText
        {
            get { return _ScriptText; }
            set
            {
                _ScriptText = value;
                rbtCopy.IsEnabled = _ScriptText != "";
                //txtScript.NavigateToString(string.Format(WebBrowserTextTemplate, _ScriptText));
                txtScript.Visibility = Visibility.Hidden;
                txtScriptTextBox.Text = _ScriptText;
                txtScriptTextBox.Visibility = Visibility.Visible;
            }
        }

        Regex removes = new Regex("<!-- remove start -->.*?<!-- remove end -->", RegexOptions.Singleline);
        public string ScriptHtml
        {
            get { return _ScriptText; }
            set
            {
                _ScriptText = removes.Replace(value, "");
                rbtCopy.IsEnabled = _ScriptText != "";
                txtScriptTextBox.Visibility = Visibility.Hidden;
                if (value.Length > 500000)
                {
                    var guid = Guid.NewGuid().ToString();
                    var uri = new Uri(Environment.ExpandEnvironmentVariables($"%TEMP%\\{guid}.html"));
                    File.WriteAllText(uri.AbsolutePath, value);
                    txtScript.Source = uri;
                }
                else
                {
                    txtScript.NavigateToString(value);
                }
                txtScript.Visibility = Visibility.Visible;
                //var doc = (IHTMLDocument2)txtScript.Document;
                //doc.charset = "utf-8";                
            }
        }

        private void DocEvents_onmouseup(IHTMLEventObj pEvtObj)
        {
            var elem = pEvtObj.srcElement;
            if (elem.tagName.ToLower() == "i")
            {
                while (elem != null && elem.className?.Contains("w3-panel") != true)
                {
                    elem = elem.parentElement;
                }
                if (elem != null)
                {
                    var id = elem.getAttribute("data-id");
                    var start = _ScriptText.IndexOf($@"<!-- Start Asset {id} -->", StringComparison.InvariantCultureIgnoreCase);
                    var end = _ScriptText.IndexOf($@"<!-- End Asset {id} -->", StringComparison.InvariantCultureIgnoreCase);
                    _ScriptText = _ScriptText.Substring(0, start) + _ScriptText.Substring(end + $@"<!-- End Asset {id} -->".Length);
                    //_ScriptText = Regex.Replace(_ScriptText, $@"(?i:<!-- Start Asset {id} -->)(?s:.*)(?i:<!-- End Asset {id} -->)", "");
                    elem.outerHTML = "";
                }
            }
            else if (elem.tagName.ToLower() == "a" && elem.innerHTML == "Script Assets")
            {
                Url = "Go!";
            }
        }


        private IEnumerable<string> AddSource(IEnumerable<string> prms)
        {
            if (!string.IsNullOrWhiteSpace(cbSource?.SelectedValue?.ToString()))
            {
                List<string> list = new List<string>(prms);
                list.AddRange(new string[] { "Source", cbSource?.SelectedValue?.ToString() });
                return list;
            }
            return prms;
        }

        public AlgoLoader()
        {
            InitializeComponent();
            var core = CoreWebView2Environment.CreateAsync().Result;
            txtScript.EnsureCoreWebView2Async(core);
            txtScript.NavigationStarting += TxtScript_NavigationStarting;
            //txtScript.NavigationCompleted += delegate (object sender, CoreWebView2NavigationCompletedEventArgs e)
            //{
            //    doc = (txtScript as mshtml.HTMLDocument);
            //    HTMLDocumentEvents2_Event docEvents = doc as HTMLDocumentEvents2_Event;
            //    if (docEvents != null)
            //    {
            //        docEvents.onmouseup += DocEvents_onmouseup;
            //    }
            //};
            if (Window1.McKesson_Mode) rbtMcKessonXml.Visibility = Visibility.Visible;
            AlgoLoaderForm = this;
            if (!IsDesignTime)
            {
                string[] sources = Settings.Default.AvailableSources.Split(';');
                foreach (var source in sources)
                {
                    if (source == "") continue;
                    cbSource.Items.Add(source);
                }
                string[] targets = Settings.Default.AvailableTargets.Split(';');
                foreach (var target in targets)
                {
                    if (target == "") continue;
                    cbTarget.Items.Add(target);
                }
                RefreshData("<root command=\"algos\" />");
            }
            ((UIElement)McKessonAlgoSummary).Visibility = Window1.McKesson_Mode ? Visibility.Visible : Visibility.Collapsed;
            ((UIElement)McKessonAssetReport).Visibility = Window1.McKesson_Mode ? Visibility.Visible : Visibility.Collapsed;
            ((UIElement)McKessonSelfcareListing).Visibility = Window1.McKesson_Mode ? Visibility.Visible : Visibility.Collapsed;
            AssetReleaseReport.Visibility = Window1.AllowReleaseStatusView ? Visibility.Visible : Visibility.Collapsed;
            //txtScript.ObjectForScripting = new ScriptInterface();
        }

        private void TxtScript_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.IsUserInitiated)
            {
                if (e.Uri.StartsWith("remove:"))
                {
                    var split = e.Uri.Split(':');
                    var id = split[1];
                    var assetType = split.Length > 2 ? split[2] : "Asset";
                    var start = _ScriptText.IndexOf($@"<!-- Start {assetType} {id} -->", StringComparison.InvariantCultureIgnoreCase);
                    var end = _ScriptText.IndexOf($@"<!-- End {assetType} {id} -->", StringComparison.InvariantCultureIgnoreCase);
                    _ScriptText = _ScriptText.Substring(0, start) + _ScriptText.Substring(end + $@"<!-- End {assetType} {id} -->".Length);
                    //_ScriptText = Regex.Replace(_ScriptText, $@"(?i:<!-- Start Asset {id} -->)(?s:.*)(?i:<!-- End Asset {id} -->)", "");
                    txtScript.ExecuteScriptAsync($"var elem = document.getElementById('asset-{id}'); elem.parentNode.removeChild(elem);");
                    e.Cancel = true;
                }
                if (e.Uri.StartsWith("up:"))
                {
                    var split = e.Uri.Split(':');
                    var id = split[1];
                    var assetType = split.Length > 2 ? split[2] : "Asset";
                    var start = _ScriptText.IndexOf($@"<div id=""asset-{id}""", StringComparison.InvariantCultureIgnoreCase);
                    var end = _ScriptText.IndexOf($@"<!-- End {assetType} {id} -->", start, StringComparison.InvariantCultureIgnoreCase);
                    end = _ScriptText.IndexOf("</div>", end) + 6;
                    if (_ScriptText.Substring(start - 6, 6) == "</div>")
                    {
                        var ps = _ScriptText.Substring(0, start).LastIndexOf($@"<!-- Start {assetType}");
                        ps = _ScriptText.Substring(0, ps).LastIndexOf(@"<div id=""asset-");
                        var pe = _ScriptText.IndexOf($@"<!-- End {assetType}", ps);
                        pe = _ScriptText.IndexOf("</div>", pe) + 6;
                        _ScriptText = _ScriptText.Substring(0, ps) + _ScriptText.Substring(start, end - start) + _ScriptText.Substring(ps, pe - ps) + _ScriptText.Substring(end);
                    }
                    txtScript.ExecuteScriptAsync($"var current = document.getElementById('asset-{id}'); var previous = current.previousSibling; if(previous !== null && previous.id !== undefined && previous.id.indexOf('asset-') === 0) {{ current.parentNode.insertBefore(current, previous); }}");
                    e.Cancel = true;
                }
                if (e.Uri.StartsWith("down:"))
                {
                    var split = e.Uri.Split(':');
                    var id = split[1];
                    var assetType = split.Length > 2 ? split[2] : "Asset";
                    var start = _ScriptText.IndexOf($@"<div id=""asset-{id}""", StringComparison.InvariantCultureIgnoreCase);
                    var end = _ScriptText.IndexOf($@"<!-- End {assetType} {id} -->", start, StringComparison.InvariantCultureIgnoreCase);
                    end = _ScriptText.IndexOf("</div>", end) + 6;
                    if (_ScriptText.Substring(end, 15) == @"<div id=""asset-")
                    {
                        var ns = _ScriptText.IndexOf(@"<div id=""asset-", end);
                        var ne = _ScriptText.IndexOf($@"<!-- End {assetType}", ns);
                        ne = _ScriptText.IndexOf("</div>", ne) + 6;
                        _ScriptText = _ScriptText.Substring(0, start) + _ScriptText.Substring(ns, ne - ns) + _ScriptText.Substring(start, end - start) + _ScriptText.Substring(ne);
                    }
                    txtScript.ExecuteScriptAsync($"var current = document.getElementById('asset-{id}'); var next = current.nextSibling; if(next !== null && next.id !== undefined && next.id.indexOf('asset-') === 0) {{ current.parentNode.insertBefore(next, current); }}");
                    e.Cancel = true;
                }
            }
        }

        private void TxtScript_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RefreshData(string command)
        {
            cmbAssessment.Items.Clear();
            XmlNodeList xnl = qcat.BuilderDefaults.SelectNodes(string.Format("*[ComboBox='{0}']", "cmbAssessment"));
            foreach (XmlNode xn in xnl)
                cmbAssessment.Items.Add(new ListItem { ID = int.Parse(xn.ChildNodes[1].InnerText), Value = xn.ChildNodes[2].InnerText });
            XElement xe = DataAccess.getData("ab_updateasset", AddSource(new string[] {
                "@xml", command
            }).ToArray(), true);
            List<string> consumeralgos = null;
            if (Window1.McKesson_Mode)
            {
                consumeralgos = DataAccess.getData("abmk_GetConsumerFlags", AddSource(new string[] { }).ToArray(), true)
                    .Elements("Table")
                    .Where(f => getValue(f, "ConsFlag") == "Y")
                    .Select(f => getValue(f, "AlgoID"))
                    .ToList();
            }
            var data = (from item in xe.Elements("Table")
                        select new LoadedAlgo
                        {
                            AlgoID = int.Parse(getValue(item, "AssetID")),
                            FileName = getValue(item, "FileName"),
                            MachineName = getValue(item, "MachineName"),
                            AssessmentType = int.Parse(getValue(item, "Assessment")),
                            UserName = getValue(item, "UserName"),
                            Title = getValue(item, "Title"),
                            Promoted = DateTime.Parse(getValue(item, "promoted")),
                            LoadID = getValue(item, "loadid"),
                            Consumer = consumeralgos != null && consumeralgos.Contains(getValue(item, "AssetID"))
                        }).OrderByDescending(f => f.Promoted);
            loadedAlgos = (LoadedAlgos)Resources["loadedAlgos"];
            loadedAlgos.Clear();
            foreach (var item in data)
            {
                loadedAlgos.Add(item);
            }
            //System.Windows.Data.CollectionView cv = new System.Windows.Data.CollectionView(data);
            //dataGrid1.ItemsSource = cv;
        }

        string getValue(XElement item, string name)
        {
            if (item.Element(name) != null) return item.Element(name).Value;
            return "";
        }

        private void cmbAssessment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chkFilterByType.IsChecked == true)
            {
                if (refreshing) return;
                refreshing = true;
                RefreshData();
                refreshing = false;
                return;
            }
            if (e.AddedItems.Count == 0 || !(e.AddedItems[0] is ListItem)) return;
            int assessmentType = (e.AddedItems[0] as ListItem).ID;
            suspend = true;
            ClearDataGrid();
            foreach (var item in dataGrid1.Items)
            {
                if (item is LoadedAlgo && (item as LoadedAlgo).AssessmentType == assessmentType)
                    dataGrid1.SelectedItems.Add(item);
            }
            setTextBox();
            suspend = false;
        }

        private void ClearDataGrid()
        {
            dataGrid1.SelectedItems.Clear();
            foreach (var item in dataGrid1.Items)
                if (item is LoadedAlgo) (item as LoadedAlgo).Exclude = false;
        }

        bool suspend = false;
        List<int> Algos = new List<int>();
        string algoList = "";
        string excludedAlgoList = "";
        bool containsOldAlgos = false;
        int algosIncluded = 0;
        int algosExcluded = 0;

        private void dataGrid1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            foreach (var item in e.AddedCells)
            {
                if (item.Item is LoadedAlgo)
                {
                    LoadedAlgo la = item.Item as LoadedAlgo;
                    if (la.Exclude != RightButtonPressedLast) la.Exclude = RightButtonPressedLast;
                }
            }
            RightButtonPressedLast = false;
            algoList = "";
            excludedAlgoList = "";
            containsOldAlgos = false;
            algosIncluded = 0;
            algosExcluded = 0;
            Algos.Clear();
            foreach (var item in dataGrid1.SelectedItems)
            {
                if (item is LoadedAlgo)
                {
                    LoadedAlgo la = item as LoadedAlgo;
                    Algos.Add(la.AlgoID);
                    if (!la.Exclude)
                    {
                        algoList += string.Format("{0}{1} ({2})", algoList == "" ? "" : ", ", la.Title, la.AlgoID);
                        algosIncluded++;
                    }
                    else
                    {
                        excludedAlgoList += string.Format("{0}{1} ({2})", excludedAlgoList == "" ? "" : ", ", la.Title, la.AlgoID);
                        algosExcluded++;
                    }
                    if (la.LoadID == "") containsOldAlgos = true;
                }
            }
            if (!suspend)
            {
                if (chkFilterByType.IsChecked != true) cmbAssessment.SelectedItem = null;
                suspend = true;
                setTextBox();
                suspend = false;
            }
            sbiInfo.Content = algosIncluded + " Algos selected" + " - " + algosExcluded + " Algos excluded";
            if (!refreshing && chkShowSelectedOnly.IsChecked == true)
            {
                if (refreshing) return;
                refreshing = true;
                RefreshData();
                refreshing = false;
            }
        }

        string excludedAlgos;

        private void setTextBox()
        {
            excludedAlgos = "";
            string list = "";
            foreach (var item in dataGrid1.SelectedItems)
            {
                LoadedAlgo la = item as LoadedAlgo;
                if (!la.Exclude)
                {
                    if (list != "") list += ",";
                    list += la.AlgoID;
                }
                else
                {
                    if (excludedAlgos != "") excludedAlgos += ",";
                    excludedAlgos += la.AlgoID;
                }
            }
            suspend = true;
            textBox1.Text = string.Format("{0}", list);
            txt_Excluded.Text = string.Format("{0}", excludedAlgos);
            suspend = false;
            setButtons();
            AlgoSelectionChangedEventArgs args = new AlgoSelectionChangedEventArgs() { Algos = list, Assets = cachedAssetList };
            OnAlgoSelectionChanged(args);
        }

        private void setButtons()
        {
            bool value = algosIncluded > 0 && algosExcluded == 0;
            setButtons(value);
        }

        private void setButtons(bool value)
        {
            bool assetMode = (bool)rbtScriptAssets.IsChecked;
            rbtGenerateScript.IsEnabled = (!assetMode && value) || (assetMode && lstAssets.Items.Count > 0);
            if (staticButtons.ContainsKey("rbtSelectReachable")) staticButtons["rbtSelectReachable"].ForEach(f => f.IsEnabled = !assetMode && value);
            rbtDelete.IsEnabled = !assetMode && value;
            rbtRefresh.IsEnabled = !assetMode;
            rbtScriptWithoutAssets.IsEnabled = !assetMode && value;
            rbtScriptXml.IsEnabled = !assetMode && algosIncluded == 1 && algosExcluded == 0;
            rbtMcKessonXml.IsEnabled = !assetMode && algosIncluded >= 1 && algosExcluded == 0;
        }

        public event EventHandler<AlgoSelectionChangedEventArgs> AlgoSelectionChanged;

        public virtual void OnAlgoSelectionChanged(AlgoSelectionChangedEventArgs e)
        {
            AlgoSelectionChanged?.Invoke(this, e);
        }
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text.Contains(' '))
            {
                int tss = tb.SelectionStart;
                int tsl = tb.SelectionLength;
                tb.Text = tb.Text.Replace(' ', ',');
                tb.SelectionStart = tss;
                tb.SelectionLength = tsl;
                return;
            }
            if (!suspend && TextCorrect(textBox1) && TextCorrect(txt_Excluded))
            {
                bool resettext = false;
                if (chkFilterByType.IsChecked != true) cmbAssessment.SelectedItem = null;
                suspend = true;
                int i;
                List<int> algos = new List<int>(textBox1.Text.Split(',', ' ').Select(f => int.TryParse(f, out i) ? i : 0));
                List<int> exclude = new List<int>(txt_Excluded.Text.Split(',', ' ').Select(f => int.TryParse(f, out i) ? i : 0));
                if (algos.Any(f => exclude.Contains(f)))
                {
                    resettext = true;
                    algos.RemoveAll(f => exclude.Contains(f));
                }

                ClearDataGrid();
                foreach (var item in dataGrid1.Items)
                {
                    if (item is LoadedAlgo)
                    {
                        LoadedAlgo la = item as LoadedAlgo;
                        if (algos.Contains(la.AlgoID))
                            dataGrid1.SelectedItems.Add(item);
                        if (exclude.Contains(la.AlgoID))
                        {
                            RightButtonPressedLast = true;
                            dataGrid1.SelectedItems.Add(item);
                        }
                    }
                }
                setButtons();
                excludedAlgos = txt_Excluded.Text;
                suspend = false;
                if (resettext) setTextBox();
                {
                    AlgoSelectionChangedEventArgs args = new AlgoSelectionChangedEventArgs() { Algos = textBox1.Text, Assets = cachedAssetList };
                    OnAlgoSelectionChanged(args);
                }
            }
            else if (!suspend) setButtons(false);
        }

        private bool TextCorrect(TextBox tb)
        {
            return !tb.Text.StartsWith(",") && !tb.Text.EndsWith(",") && !tb.Text.Contains(",,");
        }

        string SQLScript = "";

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string source = GetSource();
            GenerateExport(false, source: source);
        }

        private string GetSource()
        {
            string source = "";
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                InputBox ib = new InputBox("Enter source", "Source required", "string", WindowStartupLocation.CenterOwner);
                ib.ShowDialog();
                if (ib.DialogResult.HasValue)
                    source = ib.Text;
            }
            else
            {
                source = cbSource?.SelectedValue?.ToString() ?? "";
            }

            return source;
        }

        private void rbtScriptWithoutAssets_Click(object sender, RoutedEventArgs e)
        {
            GenerateExport(false, ScriptType.AlgoOnly);
        }

        private void rbtScriptForLanguage_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)rbtScriptForLanguage.IsChecked)
            {
                MessageBoxResult mbr = MessageBox.Show("Scripting for language can be a dangerous thing to do, and should be done only by experienced users. This will replace all the native primary language text on the target system with the language text selected. \n\nThis should only be done on systems which already has the primary language set to the secondary language on the source system. \n\nIf in any doubt DO NOT USE!", "Warning!!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (mbr == MessageBoxResult.Cancel)
                {
                    rbtScriptForLanguage.IsChecked = false;
                    return;
                }
            }
            if ((bool)rbtScriptForLanguage.IsChecked)
            {
                rbtScriptForLanguage.SmallImageSource = new BitmapImage(new Uri("/images/EnableLanguage32x32.png", UriKind.Relative));
                rbtScriptForLanguage.LargeImageSource = new BitmapImage(new Uri("/images/EnableLanguage32x32.png", UriKind.Relative));
            }
            else
            {
                rbtScriptForLanguage.SmallImageSource = new BitmapImage(new Uri("/images/DisableLanguage32x32.png", UriKind.Relative));
                rbtScriptForLanguage.LargeImageSource = new BitmapImage(new Uri("/images/DisableLanguage32x32.png", UriKind.Relative));
            }
            bool lang = (bool)rbtScriptForLanguage.IsChecked;
            if (!lang) txt_Language.Text = "";
            lbl_Language.Visibility = lang ? Visibility.Visible : Visibility.Collapsed;
            txt_Language.Visibility = lang ? Visibility.Visible : Visibility.Collapsed;
        }

        private void rbtScriptXml_Click(object sender, RoutedEventArgs e)
        {
            GenerateExport(false, ScriptType.Xml);
        }

        private void rbtMcKessonXml_Click(object sender, RoutedEventArgs e)
        {
            GenerateExport(ScriptType.McKesson);
        }

        private void GenerateExport(ScriptType st)
        {
            InputBox ib = new InputBox("Enter export parameters", "APH Export", "|8", System.Windows.WindowStartupLocation.CenterScreen);
            ib.txtReleaseName.Text = Settings.Default.McKessonReleaseName;
            ib.txtReleaseDescription.Text = Settings.Default.McKessonReleaseDescription;
            ib.txtReleaseUser.Text = Environment.UserName;
            ib.txtReleaseDate.Text = DateTime.Now.ToString("M/d/yyyy h:mm:ss tt");
            ib.ShowDialog();

            if (!ib.DialogResult.HasValue || !ib.DialogResult.Value) return;

            if (Settings.Default.McKessonReleaseName != ib.txtReleaseName.Text || Settings.Default.McKessonReleaseDescription != ib.txtReleaseDescription.Text)
            {
                Settings.Default.McKessonReleaseName = ib.txtReleaseName.Text;
                Settings.Default.McKessonReleaseDescription = ib.txtReleaseDescription.Text;
                Settings.Default.Save();
            }

            TextBlock tb = disableForm(false);
            XmlDocument export = new XmlDocument();
            string[] algoids = textBox1.Text.Split(',');
            Task<XmlDocument> t = new Task<XmlDocument>(() => GenerateExport(algoids, tb));
            t.ContinueWith((ge) =>
            {
                enableForm();
                SQLScript = SQLScript
                    .Replace("$$$ReleaseName$$$", ib.txtReleaseName.Text)
                    .Replace("$$$ReleaseDescription$$$", ib.txtReleaseDescription.Text)
                    .Replace("$$$ReleaseUser$$$", ib.txtReleaseUser.Text)
                    .Replace("$$$ReleaseDate$$$", ib.txtReleaseDate.Text);
                ScriptText = SQLScript;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            t.Start();
            //SQLScript = export.OuterXml.Replace("&#xD;", "\r").Replace("&#xA;", "\n");
        }

        private XmlDocument GenerateExport(string[] algoids, TextBlock tb)
        {
            XmlDocument export = new XmlDocument();
            try
            {
                KeyWordXml = DataAccess.getDataNode("ab_TableEdit", new string[] { "@TableName", "Keywords", "@xml", "<root command=\"get\"/>" });
                var updatedAlgos = DataAccess.getData("ab_updateAsset", new string[] { "@xml", "<root command=\"listnewalgos\"/>" }).Elements().Select
                    (f => new { AlgoID = f.ElementValue("AlgoID"), NodeID = f.ElementValue("NodeID") });

                export.LoadXml(@"<Release ReleaseName=""$$$ReleaseName$$$"" ReleaseVersion=""1.0.0"" ReleaseDescription=""$$$ReleaseDescription$$$"" ReleaseUser=""$$$ReleaseUser$$$"" ReleaseClient="""" ReleaseDate=""$$$ReleaseDate$$$""><ReleaseStatus ReferenceInd=""algo_release_status_ref"" ReferenceCode=""valid""/><ReleaseLanguage ReferenceInd=""language_ref"" ReferenceCode=""ENGL""/></Release>");
                XsltArgumentList xa = new XsltArgumentList();
                xa.AddExtensionObject("e24:Functions", new Functions());
                int algocount = 0;

                foreach (var algoid in algoids)
                {
                    var match = updatedAlgos.Where(f => f.AlgoID == algoid);
                    if (match.Any()) IncrementVersionInstance(match.First().AlgoID, match.First().NodeID);
                    XmlNode xn = DataAccess.getDataNode("mkp_GetAlgoNodes", new string[] { "@AlgoID", algoid, "@deepLoad", "0" }, true);
                    //IncrementVersion(xn);
                    string s = xn.Transform("McKesson/McKesson.xsl", xa);
                    //loadedAlgos.Add(algoid);
                    AddFragment(export, s);
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        tb.Text = string.Format("{0} Algos Complete", ++algocount);
                    }));
                }
                SQLScript = export.OuterXml.Replace("&#xD;", "\r").Replace("&#xA;", "\n");
            }
            catch (Exception ex)
            {
                App.WriteError(ex);
                SQLScript = ex.ToString();
            }
            return export;
        }

        private void IncrementVersions(string[] selectedAlgos)
        {
            if (!selectedAlgos.Any()) return;

            var updatedAlgos = DataAccess.getData("ab_updateAsset", "@xml", "<root command=\"listnewalgos\"/>").Elements().Select
                (f => new { AlgoID = f.ElementValue("AlgoID"), NodeID = f.ElementValue("NodeID") });

            foreach (var algoid in selectedAlgos)
            {
                var match = updatedAlgos.Where(f => f.AlgoID == algoid);
                if (match.Any()) IncrementVersionInstance(match.First().AlgoID, match.First().NodeID);
            }
        }

        private static void IncrementVersionInstance(string algoid, string nodeid)
        {
            XmlNode version = DataAccess.getDataNode("dsp_GetProperty",
                "@PropertyType", string.Format("Algo:{0}", algoid),
                "@DataID", string.Format("{0}:{1}", algoid, nodeid),
                "@PropertyName", "Version"
            );
            float value = 100;
            float tp;
            if (version["Table"] != null && version["Table"]["PropertyValue"] != null && float.TryParse(version["Table"]["PropertyValue"].InnerText, out tp))
            {
                value = tp;
                DataAccess.SetProperty(string.Format("Algo:{0}", algoid), string.Format("{0}:{1}", algoid, nodeid), "Version", Math.Round(value + 0.01f, 2).ToString("#.##"));
            }
            else
            {
                value = 100;
                version = DataAccess.getDataNode("dsp_GetProperty",
                    "@PropertyType", "Algo",
                    "@DataID", algoid,
                    "@PropertyName", "Version"
                );
                if (version["Table"] != null && version["Table"]["PropertyValue"] != null && float.TryParse(version["Table"]["PropertyValue"].InnerText, out tp))
                {
                    value = tp;
                }
                DataAccess.SetProperty("Algo", algoid, "Version", Math.Round(value + 0.01f, 2).ToString("#.##"));
            }
            DataAccess.SetProperty("AlgoVersion", algoid, "Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private static void AddFragment(XmlDocument export, string s)
        {
            XmlDocumentFragment frag = export.CreateDocumentFragment();
            frag.InnerXml = s;
            foreach (XmlNode node in frag.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.XmlDeclaration)
                    export.DocumentElement.AppendChild(node);
            }

        }

        private void GenerateExport(bool export, ScriptType st = ScriptType.Full, string source = "")
        {
            if (containsOldAlgos)
            {
                MessageBox.Show("The algo selection includes invalid algos that cannot be scripted correctly.\n\nPlease adjust the selection and try again.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                disableForm(false);
                if ((bool)rbtScriptAssets.IsChecked)
                {
                    if (export)
                    {
                        string result = AreYouSure($"{lstAssets.Items.Count} Asset(s)");
                        if (result == "Stop, I don't want to") return;
                    }
                    Data.Data ds = DataAccess.GetDataService();
                    Dictionary<string, List<string>> fullAssetList;
                    XmlDocument doc = getAssetXml(out fullAssetList);
                    DataAccess.LastCommand = $"{(export ? $"Export Target = '{cbTarget.Text}'," : "Script")} Source = '{source}', Language = '{txt_Language.Text}', Data = '{doc.OuterXml}' ";
                    string s;
                    if (export)
                        s = ds.exportAlgo(txt_Language.Text == "" ? source : (source + ":" + txt_Language.Text), cbTarget.Text, doc.OuterXml);
                    else
                        s = ds.generateSQL(doc.OuterXml, true, txt_Language.Text == "" ? source : (source + ":" + txt_Language.Text));
                    SQLScript = "--\n";
                    SQLScript += string.Format("-- {0}\n", Program.AssetBuilderTitle);
                    SQLScript += "--\n";
                    foreach (var item in fullAssetList)
                    {
                        SQLScript += string.Format("-- {1,-11} : {0}\n", string.Join(", ", item.Value), item.Key + (item.Value.Count > 1 ? "s" : " "));
                    }
                    if (fullAssetList.ContainsKey("Conclusion"))
                    {
                        SQLScript += string.Format("-- This script will also contain any Bullets for the above Conclusion{1}\n", string.Join(", ", fullAssetList["Conclusion"]), fullAssetList["Conclusion"].Count > 1 ? "s" : "");
                    }
                    SQLScript += string.Format("-- Language    : {0}\n", txt_Language.Text == "" ? "Default" : txt_Language.Text);
                    SQLScript += string.Format("-- Machine     : {0}\n", Environment.MachineName);
                    SQLScript += string.Format("-- User        : {0}\n", Environment.UserName);
                    SQLScript += string.Format("-- Date        : {0}\n", DateTime.Now.ToString("MMMM dd, yyyy"));
                    SQLScript += string.Format("-- Time        : {0}\n", DateTime.Now.ToString("HH:mm:ss"));
                    if (source != "") SQLScript += string.Format("-- Source      : {0}\n", source);
                    AddVersion();
                    SQLScript += "Set NoCount On\n";
                    SQLScript += "--\n";
                    SQLScript += s;
                }
                else
                {
                    if (export)
                    {
                        var result = AreYouSure($"{dataGrid1.SelectedItems.Count} Algo(s)");
                        if (result == null || result.Equals("Stop, I don't want to", StringComparison.InvariantCultureIgnoreCase)) return;
                    }

                    if (Window1.AllowProperties && source == "")
                    {
                        var algoIds = textBox1.Text.Split(',');
                        if (Window1.AllowExportReport)
                        {
                            var result = SaveExportData();
                            if (!result) return;
                        }

                        IncrementVersions(algoIds);
                    }

                    Data.Data ds = DataAccess.GetDataService();
                    DataAccess.LastCommand = $"{(export ? $"Export Target = '{cbTarget.Text}'," : "Script")} Source = '{source}', Language = '{txt_Language.Text}', Data = '({textBox1.Text})' ";
                    string s;
                    if (export)
                        s = ds.exportAlgo(txt_Language.Text == "" ? source : (source + ":" + txt_Language.Text), cbTarget.Text, "(" + textBox1.Text + ")");
                    else
                        s = ds.generateSQL("(" + textBox1.Text + ")", true, txt_Language.Text == source ? source : (source + ":" + txt_Language.Text));
                    SQLScript = "--\n";
                    SQLScript += string.Format("-- {0}\n", Program.AssetBuilderTitle);
                    SQLScript += "--\n";
                    SQLScript += string.Format("-- Algorithm{1}  : {0}\n", algoList, algosIncluded > 1 ? "s" : " ");
                    SQLScript += string.Format("-- Language    : {0}\n", txt_Language.Text == "" ? "Default" : txt_Language.Text);
                    SQLScript += string.Format("-- Machine     : {0}\n", Environment.MachineName);
                    SQLScript += string.Format("-- User        : {0}\n", Environment.UserName);
                    SQLScript += string.Format("-- Date        : {0}\n", DateTime.Now.ToString("MMMM dd, yyyy"));
                    SQLScript += string.Format("-- Time        : {0}\n", DateTime.Now.ToString("HH:mm:ss"));
                    if (source != "") SQLScript += string.Format("-- Source      : {0}\n", source);
                    AddVersion();
                    SQLScript += "Set NoCount On\n";
                    SQLScript += "--\n";
                    if (st == ScriptType.AlgoOnly) s = RemoveAssetsFromScript(s);
                    else if (st == ScriptType.Xml)
                    {
                        SQLScript = "";
                        s = ExtractXml(s);
                        XDocument doc = XDocument.Parse(s);
                        s = doc.ToString().Replace("\r", "");
                    }
                    SQLScript += s;
                }
                if (export) SQLScript += "\n\nCompleted Successfully.";
            }
            catch (Exception ex)
            {
                App.WriteError(ex);
                SQLScript = ex.ToString();
            }
            finally
            {
                enableForm();
                ScriptText = SQLScript.Replace("\n", "\r\n");
            }
        }

        private void AddVersion()
        {
            if (Properties.CustomProperties.ContainsKey("VersionConclusion"))
            {
                var id = Properties.CustomProperties["VersionConclusion"];
                var asset = DataAccess.getData("ab_GetAsset", new[] { "@AssetTypeID", "4", "@AssetID", id });
                if (asset.Element("Table") != null && asset.Element("Table").Element("Possible_Condition") != null)
                {
                    var text = asset.Element("Table").Element("Possible_Condition").Value;
                    SQLScript += string.Format("-- Version     : {0}\n", text);
                }
            }
        }

        private bool SaveExportData()
        {
            var exportData = GetExportData();
            if (exportData == null || !exportData.IsValid) return false;

            exportData.ExportedAlgos = textBox1.Text;
            exportData.SourceEnvironment = cbSource.Text;
            exportData.TargetEnvironment = cbTarget.Text;
            var success = RecordExport(exportData);
            if (success) return true;

            ErrorDialog(ExportError);
            return false;
        }

        private string AreYouSure(string content)
        {
            var message = $"You are about to rollout the following content{(cbSource.Text != "" ? $" from {cbSource.Text} " : "")} to {cbTarget.Text}\r\n\r\n{content}\r\n\r\nAre you sure you want to do this\r\nThis cannot be reversed.";
            var title = "Are you sure?";
            string result = Diva.Controls.Simple.CustomMessageBox.Show(message, title, new[] { "Stop, I don't want to", "I'm sure" }, "Stop, I don't want to", "Stop, I don't want to");
            return result;
        }

        private string ErrorDialog(string content)
        {
            const string title = "Error";
            return Diva.Controls.Simple.CustomMessageBox.Show(content, title, new[] {"OK"});
        }

        private string RemoveAssetsFromScript(string s)
        {
            s = s.Substring(s.LastIndexOf("Print 'Applying changes to table algoxml'", StringComparison.Ordinal));
            if (s.IndexOf("-- BREAK", StringComparison.Ordinal) > -1)
                s = s.Substring(0, s.IndexOf("-- BREAK", StringComparison.Ordinal));
            return s;
        }

        public static string ExtractXml(string s)
        {
            s = s.Substring(s.IndexOf("<root"));
            s = s.Substring(0, s.IndexOf("</root>") + 7);
            return s;
        }

        public XmlDocument getAssetXml(out Dictionary<string, List<string>> fullAssetList)
        {
            Dictionary<string, string> assetlist = new Dictionary<string, string>(){
                                {"ALGO_START", ""},
                                {"QUESTION", ""},
                                {"ANSWER", ""},
                                {"RECOMMENDATION", ""},
                                {"BULLET", ""},
                                {"MAP", ""},
                };
            fullAssetList = new Dictionary<string, List<string>>();
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("root"));
            foreach (var item in lstAssets.Items)
            {
                LoadedAsset la = item as LoadedAsset;
                doc.DocumentElement.AppendChild(doc.CreateElement(la.Type)).Attributes.Append(doc.CreateAttribute("id")).InnerText = la.AssetID.ToString();
                if (assetlist[la.Type] != "") assetlist[la.Type] += ", ";
                assetlist[la.Type] += string.Format("{0} ({1})", la.Text, la.AssetID);
                if (tableTypes.ContainsKey(la.Type))
                {
                    if (!fullAssetList.ContainsKey(tableTypes[la.Type])) fullAssetList.Add(tableTypes[la.Type], new List<string>());
                    fullAssetList[tableTypes[la.Type]].Add(string.Format("{0} ({1})", la.Text, la.AssetID));
                }
            }
            //fullAssetList = "";
            //conclusions = "";
            //foreach (var item in assetlist)
            //{
            //    if (fullAssetList != "" && item.Value != "") fullAssetList += ", ";
            //    if (item.Value != "")
            //    {
            //        string list = string.Format("{0}{1} ({2})", tableTypes[item.Key], item.Value.Contains(',') ? "s" : "", item.Value);
            //        if (item.Key == "RECOMMENDATION") conclusions = list;
            //        fullAssetList += list;
            //    }
            //}
            return doc;
        }

        Dictionary<string, string> assetlistPrefix = new Dictionary<string, string>(){
                                {"ALGO_START", "S"},
                                {"QUESTION", "Q"},
                                {"ANSWER", "A"},
                                {"RECOMMENDATION", "C"},
                                {"BULLET", "B"}
                };

        string cachedAssetList = "";
        private string getAssets()
        {
            string assetlist = "";
            foreach (var item in lstAssets.Items)
            {
                LoadedAsset la = item as LoadedAsset;
                if (assetlistPrefix.ContainsKey(la.Type))
                {
                    if (assetlist != "") assetlist += ",";
                    assetlist += assetlistPrefix[la.Type] + la.AssetID;
                }
            }
            cachedAssetList = assetlist;
            return assetlist;
        }

        public void SaveScript()
        {
            if (txtScript.Visibility == Visibility.Visible)
            {
                using (var wc = new WebClient())
                {
                    var data = Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(ScriptHtml));
                    var host = new Uri(Settings.Default.WebService).Host;
                    var url = wc.UploadString($"https://apps.expert-24.com/Comment/Comment/CreatePage/{PageID}/{host}/{Environment.UserName}", data);
                    var res = DataAccess.JsonDeSerialize(url);
                    if (res.ContainsKey("Location")) Process.Start(res["Location"] as string);
                    else SaveScript(ScriptText);
                }
            }
            else SaveScript(SQLScript);
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            SaveScript();
        }

        public static void SaveScript(string SQLScript)
        {
            bool ansi = SQLScript.StartsWith("<Release ");
            bool xml = SQLScript.StartsWith("<");
            bool json = SQLScript.IndexOfAny(new char[] { '[', '{' } ) == 0;
            Encoding e = ansi ? Encoding.GetEncoding(1252) /* 28591 */ : Encoding.GetEncoding(1200);
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.RestoreDirectory = true;
            sfd.Filter = xml ? "Xml Files (*.xml)|*.xml|SQL files (*.sql)|*.sql" : "SQL files (*.sql)|*.sql|Xml Files (*.xml)|*.xml";
            if (json) sfd.Filter = "Json Files (*.json)|*.json";
            sfd.OverwritePrompt = true;
            //sfd.AutoUpgradeEnabled = true;
            sfd.AddExtension = true;
            sfd.DefaultExt = "sql";
            bool? dr = sfd.ShowDialog();
            if (dr == true)
            {
                File.WriteAllText(sfd.FileName, SQLScript.Replace("\r", "").Replace("\n", "\r\n"), e);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            suspend = true;
            setTextBox();
            suspend = false;
            XmlNode xn = DataAccess.getDataNode("usp_getalgos", AddSource(new string[] {
                "@algo_list", "(" + textBox1.Text + ")"
            }).ToArray(), false);

            string list = "";
            foreach (XmlNode item in xn.SelectNodes("Table/algoid"))
            {
                if (list != "") list += ",";
                list += item.InnerText;
            }
            textBox1.Text = string.Format("{0}", list);
            dataGrid1.Focus();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            RefreshData("<root command=\"algos\" />");
        }

        private void ClockAssets_Click(object sender, RoutedEventArgs e)
        {
            InputBox ib = new InputBox("Please enter a date", "Script assets changed from date", "DateTime", System.Windows.WindowStartupLocation.CenterScreen, InputBoxValidate.Date);
            ib.ShowDialog();
            if (ib.DialogResult.HasValue && ib.DialogResult.Value)
            {
                var dt = DateTime.Parse(ib.Text);
                var xn = DataAccess.getData("ab_updateasset",
                    AddSource(new[] { "@xml", $"<root command='listnewassets' date='{dt.ToString("yyyy-MM-dd HH:mm:ss")}' />" }).ToArray());
                foreach (var newassets in xn.XPathSelectElements("*/NewAssets"))
                {
                    AddAssets(newassets.Value);
                }
                setButtons();
            }
        }

        private void ScriptAssets_Click(object sender, RoutedEventArgs e)
        {
            Duration dur = new Duration(TimeSpan.FromSeconds(0.5));
            if ((bool)rbtScriptAssets.IsChecked)
            {
                GridLengthAnimation ga = new GridLengthAnimation(0, 500, dur, GridUnitType.Pixel);
                GridLengthAnimation gb = new GridLengthAnimation(500, 0, dur, GridUnitType.Pixel);
                DoubleAnimation da = new DoubleAnimation(0, 0.25, dur);
                AlgoColumn.BeginAnimation(ColumnDefinition.WidthProperty, gb);
                AssetColumn.BeginAnimation(ColumnDefinition.WidthProperty, ga);
                imgDropAssets.BeginAnimation(OpacityProperty, da);
                lstAssets.AllowDrop = true;
                textBox1.IsEnabled = false;
                rbtClockAssets.IsEnabled = true;
            }
            else
            {
                GridLengthAnimation ga = new GridLengthAnimation(500, 0, dur, GridUnitType.Pixel);
                GridLengthAnimation gb = new GridLengthAnimation(0, 500, new Duration(TimeSpan.FromSeconds(0.5)), GridUnitType.Pixel);
                DoubleAnimation da = new DoubleAnimation(0.25, 0, dur);
                AlgoColumn.BeginAnimation(ColumnDefinition.WidthProperty, gb);
                AssetColumn.BeginAnimation(ColumnDefinition.WidthProperty, ga);
                imgDropAssets.BeginAnimation(OpacityProperty, da);
                lstAssets.AllowDrop = false;
                textBox1.IsEnabled = true;
                rbtClockAssets.IsEnabled = false;
            }

            setButtons();
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (algosIncluded == 0)
            {
                MessageBox.Show("Nothing to do", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            XDocument doc = new XDocument(new XElement("root", new XAttribute("command", "algos")));
            XElement root = doc.Element("root");
            string message = "This will permanently delete the following algos\n\n";
            foreach (var item in dataGrid1.SelectedItems)
            {
                root.Add(new XElement("Delete", new XAttribute("id", (item as LoadedAlgo).AlgoID)));
                message += (item as LoadedAlgo).Title + "\n";
            }
            message += "\nAre you sure you want to continue?";
            MessageBoxResult mbr = MessageBox.Show(message, "Please confirm algorithm deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbr == MessageBoxResult.Yes)
                RefreshData(doc.ToString());
        }

        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsAllowed(e.Text);
        }

        private static bool IsAllowed(string text)
        {
            foreach (char item in text)
            {
                if (item != ',' && (item < 48 || item > 57)) return false;
            }
            return true;
        }

        private void textBox1_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                if (!IsAllowed((string)e.DataObject.GetData(typeof(string)))) e.CancelCommand();
            }
            else e.CancelCommand();
        }

        private void chkShowSelectedOnly_Checked(object sender, RoutedEventArgs e)
        {
            if (refreshing) return;
            refreshing = true;
            if (chkShowSelectedOnly.IsChecked == true) chkFilterByType.IsChecked = false;
            RefreshData();
            refreshing = false;
            return;
            //if ((bool)chkShowSelectedOnly.IsChecked)
            //    Resources["UnselectedAlgoRow"] = Visibility.Collapsed;
            //else
            //    Resources["UnselectedAlgoRow"] = Visibility.Visible;
        }

        List<string> nonExcludingReports = new List<string>{
            "SummaryTitle",
            "MostPopularQuestionAnswer",
            "MostPopularConclusion",
        };

        private void rbtComparisonReport_Click(object sender, RoutedEventArgs e)
        {
            var tb = (sender as RibbonToggleButton);
            if (tb.IsChecked == true)
            {
                Reports.Compare c = new Reports.Compare(tb, this);
                //disableForm(false);
                DockPanel.SetDock(c, Dock.Top);
                c.Height = 180;
                FullPanel.Children.Add(c);
                //c.SetValue(DockPanel.DockProperty, Dock.Top);
                updateCompare();
            }
            else
            {
                FullPanel.Children.Clear();
            }
        }

        private void ShowReleaseStatus_Click(object sender, RoutedEventArgs e)
        {
            var algos = textBox1.Text;
            if (string.IsNullOrEmpty(algos))
            {
                MessageBox.Show("Please select the algos required for the report or type the Algo Id in the text box.", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            disableForm(false);
            if (_algoStatusViewModel == null)
            {
                _algoStatusViewModel = new AlgoReleaseStatusCollectionViewModel();
            }

            _algoStatusViewModel.Populate(algos);
            AlgoStatusList.DataContext = _algoStatusViewModel;

            AlgoStatusList.Visibility = Visibility.Visible;
            enableForm();
        }

        private void ContentReport_Click(object sender, RoutedEventArgs e)
        {
            string algos = textBox1.Text;
            if (string.IsNullOrEmpty(algos))
            {
                MessageBox.Show("Please select the algos required for the report or type the Algo Id in the text box.", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            disableForm(false);
            string command = "";
            if (sender is RibbonMenuItem) command = (sender as RibbonMenuItem).CommandParameter.ToString();
            Reports.Content c = new Reports.Content();
            if (command == "QuestionReport" || command == "ContentReport") c.QuestionAnswerXml = DataAccess.getData("ab_Report", "@ReportType", "QuestionAnswer", "@Algos", algos);
            if (command == "ConclusionReport" || command == "ContentReport") c.ConclusionXml = DataAccess.getData("ab_Report", "@ReportType", "Conclusion", "@Algos", algos);
            var rep = Reports.ContentReport<Content>.CreateReport($"Reports\\ContentReport");
            rep.Completed += delegate (object obj, Reports.CompletedEventArgs ea)
            {
                if (ea.UniqueID == null) ScriptText = ea.Content;
                else ScriptHtml = ea.Content;
                PageID = ea.UniqueID;
                //output.NavigateToString(content);
                enableForm();
            };
            rep.RunReport(c, "@Layout@");
        }

        private void rbtReport_Click(object sender, RoutedEventArgs e)
        {
            disableForm(false);
            string text = textBox1.Text;
            Reports.Report.AlgoLoader = this;
            string command = "";
            if (sender is RibbonButton) command = (sender as RibbonButton).CommandParameter.ToString();
            else if (sender is RibbonMenuItem) command = (sender as RibbonMenuItem).CommandParameter.ToString();
            if (command.StartsWith("AssetReport|") && Window1.ShowTranslation) command += "|merge:";
            if ((bool)rbtScriptAssets.IsChecked) command = command.Replace("algos", "assets");
            List<string> prms = new List<string>(command.Split('|'));
            if (prms.Count > 0 && nonExcludingReports.Contains(prms[0]) && !string.IsNullOrWhiteSpace(excludedAlgos))
            {
                MessageBoxResult mbr = MessageBox.Show("This report is not compatible with excluded algos.\n\nThe excluded algos will be ignored.\n\nDo you want to continue anyway?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (mbr == MessageBoxResult.No)
                {
                    enableForm();
                    return;
                }
            }
            Reports.Report.RunReport(text, prms, excludedAlgos, excludedAlgoList, (sender as Control)?.Tag);
        }

        public static Action EmptyDelegate = delegate () { };

        public TextBlock disableForm(bool whirly)
        {
            txtScript.Visibility = Visibility.Hidden;
            TextBlock tb = null;
            int dim = 64;
            greyCanvas.Children.Clear();
            greyCanvas.Visibility = Visibility.Visible;
            double t = (greyCanvas.ActualHeight / 2d) - (dim / 2d);
            double l = (greyCanvas.ActualWidth / 2d) - (dim / 2d);
            if (whirly)
            {
                Waiting w = new Waiting { Width = dim, Height = dim };
                w.SetValue(Canvas.TopProperty, t);
                w.SetValue(Canvas.LeftProperty, l);
                greyCanvas.Children.Add(w);
            }
            else
            {
                TextBlock w = new TextBlock { FontSize = 24, Text = "Please wait...", VerticalAlignment = System.Windows.VerticalAlignment.Center, HorizontalAlignment = System.Windows.HorizontalAlignment.Center };
                w.SetValue(Canvas.TopProperty, t);
                w.SetValue(Canvas.LeftProperty, l);
                greyCanvas.Children.Add(w);
                tb = w;
            }
            rbnApplication.IsEnabled = false;
            gridBlur.Radius = 5d;
            greyCanvas.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            return tb;
        }

        public void enableForm()
        {
            greyCanvas.Visibility = Visibility.Hidden;
            greyCanvas.Children.Clear();
            rbnApplication.IsEnabled = true;
            gridBlur.Radius = 0d;
            controlGrid.Children.Clear();
        }

        //bool sorting = false;
        private void dataGrid1_Sorting(object sender, DataGridSortingEventArgs e)
        {
            //if ((bool)chkShowSelectedOnly.IsChecked)
            //{
            //    Resources["UnselectedAlgoRow"] = Visibility.Visible;
            //    sorting = true;
            //}
        }

        void dataGrid1_LayoutUpdated(object sender, EventArgs e)
        {
            //if ((bool)chkShowSelectedOnly.IsChecked)
            //{
            //    if (sorting) Resources["UnselectedAlgoRow"] = Visibility.Collapsed;
            //    sorting = false;
            //}
        }

        Dictionary<string, string> assetTypes = new Dictionary<string, string>() {
                    {"ALGOID", "ALGO_START"},
                    {"QUESTIONID", "QUESTION"},
                    {"ANSID", "ANSWER"},
                    {"RECID", "RECOMMENDATION"},
                    {"BPID", "BULLET"},
                    {"MAPID", "MAP"}
        };
        Dictionary<string, string> tableTypes = new Dictionary<string, string>(){
                    {"ALGO_START", "Algo"},
                    {"QUESTION", "Question"},
                    {"ANSWER", "Answer"},
                    {"RECOMMENDATION", "Conclusion"},
                    {"BULLET", "Bullet"},
                    {"MAP", "Map"},
        };

        private void DropAssets(object sender, DragEventArgs e)
        {
            if (!e.Data.GetFormats().Contains("Text")) return;
            string text = (string)e.Data.GetData("Text");
            if (AddAssets(text))
                setButtons();
        }

        private bool AddAssets(string text)
        {
            string type = text.Substring(0, text.IndexOf(' '));
            if (!assetTypes.ContainsKey(type)) return false;
            type = assetTypes[type];
            int x = text.IndexOf('(') + 1;
            int y = text.IndexOf(')');
            int nx = text.IndexOf('(', y) + 1;
            int ny = text.LastIndexOf(')');
            string[] s = text.Substring(x, y - x).Split(',');
            string[] n = text.Substring(nx, ny - nx).Replace("$$BREAK$$", ((char)7).ToString()).Split((char)7);

            for (int i = 0; i < s.Length; i++)
            {
                lstAssets.Items.Add(new LoadedAsset { AssetID = int.Parse(s[i]), Type = type, Text = n[i] });
            }
            updateCompare();
            return true;
        }

        private void updateCompare()
        {
            if (FullPanel.Children.Count > 0)
            {
                AlgoSelectionChangedEventArgs args = new AlgoSelectionChangedEventArgs() { Algos = textBox1.Text, Assets = getAssets() };
                OnAlgoSelectionChanged(args);
            }
        }

        private void lstAssets_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                for (int i = 0; i < lstAssets.SelectedItems.Count; i++)
                {
                    lstAssets.Items.Remove(lstAssets.SelectedItems[i--]);
                }

                setButtons();
                updateCompare();
            }
        }

        private void txtScript_PreviewDrop(object sender, DragEventArgs e)
        {
            if ((bool)rbtScriptAssets.IsChecked) e.Handled = true;
        }

        private void rbtCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(ScriptText);
        }

        private void Reports_Selected(object sender, RoutedEventArgs e)
        {
            rbtConclusionReport.IsEnabled = !(bool)rbtScriptAssets.IsChecked;
            //rbtConclusionSummaryReport.IsEnabled = !(bool)rbtScriptAssets.IsChecked;
            rbtQuestionAnswerReport.IsEnabled = !(bool)rbtScriptAssets.IsChecked;
            rbtSummaryTitleReport.IsEnabled = !(bool)rbtScriptAssets.IsChecked;
            if (rbtScriptAssets.IsChecked == true)
            {
                //rbtScriptAssets.IsChecked = false;
                //ScriptAssets_Click(null, null);
            }
        }

        public static RoutedUICommand cmdAddSource = new RoutedUICommand("cmdAddSource", "cmdAddSource", typeof(AlgoLoader));
        public static RoutedUICommand cmdDeleteSource = new RoutedUICommand("cmdDeleteSource", "cmdDeleteSource", typeof(AlgoLoader));
        public static RoutedUICommand cmdAddTarget = new RoutedUICommand("cmdAddTarget", "cmdAddTarget", typeof(AlgoLoader));
        public static RoutedUICommand cmdDeleteTarget = new RoutedUICommand("cmdDeleteTarget", "cmdDeleteTarget", typeof(AlgoLoader));
        public static RoutedUICommand cmdExport = new RoutedUICommand("cmdExport", "cmdExport", typeof(AlgoLoader));

        string currentSource = null;

        private void rgSource_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (cbSource?.SelectedValue?.ToString() != currentSource)
            {
                currentSource = cbSource?.SelectedValue?.ToString() ?? "";
                //cbSource.SelectedValue = currentSource;
                RefreshData("<root command=\"algos\" />");
            }
        }

        private void rgSource_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cbSource?.Text != "" && !cbSource.Items.Contains(cbSource.Text)) AddSource_Executed(null, null);
            //else if (cbSource?.Text == "" && cbSource.SelectedItem != null)
            //{
            //    cbSource.SelectedItem = null;
            //    cbSource.Text = "";
            //    rgSource_SelectionChanged(null, null);
            //}
        }

        private void AddSource_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (cbSource.Text != "" && !cbSource.Items.Contains(cbSource.Text)) e.CanExecute = true;
        }

        private void AddSource_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //RibbonGalleryItem item = new RibbonGalleryItem { Content = rtbSource.Text };
            var item = cbSource?.Text;
            if (string.IsNullOrWhiteSpace(item)) return;
            cbSource.Items.Add(item);
            cbSource.SelectedItem = item;
            Window1.addSettingsValue("AvailableSources", item);
        }

        private void DeleteSource_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (cbSource.Text != "" && cbSource.Text != null) e.CanExecute = true;
        }

        private void DeleteSource_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            cbSource.Items.Remove(cbSource.Text);
            cbSource.Text = "";
            Window1.updateSettingsValue("AvailableSources", cbSource.Items);
        }

        private void rgTarget_SelectionChanged(object sender, RoutedEventArgs e)
        {
        }

        private void rgTarget_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cbTarget?.Text != "" && !cbTarget.Items.Contains(cbTarget.Text)) AddTarget_Executed(null, null);
        }

        private void AddTarget_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (cbTarget.Text != "" && !cbTarget.Items.Contains(cbTarget.Text)) e.CanExecute = true;
        }

        private void AddTarget_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = cbTarget?.Text;
            if (string.IsNullOrWhiteSpace(item)) return;
            cbTarget.Items.Add(item);
            cbTarget.SelectedItem = item;
            Window1.addSettingsValue("AvailableTargets", cbTarget.Text);
        }

        private void DeleteTarget_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (cbTarget.Text != "" && cbTarget.Text != null) e.CanExecute = true;
        }

        private void DeleteTarget_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            cbTarget.Items.Remove(cbTarget.Text);
            cbTarget.Text = "";
            Window1.updateSettingsValue("AvailableTargets", cbTarget.Items);
        }

        private void Export_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (dataGrid1 == null) return;
            bool assetMode = (bool)rbtScriptAssets.IsChecked;
            bool value = algosIncluded > 0 && algosExcluded == 0;
            if (!Window1.McKesson_Mode && (cbTarget?.SelectedValue?.ToString() ?? "") != "" && ((!assetMode && value) || (assetMode && lstAssets.Items.Count > 0))) e.CanExecute = true;
        }

        private void Export_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string source = GetSource();
            GenerateExport(true, source: source);
        }

        Dictionary<string, List<RibbonButton>> staticButtons = new Dictionary<string, List<RibbonButton>>();

        private void RibbonButton_Loaded(object sender, RoutedEventArgs e)
        {
            RibbonButton sb = sender as RibbonButton;
            if (!staticButtons.ContainsKey(sb.Name)) staticButtons.Add(sb.Name, new List<RibbonButton>());
            if (!staticButtons[sb.Name].Contains(sb)) staticButtons[sb.Name].Add(sb);
        }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IInputElement o = dataGrid1.InputHitTest(e.GetPosition(dataGrid1));
            if (o != null)
            {
                DataGridRow dr = AssetBuilder.Classes.ControlTree.getParent<DataGridRow>(dataGrid1.InputHitTest(e.GetPosition(dataGrid1)) as DependencyObject);
                if (dr != null && dr.Item != null && dr.Item is LoadedAlgo)
                {
                    LoadedAlgo la = dr.Item as LoadedAlgo;
                    LoadAlgoFromFile(la.MachineName, la.FileName);
                }
            }
        }

        public static void LoadAlgoFromFile(string machinename, string filename)
        {
            if (filename.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) ||
                filename.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            {
                System.Diagnostics.Process.Start(filename);
                return;
            }
            if (filename[1] == ':' && !string.IsNullOrEmpty(machinename) && Environment.MachineName != machinename) filename = string.Format(@"\\{0}\{1}${2}", machinename, filename[0], filename.Substring(2));
            if (filename == "") MessageBox.Show("No filename associated with this algo.", "Connot complete action.", MessageBoxButton.OK, MessageBoxImage.Hand);
            else if (!File.Exists(filename)) MessageBox.Show("File not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Stop);
            else System.Diagnostics.Process.Start(filename);
        }

        bool RightButtonPressedLast = false;

        private void dataGrid1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IInputElement o = dataGrid1.InputHitTest(e.GetPosition(dataGrid1));
            if (o != null)
            {
                DataGridRow dr = AssetBuilder.Classes.ControlTree.getParent<DataGridRow>(dataGrid1.InputHitTest(e.GetPosition(dataGrid1)) as DependencyObject);
                if (dr != null && dr.Item != null && dr.Item is LoadedAlgo)
                {
                    if (dr.IsSelected && e.ChangedButton == MouseButton.Right)
                    {
                        LoadedAlgo la = dr.Item as LoadedAlgo;
                        la.Exclude = true;
                        dataGrid1_SelectedCellsChanged(dataGrid1, new SelectedCellsChangedEventArgs(new List<DataGridCellInfo>(), new List<DataGridCellInfo>()));
                        return;
                    }
                }
            }
            RightButtonPressedLast = e.ChangedButton == MouseButton.Right;
        }

        private void rbtCheckFileLocations_Click(object sender, RoutedEventArgs e)
        {
            var algos = dataGrid1.SelectedItems.OfType<LoadedAlgo>().Select(f => new
            {
                AlgoID = f.AlgoID,
                Title = f.Title,
                FileName = f.FileName,
                Promoted = f.Promoted,
                MachineName = f.MachineName,
                UserName = f.UserName,
                LoadID = f.LoadID,
            });

            Window1.displaygridwindow(algos, "File Locations");
            //string files = "";
            //foreach (var item in algos)
            //{
            //    files += string.Format("{0}\t{1}\t{2}\t{3}\r\n", item.AlgoID, item.Title, item.FileName, item.Promoted);
            //}
            //txtScript.Text = files;
        }

        private void CollectionViewSource_Filter(object sender, System.Windows.Data.FilterEventArgs e)
        {
            LoadedAlgo t = e.Item as LoadedAlgo;
            if (t != null)
            // If filter is turned on, filter completed items.
            {
                if (chkFilterByType.IsChecked == true && cmbAssessment.SelectedValue != null)
                    e.Accepted = t.AssessmentType == (cmbAssessment.SelectedValue as ListItem).ID;
                else if (chkShowSelectedOnly.IsChecked == true)
                    e.Accepted = Algos.Contains(t.AlgoID);
                else
                    e.Accepted = true;
            }
        }

        private void chkFilterByType_Checked(object sender, RoutedEventArgs e)
        {
            if (refreshing) return;
            refreshing = true;
            if (chkFilterByType.IsChecked == true) chkShowSelectedOnly.IsChecked = false;
            RefreshData();
            refreshing = false;
        }

        bool refreshing = false;

        private void RefreshData()
        {
            var extemp = txt_Excluded.Text;
            var temp = textBox1.Text;
            CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource).Refresh();
            txt_Excluded.Text = extemp;
            textBox1.Text = temp;
        }

        private string ExtractData(string url)
        {
            if (url.Contains("data.asmx/getData"))
            {
                var doc = XElement.Parse(Extension.GetWebRequest(url));
                if (doc.Name == "Error") return DataAccess.JsonSerialize(new { Error = doc.Value });
                Dictionary<string, object> dict = new Dictionary<string, object>();
                if (url.Contains("procedure=gettextassetlocation"))
                    dict.Add("Location", doc.Element("Table").Value);
                else
                    dict.Add("12", doc.Elements("Table").Where(f => f.Element("BoxID").Value == "5" && f.Element("Description") != null).Select(f => f.Element("Description").Value).ToArray());
                return DataAccess.JsonSerialize(dict);
            }
            else return Extension.GetWebRequest(url);
        }

        private async void rbtListTranslations_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var command = (sender as RibbonButton).CommandParameter.ToString();
                if (string.IsNullOrWhiteSpace(taLanguage.Text)) return;
                disableForm(false);
                string left = null;
                string right = null;
                var language = taLanguage.Text;
                List<Task<string>> reportResults = new List<Task<string>>();
                KeyValuePair<string, string> source = new KeyValuePair<string, string>();
                KeyValuePair<string, string> target = new KeyValuePair<string, string>();
                if (taSource.SelectedItem is KeyValuePair<string, string>)
                {
                    source = (KeyValuePair<string, string>)taSource.SelectedItem;
                    if (language == "TextAssets")
                    {
                        var node = JNode.CreateFromJson(ExtractData(new Uri(new Uri(source.Value), "data.asmx/getData?procedure=gettextassetlocation&args=").AbsoluteUri));
                        if (new Uri(node["Location"]).Host != new Uri(source.Value).Host) throw new Exception($"Source Host doesn't match. Expected {new Uri(source.Value).Host} got {new Uri(node["Location"]).Host}");
                        right = new Uri(new Uri(source.Value), "data.asmx/getData?procedure=ab_getitems&args=@boxid&args=0&args=@assettypeid&args=12&args=@searchword&args=%%").AbsoluteUri;
                    }
                    else
                        right = new Uri(new Uri(source.Value), "TraversalService/ListTranslation/" + language).AbsoluteUri;
                }
                if (taTarget.SelectedItem is KeyValuePair<string, string>)
                {
                    target = (KeyValuePair<string, string>)taTarget.SelectedItem;
                    if (language == "TextAssets")
                    {
                        var node = JNode.CreateFromJson(ExtractData(new Uri(new Uri(target.Value), "data.asmx/getData?procedure=gettextassetlocation&args=").AbsoluteUri));
                        if (new Uri(node["Location"]).Host != new Uri(target.Value).Host) throw new Exception($"Target Host doesn't match. Expected {new Uri(target.Value).Host} got {new Uri(node["Location"]).Host}");
                        left = new Uri(new Uri(target.Value), "data.asmx/getData?procedure=ab_getitems&args=@boxid&args=0&args=@assettypeid&args=12&args=@searchword&args=%%").AbsoluteUri;
                    }
                    else
                        left = new Uri(new Uri(target.Value), "TraversalService/ListTranslation/" + language).AbsoluteUri;
                }

                if (!string.IsNullOrWhiteSpace(right)) reportResults.Add(Task.Run(() => ExtractData(right)));
                if (!string.IsNullOrWhiteSpace(left)) reportResults.Add(Task.Run(() => ExtractData(left)));

                await Task.WhenAll(reportResults.ToArray());
                var prms = new Dictionary<string, string>
                {
                    { "TL", language },
                    { "T", left },
                    { "SL", language },
                    { "S", right },
                    { "Title", "Language Comparison" },
                    { "Subtitle", "" },
                };

                if (reportResults.Count == 2 && command.In("Compare", "Execute"))
                {
                    var sourceItems = JNode.CreateFromJson(reportResults[0].Result);
                    var targetItems = JNode.CreateFromJson(reportResults[1].Result);
                    var sc = new Uri(new Uri(source.Value), "data.asmx/" + (language == "TextAssets" ? "getData" : "getLanguage"));
                    var tc = new Uri(new Uri(target.Value), "data.asmx/" + (language == "TextAssets" ? "getData" : "getLanguage"));
                    var ue = language == "TextAssets" ? "{0}?procedure=ab_GetAsset&args=@AssetTypeID&args=12&args=@AssetID&args={2}" : "{0}?AssetType={1}&AssetID={2}&Language={3}";
                    var te = new Uri(new Uri(target.Value), "data.asmx");
                    var sourceTasks = new Dictionary<string, Task<string>>();
                    var targetTasks = new Dictionary<string, Task<string>>();
                    prms["T"] = tc.Host;
                    prms["S"] = sc.Host;

                    foreach (var item in sourceItems)
                    {
                        foreach (var value in item)
                        {
                            var t = string.Format(ue, sc.AbsoluteUri, item.Key, value.Value.Replace("&", "%26"), language).GetContent();
                            sourceTasks.Add($"{item.Key}:{value.Value}", t);
                        }
                    }
                    var sk = sourceItems.Values.SelectMany(h => h.Values.Select(j => $"{h.Key}:{j.Value}")).ToList();
                    var tk = targetItems.Values.SelectMany(h => h.Values.Select(j => $"{h.Key}:{j.Value}")).ToList();
                    var tad = new XElement("root", new XAttribute("command", "delete"), new XAttribute("confirmed", "1"));
                    foreach (var item in tk.Except(sk))
                    {
                        var split = item.Split(':');
                        var type = split[0];
                        var value = string.Join(":", split.Skip(1));
                        if (language != "TextAssets")
                        {
                            var t = string.Format(ue, sc.AbsoluteUri, type, value, language).GetContent();
                            sourceTasks.Add($"{item}", t);
                        }
                        else tad.Add(new XElement("Delete", new XAttribute("id", value)));
                    }
                    if (tad.HasElements) sourceTasks.Add($"Items:Delete", Task.FromResult(tad.ToString()));
                    if (command == "Compare")
                    {
                        foreach (var item in targetItems)
                        {
                            foreach (var value in item)
                            {
                                targetTasks.Add($"{item.Key}:{value.Value}", string.Format(ue, tc.AbsoluteUri, item.Key, value.Value.Replace("&", "%26"), language).GetContent());
                            }
                        }
                        await Task.WhenAll(sourceTasks.Values);
                        await Task.WhenAll(targetTasks.Values);
                        CompareReport cr = new CompareReport(prms, this) { Total = sourceTasks.Count, WorkerTemplate = "Asset", Folder = @"Compare\Content", AddCommentColumn = false };
                        cr.Started += delegate (object obj, EventArgs ea)
                        {
                            disableForm(false);
                        };
                        cr.Completed += delegate (object obj, CompletedEventArgs ea)
                        {
                            ScriptHtml = ea.Content;
                            PageID = ea.UniqueID;
                            //output.NavigateToString(content);
                            enableForm();
                        };
                        BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
                        bw.RunWorkerCompleted += delegate (object worker, RunWorkerCompletedEventArgs ec)
                        {
                            cr.RunReport();
                        };
                        bw.DoWork += delegate (object worker, DoWorkEventArgs ew)
                        {
                            cr.rp = cr.GetLanguageComparePayload(prms, targetTasks, sourceTasks, worker as BackgroundWorker);
                        };
                        Progress p = new Progress(bw);
                        bw.ProgressChanged += delegate (object pc, ProgressChangedEventArgs ep)
                        {
                            p.pbStatus.Value = ep.ProgressPercentage;
                        };
                        p.Owner = this;
                        p.ShowDialog();
                    }
                    else
                    {
                        await Task.WhenAll(sourceTasks.Values).ContinueWith(f =>
                        {
                            foreach (var item in sourceTasks)
                            {
                                XElement elem = XElement.Parse(item.Value.Result.Replace("<root>", "<root xmlns=\"\">").Replace("<NewDataSet>", "<NewDataSet xmlns=\"\">"));
                                var split = item.Key.Split(':');
                                var type = split[0];
                                var value = string.Join(":", split.Skip(1));
                                if (language == "TextAssets")
                                {
                                    if(elem.Element("Table")?.Element("TextAssetID") != null) elem.Element("Table").Element("TextAssetID").Value = "new";
                                    targetTasks.Add($"{item.Key}", te.AbsoluteUri.PostSoapContent("getData", new[] { "procedure", "args.string", "args.string", "args.string", "args.string" }, new[] { "ab_updateasset", "@xml", elem.ToString().XmlEncode(), "@AssetTypeID", "12" }));
                                }
                                else
                                    targetTasks.Add($"{item.Key}", te.AbsoluteUri.PostSoapContent("setLanguage", new[] { "AssetType", "AssetID", "Language", "Content" }, new[] { type, value, language, elem.ToString() }));
                            }
                        });
                        await Task.WhenAll(targetTasks.Values);
                        var output = "";
                        foreach (var item in targetTasks)
                        {
                            XElement elem = XElement.Parse(item.Value.Result.Replace("<root>", "<root xmlns=\"\">").Replace("<NewDataSet>", "<NewDataSet xmlns=\"\">"));
                            if(elem.Name.LocalName == "Envelope") elem = elem.XPathSelectElement("*/*/*/*");
                            var split = item.Key.Split(':');
                            var type = split[0];
                            var value = string.Join(":", split.Skip(1));
                            if (output != "") output += Environment.NewLine;
                            if (int.TryParse(type, out var t)) output += LanguageSummary.titles[t] + " ";
                            if (elem.Name == "Error") output += "Error : " + elem.Value;
                            if (language == "TextAssets")
                            {
                                output += value + " - updated";
                            }
                            else
                            {
                                output += value + " ";
                                foreach (var sub in elem.Elements())
                                {
                                    output += sub.Name.LocalName + " " + sub.Value + " ";
                                }
                            }
                        }
                        ScriptText = output;
                        enableForm();
                    }
                }
                else if (reportResults.Count == 1 || command == "Summary")
                {
                    var sourceItems = JNode.CreateFromJson(reportResults[0].Result);
                    JNode targetItems = null;
                    if (reportResults.Count == 2)
                    {
                        targetItems = JNode.CreateFromJson(reportResults[1].Result);
                    }
                    Reports.LanguageSummary c = new Reports.LanguageSummary(sourceItems, targetItems);
                    var rep = new Reports.LanguageSummaryReport { Folder = $"Reports\\LanguageSummary" };
                    rep.Completed += delegate (object obj, Reports.CompletedEventArgs ea)
                    {
                        if (ea.UniqueID == null) ScriptText = ea.Content;
                        else ScriptHtml = ea.Content;
                        PageID = ea.UniqueID;
                        enableForm();
                        rbtCompareTranslations.IsEnabled = c.TotalUnmatched <= 100;
                        rbtExecuteTranslations.IsEnabled = c.Errors.Count == 0;
                    };
                    rep.RunReport(c, "@Layout@");
                    enableForm();
                }
            }
            catch (Exception ex)
            {
                ScriptText = ex.ToString();
            }
            finally
            {
                enableForm();
            }
        }

        private void rbtListTextAssets_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LoadEnvironments(object sender, RoutedEventArgs e)
        {
            DataAccess.ClearEnvironments();
            taSource.ItemsSource = DataAccess.WebBuilders;
            taSource.DisplayMemberPath = "Key";
            taTarget.ItemsSource = DataAccess.WebBuilders;
            taTarget.DisplayMemberPath = "Key";
        }

        private void LanguageRolloutChanged(object sender, EventArgs e)
        {
            rbtExecuteTranslations.IsEnabled = false;
            rbtCompareTranslations.IsEnabled = false;
        }

        private void rbtAdHoc_Drop(object sender, EventArgs e)
        {
            var b = sender as ItemsControl;
            b.Items.Clear();
            Uri source = new Uri(new Uri(Settings.Default.WebService), "TraversalService/AdHoc_List");
            var reports = source.AbsoluteUri.GetContent<JNode>();
            if(reports["reports"] != null)
            {
                var folder = new BitmapImage(new Uri("/images/Folder-Open-icon.png", UriKind.Relative));
                var report = new BitmapImage(new Uri("/images/AssetReport.png", UriKind.Relative));
                var menus = new Dictionary<string, RibbonMenuItem>();
                foreach (var item in reports["reports"]
                    .Where(f => 
                           f.Value.StartsWith("AssetBuilder_")
                        || f.Value.StartsWith("$")
                        || f.Value.StartsWith($"{Environment.UserName.Replace(".", "")}", StringComparison.InvariantCultureIgnoreCase)
                    )
                    .Select(f => f.Value))
                {
                    var menu = "";
                    var container = b;
                    var split = item.Replace("$", "").Split('_');
                    for (int j = 0; j < split.Length - 1; j++)
                    {
                        if (menu != "") menu += "_";
                        menu += split[j];
                        if (!menus.ContainsKey(menu)) container.Items.Add(menus[menu] = new RibbonMenuItem { Header = split[j], ImageSource = folder });
                        container = menus[menu] as ItemsControl;
                    }
                    var m = new RibbonMenuItem { Header = split.Last(), CommandParameter = item, ImageSource = report };
                    m.Click += RunReport;
                    container.Items.Add(m);
                }
            }            
        }

        private void RunReport(object sender, RoutedEventArgs e)
        {
            var c = sender as ICommandSource;
            Uri source = new Uri(new Uri(Settings.Default.WebService), $"TraversalService/AdHoc_Parameters/{c.CommandParameter}");
            var parameters = source.AbsoluteUri.GetContent<JNode>();
            disableForm(false);
            var erd = new Custom.ExecuteReportDialog(parameters) { Loader = this };
            controlGrid.Children.Add(erd);
        }

        private static bool RecordExport(ExportRecordData data)
        {
            try
            {
                var endpoint = new Uri(new Uri(Settings.Default.WebService), "TraversalService/UpdateLoadData");
                var headers = new[] {
                    ("Content-Type", "application/json")
                };
                var body = new { Data = data };
                var response = body.PostObject<JNode>(endpoint.AbsoluteUri, headers);
                return response != null && !response.Keys.Contains("error", StringComparer.InvariantCultureIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static ExportRecordData GetExportData()
        {
            var input = new InputBox(
                "Enter export details:", 
                "Export Report", 
                "|9", 
                WindowStartupLocation.CenterScreen)
                {
                    ExportReportResponse = new ExportRecordData(Environment.UserName)
                };

            var result = input.ShowDialog();
            return result.HasValue ? input.ExportReportResponse : null;
        }

        private void ABRibbonWindow_Closed(object sender, EventArgs e)
        {
            _algoStatusViewModel.Dispose();
            _algoStatusViewModel = null;
        }
    }

    public class LoadedAlgos : ObservableCollection<LoadedAlgo>
    {

    }

    public class LoadedAlgo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int AlgoID { get; set; }
        public string FileName { get; set; }
        public string MachineName { get; set; }
        public int AssessmentType { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public DateTime Promoted { get; set; }
        public string LoadID { get; set; }
        private bool _Exclude = false;
        public bool Exclude { get { return _Exclude; } set { this._Exclude = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Exclude")); } }
        public bool Consumer { get; set; }
    }

    public class LoadedAsset
    {
        public int AssetID { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return string.Format("{0,6}) {1} {2}", AssetID, Type, Text);
        }
    }

    enum ScriptType
    {
        Full,
        AlgoOnly,
        Xml,
        McKesson
    }

    class Functions
    {
        public int NodeID = 0;
        public Dictionary<string, int> nodes = new Dictionary<string, int>();

        public int Node(string key)
        {
            if (nodes.ContainsKey(key)) return nodes[key];
            int node = 0;
            if (key.StartsWith("Question") && int.TryParse(key.Substring(8), out node)) NodeID = node; else while (nodes.ContainsValue(++node)) { }
            if (!nodes.ContainsKey(key)) nodes.Add(key, node);
            return nodes[key];
        }

        //public int Node(string key)
        //{
        //    if (!nodes.ContainsKey(key)) nodes.Add(key, ++NodeID);
        //    return nodes[key];
        //}

        public XPathNavigator Parse(string data)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);
            return doc.CreateNavigator();
        }

        public string ToUpper(string s)
        {
            return s.ToUpper();
        }

        public string Trim(string s)
        {
            return s.Trim();
        }

        public string KeywordID(string s)
        {
            var word = AlgoLoader.KeyWordXml.SelectSingleNode(string.Format("Table[value_Keyword = '{0}']", s));
            return word == null ? "undefined" : word["value_KeywordID"].InnerText;
        }

        public XPathNavigator NextNode(XPathNodeIterator node, string AlgoID, string NextNodeID)
        {
            while (node.MoveNext())
            {
                if (node.Current != null)
                    return node.Current.SelectSingleNode(string.Format("../*[AlgoID = {0} and NodeID = {1} and name() != 'Table2']", AlgoID, NextNodeID));
            }
            return null;
        }

        public XPathNavigator NextAction(XPathNodeIterator node, string AlgoID, string NodeID, string NextNodeID)
        {
            while (node.MoveNext())
            {
                if (node.Current != null)
                {
                    if (node.Current.Name == "Table3" && node.Current.SelectSingleNode(string.Format("../Table8[AlgoID = '{0}' and DataID = '{1}' and PropertyName = 'LanguageRef']", AlgoID, AlgoID + ":" + NodeID)) != null)
                        return node.Current.SelectSingleNode(string.Format("../*[AlgoID = {0} and NodeID = {1} and name() != 'Table2']", AlgoID, NextNodeID));
                }
            }
            return node.Current;
        }

        List<string> nonQuestions = new List<string>() { "Table3", "Table5" };

        public XPathNavigator NextQuestion(XPathNodeIterator node, string AlgoID, string NextNodeID)
        {
            while (node.MoveNext())
            {
                if (node.Current != null)
                {
                    if (nonQuestions.Contains(node.Current.Name))
                    {
                        XPathNavigator res = node.Current;
                        while (res != null && nonQuestions.Contains(res.Name))
                        {
                            res = res.SelectSingleNode(string.Format("../*[AlgoID = {0} and NodeID = {1} and name() != 'Table2']", AlgoID, NextNodeID));
                            if (res != null) NextNodeID = res.SelectSingleNode("NextNodeID").Value;
                        }
                        if (res != null) return res;
                    }
                }
            }
            return node.Current;
        }

        public XPathNavigator NextConclusions(XPathNodeIterator node, string AlgoID, string NextNodeID)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode root = doc.AppendChild(doc.CreateElement("root"));
            while (node.MoveNext())
            {
                if (node.Current != null)
                {
                    if (nonQuestions.Contains(node.Current.Name))
                    {
                        XPathNavigator res = node.Current;
                        while (res != null && nonQuestions.Contains(res.Name))
                        {
                            XmlDocumentFragment frag = doc.CreateDocumentFragment();
                            frag.InnerXml = res.OuterXml;
                            root.AppendChild(frag);
                            res = res.SelectSingleNode(string.Format("../*[AlgoID = {0} and NodeID = {1} and name() != 'Table2']", AlgoID, NextNodeID));
                            if (res != null) NextNodeID = res.SelectSingleNode("NextNodeID").Value;
                        }
                    }
                }
            }
            return root.CreateNavigator();
        }
    }
}
