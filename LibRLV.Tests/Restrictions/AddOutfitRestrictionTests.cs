using Moq;

namespace LibRLV.Tests.Restrictions
{
    public class AddOutfitRestrictionTests : RestrictionsBase
    {
        #region @addoutfit[:<part>]=<y/n>
        [Fact]
        public async Task AddOutfit()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@addoutfit=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task AddOutfit_part()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@addoutfit:pants=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }
        #endregion

    }
}
