using Moq;

namespace LibRLV.Tests.Commands
{
    public class AttachAllThisCommandTests : RestrictionsBase
    {
        #region @attachallthisover @attachallthisoverorreplace @attachallthis[:<attachpt> or <clothing_layer>]=force
        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public async Task AttachAllThisForce_Recursive(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything inside of of the Clothing folder (sender exists in the clothing folder), and all of its subfolders recursively
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Spine, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedPrimId!.Value, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AttachAsync(
                    It.Is<IReadOnlyList<AttachmentRequest>>(ids =>
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

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public async Task AttachAllThisForce_Recursive_WithHiddenSubfolder(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = ".hats";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything inside of of the Clothing folder (sender exists in the clothing folder), and all of its subfolders recursively.
            //   The hats folder has a special . prefix, which means it will be ignored
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedPrimId!.Value, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AttachAsync(
                    It.Is<IReadOnlyList<AttachmentRequest>>(ids =>
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

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public async Task AttachAllThisForce_Recursive_FolderNameSpecifiesToAddInsteadOfReplace(string command, bool replaceExistingAttachments)
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (Pelvis)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "+hats";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything inside of of the Clothing folder (sender exists in the clothing folder), and all of its subfolders recursively.
            //   The hats folder has a special + prefix, which means it will use 'add to' logic instead of 'replace' logic when attaching
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, false),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Spine, false),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedPrimId!.Value, sampleTree.Root_Clothing_HappyShirt_AttachChest.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AttachAsync(
                    It.Is<IReadOnlyList<AttachmentRequest>>(ids =>
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

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public async Task AttachAllThisForce_AttachPoint(string command, bool replaceExistingAttachments)
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
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business pants (spine)";
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = RlvAttachmentPoint.Spine;

            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (neck)";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach happy shirt because it's in the same folder as our business pants (attached to spine).
            // Attach retro pants because it's in the same folder as our business pants (attached to spine).
            // Attach fancy hat because it's in a subfolder of our business pants
            // Attach party hat because it's in a subfolder of our business pants
            // Don't attach BusinessPants because they are already attached
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Neck, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:spine=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AttachAsync(
                    It.Is<IReadOnlyList<AttachmentRequest>>(ids =>
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

        [Theory]
        [InlineData("attachallthis", true)]
        [InlineData("attachallthisoverorreplace", true)]
        [InlineData("attachallthisover", false)]
        public async Task AttachAllThisForce_RlvWearableType(string command, bool replaceExistingAttachments)
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
            //        \= Glasses (attached to 'chin')

            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;

            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = RlvWearableType.Tattoo;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business pants";
            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (neck)";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach happy shirt because it's in the same folder as our retro pants (worn as tattoo).
            // Attach retro pants because it's in the same folder as our retro pants (worn as tattoo).
            // Attach fancy hat because it's in a subfolder of our retro pants
            // Attach party hat because it's in a subfolder of our retro pants
            // Don't attach retro pants because they are already attached
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Neck, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:tattoo=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.AttachAsync(
                    It.Is<IReadOnlyList<AttachmentRequest>>(ids =>
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
