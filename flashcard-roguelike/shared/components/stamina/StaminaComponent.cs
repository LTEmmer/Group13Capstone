using Godot;
using System;

public partial class StaminaComponent : Node
{
	[Export]
	public Player player;
	[Export]
	public double MaxStamina = 100.0;
	public double CurrentStamina = 0.0;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentStamina = MaxStamina;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(player.IsOnFloor()){ //doing this for now to reset the double jump
			CurrentStamina = MaxStamina;
		}
	}
}
