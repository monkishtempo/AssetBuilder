using RiskCalc.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiskCalc
{
    [Serializable]
    public class RiskHistory
    {
        public Dictionary<int, Dictionary<string, List<ScenarioHistory>>> MyHistory { get; set; }

        public RiskHistory()
        {
            MyHistory = new Dictionary<int, Dictionary<string, List<ScenarioHistory>>>();
        }
    }

    [Serializable]
    public class ScenarioHistory
    {
        public ControlType Type { get; set; }
        public int ID { get; set; }
        public int AnswerID { get; set; }
        public string Text { get; set; }
    }
}
