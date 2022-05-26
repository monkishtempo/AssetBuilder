using System;

namespace AssetBuilder.Models
{
    public class EnvironmentAlgoStatus
    {
        public string AlgoName { get; set; } = "";

        public string EnvironmentName { get; set; } = "";

        public DateTime? Loaded { get; set; }

        public string Version { get; set; } = "Unknown";
    }
}