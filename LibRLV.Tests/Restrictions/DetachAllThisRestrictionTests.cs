using Moq;

namespace LibRLV.Tests.Restrictions
{
    public class DetachAllThisRestrictionTests : RestrictionsBase
    {
        #region @detachallthis[:<layer>|<attachpt>|<path_to_folder>]=<y/n>

        [Fact]
        public async Task DetachAllThis()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task DetachAllThis_Recursive()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task DetachAllThis_Recursive_Path()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis:Clothing=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task DetachAllThis_Recursive_Worn()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis:pants=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED) - Folder locked due to RetroPants being worn as 'pants'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task DetachAllThis_Recursive_Attached()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@detachallthis:chest=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt (LOCKED)
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants (LOCKED) - Folder locked due to HappyShirt attachment of 'chest'
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

    }
}
