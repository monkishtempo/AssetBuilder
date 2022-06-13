using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class ARAnswer : BaseXmlObject
    {
        public override int ID => AnsID;
        public int AnsID { get; set; }
        public string ExpertStatement { get; set; }
        public string LayStatement { get; set; }
        public string Answer { get; set; }
        public string Explanation { get; set; }
        public string ExpertStatement_Language { get; set; }
        public string LayStatement_Language { get; set; }
        public string Answer_Language { get; set; }
        public string Explanation_Language { get; set; }

        public ARAnswer(XElement data) : base(data) { Type = "Answer"; }
    }
}
