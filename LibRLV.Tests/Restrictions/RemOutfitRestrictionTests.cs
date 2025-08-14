using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace LibRLV.Tests.Restrictions
{
    public class RemOutfitRestrictionTests : RestrictionsBase
    {

        #region @remoutfit[:<part>]=<y/n>
        [Fact]
        public async Task RemOutfit()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@remoutfit=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }

        [Fact]
        public async Task RemOutfit_part()
        {
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            Assert.True(await _rlv.ProcessMessage("@remoutfit:pants=n", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));

            // #RLV/Clothing/Retro Pants
            Assert.False(_rlv.Permissions.CanDetach(sampleTree.Root_Clothing_RetroPants_WornPants, true));

            // #RLV/Accessories/Watch
            Assert.True(_rlv.Permissions.CanDetach(sampleTree.Root_Accessories_Watch_WornTattoo, true));
        }
        #endregion

    }
}
