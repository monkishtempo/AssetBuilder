using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for uTextBox.xaml
    /// </summary>
    public partial class uTextBox : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { txtBox.Text = value; SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(uTextBox));

        public uTextBox()
        {
            InitializeComponent();
        }
    }
}
