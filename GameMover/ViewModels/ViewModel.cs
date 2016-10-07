﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using GameMover.Model;

using Microsoft.Win32;

using Prism.Commands;
using Prism.Mvvm;

namespace GameMover.ViewModels
{

    public class ViewModel : BindableBase
    {

        /// Allows you to set the selection without trigger selection change (so that when saving a control you don't reload)
        private bool _ignorePathsSelectionChange;

        public FolderCollection InstallCollection { get; private set; }
        public FolderCollection StorageCollection { get; private set; }

        public ObservableCollection<string> SavedPaths { get; } = new ObservableCollection<string>();



        public string SelectedPath
        {
            get { return _selectedPath; }
            set {
                if (_selectedPath == value) return;

                _selectedPath = value;

                if (_ignorePathsSelectionChange)
                {
                    _ignorePathsSelectionChange = false;
                    return;
                }

                string[] paths = _selectedPath?.Split(new[] {ARROWED_PATH_SEPARATOR}, StringSplitOptions.RemoveEmptyEntries);

                if (paths?.Length != 2) throw new Exception($"Unable to parse selected path \"{_selectedPath}\".");

                InstallCollection.Location = paths[0];
                StorageCollection.Location = paths[1];
            }
        }

        private string _selectedPath;

        private const string ARROWED_PATH_SEPARATOR = " -> ";

        public void Initialize()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var installSteamCommon = regKey == null
                                         ? @"C:"
                                         : regKey.GetValue("SteamPath").ToString().Replace(@"/", @"\") + @"\steamapps\common";


            InstallCollection = new FolderCollection {
                FolderBrowserDefaultLocation = installSteamCommon
            };
            StorageCollection = new FolderCollection {
                FolderBrowserDefaultLocation = @"E:\Steam\SteamApps\common"
            };

            InstallCollection.CorrespondingCollection = StorageCollection;
            StorageCollection.CorrespondingCollection = InstallCollection;

            InstallCollection.PropertyChanged += OnFolderCollectionChange;
            StorageCollection.PropertyChanged += OnFolderCollectionChange;


            SavedPaths.Add(InstallCollection.FolderBrowserDefaultLocation + ARROWED_PATH_SEPARATOR +
                           StorageCollection.FolderBrowserDefaultLocation);
            SavedPaths.Add(@"C:\Users\Nick\Desktop\folder a -> C:\Users\Nick\Desktop\folder b");
        }

        private void OnFolderCollectionChange(object sender, PropertyChangedEventArgs args)
        {
            // When a new folder location is chosen, check if it is already saved and if so display select it so that it can be displayed in the combo box
            if (args.PropertyName.Equals(nameof(FolderCollection.Location)))
            {
                SilentlySetSavedPath(SavedPaths.FirstOrDefault(
                    s => s.Equals(GetCurrentLocationsString(), StringComparison.OrdinalIgnoreCase)));
            }
        }



        [AutoLazy.Lazy]
        public DelegateCommand SaveCurrentLocationCommand => new DelegateCommand(() => {
            string arrowedPath = GetCurrentLocationsString();

            if (SavedPaths.Contains(arrowedPath) == false)
            {
                SavedPaths.Add(arrowedPath);

                SilentlySetSavedPath(arrowedPath);
            }
        }, () => true);

        private void SilentlySetSavedPath(string newPath)
        {
            if (SelectedPath == newPath) return;

            _ignorePathsSelectionChange = true;
            SelectedPath = newPath;
        }

        private string GetCurrentLocationsString()
        {
            return $"{InstallCollection.Location}{ARROWED_PATH_SEPARATOR}{StorageCollection.Location}";
        }

        [AutoLazy.Lazy]
        public DelegateCommand DeleteCurrentLocationCommand => new DelegateCommand(() => {
            if (SelectedPath != null) SavedPaths.Remove(SelectedPath);
        }, () => true);

    }

}
