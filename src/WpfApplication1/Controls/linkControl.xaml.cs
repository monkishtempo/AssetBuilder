using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AssetBuilder.Properties;
using System.Xml;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for linkControl.xaml
    /// </summary>
    public partial class linkControl : UserControl
    {
        public event SelectionChangedEventHandler WebServiceChanged;
        public List<string> dontsave = new List<string>(new string[] { "" });

        public linkControl()
        {
            InitializeComponent();
            cmbWebService.SelectionChanged += new SelectionChangedEventHandler(cmbWebService_SelectionChanged);
            cmbWebService.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(cmbWebService_LostKeyboardFocus);
        }

        void cmbWebService_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (WebServiceChanged != null)
            {
                if (!cmbWebService.Items.Contains(cmbWebService.Text) && !dontsave.Contains(cmbWebService.Text))
                {
                    cmbWebService.Items.Add(cmbWebService.Text);
                    cmbWebService.SelectedItem = cmbWebService.Text;
                }
            }
        }

        void cmbWebService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WebServiceChanged != null) WebServiceChanged(this, e);
        }

        string getValueFromMessage(string message, char separator, int index)
        {
            if (message == null) return null;
            var split = message.Split(separator);
            if (split.Length > index) return split[index];
            return null;
        }

        public void setFormForTitle(double X, double Y, string text, bool Symptom = false)
        {
            txtDisplay.Visibility = Visibility.Collapsed;
            btnProperties.Visibility = Visibility.Visible;
            Height = Symptom ? 178 : 152;
            if (Symptom)
            {
                txtAddress.Visibility = Visibility.Collapsed;
                txtMessage.Visibility = Visibility.Visible;
                cmbLevel.Visibility = Visibility.Visible;
                txtDescription.Visibility = Visibility.Visible;
                txtMessage.Text = getValueFromMessage(text, '|', 0);
                txtDescription.Text = getValueFromMessage(text, '|', 2);
                if (int.TryParse(getValueFromMessage(text, '|', 1), out int i))
                    cmbLevel.SelectedIndex = i - 1;
                txtMessage.TextChanged += ChangeTitle;
                txtDescription.TextChanged += ChangeTitle;
                cmbLevel.SelectionChanged += LevelChanged;
                lblLevel.Visibility = Visibility.Visible;
                lblDisplay.Margin = new Thickness(12, 0, 0, 143);
                lblAddress.Margin = new Thickness(12, 0, 0, 86);
                lblAddress.Content = "Description";
                lblDisplay.Content = "Message";
            }
            else
            {
                lblAddress.Visibility = Visibility.Collapsed;
                lblDisplay.Content = "Title";
                lblDisplay.Margin = new Thickness(12, 0, 0, 112);
            }
            txtAddress.Text = text;
            txtAddress.Height = 104;
            txtAddress.TextWrapping = TextWrapping.Wrap;
            txtAddress.MaxLength = 120;
            if (Window1.ShowTranslation)
            {
                XmlNode translation = DataAccess.getLanguage(0, text, Window1.TranslationLanguage);
                XmlNode titleTranslation = translation.SelectSingleNode("//Title");
                if (titleTranslation != null)
                    txtTranslation.Text = titleTranslation.InnerText;
                if (Symptom)
                {
                    txtMessage.Margin = new Thickness(138, 0, 202, 143);
                    cmbLevel.Margin = new Thickness(138, 0, 202, 114);
                    txtDescription.Margin = new Thickness(138, 0, 202, 36);
                    txtMessageTranslation.Visibility = Visibility.Visible;
                    cmbLevelTranslation.Visibility = Visibility.Visible;
                    txtDescriptionTranslation.Visibility = Visibility.Visible;
                    txtMessageTranslation.Text = getValueFromMessage(titleTranslation.InnerText, '|', 0);
                    txtDescriptionTranslation.Text = getValueFromMessage(titleTranslation.InnerText, '|', 2);
                    if (int.TryParse(getValueFromMessage(titleTranslation.InnerText, '|', 1), out int i))
                    {
                        if (i == 0) i = 5;
                        cmbLevelTranslation.SelectedIndex = i - 1;
                    }
                }
                else
                {
                    txtAddress.Margin = new Thickness(138, 0, 202, 36);
                    txtTranslation.Visibility = Visibility.Visible;
                    txtTranslation.TextWrapping = TextWrapping.Wrap;
                }
            }
            if (Window1.EditTranslation)
            {
                txtAddress.IsReadOnly = true;
                txtMessage.IsReadOnly = true;
                cmbLevel.IsEnabled = false;
                txtDescription.IsReadOnly = true;
                txtMessageTranslation.IsReadOnly = false;
                cmbLevelTranslation.IsEnabled = true;
                txtDescriptionTranslation.IsReadOnly = false;
                txtTranslation.IsReadOnly = false;
                txtMessageTranslation.TextChanged += ChangeTitle;
                txtDescriptionTranslation.TextChanged += ChangeTitle;
                cmbLevelTranslation.SelectionChanged += LevelChanged;
            }

            System.Xml.XmlNode len = qcat.BuilderDefaults.SelectSingleNode("Table[Table='REC_TYPE' and Column='TITLE']/Length");
            if (len != null) txtAddress.MaxLength = int.Parse(len.InnerText);
            txtAddress.SelectionChanged += new RoutedEventHandler(AssetBuilder.Controls.NLExtensions.textBox_AdornAndValidate);
            //Height = 71;
            this.SetValue(Canvas.LeftProperty, X);
            this.SetValue(Canvas.TopProperty, Y);
        }

        private void LevelChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeTitle(sender, null);
        }

        private void ChangeTitle(object sender, TextChangedEventArgs e)
        {
            bool trans = (sender as Control).Name.EndsWith("Translation");
            if (trans)
            {
                txtTranslation.Text = txtMessageTranslation.Text + "|" + cmbLevelTranslation.SelectedValue + "|" + txtDescriptionTranslation.Text;
                if (txtTranslation.Text.Length == 2) txtTranslation.Text = "";
                if (txtTranslation.MaxLength > 0)
                {
                    txtMessageTranslation.MaxLength = txtTranslation.MaxLength - txtDescriptionTranslation.Text.Length - 3;
                    txtDescriptionTranslation.MaxLength = txtTranslation.MaxLength - txtMessageTranslation.Text.Length - 3;
                }
            }
            else
            {
                txtAddress.Text = txtMessage.Text + "|" + cmbLevel.SelectedValue + "|" + txtDescription.Text;
                txtMessage.MaxLength = txtAddress.MaxLength - txtDescription.Text.Length - 3;
                txtDescription.MaxLength = txtAddress.MaxLength - txtMessage.Text.Length - 3;
            }
        }

        public void setFormForLogin()
        {
            forgotPassword.Visibility = Visibility.Visible;
            permissions.Visibility = Visibility.Visible;
            lblDisplay.Content = "User Name";
            lblDisplay.ToolTip = new AssetBuilder.Info { Title = "User Name", Body = "UserNameToolTip" }.ProvideValue(null);
            txtDisplay.ToolTip = lblDisplay.ToolTip;
            lblAddress.Content = "Password";
            lblAddress.ToolTip = new AssetBuilder.Info { Title = "Password", Body = "PasswordToolTip" }.ProvideValue(null);
            txtPassword.ToolTip = lblAddress.ToolTip;
            txtAddress.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
            pnlWebService.Visibility = Visibility.Visible;
            cmbWebService.Text = Settings.Default.WebService;
            foreach (string item in Settings.Default.PreviousWebService.Split(';'))
            {
                if (item != "")
                    cmbWebService.Items.Add(item);
            }
            if (!cmbWebService.Items.Contains(Settings.Default.WebService)) cmbWebService.Items.Insert(0, Settings.Default.WebService);
            cmbWebService.SelectedItem = Settings.Default.WebService;
            Height = 131;
        }
    }
}
