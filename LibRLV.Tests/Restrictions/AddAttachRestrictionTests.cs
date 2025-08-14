using Moq;

namespace LibRLV.Tests.Restrictions
{
    public class AddAttachRestrictionTests : RestrictionsBase
    {
        #region @addattach[:<attach_point_name>]=<y/n>
        [Fact]
        public async Task AddAttach()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@addattach=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Fancy Hat
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
        }

        [Fact]
        public async Task AddAttach_Specific()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@addattach:groin=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Hats/Fancy Hat
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));
        }
        #endregion

    }
}
