using Godot;

public partial class Item : Node3D
{
	[Export] private CollectibleComponent _collectibleComponent;
	[Export] private MeshInstance3D _meshInstance;
	[Export] public ItemResource Resource;

	public override void _Ready()
	{
		if (Resource == null) return;

		if (_meshInstance != null)
			_meshInstance.Mesh = Resource.Mesh;

		if (_collectibleComponent != null)
		{
			_collectibleComponent.Item = Resource;
			_collectibleComponent.Collected += OnCollected;
		}
	}

	private void OnCollected(Node collector)
	{
		if (Resource == null || collector is not Player player) return;

		AudioManager.Instance.PlayItemPickupSound();

		foreach (var effect in Resource.Effects)
			effect.Apply(player, new ItemInstance(Resource));
	}
}
