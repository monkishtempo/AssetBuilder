using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class ARConclusion : BaseXmlObject
    {
        public override int ID => RecID;
        public int RecID { get; set; }
        public string ExpertStatement { get; set; }
        public string LayStatement { get; set; }
        public string Explanation { get; set; }
        public string MoreDetail { get; set; }
        public string ExpertStatement_Language { get; set; }
        public string LayStatement_Language { get; set; }
        public string Explanation_Language { get; set; }
        public string MoreDetail_Language { get; set; }

        public ARConclusion(XElement data) : base(data) { Type = "Conclusion"; }
    }
}
