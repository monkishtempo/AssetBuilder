using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConclusionMap.Controls
{
    /// <summary>
    /// Interaction logic for Rule.xaml
    /// </summary>
    public partial class Rule : UserControl
    {
        public Rule()
        {
            InitializeComponent();
        }

        Func<RoutedEventArgs, ConclusionMap.Classes.Action> getAction => (e) => { return (e.Source as FrameworkElement).DataContext as ConclusionMap.Classes.Action; };
        Func<RoutedEventArgs, ConclusionMap.Classes.Trigger> getTrigger => (e) => { return (e.Source as FrameworkElement).DataContext as ConclusionMap.Classes.Trigger; };


        private void AddTrigger(object sender, RoutedEventArgs e)
        {
            var o = DataContext as ConclusionMap.Classes.Rule;
            o.triggers.Add(new Classes.Trigger());
        }

        private void AddAction(object sender, RoutedEventArgs e)
        {
            var o = DataContext as ConclusionMap.Classes.Rule;
            o.actions.Add(new Classes.Action());
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            var o = DataContext as ConclusionMap.Classes.Rule;
            var a = getAction(e);
            var t = getTrigger(e);
            if (a != null) o.actions.Remove(a);
            if (t != null) o.triggers.Remove(t);
        }
    }
}
