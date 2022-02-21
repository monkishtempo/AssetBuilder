using System;
using System.Collections;
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

namespace XmlTreeView
{
    /// <summary>
    /// Interaction logic for EditableTreeView.xaml
    /// </summary>
    public partial class EditableTreeView : UserControl
    {
        public IEnumerable ItemsSource { get { return editableTreeView.ItemsSource; } set { editableTreeView.ItemsSource = value; } }
        public object SelectedItem { get { return editableTreeView.SelectedItem; } }

        public string Key { get; set; }
        public TreeView TreeView { get { return editableTreeView; } }
        public EditableTreeView()
        {
            InitializeComponent();
        }

        private void EditableTextBlock_PropertyChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewSource) (sender as TreeViewSource).IsSelected = true;
        }

        private void EditableTextBlock_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }
    }
}
