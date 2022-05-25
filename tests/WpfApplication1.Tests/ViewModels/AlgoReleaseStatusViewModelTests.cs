using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AssetBuilder.Models;
using AssetBuilder.ViewModels;
using Xunit;

namespace Asset_Builder.Tests.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class AlgoReleaseStatusViewModelTests
    {
        private const int AlgoId = 1;

        private AlgoReleaseStatusViewModel _model;

        private readonly string[] _expectedEnvironments =
        {
            "Authoring",
            "Training",
            "Test",
            "Health"
        };

        [StaFact]
        public void OnCreation_ClinicalTeamStagesAreCreated()
        {
            _model = new AlgoReleaseStatusViewModel(AlgoId);
            
            var actual = _model.ClinicalReleaseStages;

            Assert.NotNull(actual);
            Assert.Equal(4, actual.Count);
            for (var i = 0; i < actual.Count; i++)
            {
                Assert.True(actual[i].StepLabel.Equals(_expectedEnvironments[i], StringComparison.InvariantCultureIgnoreCase));
            }
        }

        [StaFact]
        public void Bindable_MaxDeployedStage_EqualsHighestEnvironmentWithSameVersion()
        {
            _model = new AlgoReleaseStatusViewModel(AlgoId);

            var item1 = new EnvironmentAlgoStatus {AlgoName = "Alpha", EnvironmentName = "Authoring", Version = "101"};
            var item2 = new EnvironmentAlgoStatus {AlgoName = "Beta", EnvironmentName = "Training", Version = "101"};
            var item3 = new EnvironmentAlgoStatus {AlgoName = "Gamma", EnvironmentName = "Test", Version = "100"};
            var publishStatus = new ObservableCollection<EnvironmentAlgoStatus>
            {
                item1,
                item2,
                item3
            };
            _model.PublishStatus = publishStatus;

            var actual = _model.MaxDeployedStage;

            Assert.Equal(2, actual);
        }

        [StaFact]
        public void Bindable_DeployedStages_ShowsAllEnvironmentsWithMatchingVersion()
        {
            _model = new AlgoReleaseStatusViewModel(AlgoId);

            var item1 = new EnvironmentAlgoStatus { AlgoName = "Alpha", EnvironmentName = "Authoring", Version = "101" };
            var item2 = new EnvironmentAlgoStatus { AlgoName = "Beta", EnvironmentName = "Training", Version = "101" };
            var item3 = new EnvironmentAlgoStatus { AlgoName = "Gamma", EnvironmentName = "Test", Version = "100" };
            var publishStatus = new ObservableCollection<EnvironmentAlgoStatus>
            {
                item1,
                item2,
                item3
            };
            _model.PublishStatus = publishStatus;

            var actual = _model.DeployedStages;

            Assert.Equal(2, actual.Count);
            Assert.Contains(1, actual);
            Assert.Contains(2, actual);
            Assert.DoesNotContain(3, actual);
        }

        [StaFact]
        public void Bindable_DeploymentRowView_ReturnsOrderedEnvironmentVersions()
        {
            _model = new AlgoReleaseStatusViewModel(AlgoId);
            var expectedColumns = new[] { "Id", "Name", "Authoring", "Training", "Test", "Health" };
            var expectedData = new [] {"1", "Alpha", "101", "100", "99", "Not released"};
            var item1 = new EnvironmentAlgoStatus { AlgoName = "Alpha", EnvironmentName = "Authoring", Version = "101" };
            var item2 = new EnvironmentAlgoStatus { AlgoName = "Beta", EnvironmentName = "Training", Version = "100" };
            var item3 = new EnvironmentAlgoStatus { AlgoName = "Gamma", EnvironmentName = "Test", Version = "99" };
            var publishStatus = new ObservableCollection<EnvironmentAlgoStatus>
            {
                item1,
                item2,
                item3
            };
            _model.PublishStatus = publishStatus;

            var actual = _model.DeploymentRowView;
            Assert.NotNull(actual);
            var table = actual.Table;
            Assert.Equal(1, table.Rows.Count);
            var row = table.Rows[0];
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var colName = table.Columns[i].ColumnName;
                var colValue = row[i].ToString();

                Assert.Equal(expectedColumns[i], colName);
                Assert.Equal(expectedData[i], colValue);
            }
        }
    }
}