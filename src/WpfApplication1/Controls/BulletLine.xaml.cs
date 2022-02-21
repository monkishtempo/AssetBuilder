using System.Windows;
using System.Windows.Controls;
using AssetBuilder.AssetControls;

namespace AssetBuilder.Controls
{
    /// <summary>
    /// Interaction logic for BulletLine.xaml
    /// </summary>
    public partial class BulletLine : UserControl
    {
        private int _ID;
        public int ID { get { return _ID; } set { BPID.Text = value.ToString(); _ID = value; } }
        public string Text { get { return txtBP_Text.Text; } set { txtBP_Text.Text = value; } }
        public int Priority { get; set; }
		public string TextLanguage { get { return txtBP_TextLanguage.Text; } set { txtBP_TextLanguage.Text = value; } }

        public BulletLine()
        {
            InitializeComponent();
            txtBP_Text.IsVisibleChanged += assetControl.tb_IsVisibleChanged;
            txtBP_TextLanguage.IsVisibleChanged += assetControl.tb_IsVisibleChanged;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == Panel.BackgroundProperty)
            {
                BPID.Background = Background;
				txtBP_Text.Background = Background;
				txtBP_TextLanguage.Background = Background;
			}
        }

        public void showTop()
        {
            rect.Height = 2;
        }

        public void clearTop()
        {
            rect.Height = 0;
        }
    }
}
