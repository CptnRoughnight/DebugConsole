using Godot;
using System.Collections.Generic;

public partial class Fields : Window
{
	[Signal]
	public delegate void onItemSelectedEventHandler(int index);

	private OptionButton button;
	private int currentIndex;

	public override void _Ready()
	{
		Visible = false;
		button = GetNode<OptionButton>("OptionButton");
		currentIndex = 0;
	}

	public void AddNewList(List<string> items)
	{
		currentIndex = 0;
		button.Clear();
		foreach (string s in items)
			button.AddItem(s);
	}

	public void _on_option_button_item_selected(int index)
	{
		GD.Print("item selected " + index.ToString());
		currentIndex = index;
		EmitSignal(SignalName.onItemSelected,new Variant[] { index });
	}

	public void _on_option_button_item_focused(int index)
	{
		GD.Print("item focused");
		currentIndex = index;
	}

	public int getCurrentIndex()
	{
		return currentIndex;
	}

}
