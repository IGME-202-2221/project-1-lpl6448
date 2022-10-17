# Project Shield Shooters

### Student Info

-   Name: Luke Lepkowski (lpl6448@rit.edu)
-   Section: 04

## Game Design

-   Camera Orientation: Topdown
-   Camera Movement: The camera is fixed for the most part but can vertically follow the player as a transition between levels. This also means that the player "bounces off" the edges of the screen.
-   Player Health: Health Bar
-   End Condition: The level ends when the player destroys the two shield stations (removing the barrier) and moves to the next level. The game ends once the player has made it past all of the enemy shields.
-   Scoring: The player earns points for doing damage and earns a certain number of points for destroying each type of enemy.

### Game Description

After waking up to find you and your spaceship trapped in a bizarre alien prison, the only way out now is to fight. Shoot at enemy ships, dodge bullets and lasers, and destroy the shield stations keeping you trapped as you progress toward the freedom of open space.

### Enemy Types
-   Enforcer: a skittish robot ship that often prefers to shoot from a distance (shoots 6 bullets per second)
-   Large Enforcer: a larger, more powerful Enforcer that shoots more bullets (shoots 8 bullets per second)
-   Boulder: a sentient space boulder that occasionally shoots out asteroid fragments in different directions

### Controls

-   Movement
    -   Accelerate: W / Up Arrow
    -   Brake: S / Down Arrow (Holding down Brake while turning spins the ship much faster.)
    -   Left: A / Left Arrow
    -   Right: D / Right Arrow
-   Shoot: Space / Left Mouse
    -   Hold OR click rapidly to shoot slightly faster
-   Grader Mode: G
    -   The game is built to be a challenge, so if you're having trouble getting to the ending stages, Grader Mode makes the game easier by turning the player invincible.

## Make It Your Own

-   Spaceships move around the screen using more advanced physics (including acceleration, drag, etc.).
-   There are multiple levels, each with enemies and shield stations the player must destroy in order to progress.

## Sources

-   All art assets in the game are created by me.
-   Fonts used:
    -   Bungee - https://fonts.google.com/specimen/Bungee (Copyright 2008 The Bungee Project Authors (david@djr.com))
    -   Exo 2 - https://fonts.google.com/specimen/Exo+2 (Copyright 2013 The Exo 2 Project Authors (https://github.com/NDISCOVER/Exo-2.0))
    -   Both fonts are licensed under the OFL license (added as OFL.txt to the project).

## Known Issues

-   The game is built to run on an 8:5 aspect ratio (the default Unity WebGL ratio). Playing in fullscreen may cause display issues, particularly if playing on a 4:3 monitor.

### Requirements not completed

