using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Xsl;
using AssetBuilder.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;
using AssetBuilder;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for DebugOutput.xaml
    /// </summary>
    public partial class DebugOutput : ABRibbonWindow
    {
        public static DebugOutput DebugOuputForm = null;

        public ABListener trace = null;

        public DebugOutput()
        {
            InitializeComponent();
            DebugOutput.DebugOuputForm = this;
            trace = new ABListener();
            trace.Writer = new TextBoxStreamWriter(output);
            Trace.Listeners.Add(trace);
            foreach (var item in DataAccess.LastCommands)
            {
                if(item.Item3 != null) trace.WriteLine(item.Item1, item.Item3, item.Item4);
                else trace.WriteLine(item.Item1, item.Item2, item.Item4);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Trace.Listeners.Remove(trace);
            DebugOuputForm = null;
            base.OnClosed(e);
        }

        private void output_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StatusContent.Text = "";
            if (output.SelectedItem != null && output.SelectedItem is DebugCall)
            {
                DebugCall dc = output.SelectedItem as DebugCall;
                if (dc == null) return;
                if (dc.Data != null)
                {
                    StringBuilder sb = GetHtml(dc.Data);
                    webBrowser1.NavigateToString(string.Format("<html><head></head><body>{0}</body></html>", sb.ToString()));
                }
                else if(dc.Json != null)
                {
                    webBrowser1.NavigateToString(string.Format("<html><head></head><body>{0}</body></html>", dc.Json.ToJson()));
                }
                StatusContent.Text = dc.Duration.ToFriendlyString();
            }
            else webBrowser1.NavigateToString("<html><head></head><body></body></html>");
        }

        private StringBuilder GetHtml(XmlNode data)
        {
            if (data == null) return new StringBuilder();
            XsltArgumentList args = new XsltArgumentList();
            args.AddParam("html", "", (bool)rbtRenderHTML.IsChecked ? "yes" : "no");
            StringBuilder sb = new StringBuilder(data.Transform("Xml.xsl", args));
            return sb;
        }

        private void rbtClearDebug_Click(object sender, RoutedEventArgs e)
        {
            output.Items.Clear();
            webBrowser1.NavigateToString("<html><head></head><body></body></html>");
        }

        private void rbtRenderHTML_Click(object sender, RoutedEventArgs e)
        {
            output_SelectionChanged(null, null);
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(this);
            if (ie is ListBoxItem && sender is ContextMenu)
            {
                var s = (ie as ListBoxItem).Content.ToString();
                var m = ((sender as ContextMenu).Items[1] as MenuItem);
                if (s.ToLower().StartsWith("http")) { m.Header = "_Copy SQL"; }
                else if (s.ToLower().StartsWith("exec")) { m.Header = "_Copy Url"; }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
            {
                var m = (sender as MenuItem);
                var cp = m.CommandParameter?.ToString();
                if (cp == "1") Copy();
                if (cp == "2")
                {
                    if (m.Header.ToString().Contains("Url")) CopyUrl();
                    else CopySQL();
                }
            }
        }

        private void Copy()
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(this);
            if (ie is ListBoxItem)
            {
                Clipboard.SetText((ie as ListBoxItem).Content.ToString());
            }
        }

        private void CopyUrl()
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(this);
            if (ie is ListBoxItem)
            {
                string output = "/getData?";
                var command = (ie as ListBoxItem).Content.ToString();
                var prms = new List<string>();
                Clipboard.SetText((ie as ListBoxItem).Content.ToString());
            }
        }

        private void CopySQL()
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(this);
            if (ie is ListBoxItem)
            {
                var command = (ie as ListBoxItem).Content.ToString();
                var s = command.Split('&', '?');
                var output = "Exec ";
                var prms = "";
                var paramstart = false;
                var suppress = false;
                for (int i = 1; i < s.Length; i += 1)
                {
                    var si = s[i].Split('=');
                    var k = si[0];
                    var v = string.Join("=", si.Skip(1));
                    if (k == "procedure") output += $"[{v}]";
                    if (k == "args")
                    {
                        paramstart = !paramstart;
                        if (paramstart) {
                            if (v.StartsWith("@"))
                            {
                                if (prms != "") prms += ", ";
                                prms += $"{v}=";
                                suppress = false;
                            }
                            else suppress = true;
                        }
                        else if (!suppress) prms += $"'{v}'";
                    }
                }
                output += " " + prms;
                Clipboard.SetText(output);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if ((e.Command as RoutedCommand) != null && (e.Command as RoutedCommand).Name == "Copy") Copy();
        }

        private void rbtShowAsset_Click(object sender, RoutedEventArgs e)
        {
            output.SelectedItem = null;
            var sb = GetHtml(Window1.window?.qcat1?.loadedAsset?.asset);
            webBrowser1.NavigateToString(string.Format("<html><head></head><body>{0}</body></html>", sb.ToString()));
            string status = string.Format("{0} {1}", Window1.window?.qcat1?.loadedAsset?.assetType.ToString(), Window1.window?.qcat1?.loadedAsset?.AssetID);
            StatusContent.Text = status;
        }
    }

    public static class CustomCommands
    {
        public static readonly RoutedUICommand CopySQL = new RoutedUICommand
            (
                "Copy SQL",
                "CopySQL",
                typeof(CustomCommands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift)
                }
            );

        //Define more commands here, just like the one above
    }
}

class DebugCall
{
    public string Command { get; set; }
    public XmlNode Data { get; set; }
    public JNode Json { get; set; }
    public int CategoryTypeID { get; set; }
    public TimeSpan Duration { get; set; }

    public DebugCall(string command, XmlNode data, TimeSpan duration)
    {
        Command = command;
        Data = data;
        Duration = duration;
    }

    public DebugCall(string command, JNode data, TimeSpan duration)
    {
        Command = command;
        Json = data;
        Duration = duration;
    }

    public override string ToString()
    {
        return Command;
    }
}

public class ABListener : TextWriterTraceListener
{
    public void WriteLine(string Command, XmlNode value, TimeSpan duration)
    {
        TextBoxStreamWriter tsw = Writer as TextBoxStreamWriter;
        tsw.WriteLine(Command, value, duration);
    }
    public void WriteLine(string Command, JNode value, TimeSpan duration)
    {
        TextBoxStreamWriter tsw = Writer as TextBoxStreamWriter;
        tsw.WriteLine(Command, value, duration);
    }
}

class TextBoxStreamWriter : TextWriter
{
    ListBox Output = null;

    public TextBoxStreamWriter(ListBox listbox)
    {
        Output = listbox;
    }

    public override void WriteLine(string value)
    {
        base.WriteLine(value);
        Output.Items.Add(value);
    }

    public void WriteLine(string Command, XmlNode value, TimeSpan duration)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
            Output.Items.Add(new DebugCall(Command, value, duration));
        }));
    }

    public void WriteLine(string Command, JNode value, TimeSpan duration)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        {
            Output.Items.Add(new DebugCall(Command, value, duration));
        }));
    }

    public override Encoding Encoding
    {
        get { return System.Text.Encoding.UTF8; }
    }
}