using System.Windows;
using System.Windows.Controls;

namespace NaturalLanguageWizard
{
    /// <summary>
    /// Interaction logic for InsertText.xaml
    /// </summary>
    public partial class InsertText : UserControl, ISaveable
    {
        public InsertText()
        {
            InitializeComponent();
        }

        private void chkNone_Checked(object sender, RoutedEventArgs e)
        {
            textbox.IsEnabled = false;
        }

        private void chkNone_Unchecked(object sender, RoutedEventArgs e)
        {
            textbox.IsEnabled = true;
        }

        public string Text
        {
            get
            {
                return (chkNone.IsChecked.Value ? string.Empty : textbox.Text);
            }
        }
        
        public string Description
        {
            get
            {
                return textblock.Text;
            }

            set
            {
                textblock.Text = value;
                //textbox.SetMargin(textblock.Text.Length * (textblock.FontSize / 2) + 5, MarginDirection.Left);                
            }
        }

        public void SaveAndClear()
        {
            textbox.SaveAndClear();

            //chkNone.SaveAndInitialise(false);
        }

        public void RestoreLast()
        {
            textbox.RestoreLast();

            //chkNone.RestoreLast();
        }       
    }
}
