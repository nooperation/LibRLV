using LibRLV;
using LibRLV.EventArguments;
using System.Linq;

namespace TempClient
{
    internal class Program
    {
        public Program()
        {
            var rlv = new RLV
            {
                Enabled = true
            };

            rlv.Actions.TpTo += Rlv_TpTo;
            rlv.Get.SendReplyAsync = RLVSendMessage;
            rlv.Get.DataProviderAsync = RLVDataProvider;

            rlv.ProcessMessage("@accepttprequest=y", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");
            rlv.ProcessMessage($"@accepttprequest:{new OpenMetaverse.UUID(Guid.NewGuid())}=n", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");
            rlv.ProcessMessage("@getstatusall=123", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");

            rlv.ProcessMessage("@getsitid=123", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");

            //rlv.ProcessMessage("@tpto:1/2/3=force", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");
            //rlv.ProcessMessage("@tpto:My Land/1/2/3=force", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");
            rlv.ProcessMessage("@tpto:My Land/1/2/3;3.1415=force", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");

            rlv.Blacklist.BlacklistCommand("getstatusall");
            rlv.ProcessMessage("@getstatusall=1234", new OpenMetaverse.UUID(Guid.NewGuid()), "Sender Name");

        }

        private Task RLVSendMessage(int channel, string message, CancellationToken token)
        {
            Console.WriteLine($"[{channel}] {message}");
            return Task.CompletedTask;
        }

        private Task<string> RLVDataProvider(RLVDataRequest request, List<object> list, CancellationToken token)
        {
            Console.WriteLine($"DataProvider: {request} {string.Join(',', list)}");

            return Task.FromResult(new OpenMetaverse.UUID(Guid.NewGuid()).ToString());
        }

        private void Rlv_TpTo(object? sender, TpToEventArgs e)
        {
            Console.WriteLine($"Tp To: {e.X}/{e.Y}/{e.Z} Region={e.RegionName} Lookat={e.Lookat}");
        }

        static void Main()
        {
            new Program();
        }
    }
}
