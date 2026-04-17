using Godot;

[GlobalClass]
public partial class ModBlock: ItemEffect
{
	[Export] int _block = 0;

    public override void Apply(Node target, ItemInstance item)
	{
		DoModBlock(target, true);
	}

    public override void Remove(Node target)
    {
        base.Remove(target);
		DoModBlock(target, false);

    }

	private void DoModBlock(Node target, bool apply)
	{
		float block = apply? _block: -_block;
        GD.Print($"Adding {block} Block to {target.Name}");
		var health = target.GetNodeOrNull<HealthComponent>("HealthComponent");
		if (health == null)
		{
			GD.Print("ModHealth: Target has no health component");
			return;
		}
		
		if(block == 0)return;
		health.Shield += block; 

    	health.Shield = Mathf.Max(health.Shield, 0);
	}

}