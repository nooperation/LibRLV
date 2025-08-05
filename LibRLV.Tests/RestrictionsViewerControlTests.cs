using LibRLV.EventArguments;
using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsViewerControlTests : RestrictionsBase
    {

        #region @setdebug=<y/n>
        [Fact]
        public async Task CanSetDebug()
        {
            await CheckSimpleCommand("setDebug", m => m.CanSetDebug());
        }
        #endregion

        #region @setdebug_<setting>:<value>=force
        [Theory]
        [InlineData("RenderResolutionDivisor", "RenderResolutionDivisor Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public async Task SetDebug_Default(string settingName, string settingValue)
        {
            var raised = await Assert.RaisesAsync<SetSettingEventArgs>(
                 attach: n => _rlv.Commands.SetDebug += n,
                 detach: n => _rlv.Commands.SetDebug -= n,
                 testCode: () => _rlv.ProcessMessage($"@setdebug_{settingName}:{settingValue}=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(raised.Arguments.SettingName, settingName.ToLower());
            Assert.Equal(raised.Arguments.SettingValue, settingValue);
        }

        [Fact]
        public async Task SetDebug_Invalid()
        {
            var eventRaised = false;
            _rlv.Commands.SetDebug += (sender, args) =>
            {
                eventRaised = true;
            };

            Assert.False(await _rlv.ProcessMessage($"@setdebug_:42=force", _sender.Id, _sender.Name));
            Assert.False(eventRaised);
        }
        #endregion

        #region @getdebug_<setting>=<channel_number>
        [Theory]
        [InlineData("RenderResolutionDivisor", "RenderResolutionDivisor Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public async Task GetDebug_Default(string settingName, string settingValue)
        {
            var actual = _callbacks.RecordReplies();

            _callbacks.Setup(e =>
                e.TryGetDebugInfoAsync(settingName.ToLower())
            ).ReturnsAsync((true, settingValue));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, settingValue),
            };

            Assert.True(await _rlv.ProcessMessage($"@getdebug_{settingName}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }
        #endregion

        #region @setenv=<y/n>
        [Fact]
        public async Task CanSetEnv()
        {
            await CheckSimpleCommand("setEnv", m => m.CanSetEnv());
        }
        #endregion

        #region @setenv_<setting>:<value>=force

        [Theory]
        [InlineData("Daytime", "Daytime Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public async Task SetEnv_Default(string settingName, string settingValue)
        {
            var raised = await Assert.RaisesAsync<SetSettingEventArgs>(
                 attach: n => _rlv.Commands.SetEnv += n,
                 detach: n => _rlv.Commands.SetEnv -= n,
                 testCode: () => _rlv.ProcessMessage($"@setenv_{settingName}:{settingValue}=force", _sender.Id, _sender.Name)
             );

            Assert.Equal(raised.Arguments.SettingName, settingName.ToLower());
            Assert.Equal(raised.Arguments.SettingValue, settingValue);
        }

        #endregion

        #region @getenv_<setting>=<channel_number>

        [Theory]
        [InlineData("Daytime", "Daytime Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public async Task GetEnv_Default(string settingName, string settingValue)
        {
            var actual = _callbacks.RecordReplies();

            _callbacks.Setup(e =>
                e.TryGetEnvironmentAsync(settingName.ToLower())
            ).ReturnsAsync((true, settingValue));

            var expected = new List<(int Channel, string Text)>
            {
                (1234, settingValue),
            };

            Assert.True(await _rlv.ProcessMessage($"@getenv_{settingName}=1234", _sender.Id, _sender.Name));
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
