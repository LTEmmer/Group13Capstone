using Godot;
using System;
using System.Collections.Generic;

public partial class EModel : Node3D
{
	[Export]
	public EnemyFSM enemy;
	[Export]
	public AnimationPlayer animator;
	[Export]
	public Node StateContainer;
	public CharacterBody3D Player;
	public Dictionary<string,EnemyBaseState> States = new Dictionary<string,EnemyBaseState>();
	public EnemyBaseState CurrentState;
	public bool BattleMode = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AcceptStates();
		CurrentState = States[StateNames.idle];
		SwitchTo(StateNames.idle);
	}
	
	public void Update(double delta){
		Godot.Collections.Array verdict = CurrentState.DefaultCheckRelevance(delta);
		if((bool)verdict[0]){
			SwitchTo((String)verdict[1]);
		}
		CurrentState.Update(delta);
	}
	
	public void SwitchTo(String NextStateName){
		CurrentState.OnExitState();
		CurrentState = States[NextStateName];
		CurrentState.MarkEnterStateTime();
		CurrentState.OnEnterState();
		animator.Play(CurrentState.StateAnimation);
	}

	private void AcceptStates(){
		foreach(EnemyBaseState childState in StateContainer.GetChildren())
		{
			if(childState is EnemyBaseState){
				if(childState.StateName != StateNames.base_state ){
					States[childState.StateName] = childState;
					childState.enemy = enemy;
					//childState.States = States;
				}
			}
		}
	}
}
