using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ID;
using System;
using Nurser.Buffs;
using Terraria.ModLoader.Config;

namespace Nurser
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [LabelKey("$Config.HeartAcheDuration.Label")]
        [TooltipKey("$Config.HeartAcheDuration.Tooltip")]
        [System.ComponentModel.DefaultValue(120)]
        [Range(30, 600)]
        public int HeartAcheDuration;

        [LabelKey("$Config.MaxCoinCost.Label")]
        [TooltipKey("$Config.MaxCoinCost.Tooltip")]
        [System.ComponentModel.DefaultValue(2000000)]

        public int MaxCoinCost;

        [LabelKey("$Config.HealthThreshold.Label")]
        [TooltipKey("$Config.HealthThreshold.Tooltip")]
        [System.ComponentModel.DefaultValue(20)]
        [Range(1, 100)]

        public int HealthThreshold;
    }

    public class HealKeyMod : Mod
    {
        #pragma warning disable CA2211
        public static ModKeybind HealKey;
        public static int maxCoinCost = ModContent.GetInstance<Config>().MaxCoinCost;
        public override void Load()
        {
            HealKey = KeybindLoader.RegisterKeybind(this, "Heal Key", Keys.G);
        }
    }

    public class HealKeyPlayer : ModPlayer
    {
        bool hasDisplayedMessage = false;
        public override void ProcessTriggers(TriggersSet triggers)
        {
            Config config = ModContent.GetInstance<Config>();

            if (HealKeyMod.HealKey.JustPressed || IsHealthBelowThreshold(config.HealthThreshold / 100.0f))
            {
                if (!Player.HasBuff<HeartAche>())
                {
                    int coinCost = CalculateCoinCost(Main.LocalPlayer.statLife, Main.LocalPlayer.statLifeMax2);
                    if (HasEnoughCoins(coinCost))
                    {
                        if (!Main.LocalPlayer.dead)
                        {
                            Main.LocalPlayer.statLife = Main.LocalPlayer.statLifeMax2;
                            Main.LocalPlayer.HealEffect(Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife);
                            SubtractCoins(coinCost);
                            for (int i = 0; i < 10; i++)
                            {
                                Dust.NewDust(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, DustID.Blood);
                            }
                            int buffDuration = config.HeartAcheDuration * 60;
                            Player.AddBuff(ModContent.BuffType<HeartAche>(), buffDuration);
                            hasDisplayedMessage = false;
                        }
                    }
                    else
                    {
                        if (!hasDisplayedMessage)
                        {
                            Main.NewText("You don't have enough coins to perform this action!", 255, 50, 50);
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

        private int CalculateCoinCost(int currentHealth, int maxHealth)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            int coinCost = (int)Math.Ceiling(HealKeyMod.maxCoinCost * (1f - healthPercentage));
            return coinCost;
        }

        private bool HasEnoughCoins(int amount)
        {
            int totalCoins = GetTotalCoins(Main.LocalPlayer.inventory) + GetTotalCoins(Main.LocalPlayer.bank.item);
            return totalCoins >= amount;
        }

        private int GetTotalCoins(Item[] items)
        {
            int totalCoins = 0;
            foreach (Item item in items)
            {
                switch (item.type)
                {
                    case ItemID.CopperCoin:
                        totalCoins += item.stack;
                        break;
                    case ItemID.SilverCoin:
                        totalCoins += item.stack * 100;
                        break;
                    case ItemID.GoldCoin:
                        totalCoins += item.stack * 10000;
                        break;
                    case ItemID.PlatinumCoin:
                        totalCoins += item.stack * 1000000;
                        break;
                }
            }
            return totalCoins;
        }

        private void SubtractCoins(int amount)
        {
            int copperCoins = amount % 100;
            int silverCoins = amount / 100 % 100;
            int goldCoins = amount / 10000 % 100;
            int platinumCoins = amount / 1000000;

            SubtractCoinsFromInventory(ItemID.CopperCoin, copperCoins);
            SubtractCoinsFromInventory(ItemID.SilverCoin, silverCoins);
            SubtractCoinsFromInventory(ItemID.GoldCoin, goldCoins);
            SubtractCoinsFromInventory(ItemID.PlatinumCoin, platinumCoins);
        }

        private bool SubtractCoinsFromInventory(int coinType, int amount)
        {
            // inventory
            for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++)
            {
                Item item = Main.LocalPlayer.inventory[i];
                if (item.type == coinType && item.stack > 0)
                {
                    int coinsToSubtract = Math.Min(item.stack, amount);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract;
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
            }

            // piggy bank
            for (int i = 0; i < Main.LocalPlayer.bank.item.Length; i++)
            {
                Item item = Main.LocalPlayer.bank.item[i];
                if (item.type == coinType && item.stack > 0)
                {
                    int coinsToSubtract = Math.Min(item.stack, amount);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract;
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
