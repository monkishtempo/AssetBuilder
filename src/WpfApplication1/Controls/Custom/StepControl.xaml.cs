using AssetBuilder.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.Controls.Custom
{
    /// <summary>
    /// Interaction logic for StepControl.xaml
    /// </summary>
    public partial class StepControl : UserControl
    {
        #region Binding
        public static readonly DependencyProperty DeploymentStepsProperty =
            DependencyProperty.Register(
                "DeploymentSteps",
                typeof(ObservableCollection<Step>),
                typeof(StepControl),
                typeMetadata: new FrameworkPropertyMetadata(
                    default(ObservableCollection<Step>),
                    propertyChangedCallback: OnDeploymentStepsChanged)
            );

        public static readonly DependencyProperty MaxDeployedStageProperty =
            DependencyProperty.Register(
                "MaxDeployedStage",
                typeof(int),
                typeof(StepControl),
                typeMetadata: new FrameworkPropertyMetadata(
                    default(int))
            );

        public static readonly DependencyProperty DeployedStagesProperty =
            DependencyProperty.Register(
                "DeployedStages",
                typeof(ObservableCollection<int>),
                typeof(StepControl),
                typeMetadata: new FrameworkPropertyMetadata(
                    default(ObservableCollection<int>),
                    propertyChangedCallback: OnDeployedStepsChanged)
            );

        public ObservableCollection<Step> DeploymentSteps
        {
            get => (ObservableCollection<Step>)GetValue(DeploymentStepsProperty);
            set => SetValue(DeploymentStepsProperty, value);
        }

        public int MaxDeployedStage
        {
            get => (int) GetValue(MaxDeployedStageProperty);
            set => SetValue(MaxDeployedStageProperty, value);
        }

        public ObservableCollection<int> DeployedStages
        {
            get => (ObservableCollection<int>) GetValue(DeployedStagesProperty);
            set => SetValue(DeployedStagesProperty, value);
        }
        #endregion

        public StepControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<Step> Steps
        {
            get
            {
                var mySteps = new ObservableCollection<Step>();
                foreach (var child in StepGrid.Children)
                {
                    if (child is Step)
                    {
                        mySteps.Add((Step)child);
                    }
                }

                return mySteps;
            }
            set
            {
                StepGrid.Children.Clear();

                if (value == null)
                {
                    var singleStep = new ObservableCollection<Step>();
                    var step = new Step
                    {
                        StepContent = "1",
                        StepLabel = "Step 1",
                    };
                    singleStep.Add(step);
                    value = singleStep;
                }

                var i = 1;
                foreach (var step in value)
                {
                    var stepsCount = StepGrid.ColumnDefinitions.Count;
                    var cd = new ColumnDefinition
                    {
                        Width = new GridLength(50, GridUnitType.Auto)
                    };
                    StepGrid.ColumnDefinitions.Add(cd);

                    step.Name = "Step" + i;
                    step.SetValue(Grid.ColumnProperty, stepsCount);

                    if (i == 1)
                    {
                        step.StepExtenderVisibility = Visibility.Collapsed;
                        step.IsCompleted = true;
                    }

                    step.IsSelected = false;

                    StepGrid.Children.Add(step);
                    i++;
                }
            }
        }

        public void SelectSteps(int selected)
        {
            if (selected < 0 || selected > Steps.Count) return;

            foreach (var step in Steps)
            {
                var stepPosition = int.Parse(step.Name.Replace("Step", ""));

                if (stepPosition < selected)
                {
                    step.IsSelected = false;
                    step.IsCompleted = true;
                }

                if (stepPosition == selected)
                {
                    if (step.IsCompleted || step.IsSelected)
                    {
                        step.IsSelected = true;
                        step.IsCompleted = false;
                    }

                    if (!step.IsSelected && !step.IsCompleted)
                    {
                        step.IsSelected = true;
                        step.IsCompleted = true;
                    }
                }

                if (stepPosition > selected)
                {
                    step.IsSelected = false;
                    step.IsCompleted = false;
                }
            }
        }

        public void SelectSteps(ObservableCollection<int> selected)
        {
            if (!selected.Any()) return;

            Step previousStep = null;
            foreach (var step in Steps)
            {
                var stepPosition = int.Parse(step.Name.Replace("Step", ""));
                if (selected.Contains(stepPosition))
                {
                    if (step.IsCompleted || step.IsSelected)
                    {
                        step.IsSelected = true;
                        step.IsCompleted = false;
                    }

                    if (!step.IsSelected && !step.IsCompleted)
                    {
                        step.IsSelected = true;
                        step.IsCompleted = true;
                    }

                    if (previousStep != null && !previousStep.IsCompleted)
                    {
                        step.StepConnectorVisibility = Visibility.Collapsed;
                    }
                }

                previousStep = step;
            }
        }

        private void SetEnvironmentStatus(string stepName, bool isComplete)
        {
            var step = Steps.FirstOrDefault(x => x.StepLabel.Contains(stepName, StringComparison.InvariantCultureIgnoreCase));
            if (step != null)
            {
                step.IsCompleted = isComplete;
            }
        }

        private static void OnDeploymentStepsChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var control = (StepControl)depObj;
            if (e.NewValue is ObservableCollection<Step> newSteps)
            {
                control.Steps = newSteps;
            }

            control.SelectSteps(control.MaxDeployedStage);
        }

        private static void OnDeployedStepsChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var control = (StepControl)depObj;
            if (e.NewValue is ObservableCollection<Step> newSteps)
            {
                control.Steps = newSteps;
            }

            control.SelectSteps(control.DeployedStages);
        }
    }
}
