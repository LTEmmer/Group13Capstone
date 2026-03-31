using Godot;
using System.Collections.Generic;

public partial class ParkourEventRoom : Room, IEventRoom
{
	[Export] public BaseNPC Npc;
	[Export] public PackedScene FlashcardTFScene;

	// Boon and penalty parameters, 50% and 25% changes respectively
	[Export] public float BoonSpeedMultiplier = .5f;
	[Export] public float PenaltyMultiplier = .25f;

	public bool IsCompleted { get; private set; }
	public float Difficulty { get; private set; }

	private List<MovingPlatform> _movingPlatforms = new List<MovingPlatform>();
	private List<ScalingPlatform> _scalingPlatforms = new List<ScalingPlatform>();
	private List<BlinkingPlatform> _blinkingPlatforms = new List<BlinkingPlatform>();
	private Area3D _exitTrigger;
	private bool _npcUsed = false;
	private FlashcardChallengeTrueOrFalse _challenge;

	public override void _Ready()
	{
		base._Ready();

		_exitTrigger = GetNode<Area3D>("ExitLedge/ExitTrigger");
		_exitTrigger.BodyEntered += OnExitReached;

		if (Npc != null)
		{
			Npc.OnInteraction += TriggerEvent;
		}

		// Gather all platforms in the room
		foreach (Node3D child in GetNode<Node3D>("MovingPlatforms").GetChildren())
		{
			if (child is MovingPlatform mp)
				_movingPlatforms.Add(mp);
		}

		foreach (Node3D child in GetNode<Node3D>("ScalingPlatforms").GetChildren())
		{
			if (child is ScalingPlatform sp)
				_scalingPlatforms.Add(sp);
		}

		foreach (Node3D child in GetNode<Node3D>("BlinkingPlatforms").GetChildren())
		{
			if (child is BlinkingPlatform bp)
				_blinkingPlatforms.Add(bp);
		}
	}

	private void OnFlashcardAnswered(bool isCorrect)
	{
		float difficulty = GameDifficultyManager.Instance == null
			? 0f
			: GameDifficultyManager.Instance.getCurrentDifficultyScore();
		TaloTelemetry.TrackFlashcardAnswer(isCorrect, "parkour_event", difficulty);
		// Lock mouse
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// Remove challenge if it's still active
		_challenge?.QueueFree();

		// Apply boon or penalty based on answer
		foreach (var platform in _movingPlatforms)
		{
			platform.ApplyBoon(BoonSpeedMultiplier, isCorrect);
		}
			
		foreach (var platform in _scalingPlatforms)
		{
			platform.ApplyBoon(BoonSpeedMultiplier, isCorrect);
		}
			
		foreach (var platform in _blinkingPlatforms)
		{
			platform.ApplyBoon(BoonSpeedMultiplier, isCorrect);
		}
			
	}

	private void OnExitReached(Node3D body)
	{
		if (body.Name != "Player") 
		{
			return;
		}

		CompleteEvent(true);
	}

	public void TriggerEvent()
	{
		if (_npcUsed || FlashcardTFScene == null) 
		{
			return;
		}

		// Unlock mouse when event is triggered
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_npcUsed = true;

		_challenge = FlashcardTFScene.Instantiate<FlashcardChallengeTrueOrFalse>();
		AddChild(_challenge);

		var card = FlashcardManager.Instance.GetRandomCard();
		if (card == null)
		{
			GD.PrintErr("No flashcards available for ParkourEventRoomOne challenge.");
			_challenge.QueueFree();
			return;
		}

		_challenge.ConnectAnswerSubmitted(OnFlashcardAnswered);
		_challenge.ShowChallenge(card, "Answer correctly to slow the platforms!");
	}

	public void CompleteEvent(bool success)
	{
		if (IsCompleted)
		{
			return;
		}

		IsCompleted = true;

		if (success)
		{
			ApplyReward();
		}
		else
		{
			ApplyPenalty();
		}
	}

	public void ApplyReward()
	{
		EventManager.Instance.raise("on_room_clear", "test");
	}

	public void ApplyPenalty()
	{
		GD.Print("idk twenty lashings or something");
	}
}
