using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Asset_Builder.Tests.Helpers;
using AssetBuilder.Models;
using AssetBuilder.Services;
using Xunit;

namespace Asset_Builder.Tests.Services
{
    [ExcludeFromCodeCoverage]
    public class AlgoStatusRepositoryTests
    {
        private AlgoStatusRepository _repository;

        private const int AlgoId = 3;

        [Fact]
        public void GetDataForTestAlgo_HasExpectedReleaseData()
        {
            var expectedReleaseData = new List<EnvironmentAlgoStatus>
            {
                new EnvironmentAlgoStatus{ AlgoName = "Clinical Authoring_3", EnvironmentName = "Clinical Authoring", Loaded = new DateTime(2020, AlgoId, 25), Version = "123"},
                new EnvironmentAlgoStatus{ AlgoName = "Clinical Test_3", EnvironmentName = "Clinical Test", Loaded = new DateTime(2020, AlgoId, 25), Version = "123"},
                new EnvironmentAlgoStatus{ AlgoName = "Clinical Training_3", EnvironmentName = "Clinical Training", Loaded = new DateTime(2020, AlgoId, 25), Version = "111"},
                new EnvironmentAlgoStatus{ AlgoName = "Clinical Staging_3", EnvironmentName = "Clinical Staging", Loaded = new DateTime(2020, AlgoId, 25), Version = "99"},
                new EnvironmentAlgoStatus{ AlgoName = "Clinical Release_3", EnvironmentName = "Clinical Release", Loaded = new DateTime(2020, AlgoId, 25), Version = "99"},
                new EnvironmentAlgoStatus{ AlgoName = "Health_3", EnvironmentName = "Health", Loaded = new DateTime(2020, AlgoId, 25), Version = "111"},
            };
            _repository = new AlgoStatusRepository(new WebServiceHelper());
            
            var actual = _repository.GetStatusForAlgo(AlgoId);

            Assert.NotNull(actual);
            Assert.True(actual.Count == 6);
            Assert.True(IsMatch(expectedReleaseData, actual));
        }

        private static bool IsMatch(IReadOnlyList<EnvironmentAlgoStatus> expected, IReadOnlyList<EnvironmentAlgoStatus> actual)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                var itemResult =
                    actual[i].AlgoName.Equals(expected[i].AlgoName) &&
                    actual[i].EnvironmentName.Equals(expected[i].EnvironmentName) &&
                    actual[i].Version.Equals(expected[i].Version) &&
                    actual[i].Loaded.Equals(expected[i].Loaded);
                if (!itemResult) return false;
            }

            return true;
        }
    }
}