using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using JetBrains.Annotations;

using Prism.Mvvm;

namespace Junctionizer.Model
{
    public class GameFolderPair : BindableBase, IEquatable<GameFolderPair>, IComparable<GameFolderPair>
    {
        private const string SOURCE_ENTRY_SIZE_PROPERTY_NAME = nameof(SourceEntry) + "." + nameof(GameFolder.Size);
        private const string DESTINATION_ENTRY_SIZE_PROPERTY_NAME = nameof(DestinationEntry) + "." + nameof(GameFolder.Size);

        private GameFolder _sourceEntry;
        [CanBeNull]
        public GameFolder SourceEntry
        {
            get { return _sourceEntry; }
            set {
                if (_sourceEntry != value)
                {
                    if (_sourceEntry != null)
                    {
                        PropertyChangedEventManager.RemoveHandler(_sourceEntry, OnSourcePropertyChanged, string.Empty);
                    }

                    _sourceEntry = value;

                    if (_sourceEntry != null)
                    {
                        PropertyChangedEventManager.AddHandler(_sourceEntry, OnSourcePropertyChanged, string.Empty);
                    }

                    RaisePropertyChanged(SOURCE_ENTRY_SIZE_PROPERTY_NAME);

                    Debug.Assert(SourceEntry == null || DestinationEntry == null || SourceEntry.Name == DestinationEntry.Name);
                }
            }
        }

        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
            => RaisePropertyChanged(nameof(SourceEntry) + "." + e.PropertyName);

        private GameFolder _destinationEntry;
        [CanBeNull]
        public GameFolder DestinationEntry
        {
            get { return _destinationEntry; }
            set {
                if (_destinationEntry != value)
                {
                    if (_destinationEntry != null)
                    {
                        PropertyChangedEventManager.RemoveHandler(_destinationEntry, OnDestinationPropertyChanged, string.Empty);
                    }

                    _destinationEntry = value;

                    if (_destinationEntry != null)
                    {
                        PropertyChangedEventManager.AddHandler(_destinationEntry, OnDestinationPropertyChanged, string.Empty);
                    }
                    
                    RaisePropertyChanged(DESTINATION_ENTRY_SIZE_PROPERTY_NAME);

                    Debug.Assert(SourceEntry == null || DestinationEntry == null || SourceEntry.Name == DestinationEntry.Name);
                }
            }
        }

        private void OnDestinationPropertyChanged(object sender, PropertyChangedEventArgs e)
            => RaisePropertyChanged(nameof(DestinationEntry) + "." + e.PropertyName);

        /// <inheritdoc/>
        public GameFolderPair([CanBeNull] GameFolder sourceEntry = null, [CanBeNull] GameFolder destinationEntry = null)
        {
            Debug.Assert(sourceEntry != null || destinationEntry != null);

            SourceEntry = sourceEntry;
            DestinationEntry = destinationEntry;

            Name = SourceEntry?.Name ?? DestinationEntry.Name;
            HashCode = Name.GetHashCode();
        }

        [UsedImplicitly]
        public bool IsBeingDeleted => SourceEntry?.IsBeingDeleted == true || DestinationEntry?.IsBeingDeleted == true;

        private int HashCode { get; }

        [NotNull]
        public string Name { get; }

        /// <inheritdoc/>
        public bool Equals(GameFolderPair other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return string.Equals(Name, other.Name, StringComparison.Ordinal);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is GameFolderPair pair && Equals(pair);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode;

        /// <inheritdoc/>
        public int CompareTo(GameFolderPair other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return Comparer<GameFolder>.Default.Compare(SourceEntry ?? DestinationEntry, other.SourceEntry ?? other.DestinationEntry);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(SourceEntry)}: {SourceEntry}, {nameof(DestinationEntry)}: {DestinationEntry}";
    }
}
