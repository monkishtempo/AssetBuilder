using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AssetBuilder.Controls.Custom
{
    /// <summary>
    /// Interaction logic for Step.xaml
    /// </summary>
    public partial class Step : UserControl
    {
        private bool _clickable;

        public Step()
        {
            InitializeComponent();
        }

        public SolidColorBrush StepFill
        {
            get => (SolidColorBrush)myStepFill.Background;
            set
            {
                if (value == null) value = new SolidColorBrush(Colors.Green);
                myStepFill.Background = value;
            }
        }

        public SolidColorBrush StepBackground
        {
            get => (SolidColorBrush)myStepBackground.Background;
            set
            {
                if (value == null) value = new SolidColorBrush(Colors.LightGray);
                myStepBackground.Background = value;
            }
        }

        public SolidColorBrush StepContentForeground
        {
            get => (SolidColorBrush)myStepContent.Foreground;
            set
            {
                if (value == null) value = new SolidColorBrush(Colors.Gray);
                myStepContent.Foreground = value;
            }
        }

        public SolidColorBrush StepLabelForeground
        {
            get => (SolidColorBrush)myStepLabel.Foreground;
            set
            {
                if (value == null) value = new SolidColorBrush(Colors.Gray);
                myStepLabel.Foreground = value;
            }
        }

        public double StepLabelFontSize
        {
            get => myStepLabel.FontSize;
            set => myStepLabel.FontSize = value;
        }

        public string StepLabel
        {
            get => myStepLabel.Text;
            set
            {
                if (value == null) value = "Step 1";
                myStepLabel.Text = value;
            }
        }

        public double StepContentFontSize
        {
            get => myStepContent.FontSize;
            set => myStepContent.FontSize = value;
        }

        public string StepContent
        {
            get => myStepContent.Text;
            set
            {
                if (value == null) value = "1";
                myStepContent.Text = value;
            }
        }

        public bool CanClick
        {
            get => _clickable;
            set => _clickable = value;
        }

        public bool IsSelected
        {
            get => myStepFill.Visibility == Visibility.Visible && target.Visibility == Visibility.Visible;
            set
            {
                myStepFill.Visibility = Visibility.Collapsed;
                target.Visibility = Visibility.Collapsed;
                check.Visibility = Visibility.Collapsed;
            }
        }

        public bool IsCompleted
        {
            get => myStepFill.Visibility == Visibility.Visible && check.Visibility == Visibility.Visible;
            set
            {
                if (value && IsSelected == false)
                {
                    myStepFill.Visibility = Visibility.Visible;
                    target.Visibility = Visibility.Collapsed;
                    check.Visibility = Visibility.Visible;
                }
            }
        }

        public Visibility StepConnectorVisibility
        {
            get => myStepConnector.Visibility;
            set => myStepConnector.Visibility = value;
        }

        public Visibility StepExtenderVisibility
        {
            get => myStepExtender.Visibility;
            set => myStepExtender.Visibility = value;
        }

        public Visibility StepLabelVisibility
        {
            get => myStepLabel.Visibility;
            set => myStepLabel.Visibility = value;
        }

        public Visibility StepContentVisibility
        {
            get => myStepContent.Visibility;
            set => myStepContent.Visibility = value;
        }
    }
}
