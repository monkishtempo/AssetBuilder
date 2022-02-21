using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AssetBuilder.AssetControls
{
    class Group : assetControl
    {
        DataGrid GroupMembers;
        DockPanel eligibility;

        static Group()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Group), new FrameworkPropertyMetadata(typeof(Group)));
        }

        int[] widths = { 32, 12, 12, 12, 32 };

        public bool IsAllergy
        {
            get { return (int.Parse(asset["Table"]["EligibilityMask"].InnerText) & 1) == 1; }
            set { }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            eligibility = GetTemplateChild("eligibility") as DockPanel;
            eligibility.DataContext = this;
            GroupMembers = GetTemplateChild("GroupMembers") as DataGrid;
            GroupMembers.Loaded += delegate(object sender, RoutedEventArgs e)
            {
                for (int i = 0; i < widths.Length && i < GroupMembers.Columns.Count; i++)
                {
                    GroupMembers.Columns[i].Width = new DataGridLength(widths[i], DataGridLengthUnitType.Star);
                }
            };
        }

        public Group()
        {

        }

        public Group(XmlNode group)
            : base(group)
        {
            assetType = AssetType.Group;
            tableName = "GROUPS";
            expert = group["Table"]["GroupName"];
            cats.Add(0, "DataID");
            cats.Add(1, "CatID");
            cats.Add(2, "SubCatID");
            cats.Add(3, "Cat2ID");
        }
    }
}

namespace AssetBuilder.Controls
{
    class GroupMember : INotifyPropertyChanged
    {
        public Guid ID { get; set; }
        public string Category { get; set; }
        public string CodeSystem { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
