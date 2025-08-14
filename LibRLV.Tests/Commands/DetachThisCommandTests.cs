using Moq;

namespace LibRLV.Tests.Commands
{
    public class DetachThisCommandTests : RestrictionsBase
    {
        #region @detachthis[:<attachpt> or <clothing_layer> or <uuid>]=force
        [Fact]
        public async Task DetachThisForce_Default()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the clothing folder will be detached because happy shirt exists in the clothing folder
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachthis=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedPrimId!.Value, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name);

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
        public async Task DetachThisForce_ByRlvAttachmentPoint()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants (attached to 'groin')
            //  |    |= Happy Shirt (attached to 'chest')
            //  |    |= Retro Pants (worn on 'pants')
            //  |    \-Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat (attached to 'chin')
            //  |        \= Party Hat (attached to 'groin')
            //   \-Accessories
            //        |= Watch (worn on 'tattoo')
            //        \= Glasses (attached to 'chin') <--- Modified to be attached to chest

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = RlvAttachmentPoint.Chest;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the clothing and accessories folder will be detached, not recursive
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachthis:chest=force", _sender.Id, _sender.Name);

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
        public async Task DetachThisForce_ByRlvWearableType()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing
            //  |    |= Business Pants (attached to 'groin')
            //  |    |= Happy Shirt (attached to 'chest')
            //  |    |= Retro Pants (worn on 'pants')
            //  |    \-Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat (attached to 'chin')
            //  |        \= Party Hat (attached to 'groin')
            //   \-Accessories
            //        |= Watch (worn on 'tattoo')  <--- Modified to be worn on pants
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = RlvWearableType.Pants;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Everything under the clothing and accessories folder will be detached, not recursive
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id,
                sampleTree.Root_Clothing_HappyShirt_AttachChest.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachthis:pants=force", _sender.Id, _sender.Name);

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
        public async Task DetachThisForce_ByRlvWearableType_PrivateFolder()
        {
            // #RLV
            //  |
            //  |- .private
            //  |
            //  |- Clothing                 <--- Modified to be .Clothing
            //  |    |= Business Pants (attached to 'groin')
            //  |    |= Happy Shirt (attached to 'chest')
            //  |    |= Retro Pants (worn on 'pants')
            //  |    \-Hats
            //  |        |
            //  |        |- Sub Hats
            //  |        |    \ (Empty)
            //  |        |
            //  |        |= Fancy Hat (attached to 'chin')
            //  |        \= Party Hat (attached to 'groin')
            //   \-Accessories
            //        |= Watch (worn on 'tattoo') <--- Modified to be worn on pants
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = ".clothing";

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = RlvWearableType.Pants;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.DetachAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Only accessories will be removed even though pants exist in our clothing folder. The clothing folder is private ".clothing"
            var expected = new HashSet<Guid>()
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Accessories_Glasses_AttachChin.Id,
            };

            // Act
            await _rlv.ProcessMessage("@detachthis:pants=force", _sender.Id, _sender.Name);

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
