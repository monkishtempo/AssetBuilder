using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder
{
    /// <summary>
    /// Interaction logic for Waiting.xaml
    /// </summary>
    public partial class Waiting : UserControl
    {
        int percent = 0;
        System.Timers.Timer timer;
        delegate void setPercent();

        public Waiting()
        {
            this.InitializeComponent();
            timer = new System.Timers.Timer(400);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            timer.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            percent += 1;
            if (percent >= 100)
            {
                percent = 100;
                timer.Stop();
            }
            txtPercent.Dispatcher.Invoke(new setPercent(SetPecent), System.Windows.Threading.DispatcherPriority.Render, null);
            setDependancyObject(txtPercent, TextBox.TextProperty, percent + "%");
        }

        public void SetPecent()
        {
            txtPercent.Text = percent + "%";
        }

        delegate void setDPDelegate(UIElement ui, DependencyProperty dp, object value);

        void setDependancyObject(UIElement ui, DependencyProperty dp, object value)
        {
            if (ui.Dispatcher.CheckAccess())
                ui.SetValue(dp, value);
            else
                ui.Dispatcher.Invoke(new setDPDelegate(setDependancyObject), ui, dp, value);
        }

    }
}