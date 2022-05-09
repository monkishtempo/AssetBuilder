using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using AssetBuilder.Classes;

namespace AssetBuilder.AssetControls
{
    class Map : assetControl
    {
        static Map()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Map), new FrameworkPropertyMetadata(typeof(Map)));
        }

        public Map()
        {

        }

        public Map(XmlNode map) : base(map)
        {
            assetType = AssetType.Map;
            tableName = "MAP";
            expert = map["Table"]["KeyName"];
            cats.Add(0, "DataID");
            cats.Add(1, "CatID");
            cats.Add(2, "SubCatID");
            cats.Add(3, "Cat2ID");
        }
    }
}
