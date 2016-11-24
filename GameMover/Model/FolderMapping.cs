﻿using System;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Prism.Mvvm;

namespace GameMover.Model
{
    /// <summary>Container for two directory location.</summary>
    public sealed class FolderMapping : BindableBase, IEquatable<FolderMapping>
    {
        public FolderMapping([CanBeNull] string source, [CanBeNull] string destination, bool isSavedMapping = false)
        {
            Source = source;
            Destination = destination;
            IsSavedMapping = isSavedMapping;
        }

        [CanBeNull]
        public string Source { get; }

        [CanBeNull]
        public string Destination { get; }

        [JsonIgnore]
        public bool IsSavedMapping { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{Source} => {Destination}";

        /// <inheritdoc/>
        public bool Equals(FolderMapping other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return EqualsInternal(other);
        }

        private bool EqualsInternal(FolderMapping other)
        {
            return string.Equals(Destination, other.Destination, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Source, other.Source, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return EqualsInternal((FolderMapping) obj);
        }

        public static bool operator ==(FolderMapping left, FolderMapping right) => Equals(left, right);
        public static bool operator !=(FolderMapping left, FolderMapping right) => !Equals(left, right);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Destination != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Destination) : 0) * 397) ^
                       (Source != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Source) : 0);
            }
        }
    }
}
