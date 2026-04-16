# Billion Difficulty
This mod adds a difficulty called Billion, where every enemy gets some new abilty or changed behaviour. Made with the Cybergrind in mind, the mod aims to make the game harder while not being a pain in the ass. Staying on the move and being aware of your surroundings is crucial. (any comments about providences in cg should be addressed to hakita)

*Keep in mind that things might break if you use other mods that modify enemy behaviour*

(Not designed for a new save, but starting from scratch can still be an extra challenge if you want)

Feedback is welcome so you can dm me on discord to share your thoughts or report bugs at @somebilly

# New 2.0 mode: BRILLIANT BILLION
For those who want to try their hands at some crazy bullshit then open the Configgy menu and press the toggle in the "Billion Difficulty" tab. Anyone who has ever at any point thought that Ultrakill couldn't get harder should think again. Don't use this in the cybergrind

If you plan on playing through this mode I'd recommend not checking the list of changed enemy behaviours until you've seen them yourself to keep things more "surprising". Also the level end screen will only show the difficulty as "BRILLIANT BILLION" if you go through the whole level without switching it off, otherwise there will be an asterisk (*) at the end

And if you want to know more, read until the end:

<details>
<summary>A Brief Explanation / Apology / Rant:</summary>
I'd like to start off by acknowledging the fact that it's impossible for me to guess how good you are at the game. There's a chance you're more skilled than you think, and if you're curioius to find out how far you can go, Brilliant Billion might be for you. Still, you have to keep in mind that its main goal is to make specifically the main campaign very hard, so something like P-2 or the Encore levels may become less about skill and more about grinding and near-perfect repetition.

Now, I feel it's safe to say that difficulty plays a major role in how much you enjoy a game. Of course, even a fun game can start to feel like a boring slog if it doesn't incentivize you to push yourself in any way. The opposite is also true, as it quickly becomes frustrating when you keep losing over and over.

That's why mods like this one have a relatively small audience: out of all the people who see them, only some will try to play, and out of those only a small percentage will actually enjoy them (more likely as a novelty rather than simply enjoying at face value). However much you, the player, might end up detesting Brilliant Billion's level of difficulty, I hope that you understand that it was made from a simple desire to keep going higher, to not get stuck in the mire of having nothing to strive for while feeling there's so much more to the game, in a way that vanilla can't provide, and that you at least understand the vision and motivations for having such over-the-top difficulty. And unlike something like speedrunning, which has a tangible social aspect and a more or less defined path you have to follow, difficulty mods don't offer that, leaving you to rely purely on yourself. That, combined with the potential unintentional forcing of a specific playstyle which might not be suited for the player, contributes to how "alienating" mods like this can be.

Despite all of this negative talk, I can personally feel content with the fact that I set out to create this and managed to see it through to the end(?), however selfish that might be. Even if the final result could play better, be balanced better, or if you think it completely missed the mark, I will still be the one with more content to indulge in. Even still, I, the one who's telling you to play with the right mindset, can't always do that, but if I do feel frustrated, I treat it as a personal reminder to stop chasing arbitrary virtual achievements. Past the pointless pretentiousness, *I* can say that at least *I*n my eyes, the B*I*LL*I*ON might, in a way, actually be BRI*L*LIANT perchance... but if you want to know what I really think then read the first letter of every sentence in this text
</details>


# Enemy changes
The changes are described compared to the brutal difficulty. Changes exclusive to the Brilliant Billion mode are marked with "**BB**"

- All enemies move and attack faster, projectiles are also faster. Radiance speed buffs are nerfed to 1.25x for most enemies since they're already very fast (other speed modifiers: 1.1x - Sentries and Virtues, 1.15x - Ferrymen, Insurrectionists, Providences and Powers). A few changes were suggested by other people
- Unique bosses don't have new behaviors but they move and/or attack faster (this includes Gabriel, V2, The Corpse of King Minos, Leviathan, Minotaur, the Earthmover brain, Geryon, Flesh Prison/Panopticon, the Prime souls and the joke bosses)
- Style gain from enemy friendly fire is reduced a lot
- Added a special rare filth

Note: making enemies radiant in the sandbox with the spawner arm won't give them the right speed multipliers, so you'll need to use the `buffs forceradiance on` console command

## Husks
<details>
<summary>Filth</summary>
Has the ability to double jump.

<b>BB</b>: moves a lot faster, buffed health from 0.5 to 1.5 (radiant hp stays the same)
</details>

