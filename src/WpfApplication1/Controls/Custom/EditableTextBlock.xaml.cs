using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for EditableTextBlock.xaml
    /// </summary>
    public partial class EditableTextBlock : UserControl, INotifyPropertyChanged
    {
        public event EventHandler Changed;
        public event EventHandler Selected;
        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty TextProperty =
DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsInEditModeProperty =
    DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(EditableTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsSelectedProperty =
    DependencyProperty.Register("IsSelected", typeof(bool), typeof(EditableTextBlock), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public bool IsInEditMode
        {
            get
            {
                return (bool)GetValue(IsInEditModeProperty);
            }
            set
            {
                SetValue(IsInEditModeProperty, value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsInEditMode"));
            }
        }

        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        public string Key { get; set; }
        public string Value { get; set; }

        private bool _IsKey = false;
        public bool IsKey { get { return _IsKey; } }
        public bool IsValue { get { return !_IsKey; } }

        private bool _ReadOnly = false;
        public bool ReadOnly { get { return _ReadOnly; } }

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        public static EditableTextBlock CreateReadOnlyKey(string text)
        {
            return new EditableTextBlock { Text = text, Key = text, _IsKey = true, _ReadOnly = true };
        }

        public static EditableTextBlock CreateKey(string text)
        {
            return new EditableTextBlock { Text = text, Key = text, _IsKey = true };
        }

        public static EditableTextBlock CreateValue(string key, string text)
        {
            return new EditableTextBlock { Text = text, Key = key, Value = text, _IsKey = false };
        }

        private void editableTextBoxHeader_LostFocus(object sender, RoutedEventArgs e)
        {
            IsInEditMode = false;
            Text = (sender as TextBox).Text;
            if (Changed != null) Changed(this, new EventArgs());
        }

        private void textBlockHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ReadOnly)
            {
                IsSelected = true;
            }
            else
            {
                IsInEditMode = true;
                editableTextBoxHeader.Focus();
            }
            if (Selected != null) Selected(this, new EventArgs());
        }

        private void DockPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            buttons.Visibility = Visibility.Visible;
        }

        private void DockPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            buttons.Visibility = Visibility.Collapsed;
        }

        public void DeSelect()
        {
            SetValue(IsSelectedProperty, false);
        }

        private void editableTextBoxHeader_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter) { editableTextBoxHeader_LostFocus(sender, e); }
            else if (e.Key == System.Windows.Input.Key.Escape) { editableTextBoxHeader.Text = Text; }
        }
    }
}
