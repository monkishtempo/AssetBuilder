using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AssetBuilder.AssetControls
{
    public class Question : assetControl
    {
        ComboBox cmbType;
        TextBox txtLay;
        Image alertLay;

        static Question()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Question), new FrameworkPropertyMetadata(typeof(Question)));
        }

        public Question()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            cmbType = GetTemplateChild("cmbType") as ComboBox;
            cmbType.SelectionChanged += new SelectionChangedEventHandler(cmbType_SelectionChanged);
            txtLay = GetTemplateChild("txtSummary") as TextBox;
            txtLay.TextChanged += new TextChangedEventHandler(txtLay_TextChanged);
            alertLay = GetTemplateChild("alertLay") as Image;
        }

        void txtLay_TextChanged(object sender, TextChangedEventArgs e)
        {
            displayAlert(false);
        }

        void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            displayAlert(true);
        }

        private void displayAlert(bool updateEmpty)
        {
            if (asset["Table"]["QuestTypeID"].InnerText == "10" || asset["Table"]["QuestTypeID"].InnerText == "11")
            {
                if (asset["Table"]["Lay_Statement"].InnerText == "" && EditMode && updateEmpty)
                {
                    asset["Table"]["Lay_Statement"].InnerText = "*";
                }
                if (txtLay.Text != "*" && txtLay.Text != "")
                {
                    alertLay.Visibility = Visibility.Visible;
                }
                else alertLay.Visibility = Visibility.Collapsed;
            }
            else alertLay.Visibility = Visibility.Collapsed;
        }

        public Question(XmlNode question)
            : base(question)
        {
            assetType = AssetType.Question;
            tableName = "QUESTION";
            cats.Add(0, "AgeID");
            cats.Add(1, "HistID");
            cats.Add(2, "Hist_SubID");
            cats.Add(3, "BodyID");
            expert = question["Table"]["Clinical_Statement"];
        }
    }
}
