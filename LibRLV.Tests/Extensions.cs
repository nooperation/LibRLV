using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRLV.Tests
{
    public static class CallbackMockExtensions
    {
        public static List<(int Channel, string Text)> RecordReplies(this Mock<IRLVCallbacks> mock)
        {
            var list = new List<(int Channel, string Text)>();

            mock
                .Setup(m => m.SendReplyAsync(
                            It.IsAny<int>(),
                            It.IsAny<string>(),
                            It.IsAny<CancellationToken>())
                )
                .Callback<int, string, CancellationToken>(
                    (ch, txt, _) => list.Add((ch, txt))
                );

            return list;
        }
    }
}
