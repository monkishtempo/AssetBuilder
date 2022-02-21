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
    /// Interaction logic for Map.xaml
    /// </summary>
    public partial class Map : UserControl
    {
        internal ConclusionMap.Classes.Map map { get { return DataContext as ConclusionMap.Classes.Map; } }
        Func<RoutedEventArgs, ConclusionMap.Classes.Rule> getRule => (e) => { return (e.Source as FrameworkElement).DataContext as ConclusionMap.Classes.Rule; };

        public Map()
        {
            InitializeComponent();
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            map.rules.Remove(getRule(e));
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            var rule = getRule(e);
            var index = map.rules.IndexOf(rule);
            if (index > 0) map.rules.Move(index, index - 1);
        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {
            var rule = getRule(e);
            var index = map.rules.IndexOf(rule);
            if (index < map.rules.Count - 1) map.rules.Move(index, index + 1);
        }
    }
}
