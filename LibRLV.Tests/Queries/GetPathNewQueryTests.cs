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

            Assert.True(await _rlv.ProcessMessage("@getpathnew=1234", sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedPrimId!.Value, sampleTree.Root_Clothing_Hats_PartyHat_Spine.Name));
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

            Assert.True(await _rlv.ProcessMessage($"@getpathnew:{sampleTree.Root_Accessories_Glasses.Id}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task GetPathNew_ByUUID_Unknown()
        {
            var actual = _actionCallbacks.RecordReplies();
            var sampleTree = SampleInventoryTree.BuildInventoryTree();
            var sharedFolder = sampleTree.Root;

            sampleTree.Root_Clothing_Hats_FancyHat_Chin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedTo = null;
            sampleTree.Root_Clothing_HappyShirt.AttachedTo = null;
            sampleTree.Root_Accessories_Glasses.AttachedTo = null;
            sampleTree.Root_Clothing_RetroPants.WornOn = null;
            sampleTree.Root_Accessories_Watch.WornOn = null;

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

            sampleTree.Root_Clothing_Hats_FancyHat_Chin.AttachedTo = RlvAttachmentPoint.Groin;
            sampleTree.Root_Clothing_Hats_PartyHat_Spine.AttachedTo = null;
            sampleTree.Root_Clothing_BusinessPants_Pelvis.AttachedTo = RlvAttachmentPoint.Default;
            sampleTree.Root_Clothing_HappyShirt.AttachedTo = RlvAttachmentPoint.Chin;
            sampleTree.Root_Accessories_Glasses.AttachedTo = RlvAttachmentPoint.Groin;
            sampleTree.Root_Clothing_RetroPants.WornOn = null;
            sampleTree.Root_Accessories_Watch.WornOn = null;

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

            sampleTree.Root_Clothing_Hats_FancyHat_Chin.AttachedTo = null;
            sampleTree.Root_Clothing_Hats_FancyHat_Chin.WornOn = RlvWearableType.Pants;
            sampleTree.Root_Clothing_RetroPants.WornOn = RlvWearableType.Tattoo;
            sampleTree.Root_Accessories_Watch.WornOn = RlvWearableType.Pants;

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