<details>
<summary>Stray</summary>
Shoots a series of 3 projectiles in a row.

<b>BB</b>: projectiles are now homing
</details>

<details>
<summary>Stalker</summary>
The sand cloud stays active for longer and the stalker overheals itself by 1.5 hp for every enemy in the sanded area (healing scales with radiance). An enemy can only heal any stalker once. The sand zone gets deleted when a cybergrind wave ends

<b>BB</b>: enemies that get sanded now get overhealed by 5 hp (scales with radiance), the sand zone becomes bigger, and it shoots out a Mirror Reaper projectile when sanding enemies
</details>

<details>
<summary>Schism</summary>
Projectiles slow down until they stop and become unparriable for a few seconds, after which they disappear. They're also slightly bigger (the projectiles get deleted when a cybergrind wave ends)

<b>BB</b>: the projectiles move back and forth a few times instead of just stopping, and they don't get destroyed when colliding with walls
</details>

<details>
<summary>Soldier</summary>
Shoots out more projectiles with the center one being bigger to make it a bit easier to parry

<b>BB</b>: shoots out 2 homing explosive projectiles instead
</details>

<details>
<summary>Sisyphean Insurrectionist</summary>
Dealing 30 damage (scales with radiance) to an insurrectionist in 5 seconds creates a red explosion which deals 40 damage to you and 7 damage to enemies (including the insurrectionist itself), after which there's a 1 second cooldown until it starts counting damage again. Doing that also enrages them giving them a 1.1x speed increase for 7 seconds

<b>BB</b>: enraging lasts for 10 seconds and the enrage explosion doesn't deal damage to enemies. Their boulder now creates a black hole when it hits something. An insurrectionist can't create more than 1 black hole at a time unless it's enraged
</details>

<details>
<summary>Ferryman</summary>
Speed goes up from 1.1x to 1.3x and they become more blue the longer they stay alive. When they reach max speed you get the +I'M BLUE style bonus (more total hp = slower increase in speed: about 12 seconds for common ferrymen, 35 seconds for the boss, 17 seconds for radiant ferrymen)

