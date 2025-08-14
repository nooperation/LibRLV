using Moq;

namespace LibRLV.Tests.Commands
{
    public class RemOutfitCommandTests : RestrictionsBase
    {
        #region @remoutfit[:<folder|layer>]=force
        [Fact]
        public async Task RemOutfitForce()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            // skin, shape, eyes and hair cannot be removed
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.WornOn = RlvWearableType.Skin;

            _queryCallbacks.Setup(e =>
                e.TryGetCurrentOutfitAsync(default)
            ).ReturnsAsync((true, currentOutfit));

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.RemOutfitAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
            };

            // Act
            await _rlv.ProcessMessage("@remoutfit=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.RemOutfitAsync(
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
        public async Task RemOutfitForce_ExternalItems()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);

            var externalWearable = new RlvInventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                null,
                null,
                RlvWearableType.Tattoo);
            var externalAttachable = new RlvInventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                RlvAttachmentPoint.Jaw,
                new Guid("12312312-0002-4aaa-8aaa-ffffffffffff"),
                null);

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);

            _queryCallbacks.Setup(e =>
                e.TryGetCurrentOutfitAsync(default)
            ).ReturnsAsync((true, currentOutfit));

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.RemOutfitAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                sampleTree.Root_Clothing_RetroPants_WornPants.Id,
                externalWearable.Id
            };

            // Act
            await _rlv.ProcessMessage("@remoutfit=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.RemOutfitAsync(
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
        public async Task RemOutfitForce_ExternalItems_ByType()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            var externalWearable = new RlvInventoryItem(
                new Guid("12312312-0001-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Tattoo",
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                null,
                null,
                RlvWearableType.Tattoo);
            var externalAttachable = new RlvInventoryItem(
                new Guid("12312312-0002-4aaa-8aaa-aaaaaaaaaaaa"),
                "External Jaw Thing",
                new Guid("12312312-aaaa-4aaa-8aaa-aaaaaaaaaaaa"),
                RlvAttachmentPoint.Jaw,
                new Guid("12312312-0002-4aaa-8aaa-ffffffffffff"),
                null);

            currentOutfit.Add(externalWearable);
            currentOutfit.Add(externalAttachable);
            _queryCallbacks.Setup(e =>
                e.TryGetCurrentOutfitAsync(default)
            ).ReturnsAsync((true, currentOutfit));

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.RemOutfitAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
                externalWearable.Id
            };

            // Act
            await _rlv.ProcessMessage("@remoutfit:tattoo=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.RemOutfitAsync(
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
        public async Task RemOutfitForce_Folder()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.WornOn = RlvWearableType.Tattoo;

            _queryCallbacks.Setup(e =>
                e.TryGetCurrentOutfitAsync(default)
            ).ReturnsAsync((true, currentOutfit));

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.RemOutfitAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>
            {
                sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id,
            };

            // Act
            await _rlv.ProcessMessage("@remoutfit:Clothing/Hats=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.RemOutfitAsync(
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
        public async Task RemOutfitForce_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetCurrentOutfitAsync(default)
            ).ReturnsAsync((true, currentOutfit));

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.RemOutfitAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>
            {
                sampleTree.Root_Accessories_Watch_WornTattoo.Id,
            };

            // Act
            await _rlv.ProcessMessage("@remoutfit:tattoo=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.RemOutfitAsync(
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
        public async Task RemOutfitForce_BodyPart_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var currentOutfit = SampleInventoryTree.BuildCurrentOutfit(sampleTree.Root);
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = RlvWearableType.Skin;

            _queryCallbacks.Setup(e =>
                e.TryGetCurrentOutfitAsync(default)
            ).ReturnsAsync((true, currentOutfit));

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.RemOutfitAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            var expected = new HashSet<Guid>();

            // Act
            await _rlv.ProcessMessage("@remoutfit:skin=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.RemOutfitAsync(
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
