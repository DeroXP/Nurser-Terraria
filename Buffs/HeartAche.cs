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

        //buff is used as an placeholder for time
        //you could make this so it also give the player slower health regen values
        public override void Update(ref int buffIndex)
        {
            Main.LocalPlayer.lifeRegen -= 2; // lower health regen by 2
        }
    }
}
