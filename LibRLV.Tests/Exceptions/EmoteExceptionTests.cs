namespace LibRLV.Tests.Exceptions
{
    public class EmoteExceptionTests : RestrictionsBase
    {
        #region @emote=<rem/add>
        [Fact]
        public async Task CanEmote()
        {
            await CheckSimpleCommand("emote", m => m.CanEmote());
        }

        // TODO: Check 'ProcessChat' functionality (not yet created, but the function doesn't exist yet) to make
        //       sure it no longer censors emotes on @chat=n
        #endregion
    }
}
