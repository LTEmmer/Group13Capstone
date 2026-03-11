using Godot;
using System;
using System.Collections.Generic;

public partial class Model : Node3D
{
	[Export]
	public CharacterBody3D player;
	[Export]
	public AnimationPlayer animator;
	[Export]
	public Node StateContainer;
	
	public Dictionary<string,BaseState> States = new Dictionary<string,BaseState>();
	public BaseState CurrentState;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AcceptStates();
		CurrentState = States[StateNames.idle];
		SwitchTo(StateNames.idle);
	}
	
	public void Update(InputPackage input, double delta){
		Godot.Collections.Array verdict = CurrentState.CheckRelevance(input,delta);
		if((bool)verdict[0]){
			SwitchTo((String)verdict[1]);
		}
		CurrentState.Update(input,delta);
	}
	
	public void SwitchTo(String NextStateName){
		CurrentState.OnExitState();
		CurrentState = States[NextStateName];
		CurrentState.MarkEnterStateTime();
		CurrentState.OnEnterState();
		animator.Play(CurrentState.StateAnimation);
	}

	private void AcceptStates(){
		foreach(BaseState childState in StateContainer.GetChildren())
		{
			if(childState is BaseState){
				if(childState.StateName != StateNames.base_state ){
					States[childState.StateName] = childState;
					childState.player = player;
					childState.States = States;
				}
			}
		}
	}
	

}
