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
using Terraria.Chat;
using Terraria.GameContent.NetModules;
using Terraria.Net;

namespace Nurser
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide; //changed to client side for multiplayer bug fixes :(

        [Header("Configs")]

        [LabelKey("$Config.HeartAcheDuration.Label")]
        [TooltipArgs("$Config.HeartAcheDuration.Tooltip")]
        [System.ComponentModel.DefaultValue(120)]
        [Range(15, 600)]
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

    public class NPCNuser : GlobalNPC
    {
        public static bool bossActive = false;

        public override void PostAI(NPC entity)
        {
            int[] bosses = { NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.EaterofWorldsHead, NPCID.BrainofCthulhu, NPCID.QueenBee, NPCID.SkeletronHead, NPCID.WallofFlesh, NPCID.Retinazer, NPCID.Spazmatism, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.Plantera, NPCID.Golem, NPCID.DukeFishron, NPCID.CultistBoss, NPCID.MoonLordCore };

            int npcType = entity.type;

            if (!entity.friendly && bosses.Contains(npcType) && entity.active || entity.boss && entity.active)
            {
                bossActive = true;
            }
            else
            {
                bossActive = false;
            }
        }
    }

    public class HealKeyPlayer : ModPlayer
    {
        Config config = ModContent.GetInstance<Config>();

        public override void OnEnterWorld()
        {
            Main.NewText("If you have an idea or issue/bug with this mods please go here and send a issue. (https://github.com/DeroXP/Nurser-Terraria/issues)", 255, 182, 193);
            Main.NewText("{From Nurser Mod:} Press " + HealKeyMod.HealKey.GetAssignedKeys()[0] + " or when health is at " + config.HealthThreshold + "% health to heal.", 255, 182, 193);
        }

        bool hasDisplayedMessage = false;
        public override void ProcessTriggers(TriggersSet triggers)
        {
            if (HealKeyMod.HealKey.JustPressed || IsHealthBelowThreshold(config.HealthThreshold / 100.0f))
            {
                if (config.RequireBoss &&!NPCNuser.bossActive)
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
                    int coinCost = CalculateCoinCost(Main.LocalPlayer.statLife, Main.LocalPlayer.statLifeMax2, config.CoinCostPerHealth);
                    if (HasEnoughCoins(coinCost))
                    {
                        if (!Main.LocalPlayer.dead)
                        {
                            Main.LocalPlayer.statLife = Main.LocalPlayer.statLifeMax2;

                            int healedAmount = Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife;

                            int platinum = coinCost / 1000000;
                            int gold = coinCost % 1000000 / 10000;
                            int silver = coinCost % 10000 / 100;
                            int copper = coinCost % 100;

                            string message = $"You have regained 100% health. {platinum} platinum, {gold} gold, {silver} silver, and {copper} copper coins were spent.";
                            Color messageColor = new(224, 224, 224);
                            CombatText combatText = new()
                            {
                                text = message,
                                color = messageColor,
                                lifeTime = 150,
                                scale = 2f
                            };
                            //if (Main.netMode == NetmodeID.SinglePlayer)
                            //{
                                //Main.NewText(message, messageColor);
                            //}
                            //else if (Main.netMode == NetmodeID.MultiplayerClient)// if multiplayer it will only show on the player who healed screen. (Maybe)
                            //{
                                //ChatMessage chatMessage = new(message);
                                //var packet = NetTextModule.SerializeClientMessage(chatMessage);
                                //NetManager.Instance.Broadcast(packet, -1);
                            //}

                            //can be used for a client sided message (YOU WILL NEED TO FIX THE CODE YOURSELF!!!)

                            CombatText.NewText(Main.LocalPlayer.getRect(), combatText.color, combatText.text);

                            Main.LocalPlayer.HealEffect(healedAmount);

                            SubtractCoins(coinCost);

                            for (int i = 0; i < 10; i++)
                            {
                                Dust.NewDust(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, DustID.CoralTorch);
                                SoundEngine.PlaySound(SoundID.Item29);
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
                            int platinum = coinCost / 1000000;
                            int gold = coinCost % 1000000 / 10000;
                            int silver = coinCost % 10000 / 100;
                            int copper = coinCost % 100;

                            Main.NewText($"You don't have enough coins, {platinum} platinum, {gold} gold, {silver} silver, and {copper} copper coins needed!", 255, 50, 50);
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
