using Godot;
using System;
using System.Collections.Generic;

public partial class EventManager : Node
{
	// Singleton instance
	public static EventManager Instance {get; private set;}
	private Dictionary <string, List<Callable>> listeners = new Dictionary <string, List<Callable>>();

	public override void _Ready(){
		Instance = this;
	}

	public void listen(string Event, Callable Callback){
		GD.Print(listeners);
		if(listeners.ContainsKey(Event) != true){
			listeners.Add(Event, new List<Callable>());
			listeners[Event].Add(Callback);
		}else{
			listeners[Event].Add(Callback);
		}
	}

	public void ignore(string Event, Callable Callback){
		if(listeners.ContainsKey(Event) == true){
			listeners[Event].Remove(Callback);
		}
	}

	public void raise(string Event, String args){
		for(int i = 0; i <= listeners[Event].Count-1; i++){
			Callable callable = listeners[Event][i];
			var Obj = callable.Target;
			if(IsInstanceValid(Obj) != true){
				listeners[Event].RemoveAt(i);
				// reset i back one so we dont skip the element after the one we just removed
				i--;
			}else{
				callable.Call(args);
			}
		}
	} 
}
