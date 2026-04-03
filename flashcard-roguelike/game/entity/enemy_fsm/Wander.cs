using Godot;
using System;
using Array = Godot.Collections.Array;
[Tool]
public partial class Wander : EnemyBaseState
{
	[Export]
	public Area3D PursuitDetector;
	[Export]
	public float WanderSpeed;
	[Export]
	public NavigationAgent3D navAgent;
	private bool _playerDetected = false;
	//private float _wanderTimer = 10.0F;
	private float _currentTimer;
	private bool _wanderTimerFinished = false;
	private Vector3 _targetPosition = Vector3.Zero;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PursuitDetector.BodyEntered += OnBodyEntered;
		PursuitDetector.BodyExited += OnBodyExited;
		
	}

	public override Array CheckRelevance(double delta)
	{
		if (_playerDetected)
		{
			return [true, StateNames.pursue];
		}

		if (navAgent.IsTargetReached())
		{
			return [true, StateNames.idle];
		}

		return [false];
	}

	public override void Update(double delta)
	{
		//_currentTimer -= (float)delta;
		//if (_currentTimer <= 0.0F)
		//{
			//_wanderTimerFinished = true;
		//}

		//if (enemy.GlobalPosition == _targetPosition)
		//{
			//SetRandomNavPosition();
		//}
		
		Vector3 destination = navAgent.GetNextPathPosition();
		Vector3 localDestination = destination - enemy.GlobalPosition;
		Vector3 direction = localDestination.Normalized();
		//enemy.LookAt(_targetPosition);
		enemy.Velocity = direction * WanderSpeed;
		Vector3 enemyCurrentPosition = enemy.GlobalPosition;
		enemy.MoveAndSlide();
	}

	public override void OnEnterState()
	{
		//_currentTimer = _wanderTimer;
		//_wanderTimerFinished = false;
		SetRandomNavPosition();
	}

	private void OnBodyEntered(Node3D body){
		if (body is Player player){
			_playerDetected = true;
			enemy.EnemyModel.Player = player;
		}
	}
	
	private void OnBodyExited(Node3D body){
		if (body is Player player){
			_playerDetected = false;
		}
	}

	private void SetRandomNavPosition()
	{
		Vector3 enemyCurrentPosition = enemy.GlobalPosition;
		
		float offsetX = GD.RandRange(-10, 10);
		float offsetZ  = GD.RandRange(-10, 10);
		_targetPosition = enemyCurrentPosition +  new Vector3(offsetX, 0, offsetZ);
		_targetPosition.Y = 0.0F;
		navAgent.SetTargetPosition(_targetPosition);
	}
}
