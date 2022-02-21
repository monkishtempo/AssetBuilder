using AssetBuilder.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace AssetBuilder.Reports
{
    public abstract class ReportBase<T> : IHtmlReport<T>
    {
        public Dictionary<string, string> Templates;
        public T rp { get; set; }

        public string Content { get; set; }
        public string Folder { get; set; }

        public Dictionary<string, string> prms;
        public Window Owner;
        internal string pageid;
        //public Dictionary<string, string> ids;

        public event EventHandler<EventArgs> Started;
        public event EventHandler<CompletedEventArgs> Completed;
        public event EventHandler<UpdateProgressEventArgs> UpdateProgress;

        public string GetTemplate(string name)
        {
            //ids = new Dictionary<string, string>();
            if (Templates == null) Templates = new Dictionary<string, string>();
            if (!Templates.ContainsKey(name)) Templates.Add(name, File.Exists($"{Folder}\\{name}.html") ? File.ReadAllText($"{Folder}\\{name}.html") : ""); ;
            return Templates[name];
        }

        public void OnStarted(EventArgs e)
        {
            Started?.Invoke(this, e);
        }

        public void OnCompleted(CompletedEventArgs e)
        {
            Completed?.Invoke(this, e);
        }

        public void OnUpdateProgress(UpdateProgressEventArgs e)
        {
            UpdateProgress?.Invoke(this, e);
        }

        public string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker)
        {
            var ids = new Dictionary<string, string>();
            return Regex.Replace(s, "@(.*?)@", mi =>
            {
                var group = mi.Groups[1].Value.Split('.');
                var prop = group[0];
                var template = group.Length > 1 ? group[1] : null;
                if (prop.StartsWith("?"))
                {
                    var p = obj.GetType().GetProperty(prop.Substring(1));
                    var value = p?.GetValue(obj);
                    if (value is bool) { if ((bool)value) prop = template; else return ""; }
                    else if (value is string) { if (!string.IsNullOrWhiteSpace(value as string)) prop = template; else return ""; }
                    else if (value != null) prop = template;
                    else return "";
                }

                return Replace(s, obj, prms, worker, prop, template, ids);
            });
        }

        public bool TryDefaultReplace(string prop, string template, Dictionary<string, string> ids, out string result)
        {
            switch (prop)
            {
                case "Now":
                    result = DateTime.Now.ToString("MMM dd, yyyy HH:mm");
                    return true;
                case "User":
                    result = Environment.UserName;
                    return true;
                case "PageID":
                case "NewGuid":
                    if (template != null && ids.ContainsKey(template))
                    {
                        result = ids[template];
                    }
                    else
                    {
                        result = Guid.NewGuid().ToString();
                        if (prop == "PageID") pageid = result;
                        if (template != null) ids.Add(template, result);
                    }
                    return true;
            }
            result = null;
            return false;
        }

        public virtual string Replace(string s, object obj, Dictionary<string, string> prms, BackgroundWorker worker, string prop, string template, Dictionary<string, string> ids)
        {
            string result;
            if (TryDefaultReplace(prop, template, ids, out result)) return result;
            if (obj != null)
            {
                var p = obj.GetType().GetProperty(prop);
                var value = p?.GetValue(obj);
                if (p == null && value == null)
                {
                    if (value == null && prms?.ContainsKey(prop) == true) value = prms[prop];
                    else { template = prop; value = obj; }
                }
                if (template != null)
                {
                    template = GetTemplate(template);
                    if (value is System.Collections.IEnumerable)
                    {
                        var repl = "";
                        foreach (var item in ((System.Collections.IEnumerable)value))
                        {
                            repl += Replace(template, item, prms, worker);
                        }
                        return repl;
                    }
                    else return Replace(template, value, prms, worker);
                }
                var ret = value?.ToString() ?? "";
                if(ret.StartsWith("@") && ret.EndsWith("@"))
                {
                    return Replace(ret, obj, prms, worker);
                }
                return ret;
            }
            return "";
        }

        public virtual void RunReport(T rp, string template)
        {
            BackgroundWorker bw = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
            bw.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                CompletedEventArgs args;
                if (e.Error != null) args = new CompletedEventArgs() { Content = e.Error.ToString() };
                else args = new CompletedEventArgs() { Content = Content, UniqueID = pageid };
                OnCompleted(args);
            };
            bw.DoWork += delegate (object sender, DoWorkEventArgs e)
            {
                var worker = sender as BackgroundWorker;
                var report = "";
                report = Replace(template, rp, prms, worker);
                Content = report;
            };
            Progress p = new Progress(bw);
            bw.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                p.pbStatus.Value = e.ProgressPercentage;
            };
            p.Owner = Owner;
            p.ShowDialog();
        }
    }
}
