using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class Pursue : EnemyBaseState
{
	[Export]
	public Area3D PursuitDetector;
	[Export] 
	public float PursuitSpeed = 2.0F;
	private bool _playerDetected = true;
	private CharacterBody3D _player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PursuitDetector.BodyEntered += OnBodyEntered;
		PursuitDetector.BodyExited += OnBodyExited;
	}

	public override Array CheckRelevance(double delta)
	{
		if(enemy.EnemyModel.BattleMode){
			_playerDetected = false;
			return [true,StateNames.idlebattle];
		}
		if (!_playerDetected) // If player escapes the Pursuit Detector
		{
			return [true, StateNames.wander];
		}
		
		return [false];
	}

	public override void Update(double delta)
	{
		PursuePlayer();
	}

	private void PursuePlayer()
	{
		Vector3 TargetPosition = _player.GlobalPosition;
		enemy.LookAt(-TargetPosition);
		enemy.Velocity = enemy.GlobalPosition.DirectionTo(TargetPosition) * PursuitSpeed;
		enemy.MoveAndSlide();
	}

	public override void OnEnterState()
	{
		_player = enemy.EnemyModel.Player;
		_playerDetected = true;
	}

	private void OnBodyExited(Node3D body){
		if (body is Player player)
		{
			_playerDetected = false;
		}
	}
	
	private void OnBodyEntered(Node3D body){
		if (body is Player player){
			_playerDetected = true;
			enemy.EnemyModel.Player = player;
			_player = enemy.EnemyModel.Player;
		}
	}
}
