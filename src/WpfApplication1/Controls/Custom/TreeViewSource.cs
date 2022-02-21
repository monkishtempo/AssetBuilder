using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace XmlTreeView
{
    public class TreeViewSource
    {
        public string description { get; set; }
        public int id { get; set; }
        public Guid apId { get; set; }
        public string Category { get; set; }

        public bool IsSelected { get; set; }
        public ObservableCollection<TreeViewSource> Children { get; set; }

        public TreeViewSource(string key, string label)
        {
            Guid g;
            if (!Guid.TryParse(key, out g)) g = Guid.NewGuid();
            apId = g;
            Category = label;
            Children = new ObservableCollection<TreeViewSource>();
        }
    }
}
