# Procedural-Roguelike
An experiment in procedural content generation and torch lighting in the context of cave spelunking.

I wrote an algorithm that generates branching cave systems inhabited by denizens of the underground.
A cave system is characterized by an essential path with major branching points. Major branches may have minor branches.
Each path may have choke points, where the stone closes in, and chamber rooms, where riches and encounters await!
Glowcap mushrooms are placed along the way to hint at what lies ahead and suggest a path forward.
All procedural aspects are driven by intuitive parameters.

[Download the demo.](https://github.com/kevin-d-omara/Procedural-Roguelike/releases/latest "https://github.com/kevin-d-omara/Procedural-Roguelike/releases/latest")

[![Demo Video](<https://user-images.githubusercontent.com/11803661/32682722-626c9ebc-c62b-11e7-8c3b-c6442349e0a0.png>)](https://youtu.be/x5smQ3RO1sQ)

## Controls
Control | Keys
--- | :---:
Move | WASD
Swing Pickaxe | J, Left-Click
Place Dynamite | Spacebar
Exit Game | Escape

## Objective
Night has set and it's time for adventure! Traverse the surface to find an entrance to the cave systems below.

Once inside, spelunker forth and see what lies await.
Along the way, you may find narrow choke points and vast open chambers.
Beware the lurking denizens!
Dynamite will serve you well against both creatures and stone.
Glowcap mushrooms offer dimly illuminate respite, beckoning you forward.
Keep an eye out for shiny golden chests, for therein lies your prize.
After aquiring your prize\*, find a way back up to the surface.
Hurry now, your torch is getting low!

*\*Prize not implemented. All that lies within are cobwebs and crushed dreams.*

Find a passage above ground to enter the cave (left) or within the cave to return to the surface (right).
![Cave Entrance and Exit](https://user-images.githubusercontent.com/11803661/32682503-e7cfeab6-c629-11e7-81dd-db6d1e8a5f1e.png)

## Procedural Generation
Sample procedural caves, created without changing any values:
![Cave Variety](https://user-images.githubusercontent.com/11803661/32678518-7d8b4e00-c617-11e7-8c2e-7cf965fe8fac.png)

Long, narrow passageways:
![Long Narrow Passageways](https://user-images.githubusercontent.com/11803661/32679277-b5a3701c-c61a-11e7-81ba-80336e576dd8.png)

Short, wide passageways:
![Wide Short Passageways](https://user-images.githubusercontent.com/11803661/32679278-b5b9a544-c61a-11e7-90a3-611e57be79f5.png)

## Screenshots
An end room with treasure and a terrifying foe!
![High Stakes](https://user-images.githubusercontent.com/11803661/32675674-90fdf2d6-c60c-11e7-90e4-ba7404ac2088.png)

A large open chamber with a pair of zombies waiting for ambush! Dynamite should do the trick.
![Dynamite](https://user-images.githubusercontent.com/11803661/32675547-12f29e28-c60c-11e7-928e-a972f28ac80b.png)

The way up to the surface is peered through the darkness.
![Escape](https://user-images.githubusercontent.com/11803661/32675907-6f206b8e-c60d-11e7-942b-f9b0b9952065.png)

Visual assets are from the [Unity Asset Store: 2D Roguelike](https://www.assetstore.unity3d.com/en/?&_ga=2.164587837.320441983.1510354406-1864889917.1468427849#!/content/29825), with the exception of the Passage, Glowcaps, and Dynamite.
