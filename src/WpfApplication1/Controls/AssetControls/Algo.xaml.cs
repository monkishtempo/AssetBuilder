using System.Windows.Controls;
using System.Xml;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for Algo.xaml
    /// </summary>
    public partial class Algo : UserControl
    {
        XmlNode asset;

        public Algo()
        {
            InitializeComponent();
        }

        public Algo(XmlNode algo)
        {
            this.asset = algo;
            InitializeComponent();
            this.DataContext = asset["Table"];
        }
    }
}
