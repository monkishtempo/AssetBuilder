using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Reports
{
    public class ContentReport<T> : ReportBase<T>
    {
        public static ContentReport<T> CreateReport(string folder)
        {
            var cr = new ContentReport<T> { Folder = folder };
            return cr;
        }
    }
}
