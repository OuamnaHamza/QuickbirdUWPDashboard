﻿namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Controls;
    using Util;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private readonly Frame _contentFrame;

        /// <summary>
        ///     This action must not be inlined, it is used by the messenger via a weak-reference, inlined it will GC prematurely.
        /// </summary>
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<string> _updateAction;

        private bool _isNavOpen = true;

        private object _selectedShellListViewModel;

        /// <summary>
        ///     Initialise the shell.
        /// </summary>
        /// <param name="contentFrame">The frame that should be used for navigations.</param>
        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;

            FirstUpdate();

            _updateAction = async s => await Update();

            Messenger.Instance.NewDeviceDetected.Subscribe(_updateAction);
            Messenger.Instance.TablesChanged.Subscribe(_updateAction);
        }

        public ObservableCollection<ShellListViewModel> ShellListViewModels { get; } =
            new ObservableCollection<ShellListViewModel>();

        public bool IsNavOpen
        {
            get { return _isNavOpen; }
            set
            {
                if (value == _isNavOpen) return;
                _isNavOpen = value;
                OnPropertyChanged();
            }
        }

        public object SelectedShellListViewModel
        {
            get { return _selectedShellListViewModel; }
            set
            {
                if (value == _selectedShellListViewModel) return;
                _selectedShellListViewModel = value;
                OnPropertyChanged();
            }
        }

        public void ListItemClickedOrChanged()
        {
            var item = SelectedShellListViewModel as ShellListViewModel;
            if (item == null)
            {
                _contentFrame.Navigate(typeof(AddCropCycleView));
            }
            else
            {
                _contentFrame.Navigate(typeof(CropView), item.CropViewModel);
            }
        }

        private async void FirstUpdate()
        {
            Debug.WriteLine("Running First Update");

            await Update();

            if (ShellListViewModels.Count > 0)
            {
                var item = ShellListViewModels[0].CropViewModel;
                _contentFrame.Navigate(typeof(CropView), item);
                SelectedShellListViewModel = item;
            }
            else
            {
                //TODO: this cause ssync on first connect. Tell manish to think of where to put it properly
                var updateErrors = await DatabaseHelper.Instance.GetUpdatesFromServerAsync();
                if (updateErrors?.Any() ?? false) Debug.WriteLine(updateErrors);

                var postErrors = await DatabaseHelper.Instance.PostUpdatesAsync();
                if (postErrors?.Any() ?? false) Debug.WriteLine(string.Join(",", postErrors));

                var postHistErrors = await DatabaseHelper.Instance.PostHistoryAsync();
                if (postHistErrors?.Any() ?? false) Debug.WriteLine(postHistErrors);

                _contentFrame.Navigate(typeof(AddCropCycleView));
            }
        }

        private async Task Update()
        {
            Debug.WriteLine("Shell update triggered");

            var cropCycles = await DatabaseHelper.Instance.GetDataTreeAsync();

            // Remove items that no longer exist.
            var validIds = cropCycles.Select(cc => cc.ID).ToList();
            var toRemove = ShellListViewModels.Where(s => !validIds.Contains(s.CropRunId));
            foreach (var invalidItem in toRemove)
            {
                ShellListViewModels.Remove(invalidItem);
            }

            // Add new items, update existing.
            foreach (var cropCycle in cropCycles)
            {
                var item = ShellListViewModels.FirstOrDefault(s => s.CropRunId == cropCycle.ID);
                if (null == item)
                {
                    ShellListViewModels.Add(new ShellListViewModel(cropCycle));
                }
                else
                {
                    item.Update(cropCycle);
                }
            }
        }

        public void ToggleNav()
        {
            Debug.WriteLine("Toggle Nav.");
            if (IsNavOpen)
            {
                Debug.WriteLine("Close.");
                IsNavOpen = false;
            }
            else
            {
                Debug.WriteLine("Open.");
                IsNavOpen = true;
            }
        }

        public void NavToGraphingView()
        {
            _contentFrame.Navigate(typeof(GraphingView));
        }

        public void NavToAddNewView()
        {
            _contentFrame.Navigate(typeof(AddCropCycleView));
        }

        public void NavToArchiveView()
        {
            _contentFrame.Navigate(typeof(ArchiveView));
        }
    }
}