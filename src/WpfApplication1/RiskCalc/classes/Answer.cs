using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskCalc.classes
{
    class Answer : Asset
    {
        public string Formula { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
    }
}