<b>BB</b>: spawns additional projectiles while doing attacks. Speeds up 1.15x times faster. Enrages at 50% health. Starts spawning big unparriable projectiles around itself when either below 50% health or fully blue (you can slide under the projectiles). When fully blue, there will be 2 circling projectiles stacked on top of each other at once (that you can't slide under)
</details>

<details>
<summary>Mirror Reaper</summary>
Faster attacks and shorter cooldowns. Can shoot 30 damage projectiles that explode when they're around your height like the Sentry

<b>BB</b>: does stuff faster and has 1 more max hand
</details>


## Machines
<details>
<summary>Drone</summary>
Projectile speed goes up and down as it's flying, and every 4-6 attacks one of the orbs becomes explosive and homing and does 10 more damage. It explodes when it's within a certain distance from you. When the drone spawns in, the first big orb also appears sooner. The big orbs don't deal damage to enemies unless parried. Also drones don't move around as much

<b>BB</b>: takes reduced damage from revolvers, and damage frome explosions is clamped to 50% of its max health. Has a 50% chance to spawn the big orb when dying
</details>

<details>
<summary>Streetcleaner</summary>
Attack range is increased, and they can now shoot straight up

<b>BB</b>: after they finish dealing damage to you, you get 4 ticks of afterburn with each one dealing 4 damage
</details>

<details>
<summary>Swordsmachine</summary>
Every other shotgun attack while you're in a certain range, the swordsmachine will overpump its shotgun creating a big explosion. Phase 2 now starts at 10/30 hp instead of 15/30 hp

<b>BB</b>: takes only 25% while not enraged. Spawns Minotaur goop when downed
</details>

<details>
<summary>Mindflayer</summary>
Shoots out many unparriable green orbs around itself during the laser attack that each deal 15 damage and stay in place for a few seconds. The amount of orbs increases while the mindflayer is enraged. Sending a dying mindflayer flying will cause it to shoot out orbs while it's flying away (the orbs get deleted when a cybergrind wave ends)

<b>BB</b>: summons a Virtue beam targeted at you when doing the homing orb attack. The small green orbs are bigger, and become even bigger when enraged
</details>

<details>
<summary>Sentry</summary>
Shoots out 3 fast homing mortar projectiles that deal 30 damage (but not to enemies) when interrupted. The first projectile has more range and explodes when it's at your height or above you and when it's within a certain distance. The sentry also falls faster now to prevent it from getting too high up into the air from their own explosions when interrupting them by slamming

<b>BB</b>: also shoots a Mirror Reaper projectile when interrupted
</details>

<details>
<summary>Gutterman</summary>
Their accuracy increases faster and they dodge when you get too close

<b>BB</b>: they can now dodge while you're simply standing next to them (without needing to leave the dodge radius) with a 1.6 second cooldown, and they create a delayed Sisyphus explosion when they dodge
</details>

<details>
<summary>Guttertank</summary>
When their rockets explode, they fire a barrage of projectiles in your general direction (unless the rocket hits a solid surface)

<b>BB</b>: creates a Mirror Reaper hand when placing a mine
</details>

<details>
<summary>Defense system: Rocket launcher</summary>
Spawns oil where the rocket hits every other attack

<b>BB</b>: spawns oil after every attack
</details>

<details>
<summary>Defense system: Mortar</summary>
The projectiles are now faster and more homing and it has a shorter attack cooldown

<b>BB</b>: the projectiles create shockwaves when exploding
</details>

<details>
<summary>Defense system: Tower</summary>
Shoots out a big mannequin projectile every other attack and has a shorter attack cooldown

<b>BB</b>: the small orbs are explosive
</details>


## Demons
<details>
<summary>Malicious face</summary>
Beam count is increased by 1. Shoots more projectiles in a cool spiral pattern with a bigger one in the middle that deals 40 damage, while the others do 20 (the friendly fire from projectiles is reduced to account for how many there are). Parrying the explosive beam attack does 0.5x of the original damage to the Malicious face itself and also damages and knocks you back dealing 25 damage (can be avoided by dashing or moving fast)

<b>BB</b>: they create a damaging shockwave when falling onto the ground, their projectiles do 0% friendly fire, and they take reduced damage from hammers
</details>

<details>
<summary>Cerberus</summary>
Very quickly dashes 2 times and now the apple is homing. Reduced the dash knockback when they're enraged. Damage from cannonballs is reduced

<b>BB</b>: creates a Mirror Reaper hand when stomping, the time between 2 dashes is reduced
</details>

<details>
<summary>Idol</summary>
The idoled enemy overheals enemies around it by 0.8 hp every 2 seconds

<b>BB</b>: heals 1.2 hp every 1.5 seconds, unless the enemy has less than 8 health in which case it only get healed 0.9 hp
</details>

<details>
<summary>Mannequin</summary>
Every other attack the blue projectile quickly accelerates and increases in size

<b>BB</b>: the usual small orbs are explosive
</details>

<details>
<summary>Deathcatcher</summary>
No changes
</details>

<details>
<summary>Hideous mass</summary>
Creates 2 shockwaves, mortar projectiles now track your location more accurately, and the hook takes 1 knuckleblaster or 2 feedbacker punches to break like on Violent and below

<b>BB</b>: creates Virtue beams when doing the slam and clap attacks. The projectiles create shockwaves when exploding (when it's enraged, only every 3rd projectile will create a shockwave)
</details>


## Angels
<details>
<summary>Virtue</summary>
When not enraged, their beams stay for 1.5 seconds longer than on brutal. When enraged, they don't predict your movement but become noticeably bigger and disappear 2 second sooner than on brutal. Damage from railcannons is reduced

<b>BB</b>: the beams slowly move towards you, and now Virtues also fire blue Geryon beams. Enrages after 2 attacks
</details>

<details>
<summary>Providence</summary>
Their beams become larger and no longer damage enemies. Damage from railcannons and cannonballs is reduced

<b>BB</b>: beams rotate and are even larger, has another attack where it fires 2 Mirror Reaper projectiles. At 50% health it enrages and its attacks become harder to dodge. Damage from cannonballs is reduced even more. Also all damage they take is clamped to 50% of their max health
</details>

<details>
<summary>Power</summary>
Faster attacks

<b>BB</b>: faster attacks
</details>


# Some useful stuff
If you want to use this difficulty with the Angry Level Loader, you'll need to
1. Enter a level
2. Turn on cheats
3. Press F8
4. Type `prefs set int difficulty 19`

You can skip to any level in the game with console commands:
1. Turn on cheats
2. Press F8
3. Type `scene Level X-X` to enter a campaign or encore level (the other stuff is `scene Endless` for the cybergrind, `scene uk_construct` for the sandbox, `scene 005a4f2ce549277458596ee0f0d6e88c` for P-1, `scene 1f290c2101e628540bf9c6d1d2140750` for P-2 and `scene Main Menu`)