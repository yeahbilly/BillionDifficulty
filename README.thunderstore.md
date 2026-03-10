# The Billion Difficulty
This mod adds a difficulty called Billion, where every enemy gets some new abilty or changed behaviour. Made with the Cybergrind in mind, the mod aims to make the game harder while not being a pain in the ass. Staying on the move and being aware of your surroundings is crucial. (any comments about providences in cg should be addressed to hakita)

*Keep in mind that things will break if you use other mods that modify enemy behaviour*

(Not designed for a new save, but starting from scratch can still be an extra challenge if you want)

**Feedback is welcome so you can dm me on discord to share your thoughts or report bugs at @somebilly**

# What's coming in 2.0.0 (soon)
* An extra-hard mode that will be really hard and difficult and challenging because i want p ranking the campaign to be an actual challenge again

# Enemy changes
The changes are described compared to the brutal difficulty

- All enemies move and attack faster, projectiles are also faster. Radiance speed buffs are nerfed to 1.25x for most enemies since they're already very fast (other speed modifiers: 1.1x - Sentries and Virtues, 1.15x - Ferrymen, Insurrectionists, Providences and Powers). A few changes were suggested by other people (shoutout to [Broccolite](https://www.youtube.com/@broccolite8297))
- Unique bosses don't have new behaviors but they move and/or attack faster (this includes Gabriel, V2, The Corpse of King Minos, Leviathan, Minotaur, the Earthmover brain, Geryon, Flesh Prison/Panopticon, the Prime souls and the joke bosses)
- Added a special rare filth

Note: making enemies radiant in the sandbox with the spawner arm won't give them the right speed multipliers, so you'll need to use the `buffs forceradiance on` console command
### Husks
<details>
<summary>Filth</summary>
Has the ability to double jump
</details>
<details>
<summary>Stray</summary>
Shoots a series of 3 projectiles in a row
</details>
<details>
<summary>Stalker</summary>
The sand cloud stays active for longer and the stalker overheals itself by 1.5 hp for every enemy in the sanded area. An enemy can only heal any stalker once (the sandification zone gets deleted when a cybergrind wave ends)
</details>
<details>
<summary>Schism</summary>
Projectiles slow down until they stop and become unparriable, after which they disappear. They're also slightly bigger (the projectiles get deleted when a cybergrind wave ends)
</details>
<details>
<summary>Soldier</summary>
Shoots out more projectiles with the center one being bigger to make it a bit easier to parry, and is also not completely immune to explosions
</details>
<details>
<summary>Sisyphean Insurrectionist</summary>
Dealing 30 damage (scales with radiance) to an insurrectionist in 5 seconds creates a red explosion which deals 40 damage to you and 7 damage to enemies (including the insurrectionist itself), after which there's a 1 second cooldown until it starts counting damage again. Doing that also enrages them giving them a 1.1x speed increase for 7 seconds
</details>
<details>
<summary>Ferryman</summary>
Speed goes up from 1.1x to 1.3x and they become more blue the longer they stay alive. When they reach max speed you get the +I'M BLUE style bonus (more total hp = slower increase in speed: about 12 seconds for common ferrymen, 35 seconds for the boss, 17 seconds for radiant ferrymen)
</details>
<details>
<summary>Mirror Reaper</summary>
No changes
</details>

### Machines
<details>
<summary>Drone</summary>
Projectile speed goes up and down as it's flying, and every 4-6 attacks one of the orbs becomes explosive and homing and does 10 more damage. It explodes when it's within a certain distance from you. When the drone spawns in, the first big orb also appears sooner. The big orbs don't deal damage to enemies unless parried. Also drones don't move around as much
</details>
<details>
<summary>Streetcleaner</summary>
Attack range is increased, and they can now shoot straight up
</details>
<details>
<summary>Swordsmachine</summary>
Every other shotgun attack while you're in a certain range, the swordsmachine will overpump its shotgun creating a big explosion. Phase 2 now starts at 10 hp out of 30 instead of 15 hp out of 30
</details>
<details>
<summary>Mindflayer</summary>
Shoots out many unparriable green orbs around itself during the laser attack that each deal 15 damage and stay in place for a few seconds. The amount of orbs increases while the mindflayer is enraged. Sending a dying mindflayer flying will cause it to shoot out orbs while it's flying away (the orbs get deleted when a cybergrind wave ends)
</details>
<details>
<summary>Sentry</summary>
Shoots out 3 fast homing mortar projectiles that deal 30 damage (but not to enemies) when interrupted. The first projectile has more range and explodes when it's at your height or above you and when it's within a certain distance. The sentry also falls faster now to prevent it from getting too high up into the air from their own explosions when interrupting them by slamming
</details>
<details>
<summary>Gutterman</summary>
Their accuracy increases faster and they dodge when you get too close
</details>
<details>
<summary>Guttertank</summary>
When their rockets explode, they fire a barrage of projectiles in your general direction (unless the rocket hits a solid surface)
</details>

<details>
<summary>Defense system: Rocket launcher</summary>
Spawns oil where the rocket hits every other attack
</details>
<details>
<summary>Defense system: Mortar</summary>
The projectiles are now faster and more homing and it has a shorter attack cooldown
</details>
<details>
<summary>Defense system: Tower</summary>
Shoots out a big mannequin projectile every other attack and has a shorter attack cooldown
</details>

### Demons
<details>
<summary>Malicious face</summary>
Shoots more projectiles in a cool spiral pattern with a bigger one in the middle that deals 40 damage, while the others do 20 (the friendly fire from projectiles is reduced to account for how many there are). Parrying the explosive beam attack does 0.5x of the original damage to the Malicious face itself and also damages and knocks you back dealing 25 damage (can be avoided by dashing or moving fast)
</details>
<details>
<summary>Cerberus</summary>
Very quickly dashes 2 times and now the apple is homing. Reduced the dash knockback when they're enraged
</details>
<details>
<summary>Idol</summary>
The idoled enemy overheals enemies around it by 0.8 hp every 2 seconds
</details>
<details>
<summary>Mannequin</summary>
Every other attack the blue projectile quickly accelerates and increases in size
</details>
<details>
<summary>Deathcatcher</summary>
No changes
</details>
<details>
<summary>Hideous mass</summary>
Creates 2 shockwaves, mortar projectiles now track your location more accurately, and the hook takes 1 knuckleblaster or 2 feedbacker punches to break like on Violent and below
</details>

### Angels
<details>
<summary>Virtue</summary>
When not enraged, their beams stay for 1.5 seconds longer than on brutal. When enraged, they don't predict your movement but become noticeably bigger and disappear 2 second sooner than on brutal
</details>
<details>
<summary>Providence</summary>
Their beams become larger
</details>
<details>
<summary>Power</summary>
Faster attacks
</details>

## Some useful stuff
If you want to use this difficulty with the Angry Level Loader, you'll need to
1. Enter a level
2. Turn on cheats
3. Press F8
4. Type `prefs set int difficulty 19`

You can skip to any level in the game with console commands:
1. Turn on cheats
2. Press F8
3. Type `scene Level X-X` to enter a campaign or encore level (the other stuff is `scene Endless` for the cybergrind, `scene uk_construct` for the sandbox, `scene 005a4f2ce549277458596ee0f0d6e88c` for P-1, `scene 1f290c2101e628540bf9c6d1d2140750` for P-2 and `scene Main Menu`)