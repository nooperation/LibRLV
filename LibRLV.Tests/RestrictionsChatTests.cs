namespace LibRLV.Tests
{
    public class RestrictionsChatTests : RestrictionsBase
    {

        #region @sendChat
        [Fact]
        public async Task CanSendChat()
        {
            await CheckSimpleCommand("sendChat", m => m.CanSendChat());
        }
        #endregion

        #region @chatshout
        [Fact]
        public async Task CanChatShout()
        {
            await CheckSimpleCommand("chatShout", m => m.CanChatShout());
        }
        #endregion

        #region @chatnormal
        [Fact]
        public async Task CanChatNormal()
        {
            await CheckSimpleCommand("chatNormal", m => m.CanChatNormal());
        }
        #endregion

        #region @chatwhisper
        [Fact]
        public async Task CanChatWhisper()
        {
            await CheckSimpleCommand("chatWhisper", m => m.CanChatWhisper());
        }
        #endregion

        #region @redirchat

        [Fact]
        public async Task IsRedirChat()
        {
            await _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsRedirChat(out var channels));

            var expected = new List<int>
            {
                1234,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public async Task IsRedirChat_Removed()
        {
            await _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@redirchat:1234=rem", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.IsRedirChat(out var channels));
        }

        [Fact]
        public async Task IsRedirChat_MultipleChannels()
        {
            await _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@redirchat:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsRedirChat(out var channels));

            var expected = new List<int>
            {
                1234,
                12345,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public async Task IsRedirChat_RedirectChat()
        {
            var actual = _callbacks.RecordReplies();

            await _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            await _rlv.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.Permissions.IsRedirChat(out var channels));
            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task IsRedirChat_RedirectChatMultiple()
        {
            var actual = _callbacks.RecordReplies();

            await _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@redirchat:5678=add", _sender.Id, _sender.Name);

            await _rlv.ReportSendPublicMessage("Hello World");
            _rlv.Permissions.IsRedirChat(out var channels);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Hello World"),
                (5678, "Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task IsRedirChat_RedirectChatEmote()
        {
            var actual = _callbacks.RecordReplies();

            await _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            await _rlv.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.Permissions.IsRedirChat(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanReceiveChat @recvchat @recvchat_sec @recvchatfrom @recvemote @recvemote_sec @recvemotefrom

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
        public async Task CanRecvChat()
        {
            await _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            var userId = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.False(_rlv.Permissions.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.Permissions.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public async Task CanRecvChat_Except()
        {
            var userId = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            await _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvchat:{userId}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public async Task CanRecvChat_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@recvchat_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvchat:{userId1}=add", sender2.Id, sender2.Name);
            await _rlv.ProcessMessage($"@recvchat:{userId2}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.Permissions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId2));
        }

        [Fact]
        public async Task CanRecvChat_RecvEmote()
        {
            await _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);

            var userId = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId));
            Assert.False(_rlv.Permissions.CanReceiveChat("/me says Hello world", null));
            Assert.False(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public async Task CanRecvChat_RecvEmoteFrom()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@recvemotefrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public async Task CanRecvChat_RecvEmote_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvemote:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId2));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId1));
            Assert.False(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public async Task CanRecvChat_RecvEmote_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@recvemote_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvemote:{userId1}=add", sender2.Id, sender2.Name);
            await _rlv.ProcessMessage($"@recvemote:{userId2}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public async Task CanRecvChatFrom()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@recvchatfrom:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveChat("/me says Hello world", userId1));

            Assert.True(_rlv.Permissions.CanReceiveChat("Hello world", userId2));
        }

        #endregion

        #region @sendGesture

        [Fact]
        public async Task CanSendGesture()
        {
            await CheckSimpleCommand("sendGesture", m => m.CanSendGesture());
        }

        #endregion

        #region @emote
        [Fact]
        public async Task CanEmote()
        {
            await CheckSimpleCommand("emote", m => m.CanEmote());
        }

        // TODO: Check 'ProcessChat' functionality (not yet created, but the function doesn't exist yet) to make
        //       sure it no longer censors emotes on @chat=n
        #endregion

        #region @rediremote:<channel_number>=<rem/add>
        [Fact]
        public async Task IsRedirEmote()
        {
            await _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsRedirEmote(out var channels));

            var expected = new List<int>
            {
                1234,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public async Task IsRedirEmote_Removed()
        {
            await _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@rediremote:1234=rem", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.IsRedirEmote(out var channels));
        }

        [Fact]
        public async Task IsRedirEmote_MultipleChannels()
        {
            await _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@rediremote:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.IsRedirEmote(out var channels));

            var expected = new List<int>
            {
                1234,
                12345,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public async Task IsRedirEmote_RedirectEmote()
        {
            var actual = _callbacks.RecordReplies();

            await _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            await _rlv.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.Permissions.IsRedirEmote(out var channels));
            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/me says Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task IsRedirEmote_RedirectEmoteMultiple()
        {
            var actual = _callbacks.RecordReplies();

            await _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@rediremote:5678=n", _sender.Id, _sender.Name);

            await _rlv.ReportSendPublicMessage("/me says Hello World");
            _rlv.Permissions.IsRedirEmote(out var channels);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/me says Hello World"),
                (5678, "/me says Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task IsRedirEmote_RedirectEmoteChat()
        {
            var actual = _callbacks.RecordReplies();

            await _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            await _rlv.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.Permissions.IsRedirEmote(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanChat @sendchat @sendchannel @sendchannel_sec @sendchannel_except

        [Fact]
        public void CanChat_Default()
        {
            Assert.True(_rlv.Permissions.CanChat(0, "Hello"));
            Assert.True(_rlv.Permissions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Permissions.CanChat(5, "Hello"));
        }

        [Fact]
        public async Task CanChat_SendChatRestriction()
        {
            await _rlv.ProcessMessage("@sendchat=n", _sender.Id, _sender.Name);

            // No public chat allowed unless it starts with '/'
            Assert.False(_rlv.Permissions.CanChat(0, "Hello"));

            // Emotes and other messages starting with / are allowed
            Assert.True(_rlv.Permissions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Permissions.CanChat(0, "/ something?"));

            // Messages containing ()"-*=_^ are prohibited
            Assert.False(_rlv.Permissions.CanChat(0, "/me says Hello ^_^"));

            // Private channels are not impacted
            Assert.True(_rlv.Permissions.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanSendChannel_Default()
        {
            Assert.True(_rlv.Permissions.CanChat(123, "Hello world"));
        }

        [Fact]
        public async Task CanSendChannel()
        {
            await _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanChat(123, "Hello world"));
        }

        [Fact]
        public async Task CanSendChannel_Exception()
        {
            await _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanChat(123, "Hello world"));
        }

        [Fact]
        public async Task CanSendChannel_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@sendchannel_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage("@sendchannel:456=n", sender2.Id, sender2.Name);

            Assert.True(_rlv.Permissions.CanChat(123, "Hello world"));
            Assert.False(_rlv.Permissions.CanChat(456, "Hello world"));
        }

        [Fact]
        public async Task CanSendChannelExcept()
        {
            await _rlv.ProcessMessage("@sendchannel_except:456=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanChat(123, "Hello world"));
            Assert.False(_rlv.Permissions.CanChat(456, "Hello world"));
        }

        #endregion

        #region @sendim @sendim_sec @sendimto

        [Fact]
        public void CanSendIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Permissions.CanSendIM("Hello", userId1));
            Assert.True(_rlv.Permissions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public async Task CanSendIM()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanSendIM("Hello", userId1));
            Assert.False(_rlv.Permissions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public async Task CanSendIM_Exception()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSendIM("Hello world", userId1));
        }

        [Fact]
        public async Task CanSendIM_Exception_SingleGroup()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSendIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public async Task CanSendIM_Exception_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSendIM("Hello world", groupId1, "Group name"));
        }

        [Fact]
        public async Task CanSendIM_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:{userId2}=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Permissions.CanSendIM("Hello world", userId1));
            Assert.False(_rlv.Permissions.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public async Task CanSendIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Permissions.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.Permissions.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public async Task CanSendIM_Secure_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.Permissions.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public async Task CanSendIMTo()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@sendimto:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanSendIM("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public async Task CanSendIMTo_Group()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@sendimto:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.Permissions.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public async Task CanSendIMTo_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@sendimto:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.Permissions.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

        #region @startim @startimto

        [Fact]
        public void CanStartIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Permissions.CanStartIM(null));
            Assert.True(_rlv.Permissions.CanStartIM(userId1));
        }

        [Fact]
        public async Task CanStartIM()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanStartIM(null));
            Assert.False(_rlv.Permissions.CanStartIM(userId1));
        }

        [Fact]
        public async Task CanStartIM_Exception()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@startim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanStartIM(userId1));
            Assert.False(_rlv.Permissions.CanStartIM(userId2));
        }

        [Fact]
        public async Task CanStartIMTo()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@startimto:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanStartIM(userId1));
            Assert.False(_rlv.Permissions.CanStartIM(userId2));
        }

        #endregion

        #region @recvim @recvim_sec @recvimto @recvimfrom

        [Fact]
        public void CanReceiveIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello", userId1));
            Assert.True(_rlv.Permissions.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public async Task CanReceiveIM()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveIM("Hello", userId1));
            Assert.False(_rlv.Permissions.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public async Task CanReceiveIM_Exception()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", userId1));
        }

        [Fact]
        public async Task CanReceiveIM_Exception_SingleGroup()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public async Task CanReceiveIM_Exception_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "Group name"));
        }

        [Fact]
        public async Task CanReceiveIM_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:{userId2}=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", userId1));
            Assert.False(_rlv.Permissions.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public async Task CanReceiveIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public async Task CanReceiveIM_Secure_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            await _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            await _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public async Task CanReceiveIMFrom()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@recvimfrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveIM("Hello world", userId1));
            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public async Task CanReceiveIMFrom_Group()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@recvimfrom:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.Permissions.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public async Task CanReceiveIMTo_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            await _rlv.ProcessMessage($"@recvimfrom:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Permissions.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.Permissions.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

    }
}
