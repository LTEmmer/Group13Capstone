using Godot;
using System;

public partial class GameDifficultyManager : Node
{
	// Singleton for access since it is autoloaded
	public static GameDifficultyManager Instance { get; private set; }

	private float _currentDifficultyScore = 0.0F;
	private float _baselineDifficultyScore = 1.0F;
	private float _difficultyScoreIncrease = 0.15F;  // 0.25 per victory — slower difficulty ramp
	private float _difficultyScoreDecrease = 0.50F; // Used for running
	private int _currentFloor = 1;
	private const float _baselineFloorIncrement = 1.0f;

	public int CurrentFloor => _currentFloor;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;

		EventManager.Instance.listen("on_battle_victory", new Callable(this, MethodName.on_battle_victory));
		EventManager.Instance.listen("on_ran_from_battle", new Callable(this, MethodName.on_ran_from_battle));
		EventManager.Instance.listen("on_battle_lost", new Callable(this, MethodName.on_battle_lost));
		_currentDifficultyScore = _baselineDifficultyScore;
	}
	
	public int getEnemyCount(){
		int enemyCount = (int)_currentDifficultyScore;
		float enemyChance = _currentDifficultyScore - MathF.Floor(_currentDifficultyScore); // fractional part [0,1)
		float chanceResult = (float)GD.RandRange(0.0F, 1.0F);
		if(chanceResult < enemyChance){
			enemyCount += 1;
		}

		enemyCount = Math.Min(enemyCount, 3); // Cap enemy count at 3

		return enemyCount;
	}

	public void ResetDifficulty(){
		_currentDifficultyScore = _baselineDifficultyScore;
	}

	public void AdvanceFloor()
	{
		_currentFloor++;
		_baselineDifficultyScore += _baselineFloorIncrement;
		_currentDifficultyScore += _baselineFloorIncrement;
	}

	public void ResetGame()
	{
		_currentFloor = 1;
		_baselineDifficultyScore = 1.0f;
		_currentDifficultyScore = 1.0f;
	}
	
	public float getCurrentDifficultyScore(){
		return _currentDifficultyScore;
	}
	
	public void on_battle_victory(string test){
		_currentDifficultyScore += _difficultyScoreIncrease;
	}

	public void on_ran_from_battle(string test){
		_currentDifficultyScore -= _difficultyScoreDecrease;
		if(_currentDifficultyScore < _baselineDifficultyScore)
		{ 
			_currentDifficultyScore = _baselineDifficultyScore;
		}
	}

	public void on_battle_lost(string test){
		_currentDifficultyScore = _baselineDifficultyScore;
	}
}
