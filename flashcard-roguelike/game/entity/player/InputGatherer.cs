using Godot;
using System;

public partial class InputGatherer : Node
{
	
	public InputPackage GatherInput(){
		InputPackage new_input = new InputPackage();
		if(Input.IsActionPressed(StateNames.jump)){
			new_input.actions.Add(StateNames.jump);
		}
		Vector2 inputDirection = Input.GetVector("left", "right", "forward", "backward");
		new_input.InputDirection = inputDirection;
		if (inputDirection != Vector2.Zero)
		{
			new_input.actions.Add(StateNames.run);
			if (Input.IsActionPressed(StateNames.sprint)){
				new_input.actions.Add(StateNames.sprint);
			}
		}
		//If no new actions are added default to idle state
		if (new_input.actions.Count < 1)
		{
			new_input.actions.Add(StateNames.idle);
		}

		return new_input;
	}
}
