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
        [Range(60, 600)]
        public int HeartAcheDuration;

        [LabelKey("$Config.MaxCoinCost.Label")]
        [TooltipKey("$Config.MaxCoinCost.Tooltip")]
        [System.ComponentModel.DefaultValue(2000000)]//two plat
        [Range(60, 600)]

        public int MaxCoinCost;
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
            if (HealKeyMod.HealKey.JustPressed || IsHealthBelowThreshold(0.2f))
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
                            int buffDuration = ModContent.GetInstance<Config>().HeartAcheDuration * 60;
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
            if (SubtractCoinsFromInventory(amount))
            {
                return;
            }
            SubtractCoinsFromPiggyBank(amount - GetTotalCoins(Main.LocalPlayer.bank.item));
        }

        private bool SubtractCoinsFromInventory(int amount)
        {
            foreach (Item item in Main.LocalPlayer.inventory)
            {
                if (item.type == ItemID.CopperCoin)
                {
                    int coinsToSubtract = Math.Min(amount, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract;
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
                else if (item.type == ItemID.SilverCoin)
                {
                    int coinsToSubtract = Math.Min(amount / 100, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract * 100;
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
                else if (item.type == ItemID.GoldCoin)
                {
                    int coinsToSubtract = Math.Min(amount / 10000, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract * 10000;
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
                else if (item.type == ItemID.PlatinumCoin)
                {
                    int coinsToSubtract = Math.Min(amount / 1000000, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract * 1000000;
                    if (amount <= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SubtractCoinsFromPiggyBank(int amount)
        {
            foreach (Item item in Main.LocalPlayer.bank.item)
            {
                if (item.type == ItemID.CopperCoin)
                {
                    int coinsToSubtract = Math.Min(amount, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract;
                    if (amount <= 0)
                    {
                        return;
                    }
                }
                else if (item.type == ItemID.SilverCoin)
                {
                    int coinsToSubtract = Math.Min(amount / 100, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract * 100;
                    if (amount <=0)
                    {
                        return;
                    }
                }
                else if (item.type == ItemID.GoldCoin)
                {
                    int coinsToSubtract = Math.Min(amount / 10000, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract * 10000;
                    if (amount <= 0)
                    {
                        return;
                    }
                }
                else if (item.type == ItemID.PlatinumCoin)
                {
                    int coinsToSubtract = Math.Min(amount / 1000000, item.stack);
                    item.stack -= coinsToSubtract;
                    amount -= coinsToSubtract * 1000000;
                    if (amount <= 0)
                    {
                        return;
                    }
                }
            }
        }
    }
}
