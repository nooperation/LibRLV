using OpenMetaverse;

namespace LibRLV.Tests
{
    public class RlvCommonTests
    {
        [Theory]
        [InlineData("(spine)", AttachmentPoint.Spine)]
        [InlineData("(spine)(spine)", AttachmentPoint.Spine)]
        [InlineData("My (mouth) item (spine) has a lot of tags", AttachmentPoint.Spine)]
        [InlineData("My item (l upper leg) and some random unknown tag (unknown)", AttachmentPoint.LeftUpperLeg)]
        [InlineData("Central item (avatar center)", AttachmentPoint.Root)]
        [InlineData("Central item (root)", AttachmentPoint.Root)]
        public void TryGetAttachmentPointFromItemName(string itemName, AttachmentPoint expectedAttachmentPoint)
        {
            Assert.True(RLVCommon.TryGetAttachmentPointFromItemName(itemName, out var actualAttachmentPoint));
            Assert.Equal(expectedAttachmentPoint, actualAttachmentPoint);
        }

        [Theory]
        [InlineData("(SPINE)", AttachmentPoint.Spine)]
        [InlineData("(Spine)", AttachmentPoint.Spine)]
        [InlineData("Central item (Avatar center)", AttachmentPoint.Root)]
        [InlineData("Another item (l upper leg)", AttachmentPoint.LeftUpperLeg)]
        public void TryGetAttachmentPointFromItemName_CaseSensitive(string itemName, AttachmentPoint expectedAttachmentPoint)
        {
            Assert.True(RLVCommon.TryGetAttachmentPointFromItemName(itemName, out var actualAttachmentPoint));
            Assert.Equal(expectedAttachmentPoint, actualAttachmentPoint);
        }

        [Theory]
        [InlineData("(unknown)(hand tag)(spine tag)")]
        [InlineData("Hat")]
        [InlineData("")]
        public void TryGetAttachmentPointFromItemNameInvalid(string itemName)
        {
            Assert.False(RLVCommon.TryGetAttachmentPointFromItemName(itemName, out var actualAttachmentPoint));
        }
    }
}
