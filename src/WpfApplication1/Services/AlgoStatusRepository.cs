using System;
using System.Collections.Generic;
using System.Linq;
using AssetBuilder.Extensions;
using AssetBuilder.Models;

namespace AssetBuilder.Services
{
    public class AlgoStatusRepository : IDisposable
    {
        private const string ClinicalBaseUri = @"https://apps.expert-24.com";

        private readonly IWebService _webService;

        private readonly ICacheService<object> _cache;

        private List<EnvironmentUrl> _environments;

        private bool _disposedValue;
        
        // TODO: Get caller to tell you which environment names it requires

        private IEnumerable<EnvironmentUrl> Environments => _environments ?? (_environments = GetEnvironmentList());

        public AlgoStatusRepository(IWebService webService)
        {
            _webService = webService;

            _cache = new MemoryCacheService<object>();
        }

        public List<EnvironmentAlgoStatus> GetStatusForAlgo(int algoId)
        {
            var results = new List<EnvironmentAlgoStatus>();
            // You have to be careful here to use Equals or Contains correctly:
            foreach (var environment in Environments.Where(x =>
                         x.Name.Equals("Authoring", StringComparison.CurrentCultureIgnoreCase) ||
                         x.Name.Contains("Clinical", StringComparison.CurrentCultureIgnoreCase)
                         || x.Name.Equals("Health", StringComparison.CurrentCultureIgnoreCase)))
            {
                var environmentStatus = GetReleaseStatus(algoId, environment);
                results.Add(environmentStatus);
            }

            return results;
        }

        private List<EnvironmentUrl> GetEnvironmentList()
        {
            // TODO: Can use Settings.Default.WebService
            return (List<EnvironmentUrl>)_cache.GetOrCreate("Environments", () => _webService.GetEnvironmentUrls(ClinicalBaseUri)); 
        }

        private EnvironmentAlgoStatus GetReleaseStatus(int algoId, EnvironmentUrl environment)
        {
            var result = new EnvironmentAlgoStatus();
            try
            {
                var currentEnvironmentAlgoInformation = (List<AlgoLoadInformation>)_cache.GetOrCreate(environment.Name, () => _webService.GetAlgosForEnvironment(environment));
                var algoInEnvironment = currentEnvironmentAlgoInformation.FirstOrDefault(x => x.Id == algoId);

                if (algoInEnvironment != null)
                {
                    result.AlgoName = algoInEnvironment.Name;
                    result.Loaded = algoInEnvironment.Loaded;
                }

                result.EnvironmentName = environment.Name;
                result.Version = _webService.GetAlgoStatus(environment.BaseUrl, algoId);
            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cache.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}