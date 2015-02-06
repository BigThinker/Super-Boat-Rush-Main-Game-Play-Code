# Super-Boat-Rush-Main-Game-Play-Code
Main game play code written for the game Super Boat Rush in C# / Unity.

These are the main gameplay scripts used to make a racing game in Unity / C#.
There are scripts such as: 
* BoatController.cs used to grab the input of the player and manipulate his / her boat on the screen
via 4 wheels using torque force, braking, spinning etc. There are instructions to use it in the file.
* BoatBalanceScript.cs used to prevent flipping of the boat and reversing the bug of wheels getting stuck in the terrain
(due to fast movement / not high enough physics framerate). 
* CameraScript used to correctly visualize the experience of racing by moving the camera smoothly and appropriately with
the boat.
* ... the rest of the scripts are used for various reasons for gameplay features such as physical explosions when
player shoots a bullet or collides with different structures, or for controlling the dragons that slow down the player etc.
