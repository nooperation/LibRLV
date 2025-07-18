using LibRLV;
using LibRLV.EventArguments;
using OpenMetaverse;
using System.Collections.Generic;
using System.Linq;

namespace TempClient
{
    internal class Program
    {
        public class RlvObject
        {
            public RlvObject(string name)
            {
                Id = new UUID(Guid.NewGuid());
                Name = name;
            }

            public UUID Id { get; set; }
            public string Name { get; set; }
        }

        public class MyRLVCallbacks : RLVCallbacksDefault
        {
            public override Task<string> ProvideDataAsync(RLVDataRequest request, List<object> list, CancellationToken cancellationToken)
            {
                Console.WriteLine($"DataProvider: {request} {string.Join(',', list)}");

                return Task.FromResult(new OpenMetaverse.UUID(Guid.NewGuid()).ToString());
            }

            public override Task SendReplyAsync(int channel, string message, CancellationToken cancellationToken)
            {
                Console.WriteLine($"[{channel}] {message}");
                return Task.CompletedTask;
            }
        }

        public Program()
        {
            var rlv = new RLV(new MyRLVCallbacks())
            {
                Enabled = true
            };

            rlv.Actions.TpTo += Rlv_TpTo;
            rlv.Restrictions.RestrictionUpdated += Restrictions_RestrictionUpdated;

            var object1 = new RlvObject("First");
            var object2 = new RlvObject("Second");

            rlv.ProcessMessage("@notify:5678=add", object1.Id, object1.Name);
            rlv.ProcessMessage("@accepttprequest=add", object1.Id, object1.Name);
            rlv.ProcessMessage($"@accepttprequest:{object1.Id}=add", object1.Id, object1.Name);
            rlv.ProcessMessage("@getstatusall=123", object1.Id, object1.Name);

            rlv.ProcessMessage("@getsitid=123", object1.Id, object1.Name);

            //rlv.ProcessMessage("@tpto:1/2/3=force", object1Id.Id,object1.Name);
            //rlv.ProcessMessage("@tpto:My Land/1/2/3=force", object1Id.Id,object1.Name);
            rlv.ProcessMessage("@tpto:My Land/1/2/3;3.1415=force", object1.Id, object1.Name);

            rlv.Blacklist.BlacklistCommand("getstatusall");
            rlv.ProcessMessage("@getstatusall=1234", object1.Id, object1.Name);

            rlv.ProcessMessage("@accepttprequest=rem", object1.Id, object1.Name);

        }

        private void Restrictions_RestrictionUpdated(object? sender, RestrictionUpdatedEventArgs e)
        {
            Console.WriteLine($"Restriction update: {(e.IsDeleted ? "REM" : "ADD")} {e.Restriction}");
        }

        private void Rlv_TpTo(object? sender, TpToEventArgs e)
        {
            Console.WriteLine($"Tp To: {e.X}/{e.Y}/{e.Z} Region={e.RegionName} Lookat={e.Lookat}");
        }

        static void Main()
        {
            _ = new Program();
        }
    }
}
