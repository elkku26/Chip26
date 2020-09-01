# Chip26

How-To\n
Download the roms you want to use
Go to Program.cs in FrameworkInterpreter and change line 106 to the path of the rom you want.
Run Game1.cs in Graphics. (if you run it in Debug mode, the game window will be very unresponsive, but the game itself will work. In Release mode, the window will mostly behave like any other window.)
CHIP-8 uses keys 0-9 and A-F for input. In most games the keys 4 and 6 correspond to moving left and right, and 5 is some action. For example, to play Space Invaders you move left and right with 4 and 6 and shoot with 5

Notes: very slow, lots of flickering. Still a fair amount of bugs most likely related to program flow and the timers. While the sound timer is implemented, the sound itself isn't yet.
