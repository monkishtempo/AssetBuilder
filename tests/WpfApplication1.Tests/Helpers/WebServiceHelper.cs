using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AssetBuilder.Models;
using AssetBuilder.Services;

namespace Asset_Builder.Tests.Helpers
{
    [ExcludeFromCodeCoverage]
    public class WebServiceHelper : IWebService
    {
        private readonly List<EnvironmentUrl> _environments = new List<EnvironmentUrl>
        {
            new EnvironmentUrl{ BaseUrl = "https://clinical-authoring/", Name = "Clinical Authoring" },
            new EnvironmentUrl{ BaseUrl = "https://clinical-test/", Name = "Clinical Test" },
            new EnvironmentUrl{ BaseUrl = "https://clinical-training/", Name = "Clinical Training" },
            new EnvironmentUrl{ BaseUrl = "https://clinical-staging/", Name = "Clinical Staging" },
            new EnvironmentUrl{ BaseUrl = "https://clinical-release/", Name = "Clinical Release" },
            new EnvironmentUrl{ BaseUrl = "https://health/", Name = "Health" },
            new EnvironmentUrl{ BaseUrl = "https://aph-staging/", Name = "APH (Staging)" }
        };

        private List<AlgoLoadInformation> _environmentAlgos;

        public List<EnvironmentUrl> GetEnvironmentUrls(string baseUri)
        {
            return _environments;
        }

        public List<AlgoLoadInformation> GetAlgosForEnvironment(EnvironmentUrl environment)
        {
            _environmentAlgos = new List<AlgoLoadInformation>(5);
            
            for (var i = 1; i < 6; i++)
            {
                var loaded = new DateTime(2020, i, 25);
                var algoInfo = new AlgoLoadInformation {Id = i, Name = $"{environment.Name}_{i}", Loaded = loaded};
                _environmentAlgos.Add(algoInfo);
            }

            return _environmentAlgos;
        }

        public string GetAlgoStatus(string baseUri, int algoId)
        {
            if (string.IsNullOrWhiteSpace(baseUri)) return "Unknown";

            var clinicalEnv = baseUri.Replace("https://", "").Replace("clinical-", "").Replace("/", "");
            switch (clinicalEnv.ToLowerInvariant())
            {
                case "authoring":
                    return "123";
                case "test":
                    return "123";
                case "training":
                case "health":
                    return "111";
                case "staging":
                case "release":
                    return "99";
                default:
                    return "NA";
            }
        }
    }
}