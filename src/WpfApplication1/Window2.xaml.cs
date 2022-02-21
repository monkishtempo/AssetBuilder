using System;
using System.Windows;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Data.Data ds = new Data.Data();
            ds.Url = textBox.Text;
            DateTime then = DateTime.Now;
            var xn = ds.getData("dsp_SearchProperties", new string[]
            {
                "@PropertyType", "Question:15528"
            });
            label.Content = ToFriendlyString(DateTime.Now - then);
        }

        public static string ToFriendlyString(TimeSpan t)
        {
            string output = "";
            if (t.Days > 0) output += (output != "" ? ", " : "") + t.Days + " Day" + (t.Days > 1 ? "s" : "");
            if (t.Hours > 0) output += (output != "" ? ", " : "") + t.Hours + " Hour" + (t.Hours > 1 ? "s" : "");
            if (t.Minutes > 0) output += (output != "" ? ", " : "") + t.Minutes + " Minute" + (t.Minutes > 1 ? "s" : "");
            if (t.Seconds > 0) output += (output != "" ? ", " : "") + t.Seconds + " Second" + (t.Seconds > 1 ? "s" : "");
            if (t.Milliseconds > 0) output += (output != "" ? ", " : "") + t.Milliseconds + " Millisecond" + (t.Milliseconds > 1 ? "s" : "");
            return output;
        }
    }
}
