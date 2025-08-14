using Moq;

namespace LibRLV.Tests.Exceptions
{
    public class AttachThisExceptExceptionTests : RestrictionsBase
    {
        #region @attachthis_except:<folder>=<rem/add>

        [Fact]
        public async Task AttachAllThis_Recursive_Except()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@attachallthis=n", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));
            Assert.True(await _rlv.ProcessMessage($"@attachthis_except:Clothing/Hats=add", sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_BusinessPants_AttachGroin.Name));

            // #RLV/Clothing/Hats/Party Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin, true));

            // #RLV/Clothing/Hats/Fancy Hat (LOCKED) - Parent folder locked recursively
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_Hats_FancyHat_AttachChin, true));

            // #RLV/Clothing/Business Pants ()  - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_BusinessPants_AttachGroin, true));

            // #RLV/Clothing/Happy Shirt () - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_HappyShirt_AttachChest, true));

            // #RLV/Clothing/Retro Pants () - Locked, but folder has exception
            Assert.False(_rlv.Permissions.CanAttach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Glasses ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Glasses_AttachChin, true));

            // #RLV/Accessories/Watch ()
            Assert.True(_rlv.Permissions.CanAttach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        #endregion

    }
}
