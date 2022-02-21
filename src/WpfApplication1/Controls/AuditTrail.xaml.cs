using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using AssetBuilder.Controls.AssetControls;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for AuditTrail.xaml
    /// </summary>
    public partial class AuditTrail : UserControl
    {
        public AuditTrail()
        {
            InitializeComponent();
        }

        public AuditItem getAuditItem()
        {
            var type =
                choices.Children.OfType<Control>().Where(f => (f as ToggleButton)?.IsChecked == true).Select(f => (f as ToggleButton)?.Content.ToString()).LastOrDefault();
            var alltypes =
                string.Join(", ", choices.Children.OfType<Control>().Where(f => (f as ToggleButton)?.IsChecked == true).Select(f => (f as ToggleButton)?.Content.ToString()));
            return new AuditItem {Type = type, AllTypes = alltypes};
        }

        private void ToggleButtonChanged(object sender, RoutedEventArgs e)
        {
            OK.IsEnabled = choices.Children.OfType<Control>().Where(f => (f as ToggleButton)?.IsChecked == true).Any();
        }
    }
}
