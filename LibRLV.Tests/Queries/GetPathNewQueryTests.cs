using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace LibRLV.Tests.Queries
{
    public class GetPathNewQueryTests : RestrictionsBase
    {
        #region @getpath @getpathnew[:<attachpt> or <clothing_layer> or <uuid>]=<channel_number>

        [Fact]
        public async Task GetPathNew_BySender()
        {
            var actual = _actionCallbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Clothing/Hats"),
            };

            Assert.True(await _rlv.ProcessMessage("@getpathnew=1234", sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetPathNew_ByUUID()
        {
            var actual = _actionCallbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Accessories"),
            };

            Assert.True(await _rlv.ProcessMessage($"@getpathnew:{sampleTree.Root_Accessories_Glasses_AttachChin.Id}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetPathNew_ByUUID_Unknown()
        {
            var actual = _actionCallbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, ""),
            };

            Assert.True(await _rlv.ProcessMessage($"@getpathnew:BADBADBA-DBAD-4BAD-8BAD-BADBADBADBAD=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetPathNew_ByAttach()
        {
            var actual = _actionCallbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = RlvAttachmentPoint.Groin;
            sampleTree.Root_Clothing_Hats_PartyHat_AttachGroin.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_AttachGroin.AttachedTo = RlvAttachmentPoint.Default;
            sampleTree.Root_Clothing_HappyShirt_AttachChest.AttachedTo = RlvAttachmentPoint.Chin;
            sampleTree.Root_Accessories_Glasses_AttachChin.AttachedTo = RlvAttachmentPoint.Groin;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = null;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = null;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Accessories,Clothing/Hats"),
            };

            Assert.True(await _rlv.ProcessMessage($"@getpathnew:groin=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }


        [Fact]
        public async Task GetPathNew_ByWorn()
        {
            var actual = _actionCallbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_FancyHat_AttachChin.WornOn = RlvWearableType.Pants;
            sampleTree.Root_Clothing_RetroPants_WornPants.WornOn = RlvWearableType.Tattoo;
            sampleTree.Root_Accessories_Watch_WornTattoo.WornOn = RlvWearableType.Pants;

            _queryCallbacks.Setup(e =>
                e.TryGetSharedFolderAsync(default)
            ).ReturnsAsync((true, sharedFolder));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Accessories,Clothing/Hats"),
            };

            Assert.True(await _rlv.ProcessMessage($"@getpathnew:pants=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        #endregion

    }
}
