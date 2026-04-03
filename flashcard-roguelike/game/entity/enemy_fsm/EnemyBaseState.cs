using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
[Tool]
public partial class EnemyBaseState : Node
{
	[Export]
	public String StateName;
	[Export]
	public String StateAnimation;
	[Export]
	public AnimationPlayer animator;
	public EnemyFSM enemy;
	public CharacterBody3D PlayerBody;
	protected double _enterStateTime; 
	static protected Dictionary<string,int> StatesPriority  = new Dictionary<string,int>{
		{StateNames.idle , 1},
		{StateNames.death , 200}
	};
	
	//private String _forcedState;
	//private bool _hasforcedState
	
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
	
	public virtual Godot.Collections.Array DefaultCheckRelevance(double delta){
		return CheckRelevance(delta);
	}
	
	public virtual Godot.Collections.Array CheckRelevance(double delta){
		return [false,"ERROR: CheckRelevance not implemented in child State"];
	}
	
	public virtual void Update(double delta){
		return;
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
	
	public bool WorksLessThan(double TimeStamp){
		if(GetProgress() < TimeStamp){
			return true;
		}
		return false;
	}
	
	public bool WorksLongerThan(double TimeStamp){
		if(GetProgress() >= TimeStamp){
			return true;
		}
		return false;
	}
}
