using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AssetBuilder.Controls;
using AssetBuilder.ViewModels;

namespace AssetBuilder.AssetControls
{
    /// <summary>
    /// Interaction logic for FindReplace.xaml
    /// </summary>
    public partial class FindReplace : UserControl
    {
        public assetControl asset { get; set; }
        List<string> selected = new List<string>();

        public FindReplace()
        {
            InitializeComponent();
        }

        public void init(assetControl ac)
        {
            asset = ac;
            comboBox1.Focus();
            if (comboBox3.Items.Count == 0)
            {
                int i = 0;
                foreach (var item in asset.TextChildren)
                {
                    if (!item.Value.SpellCheck.IsEnabled) continue;
                    string name = item.Key.Substring(3);
                    CheckBox cb = new CheckBox { Content = new ListItem { ID = i++, Value = name }, IsChecked = true };
                    cb.Checked += new RoutedEventHandler(cb_Checked);
                    cb.Unchecked += new RoutedEventHandler(cb_Checked);
                    comboBox3.Items.Add(cb);
                }
                cb_Checked(null, null);
            }
            TextBox TB = (TextBox) comboBox1.Template.FindName("PART_EditableTextBox", comboBox1);
            TB.Text = asset.cat.textBox5.Text;
            TB.SelectionLength = 0;
            TB.SelectionStart = TB.Text.Length;
        }

        void cb_Checked(object sender, RoutedEventArgs e)
        {
            selected.Clear();
            string test = "";
            bool all = true;
            foreach (var item in comboBox3.Items)
            {
                if (item is CheckBox)
                {
                    CheckBox cb = item as CheckBox;
                    if ((bool)cb.IsChecked)
                    {
                        ListItem li = cb.Content as ListItem;
                        selected.Add(li.Value);
                        if (test != "") test += ", ";
                        test += li.Value;
                    }
                    else all = false;
                }
            }
            if (all)
                comboBox3.Text = "All";
            else
                comboBox3.Text = test;
        }

        private void comboBox1_TextChanged(object sender, RoutedEventArgs e)
        {
            foreach (var item in asset.TextChildren)
            {
                string name = item.Key.Substring(3);
                qcat.SetSearch(comboBox1.Text, (bool)checkBox1.IsChecked, (bool)checkBox2.IsChecked, (bool)checkBox3.IsChecked);
                if (selected.Contains(name))
                    NLExtensions.textBox_AdornAndValidate(item.Value, null);
                else
                    item.Value.clearAdornerLayer();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            replaceAll = false;
            bool b = Find();
            if (!b) MessageBox.Show("The search item was not found.", "Asset Builder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            replaceAll = false;
            replaceResult r = Replace();
            if (r == replaceResult.LastReplaced) MessageBox.Show("No more matches were found.", "Asset Builder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            if (r == replaceResult.NotFound) MessageBox.Show("The search item was not found.", "Asset Builder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        bool replaceAll = false;

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            replaceAll = true;
            asset.TextChildren.First().Value.Focus();
            asset.TextChildren.First().Value.Select(0, 0);

            int i = 0;
            replaceResult r;
            while ((int)(r = Replace()) > 1) if(r == replaceResult.ReplacedAndFoundNext) i++;
            if (r == replaceResult.LastReplaced) i++;
            if (i == 0) MessageBox.Show("The search item was not found.", "Asset Builder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else MessageBox.Show(string.Format("{0} replacement{1} made.", i, i > 1 ? "s were" : " was"), "Asset Builder", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            replaceAll = false;
        }

        private replaceResult Replace()
        {
            bool replace = false;
            TextBox t = getTextBox();
            if (t != null && Regex.IsMatch(t.SelectedText, qcat.CurrentSearch, RegexOptions.Multiline | RegexOptions.IgnoreCase) && t.IsReadOnly == false)
            {
                replace = true;
                int ss = t.SelectionStart;
                int se = t.SelectionLength + ss;
                t.Text = t.Text.Substring(0, ss) + comboBox2.Text + t.Text.Substring(se);
                t.SelectionStart = ss + comboBox2.Text.Length;
                t.SelectionLength = 0;
            }
            bool b = Find();
            if (replace && !b) return replaceResult.LastReplaced;
            if (!replace && !b) return replaceResult.NotFound;
            if (replace && b) return replaceResult.ReplacedAndFoundNext;
            return replaceResult.FoundNext;
        }

        private bool Find()
        {
            if (qcat.CurrentSearch == "") return false;
            TextBox t = getTextBox();
            if (t == null) t = asset.TextChildren.First().Value;

            RegexOptions ro = RegexOptions.Multiline | RegexOptions.IgnoreCase;
            //if (!(bool)checkBox1.IsChecked) ro = ro | RegexOptions.IgnoreCase;
            //string search = qcat.currentSearch;
            //if ((bool)checkBox2.IsChecked) search = @"\b" + search + @"\b";
            Regex r = new Regex(qcat.CurrentSearch, ro);
            bool pass = false;
            int count = (replaceAll ? 1 : 2);
            for (int i = 0; i < count; i++)
            {
                foreach (var item in asset.TextChildren)
                {
                    if (item.Value == t) pass = true;
                    else item.Value.Select(0, 0);
                    if (pass && selected.Contains(item.Key.Substring(3)))
                    {
                        Match match = r.Match(item.Value.Text, item.Value.SelectionStart + item.Value.SelectionLength);
                        if (match.Success)
                        {
                            item.Value.Focus();
                            //item.Value.ScrollToHome();
                            item.Value.Select(match.Index, match.Length);
                            return true;
                        }
                    }
                }
                t = asset.TextChildren.First().Value;
            }
            return false;
        }

        private TextBox getTextBox()
        {
            System.Windows.IInputElement ie = FocusManager.GetFocusedElement(asset.cat.Form);

            if (ie is TextBox && asset.TextChildren.ContainsValue(ie as TextBox))
                return ie as TextBox;
            return null;
        }
    }

    enum replaceResult
    {
        NotFound,
        LastReplaced,
        FoundNext,
        ReplacedAndFoundNext
    }
}
