namespace LibRLV.Tests.Restrictions
{
    public class ViewNoteRestrictionTests : RestrictionsBase
    {
        #region @viewnote=<y/n>
        [Fact]
        public async Task CanViewNote()
        {
            await CheckSimpleCommand("viewNote", m => m.CanViewNote());
        }
        #endregion
    }
}
