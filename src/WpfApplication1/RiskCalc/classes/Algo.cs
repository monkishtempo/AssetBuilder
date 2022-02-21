using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskCalc.classes
{
    class Algo : Asset
    {
        public List<Question> Questions = new List<Question>();
        public List<Conclusion> Conclusions = new List<Conclusion>();
        public List<Transfer> Transfers = new List<Transfer>();
        public List<Stop> Stops = new List<Stop>();

        public List<Asset> Nodes;

        public double ScalingFactor { get; set; }
        public string OffsetFormula { get; set; }

        public void PopulateNodes()
        {
            Nodes = new List<Asset>();
            Nodes.AddRange(Questions);
            Nodes.AddRange(Conclusions);
            Nodes.AddRange(Transfers);
            Nodes.AddRange(Stops);
        }
    }
}
