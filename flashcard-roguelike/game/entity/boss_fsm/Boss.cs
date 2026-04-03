using Godot;
using System;

public partial class Boss : EnemyFSM, IBossEnemy
{
	[Export] public int StreakRequired { get; set; } = 3;
	[Export] public float BlockReduction { get; set; } = 0.5f;
	
}
