# Nurser-Terraria

A tModLoader mod that brings Nurse-style healing anywhere in the world, kept fair with a set of anti-cheat configs.

To see exactly how it was made, download it from the release, but please also check the code here on GitHub against the files you downloaded to make sure they're up to date.

Since there isn't a lot of help out there for Terraria modding (especially on the newer preview versions) for starting devs like I was, this repo is meant as a reference for things like: Configs, Config Headers, custom DisplayNames and Tooltips, Buffs, ModKeybinds, NPCs, Items, Chest Loot (only in the download), getting coins from the bank or inventory, and some simple math.

Side note: the downloaded version of this build includes an example of how to open links, in case you ever need it.

Original Mod: [Nurser](https://steamcommunity.com/sharedfiles/filedetails/?id=3234638824)

From Derourn. (I'll probably start making Terraria modding tutorials.)

Some contributions from Maxhax64.

# Info

Nurser was made just because — and to let players use Nurse-NPC logic anywhere, but with anti-cheat configs to keep the game fair. Basically, Nurser heals you all the way back to full health, then taxes you for the amount of health healed (configurable via Mod Settings). The intended use is dev-testing, project referencing, and general play.

Nurser is open-source and updated every once in a while. Most updates are bug fixes; if it hasn't updated in a while, it's usually because a bigger update is in progress and won't ship until it's fully working. This doesn't guarantee a bug-free experience — the dev team is two people, one programmer and one manager.

# Building

This is a standard tModLoader mod. Place the repository folder inside your tModLoader `ModSources` directory and build it in-game via **Workshop → Develop Mods**. The `build.txt` file holds the mod metadata (author, version, display name).

# Configurations

**Cost Per Health**: Adjustable, so you can set the cost yourself. The default is `100`, which is 1 silver coin per missing health point.

The coin math works like this:
1. `1` = copper coin
2. `100` = silver coin
3. `10000` = gold coin
4. `1000000` = platinum coin

It's configurable because I wanted you to feel in full control of the mod.

**Heart Ache Duration**: How long the "Heart Ache" debuff lasts, in seconds (range: 5 seconds to 10 minutes).

**Require Boss**: Keeps the mod from being even cheatier — healing on demand would ruin the challenge (requested by Maxhax64). When enabled, you can only heal while a boss is active.

**Health Threshold**: Lets the heal trigger automatically when your health drops below a set percentage, so you don't have to press the Heal Key. Example: if you have 500 max health and the threshold is 20%, it heals once you hit 100 health.

**Cost Multiplier / Multiplier Amount**: When the multiplier is enabled, the healing cost is multiplied by the Multiplier Amount.

# Credits

Contributions from the authors of

VETERAN MODE https://github.com/DeroXP/VeteranMode-Terraria

Derourn, Maxhax64
