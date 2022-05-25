using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using AssetBuilder.Models;
using AssetBuilder.Services;

namespace AssetBuilder.ViewModels
{
    public class AlgoReleaseStatusCollectionViewModel : ViewModelBase
    {
        private readonly AlgoStatusRepository _repository;

        private bool _disposedValue;

        #region Public Interface

        public ObservableCollection<AlgoReleaseStatusViewModel> AllAlgosStatus { get; private set; }

        /// <summary>
        /// Retrieve the status information for the selected Algo IDs
        /// </summary>
        /// <param name="algoIds">Comma separated string containing one or more Algo IDs</param>
        /// <returns>Populates the Observable collection of Algo release status data</returns>
        public void Populate(string algoIds)
        {
            var selectedAlgos = SelectedAlgoIds(algoIds).ToArray();
            if (!selectedAlgos.Any()) return;

            DisplayName = $"Status of: {algoIds}";

            var currentAlgos = AllAlgosStatus.Select(x => x.Id).ToArray();
            var algosNeeded = selectedAlgos.Except(currentAlgos);
            var algosNotNeeded = currentAlgos.Except(selectedAlgos);

            // Remove from collection those not in the list
            foreach (var id in algosNotNeeded)
            {
                var removeItem = AllAlgosStatus.First(x => x.Id == id);
                AllAlgosStatus.Remove(removeItem);
            }
            // Add those not already in the collection to the collection
            // TODO: Set a limit on the number of algos to retrieve due to the current memory overhead
            foreach (var algoId in algosNeeded)
            {
                var status = new AlgoReleaseStatusViewModel(algoId);
                var envStatus = _repository.GetStatusForAlgo(algoId);
                status.PublishStatus = new ObservableCollection<EnvironmentAlgoStatus>(envStatus);
                AllAlgosStatus.Add(status);
            }
        }

        #endregion
        
        public AlgoReleaseStatusCollectionViewModel()
        {
            _repository = new AlgoStatusRepository(new AlgoStatusWebService());
            Initialise();
        }

        private void Initialise()
        {
            AllAlgosStatus = new ObservableCollection<AlgoReleaseStatusViewModel>();
            AllAlgosStatus.CollectionChanged += OnCollectionChanged;
        }

        #region Base Class overrides

        protected override void OnDispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _repository.Dispose();
                    foreach (var algoStatus in AllAlgosStatus)
                    {
                        algoStatus.Dispose();
                    }

                    AllAlgosStatus.Clear();
                }

                _disposedValue = true;
            }
        }

        #endregion

        #region Event Handlers

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Subscribe/Unsubscribe from Property Changed events of changed AlgoStatusViewModel objects in e.NewItems/e.OldItems
        }
        #endregion

        public IEnumerable<int> SelectedAlgoIds(string algoIds)
        {
            var currentIds = algoIds.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var selectedAlgoIds = new List<int>();
            foreach (var id in currentIds)
            {
                var result = int.TryParse(id.Trim(), out var algoId);
                if (!result) continue;

                if (!selectedAlgoIds.Contains(algoId))
                {
                    selectedAlgoIds.Add(algoId);
                }
            }

            return selectedAlgoIds;
        }
    }
}