using Godot;
using System;

public partial class EnemyStatusComponent : Node3D
{
	[Export] public float SlideDuration = 1.5f;
	
    // These positions are relative to the EnemyStatusComponent's position, 
    // which should be set to the enemy's head or desired label anchor point in the scene
    // You can adjust these offsets in the editor to position the labels correctly above the enemy

	private Label3D _nameLabel;
	private Label3D _statsLabel;
	private Label3D _healthLabel;
	
	private EnemyFSM _enemy;

	private Vector3 _nameOnScreenPos;
	private Vector3 _statsOnScreenPos;
	private Vector3 _healthOnScreenPos;
	

	private HealthComponent _health;
	private AttackComponent _attack;
	private bool _isVisible;

	public override void _Ready()
	{
		// Get references to the labels
		_nameLabel = GetNode<Label3D>("NameLabel");
		_statsLabel = GetNode<Label3D>("StatsLabel");
		_healthLabel = GetNode<Label3D>("HealthLabel");

		_nameOnScreenPos = _nameLabel.Position;
		_statsOnScreenPos = _statsLabel.Position;
		_healthOnScreenPos = _healthLabel.Position;

		Visible = false;
		_isVisible = false;
	}

	public void Initialize(EnemyFSM enemy, HealthComponent health, AttackComponent attack)
	{
		// Store references to the enemy and its components for updating the display later
		_enemy = enemy;
		_health = health;
		_attack = attack;

		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		// Check if we have the necessary references before trying to update the display
		if (_nameLabel == null || _healthLabel == null || _statsLabel == null) return;

		if (_enemy != null)
		{
			_nameLabel.Text = _enemy.Name;
		}

		if (_attack != null)
		{
			_statsLabel.Text = $"ATK: {Mathf.Ceil(_attack.BaseDamage)}";
		}

		if (_health != null)
		{
			SetHealth(_health.CurrentHealth, _health.MaxHealth);
		}
	}

	public void SetHealth(float current, float max)
	{   
		// Check if we have the health label reference before trying to update it
		if (_healthLabel == null) return;

		// Update the health label text to show current and max health, rounding up for display purposes
		_healthLabel.Text = $"HP: {Mathf.Ceil(current)}/{max}";
	}

	public void SlideIn()
	{
		Visible = true;
		_isVisible = true;

		// Calculate offset based on label size + small margin, not full screen width
		float offset = 150f; // Reasonable off-screen distance for label width
		Random rand = new Random();

		// Each label starts off-screen to the left or right randomly
		_nameLabel.Position = _nameLabel.Position with { X = rand.Next(0, 2) == 0 ? -offset : offset };
		_statsLabel.Position = _statsLabel.Position with { X = rand.Next(0, 2) == 0 ? -offset : offset };
		_healthLabel.Position = _healthLabel.Position with { X = rand.Next(0, 2) == 0 ? -offset : offset };

		Tween tween = CreateTween();
		
        // Animate each label sliding in to its on-screen position with a slight delay between them for a staggered effect
        tween.SetParallel(true);
        tween.TweenProperty(_nameLabel, "position", _nameOnScreenPos, SlideDuration)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_statsLabel, "position", _statsOnScreenPos, SlideDuration)
             .SetDelay(0.05f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_healthLabel, "position", _healthOnScreenPos, SlideDuration)
             .SetDelay(0.1f)
             .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
    }

	public void SlideOut(Action onComplete = null)
	{
		if (!_isVisible) { onComplete?.Invoke(); return; } // If already hidden, just call onComplete and return
		_isVisible = false;

		// Use same reasonable offset as SlideIn
		float offset = 150f;
		Random rand = new Random();

		// Each label exits to the left or right randomly
		Tween tween = CreateTween();

		// Animate each label sliding out to the left or right randomly with a slight delay between them for a staggered effect
		tween.SetParallel(true);
		tween.TweenProperty(_nameLabel, "position:x", rand.Next(0, 2) == 0 ? -offset : offset, SlideDuration)
			 .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		tween.TweenProperty(_statsLabel, "position:x", rand.Next(0, 2) == 0 ? -offset : offset, SlideDuration)
			 .SetDelay(0.05f)
			 .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		tween.TweenProperty(_healthLabel, "position:x", rand.Next(0, 2) == 0 ? -offset : offset, SlideDuration)
			 .SetDelay(0.1f)
			 .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

		// Chain a callback to hide the UI and call onComplete after all labels have finished sliding out
		tween.Chain().TweenCallback(Callable.From(() =>
		{
			Visible = false;
			onComplete?.Invoke();
		}));
	}
}
