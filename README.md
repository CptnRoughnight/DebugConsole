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
	- "set" (instance,field,value)
	- "get" (instance,field) -> print
	- "call" (instance,method)
	- "watch" (instance,field,timeToUpdate)
	
		
- register methods you can call with the console with "[ConsoleInstance].RegisterMethod"


