using Godot;
using System;

public partial class FlashCardInteractable : Node3D
{
	[Export]
	public PackedScene FlashCardEntityScene {get; set;}
	[Export]
	public Node3D FlashCardContainer {get; set;}
	
	private bool _playerInRange = false;
	private Node3D _player;
	private Area3D _area;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_area = GetNode<Area3D>("Area3D");
		_area.BodyEntered += OnBodyEntered;
		_area.BodyExited += OnBodyExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		return;
	}

	public override void _Input(InputEvent @event)
	{
		if (!_playerInRange)
			return;

		if (@event.IsActionPressed("interact"))
		{
			revealFlashCard();
		}
	}
	
	public void revealFlashCard()
	{
		// Get FlashCard singleton and access flashcard set associated to this study session
		// Array activeFlashCardList = FlashCardSingleton.getActiveFlashCardList()
		// Array flashCardSet
		// if activeFlashCardList.length() > 1{
			// flashCardSet = (activeFlashCardList[0])
		//}
		//else{
			// int randomFlashCardSetIndex = (GD.Randi() % activeFlashCardList.length()
			//flashCardSet = (activeFlashCardList[randomFlashCardSetIndex]
		//}
		// generate random number and get flash card based on random number
		// int randomFlashCardIndex = (GD.Randi() % flashCardSet.length()
		// FlashCardObj? flashCardSet = flashCardSet[randomFlashCardIndex]
		// Construct flash_card_entity based on info gathered from flash card 
		// FlashCardEntity flashcard = FlashCardEntityScene.instantiate()
		// flashcard.setLabels(flashCardSet.question,flashCardSet.answer)
		// If a flashcard entity already exists remove that child
		// if FlashCardContainer.GetChildren().Count() > 0
		//{
			// Node child = (FlashCardContainer.GetChildren())[0]
			//FlashCardContainer.RemoveChild(child)
		//}
		//FlashCardContainer.AddChild(flashcard)
		// Add flashcard entity to be a child of the flashcardcontainer
		// flashcontainer already has proper position
		
	}
	
	// Signals to detect whether player is in range to interact with object
	private void OnBodyEntered(Node body)
	{
		if (body is Node3D node && body.Name == "Player")
		{
			_playerInRange = true;
			_player = node;
		}
	}

	private void OnBodyExited(Node body)
	{
		if (body == _player)
		{
			_playerInRange = false;
			_player = null;
		}
	}
}
