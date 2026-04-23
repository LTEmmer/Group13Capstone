using Godot;
using System.Collections.Generic;

public partial class ParkourEventRoom : Room, IEventRoom
{
	[Export] public BaseNPC Npc;
	[Export] public PackedScene FlashcardTFScene;

	// Boon and penalty parameters, 50% and 25% changes respectively
	[Export] public float BoonSpeedMultiplier = .5f;
	[Export] public float PenaltyMultiplier = .25f;

	[Export] public TreasureChest Reward;
	[Export] public int PenaltyDamage = 25;
	[Export] public PackedScene SkipPromptScene;
	[Export] public int FallsToShowSkip = 3;

	public bool IsCompleted { get; private set; }
	public float Difficulty { get; private set; }

	private List<MovingPlatform> _movingPlatforms = new List<MovingPlatform>();
	private List<ScalingPlatform> _scalingPlatforms = new List<ScalingPlatform>();
	private List<BlinkingPlatform> _blinkingPlatforms = new List<BlinkingPlatform>();
	private Area3D _exitTrigger;
	private bool _npcUsed = false;
	private FlashcardChallengeTrueOrFalse _challenge;
	private int _fallCount = 0;
	private bool _skipShown = false;
	private SkipPrompt _skipPrompt;

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

		if (Reward == null)
        {
            GD.PrintErr("Reward not set, check inspector");
        }

		Reward.SetCollision(false);

		var damageZone = GetNodeOrNull<DamageZone>("WaterPlane/DamageZone");
		if (damageZone != null)
		{
			damageZone.Connect("PlayerFell", Callable.From(OnPlayerFell));
		}
		else
		{
			GD.PrintErr("DamageZone not found at WaterPlane/DamageZone, skip prompt won't work");
		}
	}

	private void OnPlayerFell()
	{
		if (IsCompleted || _skipShown) 
		{
			return;
		}

		_fallCount++;
		if (_fallCount >= FallsToShowSkip)
		{
			ShowSkipPrompt();
		}
	}

	private void ShowSkipPrompt()
	{
		if (SkipPromptScene == null)
		{
			GD.PrintErr("SkipPromptScene not set, check inspector");
			return;
		}

		_skipShown = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;

		_skipPrompt = SkipPromptScene.Instantiate<SkipPrompt>();
		AddChild(_skipPrompt);
		_skipPrompt.Visible = true;
		_skipPrompt.Connect("SkipPressed", Callable.From(OnSkipPressed));
		_skipPrompt.Connect("KeepTryingPressed", Callable.From(OnKeepTryingPressed));
	}

	private void OnSkipPressed()
	{
		_skipPrompt?.QueueFree();
		_skipPrompt = null;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		var exitPoint = GetNodeOrNull<Marker3D>("ExitPoint");
		if (exitPoint != null && _player != null)
			_player.GlobalPosition = exitPoint.GlobalPosition;

		CompleteEvent(false);
	}

	private void OnKeepTryingPressed()
	{
		_skipPrompt?.QueueFree();
		_skipPrompt = null;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void OnFlashcardAnswered(bool isCorrect)
	{
		float difficulty = GameDifficultyManager.Instance == null
			? 0f
			: GameDifficultyManager.Instance.getCurrentDifficultyScore();
		TaloTelemetry.TrackFlashcardAnswer(isCorrect, "parkour_event", difficulty);
		// Lock mouse
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().GetFirstNodeInGroup("player")?.Call("SetAcceptKeyboardInput", true);

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

		GetTree().GetFirstNodeInGroup("player")?.Call("SetAcceptKeyboardInput", false); // Disable player input when event starts

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

		EventManager.Instance.raise("on_room_clear", "test");
	}

	public void ApplyReward()
	{
		GD.Print("have fun i guess");
		Reward.Visible = true;
		Reward.SetCollision(true); // Enable chest collision so player can interact with it
		AudioManager.Instance.PlayGameVictorySound(); // temp until sound added
	}

	public void ApplyPenalty()
	{
		GD.Print("idk twenty lashings or something");
		_player.healthComponent.TakeDamage(PenaltyDamage);
	}
}
