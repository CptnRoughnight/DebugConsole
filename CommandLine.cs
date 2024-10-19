using Godot;
using System;

public partial class CommandLine : LineEdit
{
	[Signal]
	public delegate void onCommandUpEventHandler();
	[Signal]
	public delegate void onCommandDownEventHandler();
	[Signal]
	public delegate void onCommandEnterEventHandler();
	[Signal]
	public delegate void onHelpPressedEventHandler();
	[Signal]
	public delegate void onAllAccessToggleEventHandler();


	 public override void _Input(InputEvent @event)
    {
    	    if (@event is InputEventKey eventKey)
	        {
	        	if (eventKey.Pressed)
	        	{
		            if (eventKey.Keycode==Key.Up)
		                EmitSignal(SignalName.onCommandUp);
		            if (eventKey.Keycode==Key.Down)
		                EmitSignal(SignalName.onCommandDown);
		            if (eventKey.Keycode==Key.Enter)
		            	EmitSignal(SignalName.onCommandEnter);
		            if (eventKey.Keycode==Key.F1)
		            	EmitSignal(SignalName.onHelpPressed);
		            if (eventKey.Keycode==Key.F2)
		            	EmitSignal(SignalName.onAllAccessToggle);
		        }
	        }
	    
    }
}
