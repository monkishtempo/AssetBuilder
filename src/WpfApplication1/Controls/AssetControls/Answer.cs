using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using AssetBuilder.Classes;
using AssetBuilder.ViewModels;

namespace AssetBuilder.AssetControls
{
    public class Answer : assetControl
    {
        Button updateButton;
        Button graphButton;
        ComboBox anstype;
        ComboBox multiplier;

        static Answer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Answer), new FrameworkPropertyMetadata(typeof(Answer)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            updateButton = GetTemplateButton("updateButton");
            updateButton.Click += new RoutedEventHandler(updateButton_Click);
            graphButton = GetTemplateButton("graphButtonInner");
            graphButton.Click += new RoutedEventHandler(graphButton_Click);
            if (!Window1.AllowGraph) graphButton.Visibility = System.Windows.Visibility.Collapsed;
            anstype = GetTemplateChild("cmbAnsType") as ComboBox;
            multiplier = GetTemplateChild("cmbMultiplier") as ComboBox;
            anstype.SelectionChanged += new SelectionChangedEventHandler(anstype_SelectionChanged);
            ageDefaults = cat.Defaults.Clone();
            ageDefaults.SelectSingleNode("/*[ComboBox = 'cmbMultiplier' and Multiplier = '1']/Description").InnerText = "Days";
        }

        private void graphButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox answerText = GetTemplateChild("txtAnswer") as TextBox;
            new CurveVisualiser.MainWindow(answerText).Show();
        }

        XmlNode ageDefaults;

        void anstype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if ((e.AddedItems[0] as ListItem).ID == 8)
                {
                    setComboText(ageDefaults.SelectNodes("//*[ComboBox = 'cmbMultiplier']"));
                    if(e.RemovedItems.Count > 0) asset["Table"]["Multiplier"].InnerText = "365.25";
                }
                else
                {
                    setComboText(cat.Defaults.SelectNodes("//*[ComboBox = 'cmbMultiplier']"));
					if (e.RemovedItems.Count > 0) asset["Table"]["Multiplier"].InnerText = "1";
                }
            }
        }

        void setComboText(XmlNodeList xnl)
        {
            string mult = asset["Table"]["Multiplier"].InnerText;
            multiplier.Items.Clear();
            multiplier.ItemContainerStyle = new Style(typeof(ComboBoxItem));
            multiplier.ItemContainerStyle.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding("MultID")));
            foreach (XmlNode xn in xnl)
                multiplier.Items.Add(new ListItem { MultID = xn.ChildNodes[1].InnerText, Value = xn.ChildNodes[2].InnerText });
            asset["Table"]["Multiplier"].InnerText = mult;
        }

        public Answer(XmlNode answer)
            : base(answer)
        {
            assetType = AssetType.Answer;
            tableName = "ANSWER";
            cats.Add(0, "AgeID");
            cats.Add(1, "HistID");
            cats.Add(2, "Hist_SubID");
            cats.Add(3, "BodyID");
            expert = answer["Table"]["Clinical_Answer"];
            string sin = answer["Table"]["StoreIfNeg"].InnerText;
            if (sin == "-1")
                answer["Table"]["StoreIfNeg"].InnerText = AssetBuilder.Properties.Settings.Default.StoreIfNeg;
        }

        void updateButton_Click(object sender, RoutedEventArgs e)
        {
            string s = "> or = ";
            double min = double.Parse(asset["Table"]["Min_Value"].InnerText);
            double max = double.Parse(asset["Table"]["Max_Value"].InnerText);
            s += min.ToString();
            s += " but < ";
            s += max.ToString();
            string value = (ComboChildren["cmbMultiplier"].SelectedItem as ListItem).Value;
            if ((string)ComboChildren["cmbMultiplier"].SelectedValue != "1" || ComboChildren["cmbAnsType"].SelectedValue.ToString() == "8") s += " " + value;
            string type = asset["Table"]["AnsTypeID"].InnerText;
            if (type == "18")
            {
                string calc = asset["Table"]["Answer_Text"].InnerText;
                asset["Table"]["Clinical_Answer"].InnerText = calc + ((max > min) ? " (" + s + ")" : " (Unvalidated)");
                asset["Table"]["Lay_Answer"].InnerText = "*";
            }
            else if (type == "8" || type == "12")
            {
                if (max > min)
                {
                    asset["Table"]["Clinical_Answer"].InnerText = s;
                    asset["Table"]["Lay_Answer"].InnerText = s;
                    asset["Table"]["Answer_Text"].InnerText = s;
                }
                else
                {
                    asset["Table"]["Clinical_Answer"].InnerText = "Unvalidated";
                    asset["Table"]["Lay_Answer"].InnerText = "Unvalidated";
                    asset["Table"]["Answer_Text"].InnerText = "Unvalidated";
                }
            }
            else if (type == "20" || type == "21" || type == "22")
            {
                Regex r = new Regex(@"\([^)(]*or[^)(]*but[^)(]*\)");
                string f = asset["Table"]["Clinical_Answer"].InnerText;
                if (max > min)
                {
                    if (r.IsMatch(f))
                        asset["Table"]["Clinical_Answer"].InnerText = r.Replace(f.Replace(" Unvalidated", ""), "(" + s + ")");
                    else
                        asset["Table"]["Clinical_Answer"].InnerText = f.Replace(" Unvalidated", "") + " (" + s + ")";
                }
                else
                {
                    asset["Table"]["Clinical_Answer"].InnerText = f.Replace(" Unvalidated", "") + " Unvalidated";
                }
                if (asset["Table"]["Lay_Answer"].InnerText == "") asset["Table"]["Lay_Answer"].InnerText = "*";
            }
        }
    }
}
