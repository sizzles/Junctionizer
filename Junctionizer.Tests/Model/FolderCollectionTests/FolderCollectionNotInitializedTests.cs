﻿using Junctionizer.Model;

using NUnit.Framework;

namespace Junctionizer.Tests.Model.FolderCollectionTests
{
    public class FolderCollectionNotInitializedTests
    {
        [Test]
        public void CanExecuteNoLocation()
        {
            var (folderCollection, _) = FolderCollection.Factory.CreatePair();

            Assert.That(folderCollection.SelectLocationCommand.CanExecute, Is.True);

            Assert.That(folderCollection.ArchiveSelectedCommand.CanExecute, Is.False);
            Assert.That(folderCollection.CopySelectedCommand.CanExecute, Is.False);
            Assert.That(folderCollection.CreateSelectedJunctionCommand.CanExecute, Is.False);
            Assert.That(folderCollection.DeleteSelectedFoldersCommand.CanExecute, Is.False);
            Assert.That(folderCollection.DeleteSelectedJunctionsCommand.CanExecute, Is.False);
            Assert.That(folderCollection.SelectFoldersNotInOtherPaneCommand.CanExecute, Is.False);
        }
    }
}
