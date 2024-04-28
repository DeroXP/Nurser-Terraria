using Terraria;
using Terraria.ModLoader;

namespace Nurser.Buffs
{
    public class HeartAche : ModBuff
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffTime[buffIndex]--;
            if (player.buffTime[buffIndex] <= 0) //this doesn't do anything sorta
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
// btw this is mostly bullshit so no need for that this is just a place holder buff that all it does is stays in player for sixty seconds, also for some reason in the newest version of tmod i can't do Description.SetDefault because no i guess.
