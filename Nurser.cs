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

namespace Nurser
{
    // Configuration class for the mod
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide; // Set config to client-side for multiplayer compatibility

        [Header("Configs")]

        [LabelKey("$Config.HeartAcheDuration.Label")]
        [TooltipArgs("$Config.HeartAcheDuration.Tooltip")]
        [System.ComponentModel.DefaultValue(120)]
        [Range(5, 600)]
        public int HeartAcheDuration; // Duration of the HeartAche debuff

        [LabelKey("$Config.CoinCostPerHealth.Label")]
        [TooltipArgs("$Config.CoinCostPerHealth.Tooltip")]
        [System.ComponentModel.DefaultValue(100)]
        [Range(10, float.PositiveInfinity)]
        public int CoinCostPerHealth; // Cost of health in coins

        [LabelKey("$Config.HealthThreshold.Label")]
        [TooltipArgs("$Config.HealthThreshold.Tooltip")]
        [System.ComponentModel.DefaultValue(20)]
        [Range(1, 100)]
        public int HealthThreshold; // Health percentage threshold for auto-healing

        [LabelKey("$Config.RequireBoss.Label")]
        [TooltipArgs("$Config.RequireBoss.Tooltip")]
        [System.ComponentModel.DefaultValue(false)]
        public bool RequireBoss; // Whether a boss needs to be active for healing
    }

    // Mod class to define a custom keybind
    public class HealKeyMod : Mod
    {
        #pragma warning disable CA2211
        public static ModKeybind HealKey;
        public override void Load()
        {
            // Register a keybind for healing
            HealKey = KeybindLoader.RegisterKeybind(this, "Heal Key", Keys.G);
        }
    }

    // Class to track whether a boss is active
    public class NPCNuser : GlobalNPC
    {
        public static bool bossActive = false;

        public override void PostAI(NPC entity)
        {
            // List of boss NPC IDs
            int[] bosses = { NPCID.KingSlime, NPCID.EyeofCthulhu, NPCID.EaterofWorldsHead, NPCID.BrainofCthulhu, NPCID.QueenBee, NPCID.SkeletronHead, NPCID.WallofFlesh, NPCID.Retinazer, NPCID.Spazmatism, NPCID.TheDestroyer, NPCID.SkeletronPrime, NPCID.Plantera, NPCID.Golem, NPCID.DukeFishron, NPCID.CultistBoss, NPCID.MoonLordCore };

            int npcType = entity.type;

            // Check if any boss is active
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

    // Class to handle player actions related to the mod
    public class HealKeyPlayer : ModPlayer
    {
        // Get configuration instance
        Config config = ModContent.GetInstance<Config>();

        // Method called when player enters the world
        public override void OnEnterWorld()
        {
            // Delay to avoid immediate execution
            Task.Delay(6000);

            // Display mod info and instructions to the player
            Main.NewText("If you have an idea or issue/bug with this mod please go here and send an issue. (https://github.com/DeroXP/Nurser-Terraria/issues)", 255, 182, 193);
            
            var assignedKeys = HealKeyMod.HealKey.GetAssignedKeys();
            string healKeyText = assignedKeys.Count > 0 ? assignedKeys[0] : "['UNBOUND' PLEASE ASSIGN KEY IN CONTROLS]";
            
            Main.NewText("{From Nurser Mod:} Press " + healKeyText + " or when health is at " + config.HealthThreshold + "% health to heal.", 121, 6, 4);
        }

        bool hasDisplayedMessage = false;
        // Method to process key triggers
        public override void ProcessTriggers(TriggersSet triggers)
        {
            // Check if heal key is pressed or health is below threshold
            if (HealKeyMod.HealKey.JustPressed || IsHealthBelowThreshold(config.HealthThreshold / 100.0f))
            {
                // Check if healing requires a boss to be active and if a boss is indeed active
                if (config.RequireBoss && !NPCNuser.bossActive)
                {
                    if (!hasDisplayedMessage)
                    {
                        Main.NewText("You can't heal right now. Wait until a boss is active.", 255, 50, 50);
                        hasDisplayedMessage = true;
                    }
                    return;
                }

                // Check if player has HeartAche debuff
                if (!Player.HasBuff<HeartAche>())
                {
                    int coinCost = CalculateCoinCost(Main.LocalPlayer.statLife, Main.LocalPlayer.statLifeMax2, config.CoinCostPerHealth);
                    // Check if player has enough coins to heal
                    if (HasEnoughCoins(coinCost))
                    {
                        if (!Main.LocalPlayer.dead)
                        {
                            Main.LocalPlayer.statLife = Main.LocalPlayer.statLifeMax2; // Heal player to max health

                            int healedAmount = Main.LocalPlayer.statLifeMax2 - Main.LocalPlayer.statLife;

                            // Calculate coin breakdown
                            int platinum = coinCost / 1000000;
                            int gold = coinCost % 1000000 / 10000;
                            int silver = coinCost % 10000 / 100;
                            int copper = coinCost % 100;

                            // Display coin cost message
                            string message = $"{platinum} platinum, {gold} gold, {silver} silver, and {copper} copper coins were spent.";
                            Color messageColor = new(224, 224, 224);
                            CombatText combatText = new()
                            {
                                text = message,
                                color = messageColor,
                                lifeTime = 150,
                                scale = 2f
                            };

                            // Display combat text
                            CombatText.NewText(Main.LocalPlayer.getRect(), combatText.color, combatText.text);

                            Main.LocalPlayer.HealEffect(healedAmount); // Show heal effect

                            SubtractCoins(coinCost); // Subtract the coins from player inventory

                            // Create dust and play sound effects
                            for (int i = 0; i < 10; i++)
                            {
                                Dust.NewDust(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, DustID.CoralTorch);
                                SoundEngine.PlaySound(SoundID.Item29);
                            }

                            int buffDuration = config.HeartAcheDuration * 60;
                            Player.AddBuff(ModContent.BuffType<HeartAche>(), buffDuration); // Add HeartAche debuff

                            hasDisplayedMessage = false;
                        }
                    }
                    else
                    {
                        if (!hasDisplayedMessage)
                        {
                            // Notify player if they don't have enough coins
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
                        // Notify player if they have HeartAche debuff
                        Main.NewText("You can't heal right now. Wait until HeartAche wears off.", 255, 50, 50);
                        hasDisplayedMessage = true;
                    }
                }
            }
        }

        // Helper method to check if health is below the threshold
        private bool IsHealthBelowThreshold(float threshold)
        {
            float healthPercentage = (float)Main.LocalPlayer.statLife / Main.LocalPlayer.statLifeMax2;
            return healthPercentage <= threshold;
        }

        // Helper method to calculate coin cost based on missing health
        private int CalculateCoinCost(int currentHealth, int maxHealth, int coinCostPerHealth)
        {
            int coinCost = (maxHealth - currentHealth) * coinCostPerHealth;
            return coinCost;
        }

        // Helper method to check if player has enough coins
        private bool HasEnoughCoins(int amount)
        {
            int totalCoins = GetTotalCoins(Main.LocalPlayer.inventory) + GetTotalCoins(Main.LocalPlayer.bank.item);
            return totalCoins >= amount;
        }

        // Helper method to calculate total coins in player's inventory and piggy bank
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

        // Helper method to subtract the specified amount of coins from inventory and piggy bank
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

        // Helper method to subtract coins from a specific inventory (inventory or piggy bank)
        private bool SubtractCoinsFromInventory(int coinType, int amount)
        {
            // Subtract coins from player inventory
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

            // Subtract coins from piggy bank
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
//chatgpt used for side notes, made by me :D
