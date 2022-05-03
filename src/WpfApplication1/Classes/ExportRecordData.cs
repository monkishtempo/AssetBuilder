using System;
using System.ComponentModel;

namespace AssetBuilder.Classes
{
    public class ExportRecordData : INotifyPropertyChanged
    {
        private DateTime _exportDateTime;

        private string _jiraReferences;

        private string _reason;

        private string _algoReleaseNoteLink;

        private const string JiraPlaceholder = "Enter related JIRA Ticket number(s)";

        private const string ReasonPlaceholder = "Reason for the export?";

        public event PropertyChangedEventHandler PropertyChanged;

        public string JiraReferences
        {
            get => _jiraReferences;
            set
            {
                _jiraReferences = value;
                OnPropertyChanged(nameof(JiraReferences));
            }
        }

        public string Reason
        {
            get => _reason;
            set
            {
                _reason = value;
                OnPropertyChanged(nameof(Reason));
            }
        }

        public string AlgoReleaseNoteLink
        {
            get => _algoReleaseNoteLink;
            set
            {
                _algoReleaseNoteLink = value;
                OnPropertyChanged(nameof(AlgoReleaseNoteLink));
            }
        }

        public string ExportedBy { get; set; }

        public string ExportedAlgos { get; set; }

        public string SourceEnvironment { get; set; }

        public string TargetEnvironment { get; set; }
        
        public string ExportDateTimeDisplay => _exportDateTime.ToString("dd/MMM/yyyy h:mm:ss tt");

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(JiraReferences)
            && !JiraReferences.Equals(JiraPlaceholder, StringComparison.InvariantCultureIgnoreCase)
            && !string.IsNullOrWhiteSpace(ExportedBy)
            && !string.IsNullOrWhiteSpace(Reason)
            && !Reason.Equals(ReasonPlaceholder, StringComparison.InvariantCultureIgnoreCase);

        public ExportRecordData(string author)
        {
            AlgoReleaseNoteLink = string.Empty;
            ExportedBy = author.Trim();
            _exportDateTime = DateTime.Now;
        }

        public ExportRecordData(string jiraReferences, string exportedBy, string reason, string releaseNotes)
        {
            JiraReferences = jiraReferences;
            AlgoReleaseNoteLink = releaseNotes;
            Reason = reason;
            ExportedBy = exportedBy;
            
            _exportDateTime = DateTime.Now;
        }

        private void OnPropertyChanged(string info)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}