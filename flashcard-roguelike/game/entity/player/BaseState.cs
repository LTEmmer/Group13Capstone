using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
[Tool]
public partial class BaseState : Node
{
	[Export]
	public String StateAnimation;
	[Export]
	public String StateName;
	[Export]
	public AnimationPlayer animator;
	
	public CharacterBody3D player;
	public Dictionary<string,BaseState> States = new Dictionary<string,BaseState>();

	protected double _enterStateTime; 
	
	static protected Dictionary<string,int> StatesPriority  = new Dictionary<string,int>{
		{StateNames.idle , 1},
		{StateNames.run , 2},
		{StateNames.sprint , 3},
		{StateNames.interact , 5},
		{StateNames.jump , 10},
		{StateNames.midair, 10},
		{StateNames.landing , 10},
		{StateNames.death , 200}
	};
	
	//Dropdown menu for assigning states and animations
	public override void _ValidateProperty(Godot.Collections.Dictionary property){
		if(property["name"].AsStringName() == PropertyName.StateName){
			var StateNameValues = "";
			Type type = typeof(StateNames);
			PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
			foreach(PropertyInfo prop in propertyInfos){
				StateNameValues += prop.Name;
				StateNameValues +=  ",";
			}
			property["hint"] = (int)PropertyHint.Enum;
			property["hint_string"] = StateNameValues;
		}
		if(property["name"].AsStringName() == PropertyName.StateAnimation){
			var animations = "";
			var animationsList = animator.GetAnimationList();
			for(int i = 0; i < animationsList.Length; i++){
				animations += animationsList[i];
				if(i < animations.Length - 1){
					animations += ",";
				}
			}
			property["hint"] = (int)PropertyHint.Enum;
			property["hint_string"] = animations;
		}
	}
	
	public virtual Godot.Collections.Array CheckRelevance(InputPackage input, double delta){
		return [false,"ERROR: CheckRelevance not implemented in child State"];
	}
	
	public virtual void Update(InputPackage input, double delta){
		return;
	}
	
	
	public Godot.Collections.Array BestNextInput(InputPackage input){
		SortInputActionsByStatePriority(input);
		foreach(String action in input.actions){
			if(States[action] == this){
				return [false];
			}else{
				return [true, action];
			}
		}
		return [false,"ERROR: StatesContainer does not contain idle"];
	}
	
	public virtual void OnEnterState(){
		return;
	}
	
	
	public virtual void OnExitState(){
		return;
	}

	
	public void MarkEnterStateTime(){
		_enterStateTime = Time.GetUnixTimeFromSystem();
	}
	
	public double GetProgress(){
		double now = Time.GetUnixTimeFromSystem();
		return now - _enterStateTime;
	}
	
	public bool WorksLongerThan(double TimeStamp){
		if(GetProgress() < TimeStamp){
			return true;
		}
		return false;
	}
	
	private static void SortInputActionsByStatePriority(InputPackage input){
		input.actions.Sort((a,b) => StatesPriority[b].CompareTo(StatesPriority[a]));
	}
	
}
