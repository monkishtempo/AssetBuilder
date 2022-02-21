using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskCalc.classes
{
    class Asset
    {
        public int AlgoID { get; set; }
        public int NodeID { get; set; }
        public int NextNodeID { get; set; }
        public int NodeTypeID { get; set; }
        public double Counter { get; set; }
        public Algo Algo { get; set; }

        public int ID { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return ID + ") " + Text;
        }

        public virtual Asset GetNextNode()
        {
            var node = Algo.Nodes.Where(n => n.NodeID == NextNodeID);
            if (node.Any()) return node.First();
            return null;
        }
    }
}
