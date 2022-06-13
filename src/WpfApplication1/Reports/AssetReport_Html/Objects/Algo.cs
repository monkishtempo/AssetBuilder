using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class ARAlgo : BaseXmlObject
    {
        public override int ID => AlgoID;
        public int AlgoID { get; set; }
        public string Algo_Name { get; set; }
        public string Word_Merge { get; set; }
        public string WM2 { get; set; }
        public string Algo_Name_Language { get; set; }
        public string Word_Merge_Language { get; set; }
        public string WM2_Language { get; set; }

        public ARAlgo(XElement data) : base(data) { Type = "Algo"; }
    }
}
