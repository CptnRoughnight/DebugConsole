# DebugConsole
Ingame DebugConsole Godot 4+ C#

# USAGE

- Add "debug_console.tscn" to your scene
- defined in "CommandLine.cs" there are some hotkeys
	- F10 to open the console
	- F2 to switch to unsafe mode (all fields are accessable)
	- F1 to open Command-Selector
	- ArrowUp - last command
	- ArrowDown - next command

- commands : (parameter list)
	- "set" (instance,field,value)			set a value of a field of an instance
	- "get" (instance,field) -> print		get a value from a field of an instance
	- "call" (instance,method)			call a registered method with parameters
	- "watch" (instance,field,timeToUpdate)		adds field of instance to the watchlist with update interval
	- "exit" 					exits the whole game
	- "reload"					reloads current scene
	
- Attributes :
	- for "safe-mode" you can Tag classes and field with [ExposeDebugConsole], so you can easily acces the values you want

		
- register methods you can call with the console with "[ConsoleInstance].RegisterMethod"


