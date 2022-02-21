using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder.Classes
{
    public class AlgoSelectionChangedEventArgs : EventArgs
    {
        public string Algos { get; set; }
        public string Assets { get; set; }
    }
}
