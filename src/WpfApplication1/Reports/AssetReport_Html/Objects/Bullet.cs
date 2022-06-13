using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AssetBuilder.Reports
{
    public class ARBullet : BaseXmlObject
    {
        public override int ID => BPID;
        public int BPID { get; set; }
        public string Bullet { get; set; }
        public string Bullet_Language { get; set; }

        public ARBullet(XElement data) : base(data) { Type = "Bullet"; }
    }
}
