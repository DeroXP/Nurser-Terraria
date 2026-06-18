using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ID;
using System;
using Nurser.Buffs;
using Terraria.ModLoader.Config;
using System.Linq;
using Microsoft.Xna.Framework;
using tModPorter.Rewriters;
using Terraria.Audio;
using System.Threading.Tasks;

namespace Nurser // side note there isn't much explainations this time if you want more just go to earlier versions of this file on github
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide; // could try to make this so it'll work server sided but am too lazy

        [Header("Configs")]

        [LabelKey("$Config.HeartAcheDuration.Label")]
        [TooltipArgs("$Config.HeartAcheDuration.Tooltip")]
        [System.ComponentModel.DefaultValue(120)]
        [Range(5, 600)]
        public int HeartAcheDuration;

        [LabelKey("$Config.CoinCostPerHealth.Label")]
        [TooltipArgs("$Config.CoinCostPerHealth.Tooltip")]
        [System.ComponentModel.DefaultValue(100)]
        [Range(10, float.PositiveInfinity)]
        public int CoinCostPerHealth;

        [LabelKey("$Config.HealthThreshold.Label")]
        [TooltipArgs("$Config.HealthThreshold.Tooltip")]
        [System.ComponentModel.DefaultValue(20)]
        [Range(1, 100)]
        public int HealthThreshold;

        [LabelKey("$Config.RequireBoss.Label")]
        [TooltipArgs("$Config.RequireBoss.Tooltip")]
        [System.ComponentModel.DefaultValue(false)]
        public bool RequireBoss;

        [LabelKey("$Config.CostMultiplier.Label")]
        [TooltipArgs("$Config.CostMultiplier.Tooltip")]
        [System.ComponentModel.DefaultValue(false)]
        public bool CostMultiplier; //didn't have many ideas as of recent so quick and kinda forced update

        [LabelKey("$Config.MultiplierAmount.Label")]
        [TooltipArgs("$Config.MultiplierAmount.Tooltip")]
        [System.ComponentModel.DefaultValue(2)]
        [Range(1, 100)]
        public int MultiplierAmount;
    }

    public class HealKeyMod : Mod
    {
        #pragma warning disable CA2211
        public static ModKeybind HealKey;
        public override void Load()
        {
            HealKey = KeybindLoader.RegisterKeybind(this, "Heal Key", Keys.G);
        }
    }

    public class HealKeyPlayer : ModPlayer
    {
        Config config = ModContent.GetInstance<Config>();

        public override void OnEnterWorld()
        {
            Main.NewText("If you have an idea or issue/bug with this mod please go here and send an issue. (https://github.com/DeroXP/Nurser-Terraria/issues)", 255, 182, 193);
            
            var assignedKeys = HealKeyMod.HealKey.GetAssignedKeys();
            string healKeyText = assignedKeys.Count > 0 ? assignedKeys[0] : "['UNBOUND' PLEASE ASSIGN KEY IN CONTROLS]";
            
            Main.NewText("{From Nurser Mod:} Press " + healKeyText + " or when health is at " + config.HealthThreshold + "% health to heal.", 121, 6, 4);
        }

        bool hasDisplayedMessage = false;
        public override void ProcessTriggers(TriggersSet triggers)
        {
            if (HealKeyMod.HealKey.JustPressed || IsHealthBelowThreshold(config.HealthThreshold / 100.0f))
            {
                if (config.RequireBoss && !IsBossActive())
                {
                    if (!hasDisplayedMessage)
                    {
                        Main.NewText("You can't heal right now. Wait until a boss is active.", 255, 50, 50);
                        hasDisplayedMessage = true;
                    }
                    return;
                }

                if (!Player.HasBuff<HeartAche>())
                {
                    if (Main.LocalPlayer.dead || Main.LocalPlayer.statLife >= Main.LocalPlayer.statLifeMax2)
                    {
                        // Nothing to heal: avoid charging the player or applying HeartAche for free.
                        return;
                    }

                    int coinCost = CalculateCoinCost(Main.LocalPlayer.statLife, Main.LocalPlayer.statLifeMax2, config.CoinCostPerHealth);
                    if (Main.LocalPlayer.CanBuyItem(coinCost))
                    {
                        int healedAmount = Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife;
                        Main.LocalPlayer.statLife = Main.LocalPlayer.statLifeMax2;

                        string message = FormatCoins(coinCost) + " were spent.";
                        Color messageColor = new(224, 224, 224);
                        CombatText.NewText(Main.LocalPlayer.getRect(), messageColor, message);

                        Main.LocalPlayer.HealEffect(healedAmount);

                        Main.LocalPlayer.BuyItem(coinCost);

                        SoundEngine.PlaySound(SoundID.Item29);
                        for (int i = 0; i < 10; i++)
                        {
                            Dust.NewDust(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, DustID.CoralTorch);
                        }

                        int buffDuration = config.HeartAcheDuration * 60;
                        Player.AddBuff(ModContent.BuffType<HeartAche>(), buffDuration);

                        hasDisplayedMessage = false;
                    }
                    else
                    {
                        if (!hasDisplayedMessage)
                        {
                            Main.NewText($"You don't have enough coins, {FormatCoins(coinCost)} needed!", 255, 50, 50);
                            hasDisplayedMessage = true;
                        }
                    }
                }
                else
                {
                    if (!hasDisplayedMessage)
                    {
                        Main.NewText("You can't heal right now. Wait until HeartAche wears off.", 255, 50, 50);
                        hasDisplayedMessage = true;
                    }
                }
            }
        }

        private bool IsHealthBelowThreshold(float threshold)
        {
            float healthPercentage = (float)Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2;
            return healthPercentage <= threshold;
        }

        private int CalculateCoinCost(int currentHealth, int maxHealth, int coinCostPerHealth)
        {
            int coinCost = (maxHealth - currentHealth) * coinCostPerHealth;

            if (config.CostMultiplier && config.MultiplierAmount > 0)
            {
                coinCost *= config.MultiplierAmount;
            }

            return coinCost;
        }

        // Returns true if any boss-type NPC is currently active anywhere in the world.
        private static bool IsBossActive()
        {
            int[] bosses = { NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.EaterofWorldsHead, NPCID.BrainofCthulhu, NPCID.QueenBee, NPCID.SkeletronHead, NPCID.WallofFlesh, NPCID.Retinazer, NPCID.Spazmatism, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.Plantera, NPCID.Golem, NPCID.DukeFishron, NPCID.CultistBoss, NPCID.MoonLordCore };

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && (npc.boss || bosses.Contains(npc.type)))
                {
                    return true;
                }
            }

            return false;
        }

        // Formats a copper-denominated amount into a "P platinum, G gold, S silver, and C copper coins" string.
        private static string FormatCoins(int amount)
        {
            int platinum = amount / 1000000;
            int gold = amount % 1000000 / 10000;
            int silver = amount % 10000 / 100;
            int copper = amount % 100;

            return $"{platinum} platinum, {gold} gold, {silver} silver, and {copper} copper coins";
        }
    }
}
