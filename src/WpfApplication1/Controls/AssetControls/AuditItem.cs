using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Controls.AssetControls
{
    public class AuditItem
    {
        public string Type { get; set; }
        public string AllTypes { get; set; }
        internal DateTime Date { get; set; }
        internal string User { get; set; }

        public override string ToString()
        {
            return $"{(AllTypes ?? Type)}\n{Date.ToString("MMM dd, yyyy - HH:mm")}\n{User}";
        }
    }
}
