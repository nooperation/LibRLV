using Moq;

namespace LibRLV.Tests.Commands
{
    public class DetachAllCommandTests : RestrictionsBase
    {
        #region @detachall:<folder1/.../folderN>=force
        [Fact]
        public async Task DetachAllForce_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the clothing folder, and all of its subfolders will be removed
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachall:Clothing=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.DetachAsync(
                    It.Is<IReadOnlyList<Guid>>(ids =>
                        ids != null &&
                        ids.Count == expected.Count &&
                        expected.SetEquals(ids)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DetachAllForce_Recursive_IgnoreRestrictions()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the clothing folder, and all of its subfolders will be removed even though the clothing folder is restricted from
            //  being detached - commands bypass these restrictions
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            };
            await _rlv.ProcessMessage("@detachthis:Clothing=n", _sender.Id, _sender.Name);

            // Act
            await _rlv.ProcessMessage("@detachall:Clothing=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.DetachAsync(
                    It.Is<IReadOnlyList<Guid>>(ids =>
                        ids != null &&
                        ids.Count == expected.Count &&
                        expected.SetEquals(ids)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DetachAllForce_Recursive_PrivateTargetDir()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = ".clothing";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the clothing folder, and all of its subfolders will be removed
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id,
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachall:.clothing=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.DetachAsync(
                    It.Is<IReadOnlyList<Guid>>(ids =>
                        ids != null &&
                        ids.Count == expected.Count &&
                        expected.SetEquals(ids)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task DetachAllForce_Recursive_PrivateSubFolders()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            clothingFolder.Name = ".clothing";
            hatsFolder.Name = ".hats";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the .clothing folder, and all of its non-private subfolders will be removed, except the private .hats folder
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachall:.clothing=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.DetachAsync(
                    It.Is<IReadOnlyList<Guid>>(ids =>
                        ids != null &&
                        ids.Count == expected.Count &&
                        expected.SetEquals(ids)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );

            _actionCallbacks.VerifyNoOtherCalls();
        }
        #endregion

    }
}
