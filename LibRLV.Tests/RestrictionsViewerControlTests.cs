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
            _actionCallbacks
                .Setup(e => e.SetDebugAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage($"@setdebug_{settingName}:{settingValue}=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.SetDebugAsync(settingName.ToLower(), settingValue, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SetDebug_Invalid()
        {
            _actionCallbacks
                .Setup(e => e.SetDebugAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Assert.False(await _rlv.ProcessMessage($"@setdebug_:42=force", _sender.Id, _sender.Name));

            // Assert
            _actionCallbacks.VerifyNoOtherCalls();
        }
        #endregion

        #region @getdebug_<setting>=<channel_number>
        [Theory]
        [InlineData("RenderResolutionDivisor", "RenderResolutionDivisor Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public async Task GetDebug_Default(string settingName, string settingValue)
        {
            var actual = _actionCallbacks.RecordReplies();

            _queryCallbacks.Setup(e =>
                e.TryGetDebugSettingValueAsync(settingName.ToLower(), default)
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
            _actionCallbacks
                .Setup(e => e.SetEnvAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _rlv.ProcessMessage($"@setenv_{settingName}:{settingValue}=force", _sender.Id, _sender.Name);

            // Assert
            _actionCallbacks.Verify(e =>
                e.SetEnvAsync(settingName.ToLower(), settingValue, It.IsAny<CancellationToken>()),
                Times.Once);

            _actionCallbacks.VerifyNoOtherCalls();
        }

        #endregion

        #region @getenv_<setting>=<channel_number>

        [Theory]
        [InlineData("Daytime", "Daytime Success")]
        [InlineData("Unknown Setting", "Unknown Setting Success")]
        public async Task GetEnv_Default(string settingName, string settingValue)
        {
            var actual = _actionCallbacks.RecordReplies();

            _queryCallbacks.Setup(e =>
                e.TryGetEnvironmentSettingValueAsync(settingName.ToLower(), default)
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
