using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskCalc.classes
{
    class History
    {
        public double? Value { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public int ConclusionID { get; set; }
        public int NodeID { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {4}",
                QuestionID, AnswerID, ConclusionID, Value
            );
        }
    }
}
