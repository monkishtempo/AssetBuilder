using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AssetBuilder.Reports
{
    public interface IHtmlReport<T>
    {
        string Content { get; set; }
        string Folder { get; set; }
        T rp { get; set; }
        void RunReport(T rp, string template);
        string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker, string prop, string template, Dictionary<string, string> ids);
        string GetTemplate(string template);

        event EventHandler<EventArgs> Started;
        event EventHandler<CompletedEventArgs> Completed;
        event EventHandler<UpdateProgressEventArgs> UpdateProgress;

        void OnStarted(EventArgs e);
        void OnCompleted(CompletedEventArgs e);
        void OnUpdateProgress(UpdateProgressEventArgs e);
    }

    public class UpdateProgressEventArgs : EventArgs
    {
        public int Percentage { get; set; }
    }

    public class CompletedEventArgs : EventArgs
    {
        public string Content { get; set; }
        public string UniqueID { get; set; }
    }
}
