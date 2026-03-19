using Godot;
using System;

public partial class Item : Node3D
{
	[Export] private CollectibleComponent _collectibleComponent;
	[Export] public ItemResource Resource;
	[Export] public Mesh _mesh;
	[Export] private MeshInstance3D _meshInstance;
	private ItemInstance _itemInstance;

	public override void _Ready()
	{
		if (Resource != null)
			_itemInstance = new ItemInstance(Resource);

		if (_collectibleComponent != null)
			_collectibleComponent.Collected += OnCollected; // connect the signal

		if (_mesh != null)
			_meshInstance.Mesh = _mesh;
	}

	private void OnCollected(Node collector)
	{
		if (_itemInstance == null || _itemInstance.Resource == null)
			throw new ArgumentNullException(nameof(_itemInstance), "ItemInstance or its Resource cannot be null!");

		if (collector is Player player)
		{
			foreach (var effect in _itemInstance.Resource.Effects)
			{
				effect.Apply(player, _itemInstance);
			}
		}
	}
}