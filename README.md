# Project Shield Shooters

[Markdown Cheatsheet](https://github.com/adam-p/markdown-here/wiki/Markdown-Here-Cheatsheet)

### Student Info

-   Name: Luke Lepkowski (lpl6448@rit.edu)
-   Section: 04

## Game Design

-   Camera Orientation: Topdown
-   Camera Movement: The camera is fixed for the most part but can vertically follow the player as a transition between levels. This also means that the player "bounces off" the edges of the screen.
-   Player Health: Healthbar
-   End Condition: The level ends when the player destroys the two shield stations (removing the barrier) and moves to the next level. The game ends once the player has made it past all of the enemy shields.
-   Scoring: The player earns a certain number of points for destroying each type of enemy.

### Game Description

After waking up to find you and your spaceship trapped in a bizarre alien prison, the only way out now is to fight. Shoot at enemy ships, dodge bullets and lasers, and destroy the shield stations keeping you trapped as you progress toward the freedom of open space.

### Enemy Types
-   Enforcer: a skittish robot ship that often prefers to shoot from a distance
-   Boulder (not implemented yet): a sentient space boulder that occasionally shoots out shards in different directions

### Controls

-   Movement
    -   Accelerate: W / Up Arrow
    -   Brake: S / Down Arrow (Holding down Brake while turning spins the ship much faster.)
    -   Left: A / Left Arrow
    -   Right: D / Right Arrow
-   Shoot: Space / Left Mouse
    - Hold OR click rapidly to shoot slightly faster

## Make It Your Own

-   Spaceships move around the screen using more advanced physics (including acceleration, drag, etc.).
-   There will be multiple levels, each with enemies and some kind of target the player must destroy in order to progress. (This means that the camera might follow the player around the map--I'm assuming that's okay, but if that doesn't count as a SHMUP I can change it.)

## Sources

-   Currently, all assets in the game are created by me.

## Known Issues

-   While health is currently mentioned in the code and there are damage effects, health is not fully implemented yet.

### Requirements not completed

