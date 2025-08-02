using Moq;

namespace LibRLV.Tests
{
    public class RestrictionsBase
    {
        public record RlvObject(string Name, Guid Id);

        protected readonly RlvObject _sender;
        protected readonly Mock<IRLVCallbacks> _callbacks;
        protected readonly RLV _rlv;

        public const float FloatTolerance = 0.00001f;

        public RestrictionsBase()
        {
            _sender = new RlvObject("Sender 1", new Guid("ffffffff-ffff-4fff-8fff-ffffffffffff"));
            _callbacks = new Mock<IRLVCallbacks>();
            _rlv = new RLV(_callbacks.Object, true);
        }

        protected void CheckSimpleCommand(string cmd, Func<RLVManager, bool> canFunc)
        {
            _rlv.ProcessMessage($"@{cmd}=n", _sender.Id, _sender.Name);
            Assert.False(canFunc(_rlv.Restrictions));

            _rlv.ProcessMessage($"@{cmd}=y", _sender.Id, _sender.Name);
            Assert.True(canFunc(_rlv.Restrictions));
        }

        //
        // RLVA stuff to implement
        //

        // @getattachnames[:<grp>]=<channel>
        // @getaddattachnames[:<grp>]=<channel>
        // @getremattachnames[:<grp>]=<channel>
        // @getoutfitnames=<channel>
        // @getaddoutfitnames=<channel>
        // @getremoutfitnames=<channel>

        // @fly:[true|false]=force

        // @setcam_eyeoffset[:<vector3>]=force,
        // @setcam_eyeoffsetscale[:<float>]=force
        // @setcam_focusoffset[:<vector3>]=force
        // @setcam_focus:<uuid>[;<dist>[;<direction>]]=force
        // @setcam_mode[:<option>]=force

        // @setcam_focusoffset:<vector3>=n|y
        // @setcam_eyeoffset:<vector3>=n|y
        // @setcam_eyeoffsetscale:<float>=n|y
        // @setcam_mouselook=n|y
        // @setcam=n|y

        // @getcam_avdist=<channel>
        // @getcam_textures=<channel>


        // @setoverlay_tween:[<alpha>];[<tint>];<duration>=force
        // @setoverlay=n|y
        // @setoverlay_touch=n
        // @setsphere=n|y

        // @getcommand[:<behaviour>[;<type>[;<separator>]]]=<channel>
        // @getheightoffset=<channel>

        // @buy=n|y
        // @pay=n|y

        // @showself=n|y
        // @showselfhead=n|y 
        // @viewtransparent=n|y
        // @viewwireframe=n|y


        // Probably don't care about/not going to touch:
        //  @bhvr=n|y
        //  @bhvr:<uuid>=n|y
        //  @bhvr[:<uuid>]=n|y
        //  @bhvr:<modifier>=n|y
        //  @bhvr:<global modifier>=n|y
        //  @bhvr:<local modifier>=force
        //  @bhvr:<modifier>=force

    }
}
