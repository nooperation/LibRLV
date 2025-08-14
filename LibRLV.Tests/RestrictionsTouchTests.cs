namespace LibRLV.Tests
{
    public class RestrictionsTouchTests : RestrictionsBase
    {

        #region  @touchfar @fartouch[:max_distance]=<y/n>

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public async Task CanFarTouch(string command)
        {
            await _rlv.ProcessMessage($"@{command}:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public async Task CanFarTouch_Synonym(string command)
        {
            await _rlv.ProcessMessage($"@{command}:0.9=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanFarTouch(out var distance));
            Assert.Equal(0.9f, distance);
        }

        [Theory]
        [InlineData("fartouch")]
        [InlineData("touchfar")]
        public async Task CanFarTouch_Default(string command)
        {
            await _rlv.ProcessMessage($"@{command}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanFarTouch(out var distance));
            Assert.Equal(1.5f, distance);
        }

        [Theory]
        [InlineData("fartouch", "fartouch")]
        [InlineData("fartouch", "touchfar")]
        [InlineData("touchfar", "touchfar")]
        [InlineData("touchfar", "fartouch")]
        public async Task CanFarTouch_Multiple_Synonyms(string command1, string command2)
        {
            await _rlv.ProcessMessage($"@{command1}:12.34=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@{command2}:6.78=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanFarTouch(out var actualDistance2));

            await _rlv.ProcessMessage($"@{command1}:6.78=y", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanFarTouch(out var actualDistance1));

            Assert.Equal(12.34f, actualDistance1, FloatTolerance);
            Assert.Equal(6.78f, actualDistance2, FloatTolerance);
        }

        #endregion

        #region @touchall=<y/n>

        [Fact]
        public void TouchAll()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("11111111-1111-4111-8111-111111111111");

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public async Task TouchAll_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@touchall=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchworld=<y/n> @touchworld:<Guid>=<rem/add>

        [Fact]
        public async Task TouchWorld_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage("@touchworld=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public async Task TouchWorld_Exception()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage("@touchworld=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@touchworld:{objectId2}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId2, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId2, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId2, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId2, null, null));
        }

        #endregion

        #region @touchthis:<Guid>=<rem/add>

        [Fact]
        public async Task TouchThis_default()
        {
            var objectPrimId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectPrimId2 = new Guid("11111111-1111-4111-8111-111111111111");

            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage($"@touchthis:{objectPrimId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectPrimId1, null, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectPrimId1, userId1, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectPrimId1, null, 5.0f));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectPrimId1, null, null));

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectPrimId2, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectPrimId2, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectPrimId2, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectPrimId2, null, null));
        }

        #endregion

        #region @touchme=<rem/add>

        [Fact]
        public async Task TouchMe_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage("@touchall=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@touchme=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, _sender.Id, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, _sender.Id, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, _sender.Id, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, _sender.Id, null, null));
        }

        #endregion

        #region @touchattach=<y/n>

        [Fact]
        public async Task TouchAttach_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage("@touchattach=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchattachself=<y/n>

        [Fact]
        public async Task TouchAttachSelf_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage("@touchattachself=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchattachother=<y/n> @touchattachother:<Guid>=<y/n>

        [Fact]
        public async Task TouchAttachOther_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage("@touchattachother=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public async Task TouchAttachOther_Specific()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");
            var userId2 = new Guid("66666666-6666-4666-8666-666666666666");

            await _rlv.ProcessMessage($"@touchattachother:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId2, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        #endregion

        #region @touchhud[:<Guid>]=<y/n>

        [Fact]
        public async Task TouchHud_default()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage($"@touchhud=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
        }

        [Fact]
        public async Task TouchHud_specific()
        {
            var objectId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var objectId2 = new Guid("11111111-1111-4111-8111-111111111111");
            var userId1 = new Guid("55555555-5555-4555-8555-555555555555");

            await _rlv.ProcessMessage($"@touchhud:{objectId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedSelf, objectId1, null, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.AttachedOther, objectId1, userId1, null));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.RezzedInWorld, objectId1, null, 5.0f));
            Assert.True(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId1, null, null));
            Assert.False(_rlv.Permissions.CanTouch(RlvPermissionsService.TouchLocation.Hud, objectId2, null, null));
        }

        #endregion

    }
}
