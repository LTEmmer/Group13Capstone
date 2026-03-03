using Godot;
using System;
using System.Collections.Generic;

public partial class Room : Node3D
{
	private List<RoomConnection> _exitRoomConnections = new List<RoomConnection>(); 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Connect("child_entered_tree", Callable.From<Node>(OnChildEnteredTree));
		this.Connect("child_exited_tree", Callable.From<Node>(OnChildExitedTree));

	}

	public void OnChildEnteredTree(Node body){
		if(body is Node3D node && body.Name == "Player"){
			Node3D Exits = GetNode<Node3D>("Exits");
			var children = Exits.GetChildren();
			foreach(Node child in children){
			if(child.GetChildren().Count > 0){
				RoomConnection room_conn = (RoomConnection)child.GetChild(0);
				_exitRoomConnections.Add(room_conn);
				}
			}
			foreach(RoomConnection room_conn in _exitRoomConnections){
				room_conn.PlayerInRoom = true;
			}
		}
	}
	
	public void OnChildExitedTree(Node body){
		if(body is Node3D node && body.Name == "Player"){
			foreach(RoomConnection room_conn in _exitRoomConnections){
				room_conn.PlayerInRoom = false;
			}
		}
	}
}
