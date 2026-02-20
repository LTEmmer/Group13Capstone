using Godot;
using System;
using System.Collections.Generic;

public partial class FlashCardInteractable : Node3D
{
	[Export]
	public PackedScene FlashCardEntityScene {get; set;}
	[Export]
	public Node3D FlashCardContainer {get; set;}
	[Export]
	public CharacterBody3D Player {get; set;}
	
	
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
		FlashCardContainer.LookAt(Player.GlobalPosition);
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
		List<FlashcardSet> activeFlashCardSetList = FlashcardManager.Instance.ActiveFlashCardLists;
		FlashcardSet flashCardSet;
		
		// Get random flashcard set from active flash card sets
		if (activeFlashCardSetList.Count > 1){
			flashCardSet = (activeFlashCardSetList[0]);
		}
		else{
			int randomFlashCardSetIndex = (int)(GD.Randi() % activeFlashCardSetList.Count);
			flashCardSet = activeFlashCardSetList[randomFlashCardSetIndex];
		}
		
		// generate random number and get flash card based on random number
		int randomFlashCardIndex = (int)(GD.Randi() % flashCardSet.Cards.Count);
		Flashcard flashCardData = flashCardSet.Cards[randomFlashCardIndex];
		
		// Construct flash_card_entity based on info gathered from flash card 
		FlashCardEntity flashcard = FlashCardEntityScene.Instantiate() as FlashCardEntity;
		flashcard.setLabels(flashCardData.Question,flashCardData.Answer);
		
		// If a flashcard entity already exists remove that child
		if (FlashCardContainer.GetChildren().Count > 0)
		{
			Node flashcard_child = (FlashCardContainer.GetChildren())[0];
			FlashCardContainer.RemoveChild(flashcard_child);
		}
		
		// Add flashcard entity to be a child of the flashcardcontainer
		// flashcardcontainer already has proper position
		FlashCardContainer.AddChild(flashcard);
		
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
