namespace LibRLV.Tests
{
    public class RestrictionsChatTests : RestrictionsBase
    {

        #region @sendChat
        [Fact]
        public void CanSendChat()
        {
            CheckSimpleCommand("sendChat", m => m.CanSendChat());
        }
        #endregion

        #region @chatshout
        [Fact]
        public void CanChatShout()
        {
            CheckSimpleCommand("chatShout", m => m.CanChatShout());
        }
        #endregion

        #region @chatnormal
        [Fact]
        public void CanChatNormal()
        {
            CheckSimpleCommand("chatNormal", m => m.CanChatNormal());
        }
        #endregion

        #region @chatwhisper
        [Fact]
        public void CanChatWhisper()
        {
            CheckSimpleCommand("chatWhisper", m => m.CanChatWhisper());
        }
        #endregion

        #region @redirchat

        [Fact]
        public void IsRedirChat()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));

            var expected = new List<int>
            {
                1234,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirChat_Removed()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:1234=rem", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.IsRedirChat(out var channels));
        }

        [Fact]
        public void IsRedirChat_MultipleChannels()
        {
            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));

            var expected = new List<int>
            {
                1234,
                12345,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirChat_RedirectChat()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));
            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirChat_RedirectChatMultiple()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@redirchat:5678=add", _sender.Id, _sender.Name);

            _rlv.ReportSendPublicMessage("Hello World");
            _rlv.Restrictions.IsRedirChat(out var channels);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "Hello World"),
                (5678, "Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirChat_RedirectChatEmote()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@redirchat:1234=add", _sender.Id, _sender.Name);

            _rlv.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.Restrictions.IsRedirChat(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanReceiveChat @recvchat @recvchat_sec @recvchatfrom @recvemote @recvemote_sec @recvemotefrom

        [Fact]
        public void CanRecvChat_Default()
        {
            var userId1 = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");
            var userId2 = new Guid("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb");

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat()
        {
            _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            var userId = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_Except()
        {
            var userId = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            _rlv.ProcessMessage("@recvchat=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvchat:{userId}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvchat_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvchat:{userId1}=add", sender2.Id, sender2.Name);
            _rlv.ProcessMessage($"@recvchat:{userId2}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote()
        {
            _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);

            var userId = new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa");

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", null));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", null));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId));
        }

        [Fact]
        public void CanRecvChat_RecvEmoteFrom()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvemotefrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote_Except()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvemote=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvemote:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChat_RecvEmote_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvemote_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvemote:{userId1}=add", sender2.Id, sender2.Name);
            _rlv.ProcessMessage($"@recvemote:{userId2}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
            Assert.False(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId2));
        }

        [Fact]
        public void CanRecvChatFrom()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvchatfrom:{userId1}=add", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveChat("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveChat("/me says Hello world", userId1));

            Assert.True(_rlv.Restrictions.CanReceiveChat("Hello world", userId2));
        }

        #endregion

        #region @sendGesture

        [Fact]
        public void CanSendGesture()
        {
            CheckSimpleCommand("sendGesture", m => m.CanSendGesture());
        }

        #endregion

        #region @emote
        [Fact]
        public void CanEmote()
        {
            CheckSimpleCommand("emote", m => m.CanEmote());
        }

        // TODO: Check 'ProcessChat' funcationality (not yet created, but the function doesn't exist yet) to make
        //       sure it no longer censors emotes on @chat=n
        #endregion

        #region @rediremote:<channel_number>=<rem/add>
        [Fact]
        public void IsRedirEmote()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));

            var expected = new List<int>
            {
                1234,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirEmote_Removed()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:1234=rem", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.IsRedirEmote(out var channels));
        }

        [Fact]
        public void IsRedirEmote_MultipleChannels()
        {
            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:12345=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));

            var expected = new List<int>
            {
                1234,
                12345,
            };

            Assert.Equal(expected, channels);
        }

        [Fact]
        public void IsRedirEmote_RedirectEmote()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportSendPublicMessage("/me says Hello World");

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));
            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/me says Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirEmote_RedirectEmoteMultiple()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@rediremote:5678=n", _sender.Id, _sender.Name);

            _rlv.ReportSendPublicMessage("/me says Hello World");
            _rlv.Restrictions.IsRedirEmote(out var channels);

            var expected = new List<(int Channel, string Text)>
            {
                (1234, "/me says Hello World"),
                (5678, "/me says Hello World"),
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsRedirEmote_RedirectEmoteChat()
        {
            var actual = _callbacks.RecordReplies();

            _rlv.ProcessMessage("@rediremote:1234=add", _sender.Id, _sender.Name);
            _rlv.ReportSendPublicMessage("Hello World");

            Assert.True(_rlv.Restrictions.IsRedirEmote(out var channels));
            Assert.Empty(actual);
        }

        #endregion

        #region CanChat @sendchat @sendchannel @sendchannel_sec @sendchannel_except

        [Fact]
        public void CanChat_Default()
        {
            Assert.True(_rlv.Restrictions.CanChat(0, "Hello"));
            Assert.True(_rlv.Restrictions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Restrictions.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanChat_SendChatRestriction()
        {
            _rlv.ProcessMessage("@sendchat=n", _sender.Id, _sender.Name);

            // No public chat allowed unless it starts with '/'
            Assert.False(_rlv.Restrictions.CanChat(0, "Hello"));

            // Emotes and other messages starting with / are allowed
            Assert.True(_rlv.Restrictions.CanChat(0, "/me says Hello"));
            Assert.True(_rlv.Restrictions.CanChat(0, "/ something?"));

            // Messages containing ()"-*=_^ are prohibited
            Assert.False(_rlv.Restrictions.CanChat(0, "/me says Hello ^_^"));

            // Private channels are not impacted
            Assert.True(_rlv.Restrictions.CanChat(5, "Hello"));
        }

        [Fact]
        public void CanSendChannel_Default()
        {
            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel()
        {
            _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel_Exception()
        {
            _rlv.ProcessMessage("@sendchannel=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
        }

        [Fact]
        public void CanSendChannel_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@sendchannel_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:123=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage("@sendchannel:456=n", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
            Assert.False(_rlv.Restrictions.CanChat(456, "Hello world"));
        }

        [Fact]
        public void CanSendChannelExcept()
        {
            _rlv.ProcessMessage("@sendchannel_except:456=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanChat(123, "Hello world"));
            Assert.False(_rlv.Restrictions.CanChat(456, "Hello world"));
        }

        #endregion

        #region @sendim @sendim_sec @sendimto

        [Fact]
        public void CanSendIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanSendIM("Hello", userId1));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello", userId1));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM_Exception()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", userId1));
        }

        [Fact]
        public void CanSendIM_Exception_SingleGroup()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public void CanSendIM_Exception_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group name"));
        }

        [Fact]
        public void CanSendIM_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:{userId2}=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", userId1));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public void CanSendIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:Group Name=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanSendIM_Secure_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@sendim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@sendim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanSendIMTo()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", userId2));
        }

        [Fact]
        public void CanSendIMTo_Group()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.Restrictions.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public void CanSendIMTo_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@sendimto:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.Restrictions.CanSendIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

        #region @startim @startimto

        [Fact]
        public void CanStartIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanStartIM(null));
            Assert.True(_rlv.Restrictions.CanStartIM(userId1));
        }

        [Fact]
        public void CanStartIM()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanStartIM(null));
            Assert.False(_rlv.Restrictions.CanStartIM(userId1));
        }

        [Fact]
        public void CanStartIM_Exception()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@startim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@startim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanStartIM(userId1));
            Assert.False(_rlv.Restrictions.CanStartIM(userId2));
        }

        [Fact]
        public void CanStartIMTo()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@startimto:{userId2}=n", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanStartIM(userId1));
            Assert.False(_rlv.Restrictions.CanStartIM(userId2));
        }

        #endregion

        #region @recvim @recvim_sec @recvimto @recvimfrom

        [Fact]
        public void CanReceiveIM_Default()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello", userId1));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello", userId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM_Exception()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", userId1));
        }

        [Fact]
        public void CanReceiveIM_Exception_SingleGroup()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group Name"));
        }

        [Fact]
        public void CanReceiveIM_Exception_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group name"));
        }

        [Fact]
        public void CanReceiveIM_Secure()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId1}=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:{userId2}=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", userId1));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public void CanReceiveIM_Secure_Group()
        {
            var sender2 = new RlvObject("Sender 2", new Guid("aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa"));

            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:Group Name=add", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", sender2.Id, sender2.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanReceiveIM_Secure_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");

            _rlv.ProcessMessage("@recvim_sec=n", _sender.Id, _sender.Name);
            _rlv.ProcessMessage($"@recvim:allgroups=add", _sender.Id, _sender.Name);

            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Group Name"));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "Another Group"));
        }

        [Fact]
        public void CanReceiveIMFrom()
        {
            var userId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var userId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:{userId1}=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", userId1));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", userId2));
        }

        [Fact]
        public void CanReceiveIMFrom_Group()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:First Group=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.True(_rlv.Restrictions.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        [Fact]
        public void CanReceiveIMTo_AllGroups()
        {
            var groupId1 = new Guid("00000000-0000-4000-8000-000000000000");
            var groupId2 = new Guid("11111111-1111-4111-8111-111111111111");

            _rlv.ProcessMessage($"@recvimfrom:allgroups=n", _sender.Id, _sender.Name);

            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId1, "First Group"));
            Assert.False(_rlv.Restrictions.CanReceiveIM("Hello world", groupId2, "Second Group"));
        }

        #endregion

    }
}
