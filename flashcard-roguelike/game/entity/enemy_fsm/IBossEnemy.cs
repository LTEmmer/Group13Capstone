// Interface for bosses, at minimum a boss should have these properties.
// BattleManager uses this interface to detect boss battles and read configuration.
public interface IBossEnemy
{
    int StreakRequired { get; } // consecutive correct attack answers needed to deal damage
    float BlockReduction { get; } // fraction of enemy BaseDamage taken on a successful block (0 = no damage, 1 = full damage)
}
