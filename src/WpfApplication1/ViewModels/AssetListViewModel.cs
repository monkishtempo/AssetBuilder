using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AssetBuilder.ViewModels
{
    public class AssetListViewModel : ViewModelBase
    {
        #region View Model Public Interface

        public ObservableCollection<ListItem> AllAssets { get; private set; }

        // Selected Items

        // Selected Value

        #endregion

        public AssetListViewModel()
        {
            AllAssets = new ObservableCollection<ListItem>();
            AllAssets.CollectionChanged += OnCollectionChanged;
        }

        protected override void OnDispose()
        {
            AllAssets.Clear();
            AllAssets.CollectionChanged -= OnCollectionChanged;
        }

        #region Event Handlers

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (ListItem assetItem in e.NewItems)
                {
                    assetItem.PropertyChanged += OnAssetItemPropertyChanged;
                }
            }

            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                foreach (ListItem assetItem in e.OldItems)
                {
                    assetItem.PropertyChanged -= OnAssetItemPropertyChanged;
                }
            }
        }

        void OnAssetItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var IsSelected = "IsSelected";

            if (e.PropertyName == IsSelected)
            {
                // Any linked/Derived Asset property changes e.g. TotalItems ?
                // OnPropertyChanged(<linkedProperty>);
            }
        }

        #endregion
    }
}