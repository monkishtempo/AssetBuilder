using System.Windows;
using System.Xml;
using AssetBuilder.Classes;

namespace AssetBuilder.AssetControls
{
    public class Bullet : assetControl
    {
        static Bullet()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Bullet), new FrameworkPropertyMetadata(typeof(Bullet)));
        }

        public Bullet()
        {
        }

        public Bullet(XmlNode bullet) : base(bullet)
        {
            assetType = AssetType.Bullet;
            tableName = "BULLET";
            expert = bullet["Table"]["BP_TEXT"];
            cats.Add(0, "AGEID");
            cats.Add(3, "BodyID");
        }
    }
}
