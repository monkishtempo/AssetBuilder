using System.Collections.Generic;
using AssetBuilder.Models;

namespace AssetBuilder.Services
{
    public interface IWebService
    {
        List<EnvironmentUrl> GetEnvironmentUrls(string baseUri);

        List<AlgoLoadInformation> GetAlgosForEnvironment(EnvironmentUrl environment);

        string GetAlgoStatus(string baseUri, int algoId);
    }
}