using Moq;

namespace LibRLV.Tests.Commands
{
    public class AttachAllCommandTests : RestrictionsBase
    {
        #region @attachallover @attachalloverorreplace @attachall:<folder1/.../folderN>=force

        [Theory]
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public async Task AttachForce_Recursive(string command, bool replaceExistingAttachments)
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

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Spine, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name);

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
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public async Task AttachForce_Recursive_WithHiddenSubfolder(string command, bool replaceExistingAttachments)
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

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively. The hats folder has a special . prefix, which means it will be ignored
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name);

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
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public async Task AttachForce_Recursive_FolderNameSpecifiesToAddInsteadOfReplace(string command, bool replaceExistingAttachments)
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

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively. The hats folder has a special + prefix, which means it will use 'add to' logic instead of 'replace' logic when attaching
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, false),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Spine, false),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:Clothing=force", _sender.Id, _sender.Name);

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
        [InlineData("attachall", true)]
        [InlineData("attachalloverorreplace", true)]
        [InlineData("attachallover", false)]
        public async Task AttachForce_Recursive_SourceFolderPrivate(string command, bool replaceExistingAttachments)
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
            clothingFolder.Name = ".auto_attach";

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            _actionCallbacks.Setup(e =>
                e.AttachAsync(It.IsAny<IReadOnlyList<AttachmentRequest>>(), It.IsAny<CancellationToken>())
            ).Returns(Task.CompletedTask);

            // Attach everything inside of of the Clothing folder, and all of its subfolders recursively. The hats folder has a special + prefix, which means it will use 'add to' logic instead of 'replace' logic when attaching
            var expected = new HashSet<AttachmentRequest>()
            {
                new(sampleTree.Root_Clothing_HappyShirt_AttachChest.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_RetroPants_WornPants.Id, RlvAttachmentPoint.Default, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_BusinessPants_AttachGroin.Id, RlvAttachmentPoint.Pelvis, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.Id, RlvAttachmentPoint.Chin, replaceExistingAttachments),
                new(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Id, RlvAttachmentPoint.Spine, replaceExistingAttachments),
            };

            // Act
            await _rlv.ProcessMessage($"@{command}:.auto_attach=force", _sender.Id, _sender.Name);

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
