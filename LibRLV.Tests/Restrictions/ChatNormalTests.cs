namespace LibRLV.Tests.Restrictions
{
    public class ChatNormalTests : RestrictionsBase
    {
        #region @chatnormal=<y/n>
        [Fact]
        public async Task CanChatNormal()
        {
            await CheckSimpleCommand("chatNormal", m => m.CanChatNormal());
        }
        #endregion

    }
}
