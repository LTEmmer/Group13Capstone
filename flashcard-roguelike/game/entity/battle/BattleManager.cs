using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node
{
	[Export] public BattleTransition Transition;
	[Export] public Node3D BattleArea;
    private Transform3D _originalPlayerTransform;
    private Transform3D[] _originalEnemyTransforms;

	private Node3D _battleArea;

	public void StartBattle(Player player, EnemyExample[] enemies)
	{
		if (Transition == null)
		{
			GD.PrintErr("BattleManager: Transition is not assigned.");
			return;
		}

		if (player == null)
		{
			GD.PrintErr("BattleManager: Player is null.");
			return;
		}

		if (enemies == null || enemies.Length == 0)
		{
			GD.PrintErr("BattleManager: No enemies provided.");
			return;
		}

        _battleArea = BattleArea;

		EnemyExample focusEnemy = enemies[0];
		Transition.Cover(focusEnemy, player, () =>
		{
			SetupBattleArea(player, enemies);
			Transition.Reveal();
		});
	}

	private void SetupBattleArea(Player player, EnemyExample[] enemies)
	{
		if (_battleArea == null)
		{
			GD.PrintErr("BattleManager: BattleAreaScene is not assigned. Combat area cannot be set up.");
			return;
		}

        _originalPlayerTransform = player.GlobalTransform;
        int originalIndex = 0;
        _originalEnemyTransforms = new Transform3D[enemies.Length];
        foreach (var enemy in enemies)
        {
            _originalEnemyTransforms[originalIndex] = enemy.GlobalTransform;
            originalIndex++;
        }

		Marker3D playerSpot = _battleArea.GetNode<Marker3D>("PlayerSpot");
		player.GlobalTransform = playerSpot.GlobalTransform;

		int spotIndex = 0;
        foreach (var enemy in enemies)
        {   
            // Position each enemy at their designated spot in the battle area, starts at EnemySpot0
            enemy.GlobalTransform = _battleArea.GetNode<Marker3D>($"EnemySpot{spotIndex}").GlobalTransform;
            spotIndex++;

        }
	}

    private void ResetPlayerEnemyPositions(Player player, EnemyExample[] enemies)
    {
        // Reset player position
        player.GlobalTransform = _originalPlayerTransform;

        // Reset enemy positions
        int index = 0;
        foreach (var enemy in enemies)
        {
            if (enemy is Node3D enemyNode)
            {
                enemy.GlobalTransform = _originalEnemyTransforms[index];
                index++;
            }
        }

        _originalEnemyTransforms = null; // Clear stored enemy transforms after resetting
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            // Example input to trigger the transition for testing
            Player player = GetParent().GetNode<Player>("Player");
            EnemyExample[] enemies = { GetParent().GetNode<EnemyExample>("EnemyExample") };

            StartBattle(player, enemies);
        }
    }
}
