﻿using System;
using System.ComponentModel;
using System.IO;

using GameMover.Code;
using GameMover.External_Code;
using GameMover.Model;

using MaterialDesignThemes.Wpf;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using Prism.Commands;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;

using static GameMover.Code.StaticMethods;

namespace GameMover.ViewModels
{

    public class MainWindowViewModel : BindableBase
    {

        public FindJunctionsViewModel FindJunctionsViewModel { get; } = new FindJunctionsViewModel();

        public InteractionRequest<INotification> CloseDialogRequest { get; } = new InteractionRequest<INotification>();

        [AutoLazy.Lazy]
        public DelegateCommand<DialogClosingEventArgs> DialogClosedCommand
            => new DelegateCommand<DialogClosingEventArgs>(args => OnDialogClosed?.Invoke());
        private event Action OnDialogClosed;

        public FolderCollection InstallCollection { get; private set; }
        public FolderCollection StorageCollection { get; private set; }

        public AsyncObservableCollection<FolderMapping> DisplayedMappings { get; } = new AsyncObservableCollection<FolderMapping>();

        private FolderMapping _selectedMapping;
        public FolderMapping SelectedMapping
        {
            get { return _selectedMapping; }
            set {
                var previousValue = _selectedMapping;
                _selectedMapping = value;
                if (!DisplayedMappings.Contains(_selectedMapping)) DisplayedMappings.Add(_selectedMapping);
                if (previousValue?.SaveMapping == false) DisplayedMappings.Remove(previousValue);

                IsSelectedMappingModificationAllowed = false;
                InstallCollection.Location = _selectedMapping.Source;
                StorageCollection.Location = _selectedMapping.Destination;
                IsSelectedMappingModificationAllowed = true;
            }
        }

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

            DisplayedMappings.Add(new FolderMapping(InstallCollection.FolderBrowserDefaultLocation,
                StorageCollection.FolderBrowserDefaultLocation, true));
            DisplayedMappings.Add(new FolderMapping(@"C:\Users\Nick\Desktop\Folder a", @"C:\Users\Nick\Desktop\Folder B", true));
        }

        [AutoLazy.Lazy]
        public DelegateCommand SaveCurrentLocationCommand => new DelegateCommand(() => {
            if (SelectedMapping != null) SelectedMapping.SaveMapping = true;
        }, () => true);

        [AutoLazy.Lazy]
        public DelegateCommand DeleteCurrentLocationCommand => new DelegateCommand(() => {
            if (SelectedMapping != null) SelectedMapping.SaveMapping = false;
        });

        [AutoLazy.Lazy]
        public DelegateCommand FindExistingJunctionsCommand => new DelegateCommand(async () => {
            var folderDialog = NewFolderDialog("Select Root Directory");
            if (folderDialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            var selectedPath = folderDialog.FileName;

            OnDialogClosed = () => FindJunctionsViewModel.Cancel();
            var junctions = await FindJunctionsViewModel.GetJunctions(selectedPath);
            CloseDialogRequest.Raise(null);

            foreach (var directoryInfo in junctions)
            {
                var folderMapping = new FolderMapping(directoryInfo.Parent.FullName,
                    Directory.GetParent(JunctionPoint.GetTarget(directoryInfo)).FullName);
                if (!DisplayedMappings.Contains(folderMapping)) DisplayedMappings.Add(folderMapping);
            }
        });

        private bool IsSelectedMappingModificationAllowed { get; set; } = true;

        private void OnFolderCollectionChange(object sender, PropertyChangedEventArgs args)
        {
            // When a new folder location is chosen, check if it is already saved and if so select it so that it can be displayed in the combo box
            if (args.PropertyName.Equals(nameof(FolderCollection.Location)) && IsSelectedMappingModificationAllowed)
            {
                SelectedMapping = new FolderMapping(InstallCollection.Location, StorageCollection.Location);
            }
        }

    }

}