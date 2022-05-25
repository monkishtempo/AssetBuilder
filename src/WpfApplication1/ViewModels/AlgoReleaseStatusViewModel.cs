using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using AssetBuilder.Controls.Custom;
using AssetBuilder.Extensions;
using AssetBuilder.Models;

namespace AssetBuilder.ViewModels
{
    public class AlgoReleaseStatusViewModel : ViewModelBase
    {
        #region Fields
        
        private readonly string[] _stepDisplayOrder =
        {
            "Authoring",
            "Training",
            "Test",
            "Health"
        };

        private int _algoId;

        private ObservableCollection<int> _deployedStages;

        private ObservableCollection<Step> _steps = new ObservableCollection<Step>();

        private DataView _deploymentRowView;

        #endregion

        #region Public Interface - Used for Binding

        /// <summary>
        /// The Id of the currently selected Algo instance
        /// </summary>
        public int Id
        {
            get => _algoId;
            set
            {
                _algoId = value;
                base.OnPropertyChanged(nameof(Id));
            }
        }

        /// <summary>
        /// The name of the currently selected Algo instance
        /// </summary>
        public string Name => PublishStatus.Any() ? PublishStatus.First().AlgoName : string.Empty;

        /// <summary>
        /// The version of the currently selected Algo instance
        /// </summary>
        public string Version => PublishStatus.Any() ? PublishStatus.First().Version : string.Empty;
        
        /// <summary>
        /// A collection of Environment deployment status information.
        /// Each environment item contains the details of the release version in that environment. 
        /// </summary>
        public ObservableCollection<EnvironmentAlgoStatus> PublishStatus { get; set; } = new ObservableCollection<EnvironmentAlgoStatus>();

        /// <summary>
        /// Creates the initial definition of the available ordered steps from 'start' to 'finish'
        /// </summary>
        public ObservableCollection<Step> ClinicalReleaseStages // TODO: Disconnect/Avoid using 'Step' as it forces STA thread in tests
        {
            get => _steps;
            set => _steps = value;
        }

        /// <summary>
        /// Gets a 1 based index value for the maximum stage a release has been deployed to
        /// </summary>
        public int MaxDeployedStage
        {
            get
            {
                if (Version.Equals("Unknown", StringComparison.InvariantCultureIgnoreCase)) return 0;

                for (var i = _stepDisplayOrder.Length -1; i > 0; i--)
                {
                    var namedStep = PublishStatus.FirstOrDefault(x => x.EnvironmentName.Contains(_stepDisplayOrder[i]));
                    if (namedStep == null) continue;

                    if (namedStep.Version.Equals(Version, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return i + 1; // Result is 1 based as opposed to array being 0 based
                    }
                }

                return 1; // As always exists in the 'Authoring' environment
            }
        }

        /// <summary>
        /// Gets an array of integers (1 based) that define which stages the current 'Authoring' version has been deployed to
        /// </summary>
        public ObservableCollection<int> DeployedStages
        {
            get
            {
                if (_deployedStages == null)
                {
                    _deployedStages = new ObservableCollection<int>();
                    for (var i= 0; i < _stepDisplayOrder.Length; i++)
                    {
                        var namedStep = PublishStatus.FirstOrDefault(x => x.EnvironmentName.Contains(_stepDisplayOrder[i]));
                        if (namedStep == null) continue;

                        if (namedStep.Version.Equals(Version, StringComparison.InvariantCultureIgnoreCase))
                        {
                            _deployedStages.Add(i + 1);
                        }
                    }
                }

                return _deployedStages;
            }
        }

        /// <summary>
        /// Provides a 'flat' model for data binding, allowing for auto column generation at runtime
        /// </summary>
        public DataView DeploymentRowView
        {
            get
            {
                if (_deploymentRowView != null) return _deploymentRowView;

                var data = new XDocument();
                var root = new XElement("root");
                data.Add(root);
                var status = new XElement("status");

                status.SetAttributeValue("Id", Id);
                status.SetAttributeValue("Name", Name);
                foreach (var stage in _stepDisplayOrder)
                {
                    var matchingStage = PublishStatus.FirstOrDefault(x => x.EnvironmentName.Contains(stage));
                    var loadedInfo = "Unknown";
                    if (matchingStage != null)
                    {
                        loadedInfo = matchingStage.Loaded.HasValue ? $"{matchingStage.Version} loaded on {matchingStage.Loaded}" : $"{matchingStage.Version}";
                    }

                    status.SetAttributeValue(stage, matchingStage == null || matchingStage.Version.Equals("Unknown") ? "Not released" : loadedInfo);
                }

                root.Add(status);
                var dataSet = new DataSet();
                var reader = data.CreateReader();
                dataSet.ReadXml(reader);
                _deploymentRowView = new DataView(dataSet.Tables[0]);

                return _deploymentRowView;
            }
        }
        #endregion

        public AlgoReleaseStatusViewModel(int algoId)
        {
            _algoId = algoId;

            CreateDeploymentStages();
        }

        private void CreateDeploymentStages()
        {
            if (_steps == null || !_steps.Any())
            {
                _steps = new ObservableCollection<Step>();
                for (var i = 0; i < _stepDisplayOrder.Length; i++)
                {
                    var stepNum = (i + 1).ToString();
                    var step = new Step
                    {
                        StepContent = stepNum,
                        StepLabel = _stepDisplayOrder[i],
                        IsCompleted = IsStepComplete(_stepDisplayOrder[i]) // Depends on state of 'steps' at this point, but will set 'Authoring'
                    };
                    _steps.Add(step);
                }
            }
        }

        private bool IsStepComplete(string environment)
        {
            // For the 'Authoring' environment always return true
            if (environment.Equals(_stepDisplayOrder[0], StringComparison.InvariantCultureIgnoreCase)) return true;

            var matchingEnvironment = PublishStatus.FirstOrDefault(x => x.EnvironmentName.Contains(environment, StringComparison.InvariantCultureIgnoreCase));
            if (matchingEnvironment == null) return false;

            var version = matchingEnvironment.Version;
            return !version.Equals("Unknown") && version.Equals(Version, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void OnDispose()
        {
            PublishStatus.Clear();
            ClinicalReleaseStages.Clear();
        }
    }
}