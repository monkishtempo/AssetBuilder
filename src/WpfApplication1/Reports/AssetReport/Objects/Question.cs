using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class ARQuestion : BaseXmlObject
    {
        public override int ID => QuestionID;
        public int QuestionID { get; set; }
        public string ExpertStatement { get; set; }
        public string LayStatement { get; set; }
        public string Question { get; set; }
        public string Explanation { get; set; }
        public string ExpertStatement_Language { get; set; }
        public string LayStatement_Language { get; set; }
        public string Question_Language { get; set; }
        public string Explanation_Language { get; set; }

        public ARQuestion(XElement data) : base(data) { Type = "Question"; }
    }
}
