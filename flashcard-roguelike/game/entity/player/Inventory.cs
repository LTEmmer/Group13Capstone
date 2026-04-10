using Godot;

public partial class Inventory : CanvasLayer
{
	public override void _Ready()
	{
		Visible = false;
	}

	
	public override void _Input(InputEvent @event)
	{
		if (!Visible)
		{
			GetChild<Control>(0).MouseFilter = Control.MouseFilterEnum.Stop;
		} else
		{
			GetChild<Control>(0).MouseFilter = Control.MouseFilterEnum.Pass;
		}
	}
}
