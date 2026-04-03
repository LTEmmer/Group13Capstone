using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class EnemyIdle : EnemyBaseState
{
	[Export]
	private float _idleTimer = 3.0F;
	[Export]
	private bool _staticEnemy = true;
	[Export]
	public Area3D PursuitDetector;
	private float _currentTimer;
	private bool _playerDetected = false;
	
	public override void _Ready()
	{
		PursuitDetector.BodyEntered += OnBodyEntered;
		PursuitDetector.BodyExited += OnBodyExited;
		_currentTimer = _idleTimer;
	}
	
	public override Array CheckRelevance(double delta)
	{
		if(enemy.EnemyModel.BattleMode){
			return [true,StateNames.idlebattle];
		}
		if(_currentTimer <= 0.0F && _staticEnemy == false){
			return [true, StateNames.wander];
		}
		if (_playerDetected)
		{
			return [true, StateNames.pursue];
		}
		return [false]; //Enemy remains in idle by default
	}
	
	public override void Update(double delta){
		if(_staticEnemy == false){
			_currentTimer -= (float)delta;
		}
	}
	
	public override void OnEnterState()
	{
		_currentTimer = _idleTimer;
		enemy.Velocity = Vector3.Zero;
	}
	
		private void OnBodyEntered(Node3D body){
		if (body is Player player){
			_playerDetected = true;
			PlayerBody = player;
		}
	}
	
	private void OnBodyExited(Node3D body){
		if (body is Player player){
			_playerDetected = false;
			PlayerBody = null;
		}
	}
}
