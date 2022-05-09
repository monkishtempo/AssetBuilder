using System.ComponentModel;
using AssetBuilder.Controls.AssetControls;

namespace AssetBuilder.ViewModels
{
    public class ListItem : INotifyPropertyChanged
    {
        private int _categoryTypeId;

        private AuditItem _audit;

        private bool _hidden;

        private bool _isSelected;

        #region Properties
        public string ToolTip => Audit == null ? ID.ToString() : $"{ID}\n{Audit}";

        public int ID { get; set; }

        public int NotInUse { get; set; }

        public bool IsFlagged => Flags > 0;

        public string IconImage { get; set; } = "/images/flag.png";

        public int Flags
        {
            get
            {
                if (qcat.AssetFlags == null) return 0;
                var typeid = Window1.window.qcat1.AssetTypeId;
                var flags = qcat.AssetFlags.ContainsKey(typeid) ? (qcat.AssetFlags[typeid].ContainsKey(ID) ? qcat.AssetFlags[typeid][ID] : 0) : 0;
                if (flags.In(1, 2, 3))
                {
                    IconImage = $"/images/{flags}.png";
                }
                return flags;
            }
        }
        
        public int CategoryTypeID
        {
            get => _categoryTypeId;
            set
            {
                _categoryTypeId = value;

                NotifyPropertyChanged("CategoryTypeID");
            }
        }
        
        public AuditItem Audit
        {
            get => _audit;
            set
            {
                _audit = value;
                NotifyPropertyChanged("Audit");
                NotifyPropertyChanged("ToolTip");
            }
        }

        public string MultID { get; set; }

        public string Value { get; set; }

        public int Priority { get; set; }

        public string _Language;

        public string Language
        {
            get => _Language;
            set
            {
                if (_Language != value)
                {
                    if (!string.IsNullOrEmpty(_Language))
                    {
                        _Language = "";
                        NotifyPropertyChanged("HasLanguage");
                    }

                    _Language = value;
                    NotifyPropertyChanged("HasLanguage");
                }
            }
        }

        public bool HasLanguage => !string.IsNullOrEmpty(Language);
        
        public bool Hidden
        {
            get => _hidden;
            set { _hidden = value; NotifyPropertyChanged("Hidden"); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value == _isSelected) return;

                _isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected));
            }
        }

        /// <summary>
        /// Bound property used to display the Asset.
        /// Bound to in App.xaml -> TemplateListBoxItem -> TextBlock
        /// </summary>
        public object Content => Value;

        public override string ToString()
        {
            return Value;
        }

        public string ToCopyString()
        {
            return ID + "\t" + Value;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}