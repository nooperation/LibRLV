using Moq;

namespace LibRLV.Tests.Commands
{
    public class AttachThisCommandTests : RestrictionsBase
    {
        #region @attachthisoverorreplace @attachthisover @attachthis[:<attachpt> or <clothing_layer> or <uuid>]=force
        [Theory]
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public async Task AttachThis_Default(string command, bool replaceExistingAttachments)
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

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (Spine)";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything in #RLV/Clothing/Hats because that's where the source item (fancy hat) is calling @attachthis from
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Spine, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name);

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
        [InlineData("attachthis")]
        [InlineData("attachthisoverorreplace")]
        [InlineData("attachthisover")]
        public async Task AttachThis_FolderNameSpecifiesToAddInsteadOfReplace(string command)
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

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business Pants (groin)";
            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (Spine)";
            sampleTree.Root_Clothing_RetroPants_WornPants.Name = "Worn Pants";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            clothingFolder.Name = "+clothing";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything in #RLV/+clothing because that's where the source item (business pants) is calling @attachthis from, but use 'add-to' logic instead of 'replace' logic
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Groin, false),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Spine, false),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, false),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name);

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
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public async Task AttachThis_FolderNameSpecifiesRlvAttachmentPoint(string command, bool replaceExistingAttachments)
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

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat";

            var clothingFolder = sampleTree.Root.Children.Where(n => n.Name == "Clothing").First();
            var hatsFolder = clothingFolder.Children.Where(n => n.Name == "Hats").First();
            hatsFolder.Name = "(skull) hats";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything in #RLV/Clothing/+Hats because that's where the source item (fancy hat) is calling @attachthis from,
            // but attach "party hat" to the skull because it doesn't specify an attachment point but the folder name does
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Skull, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name);

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
        [InlineData("attachthis")]
        [InlineData("attachthisoverorreplace")]
        [InlineData("attachthisover")]
        public async Task AttachThis_FromHiddenSubfolder(string command)
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

            // Nothing from ./Clothing/.Hats is worn because it's private, even though the sender exists in this folder
            var expected = new HashSet<AttachmentRequest>()
            {
            };

            // Act
            await _rlv.ProcessMessage($"@{command}=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name);

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
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public async Task AttachThis_AttachPoint(string command, bool replaceExistingAttachments)
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

            sampleTree.Root_Clothing_HappyShirt_AttachChest.Name = "Happy Shirt (chest)";
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name = "Fancy Hat (chin)";
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;

            sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name = "Business pants (spine)";
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = RlvAttachmentPoint.Spine;

            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name = "Party Hat (spine)";
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = RlvAttachmentPoint.Spine;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach happy shirt because it's in the same folder as our business pants (attached to spine).
            // Attach retro pants because it's in the same folder as our business pants (attached to spine).
            // Attach fancy hat because it's in the same folder as our party hat (attached to spine)
            // Don't attach BusinessPants or PartyHat because they are already attached
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Chest, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:spine=force", sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Name);

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
        [InlineData("attachthis", true)]
        [InlineData("attachthisoverorreplace", true)]
        [InlineData("attachthisover", false)]
        public async Task AttachThis_RlvWearableType(string command, bool replaceExistingAttachments)
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
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;

            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = RlvWearableType.Tattoo;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = RlvWearableType.Tattoo;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // We are currently wearing Tattoo items in "./Clothing" and "./Accessories". Wear everything from these two folders
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Accessories_Glasses_AttachChin.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
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
