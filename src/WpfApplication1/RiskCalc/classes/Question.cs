using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskCalc.classes
{
    class Question : Asset
    {
        public List<Answer> Answers = new List<Answer>();
        public List<Conclusion> Conclusions = new List<Conclusion>();
        public List<Question> Questions = new List<Question>();

        public string Category { get; set; }
        public string Subcategory { get; set; }

        public RiskType Risk { get; set; }
    }

    enum RiskType
    {
        NoRisk,
        CurrentRisk,
        ReducedRisk,
        MinimumRisk
    }
}
