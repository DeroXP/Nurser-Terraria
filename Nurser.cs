using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
using Terraria.ID;
using System;
using Nurser.Buffs;

namespace Nurser
{
    public class HealKeyMod : Mod
    {
        public static ModKeybind HealKey; //mod config so Settings->Controls->Nurser->HealKey (change it from >UnBound< to any key you like.)
        public static int maxCoinCost = 2000000; // 2 platnium

        public override void Load()
        {
            HealKey = KeybindLoader.RegisterKeybind(this, "Heal Key", Keys.G); // pretty sure this doesn't do much
        }
    }

    public class HealKeyPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggers)
        {
            if (HealKeyMod.HealKey.JustPressed)
            {
                if (!Player.HasBuff<HeartAche>())
                {
                    int coinCost = CalculateCoinCost(Main.LocalPlayer.statLife, Main.LocalPlayer.statLifeMax2);
                    if (HasEnoughCoins(coinCost))
                    {
                        Main.LocalPlayer.statLife = Main.LocalPlayer.statLifeMax2;
                        Main.LocalPlayer.HealEffect(Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife);
                        SubtractCoins(coinCost);
                        for (int i = 0; i < 10; i++)
                        {
                            Dust.NewDust(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, DustID.Blood);
                        }
                        Player.AddBuff(ModContent.BuffType<HeartAche>(), 60 * 60); // a minute or sixty seconds
                    }
                    else
                    {
                        Main.NewText("You don't have enough coins to perform this action!", 255, 50, 50);
                    }
                }
                else
                {
                    Main.NewText("You can't heal right now. Wait until HeartAche wears off.", 255, 50, 50);
                }
            }
        }

        private int CalculateCoinCost(int currentHealth, int maxHealth)
        {
            float healthPercentage = (float)currentHealth / maxHealth;
            int coinCost = (int)(HealKeyMod.maxCoinCost * (1f - healthPercentage));
            return coinCost;
        }

        private bool HasEnoughCoins(int amount)
        {
            int totalCoins = GetTotalCoins(Main.LocalPlayer.inventory);
            if (totalCoins >= amount)
            {
                return true;
            }
            else
            {
                totalCoins += GetTotalCoins(Main.LocalPlayer.bank.item);
                return totalCoins >= amount;
            }
        }

        private int GetTotalCoins(Item[] items)
        {
            int totalCoins = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].type == ItemID.CopperCoin)
                {
                    totalCoins += items[i].stack;
                }
                else if (items[i].type == ItemID.SilverCoin)
                {
                    totalCoins += items[i].stack * 100;
                }
                else if (items[i].type == ItemID.GoldCoin)
                {
                    totalCoins += items[i].stack * 10000;
                }
                else if (items[i].type == ItemID.PlatinumCoin)
                {
                    totalCoins += items[i].stack * 1000000;
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
			
			SubtractCoinsFromPiggyBank(amount - GetTotalCoins(Main.LocalPlayer.inventory));
   			//not sure if these work they might.
		}

        private bool SubtractCoinsFromInventory(int amount)
		{
			for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++)
			{
				if (Main.LocalPlayer.inventory[i].type == ItemID.CopperCoin)
				{
					int coinsToSubtract = Math.Min(Main.LocalPlayer.inventory[i].stack, amount);
					Main.LocalPlayer.inventory[i].stack -= coinsToSubtract;
					amount -= coinsToSubtract;
				}
				else if (Main.LocalPlayer.inventory[i].type == ItemID.SilverCoin)
				{
					int coinsToSubtract = Math.Min(amount / 100, Main.LocalPlayer.inventory[i].stack);
					Main.LocalPlayer.inventory[i].stack -= coinsToSubtract;
					amount -= coinsToSubtract * 100;
				}
				else if (Main.LocalPlayer.inventory[i].type == ItemID.GoldCoin)
				{
					int coinsToSubtract = Math.Min(amount / 10000, Main.LocalPlayer.inventory[i].stack);
					Main.LocalPlayer.inventory[i].stack -= coinsToSubtract;
					amount -= coinsToSubtract * 10000;
				}
				else if (Main.LocalPlayer.inventory[i].type == ItemID.PlatinumCoin)
				{
					int coinsToSubtract = Math.Min(amount / 1000000, Main.LocalPlayer.inventory[i].stack);
					Main.LocalPlayer.inventory[i].stack -= coinsToSubtract;
					amount -= coinsToSubtract * 1000000;
				}

				if (amount <= 0)
				{
					return true;
				}
			}

			return false;
		}

        private void SubtractCoinsFromPiggyBank(int amount)
        {
            for (int i = 0; i < Main.LocalPlayer.bank.item.Length; i++)
            {
                if (Main.LocalPlayer.bank.item[i].type == ItemID.CopperCoin)
                {
                    if (Main.LocalPlayer.bank.item[i].stack >= amount)
                    {
                        Main.LocalPlayer.bank.item[i].stack -= amount;
                        return;
                    }
                    else
                    {
                        amount -= Main.LocalPlayer.bank.item[i].stack;
                        Main.LocalPlayer.bank.item[i].stack = 0;
                    }
                }
                else if (Main.LocalPlayer.bank.item[i].type == ItemID.SilverCoin)
                {
                    int coinsToSubtract = amount / 100;
                    if (Main.LocalPlayer.bank.item[i].stack >= coinsToSubtract)
                    {
                        Main.LocalPlayer.bank.item[i].stack -= coinsToSubtract;
                        return;
                    }
                    else
                    {
                        amount -= Main.LocalPlayer.bank.item[i].stack * 100;
                        Main.LocalPlayer.bank.item[i].stack = 0;
                    }
                }
                else if (Main.LocalPlayer.bank.item[i].type == ItemID.GoldCoin)
                {
                    int coinsToSubtract = amount / 10000;
                    if (Main.LocalPlayer.bank.item[i].stack >= coinsToSubtract)
                    {
                        Main.LocalPlayer.bank.item[i].stack -= coinsToSubtract;
                        return;
                    }
                    else
                    {
                        amount -= Main.LocalPlayer.bank.item[i].stack * 10000;
                        Main.LocalPlayer.bank.item[i].stack = 0;
                    }
                }
                else if (Main.LocalPlayer.bank.item[i].type == ItemID.PlatinumCoin)
                {
                    int coinsToSubtract = amount / 1000000;
                    if (Main.LocalPlayer.bank.item[i].stack >= coinsToSubtract)
                    {
                        Main.LocalPlayer.bank.item[i].stack -= coinsToSubtract;
                        return;
                    }
                    else
                    {
                        amount -= Main.LocalPlayer.bank.item[i].stack * 1000000;
                        Main.LocalPlayer.bank.item[i].stack = 0;
                    }
                }
            }
        }
    }
}
