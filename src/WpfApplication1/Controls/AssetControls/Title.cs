using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using AssetBuilder.Classes;

namespace AssetBuilder.AssetControls
{
    class Title : assetControl
    {
        static Title()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Title), new FrameworkPropertyMetadata(typeof(Title)));
        }

        public Title()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            usage.ConcatID = "concat(\"\", \"" + string.Join("\", '\"', \"", usage.AssetID.Split('\"')) + "\")";
        }

        public Title(XmlNode title) : base(title)
        {
            assetType = AssetType.Title;
            tableName = "TITLE";
            expert = title["Table"]["Title"];
        }

    }
}
