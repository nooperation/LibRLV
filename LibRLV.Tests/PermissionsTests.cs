namespace LibRLV.Tests
{
    public class PermissionsTests : RestrictionsBase
    {
        [Fact]
        public void CanRecvChat_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
            var userId2 = new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");

            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanChat_Default()
        {
            Assert.True(_rlv.Permissions.CanChat(0, "Hello"));
            Assert.True(_rlv.Permissions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Permissions.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanSendIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Permissions.CanSendIM("Hello", userId1));
            Assert.True(_rlv.Permissions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanStartIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Permissions.CanStartIM(null));
            Assert.True(_rlv.Permissions.CanStartIM(userId1));
        }

        [Fact]
        public void CanReceiveIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello", userId1));
            Assert.True(_rlv.Permissions.CanReceiveIM("Hello", userId1, "Group Name"));
        }
    }
}
