using Godot;
using System.Collections.Generic;

// Handles battle area setup, entity positioning, and position restoration.
// Manages the physical arrangement of combatants in the battle scene.
public class BattleSetup
{
    private Transform3D _originalPlayerTransform;
    private Transform3D[] _originalEnemyTransforms;
    private Node3D _battleArea;
    
    // Store the battle area reference
    public void SetBattleArea(Node3D battleArea)
    {
        _battleArea = battleArea;
    }
    
    // Move player and enemies to their battle positions and store original transforms
    public void SetupBattlePositions(Player player, List<EnemyFSM> enemies)
    {
        if (_battleArea == null)
        {
            GD.PrintErr("BattleSetup: BattleArea is not assigned. Combat area cannot be set up.");
            return;
        }
        
        // Store original positions to reset later
        _originalPlayerTransform = player.GlobalTransform;
        _originalEnemyTransforms = new Transform3D[enemies.Count];
        
        for (int i = 0; i < enemies.Count; i++)
        {
            _originalEnemyTransforms[i] = enemies[i].GlobalTransform;
        }
        
        // Move player to player spot
        Marker3D playerSpot = _battleArea.GetNode<Marker3D>("PlayerSpot");
        player.GlobalTransform = playerSpot.GlobalTransform;
        
        // Move each enemy to their respective spots
        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].GlobalPosition = _battleArea.GetNode<Marker3D>($"EnemySpot{i}").GlobalPosition +  new Vector3(0, 1.25F, 0);
        }
    }
    
    // Initialize enemy status UI components
    public void InitializeEnemyStatusUI(List<EnemyFSM> enemies, 
        Dictionary<EnemyFSM, HealthComponent> healthComponents,
        Dictionary<EnemyFSM, AttackComponent> attackComponents,
        Dictionary<EnemyFSM, EnemyStatusComponent> statusComponents)
    {
        foreach (var enemy in enemies)
        {
            var status = statusComponents[enemy];
            var health = healthComponents[enemy];
            var attack = attackComponents[enemy];
            
            status.Initialize(enemy, health, attack);
            status.SlideIn();
        }
    }
    
    // Reset player and enemy positions to their original locations
    public void ResetPositions(Player player, List<EnemyFSM> enemies, bool enemiesAlive)
    {
        // Reset player position
        player.GlobalTransform = _originalPlayerTransform;
        
        // Reset enemy positions if they're still alive
        if (enemiesAlive && _originalEnemyTransforms != null)
        {
            for (int i = 0; i < enemies.Count && i < _originalEnemyTransforms.Length; i++)
            {
                enemies[i].GlobalTransform = _originalEnemyTransforms[i];
            }
        }
        
        _originalEnemyTransforms = null; // Clear stored enemy transforms after resetting
    }
    
    // Find and validate battle area in the given room
    public bool FindBattleArea(Node3D room)
    {
        if (room == null)
        {
            GD.PrintErr("BattleSetup: Room is null.");
            return false;
        }
        
        // Look for a child node named "BattleArea" in the room
        var battleAreaInRoom = room.GetNodeOrNull<Node3D>("BattleArea");
        if (battleAreaInRoom != null)
        {
            _battleArea = battleAreaInRoom;
            return true;
        }
        else
        {
            GD.PrintErr($"BattleSetup: No BattleArea found in room {room.Name}.");
            return false;
        }
    }
}
