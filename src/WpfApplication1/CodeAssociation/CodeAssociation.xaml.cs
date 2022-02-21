using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AssetBuilder.Controls;
using System.Xml;

namespace AssetBuilder.CodeAssociation
{
    /// <summary>
    /// Interaction logic for CodeAssociation.xaml
    /// </summary>
    public partial class CodeAssociation : ABRibbonWindow
    {
        public CodeAssociation()
        {
            InitializeComponent();
            XmlNode test = DataAccess.TableUpdate("EMR_CodeAssociation", "<root command=\"get\" />");
            grid.ItemsSource = test;
            //grid.data.LoadingRow += data_LoadingRow;
        }

        void data_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        DependencyObject getChild<T>(DependencyObject dp)
        {
            if (dp == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
            {
                DependencyObject p = VisualTreeHelper.GetChild(dp, i);
                if (p is T) return p as DependencyObject;
                else
                {
                    var c = getChild<T>(p);
                    if (c != null) return c;
                }
            }
            return null;
        }
    }
}
