using Hacknet;
using Pathfinder.Util;

namespace KT0Mods.Tags
{
    public class RAMSetter : Pathfinder.Action.DelayablePathfinderAction
    {
        [XMLStorage] public float ram;
        [XMLStorage] public float Ram;
        [XMLStorage] public float RAM;

        public override void Trigger(OS os)
        {
            var r = (int)(ram != default ? ram : (Ram != default ? Ram : RAM));
            os.ramAvaliable += r - os.totalRam;
            os.totalRam = r - (OS.TOP_BAR_HEIGHT + 2);
        }
    }
}

