using Godot;
using System;
using System.Collections.Generic;

public partial class ShootingEventRoom : Room, IEventRoom
{
    [Export] public BaseNPC TriggerNPC;
    [Export] public Turret RoomTurret;

    public bool IsCompleted { get; private set; }
    public float Difficulty { get; private set; }

    private Camera3D _playerCamera;
    private List<QAPanel> _panelsToAssign = new List<QAPanel>();
    private QAPanel[] _currentPair = new QAPanel[2]; // To track the current pair being evaluated
    private int _pairs;
    private int _matches;
    private Node3D _panelsNode;

    public override void _Ready()
    {
        base._Ready();

        // Unused for now, maybe can influence ammo
        Difficulty = GameDifficultyManager.Instance.getCurrentDifficultyScore();

        TriggerNPC.OnInteraction += TriggerEvent;
        RoomTurret.OutOfAmmo += OnNoAmmo;

        _panelsNode = GetNode<Node3D>("Panels");
        foreach (Node3D panel in _panelsNode.GetChildren())
        {
            if (panel is QAPanel qaPanel)
            {
                _panelsToAssign.Add(qaPanel);
                qaPanel.OnPanelHit += OnPanelHit;
            }
        }

        if (_panelsToAssign.Count % 2 != 0)
        {
            GD.PrintErr("Error: Odd number of QAPanels in ShootingEventRoom. Ensure panels are even.");
        }
        else
        {
            _pairs = _panelsToAssign.Count / 2;
            RoomTurret.SetAmmo(_panelsToAssign.Count); // Set turret ammo based on the number of panels
        }

        GD.Print($"ShootingEventRoom ready with difficulty {Difficulty} and {_pairs} Q&A pairs.");
    }

    public void TriggerEvent()
    {
        // Deactivate player camera and activate turret camera
        _playerCamera = _player.GetNode<Camera3D>("CameraPivot/Camera3D");
        _player.Visible = false;
        RoomTurret.ActivateTurret(_playerCamera);
        _panelsNode.Visible = true; // Show panels when event starts

        // Get all flashcards from active sets for panel pair assignment
        List<FlashcardSet> sets = FlashcardManager.Instance.ActiveFlashCardLists;
        List<Flashcard> flashcards = new();
        foreach (FlashcardSet set in sets)
        {
            flashcards.AddRange(set.Cards);
        }

        Random rng = new Random();
        for (int i = 0; i < _pairs; i++)
        {
            if (flashcards.Count == 0)
            {
                GD.PrintErr("Error: No flashcards available for QAPanel assignment.");
                break;
            }

            // Randomly select a flashcard for the pair
            int cardIndex = rng.Next(flashcards.Count);
            Flashcard card = flashcards[cardIndex];
            flashcards.RemoveAt(cardIndex); // Remove to prevent reuse

            int panelQIndex = rng.Next(_panelsToAssign.Count);
            QAPanel qPanel = _panelsToAssign[panelQIndex];
            _panelsToAssign.RemoveAt(panelQIndex); // Remove to prevent reuse
            qPanel.SetPanelText(card.Question);
            qPanel.AssignFlashcard(card);

            int panelAIndex = rng.Next(_panelsToAssign.Count);
            QAPanel aPanel = _panelsToAssign[panelAIndex];
            _panelsToAssign.RemoveAt(panelAIndex); // Remove to prevent reuse
            aPanel.SetPanelText(card.Answer);
            aPanel.AssignFlashcard(card);

            GD.Print($"Assigned Q&A pair: '{card.Question}' -> '{card.Answer}' to panels '{qPanel.Name}' and '{aPanel.Name}'");
        }
    }

    public void CompleteEvent(bool success)
    {
        if (IsCompleted) return; // Prevent multiple completions
        IsCompleted = true;

        if (success)
        {
            ApplyReward();
        }
        else
        {
            ApplyPenalty();
        }    

        _player.Visible = true;
        RoomTurret.DeactivateTurret(_playerCamera); // Deactivate turret
        EventManager.Instance.raise("on_room_clear", "test"); // Tell connections to open
    }

    public void ApplyReward()
    {
        GD.Print("Reward have a treat :)");
    }

    public void ApplyPenalty()
    {
        GD.Print("PENALTY 1 TRILLION LASHINGS");
    }

    public void OnPanelHit(QAPanel panel)
    { 
        // Add hit panel to current pair tracking
        if (_currentPair[0] == null) // First panel of the pair is empty
        {
            _currentPair[0] = panel;
            panel.GlowSelected(); // Highlight first selected panel
        }
        else // Second panel of the pair is empty
        {
            _currentPair[1] = panel;
            CompareAndReset(); // We have a pair, compare and reset for next pair
        }
    }

    public void CompareAndReset()
    {
        if (_currentPair[1] == null)
        {
            GD.PrintErr("Left over selection, player presumably missed and we are out of ammo.");
            _currentPair[0] = null;
            return;
        }

        // Get panels to compare
        QAPanel panel1 = _currentPair[0];
        QAPanel panel2 = _currentPair[1];

        // Only one question and answer panel exist per flashcard, so if the flashcards match, it's a correct pair
        if (panel1.GetCard() == panel2.GetCard()) 
        {
            GD.Print($"Correct match: '{panel1.GetPanelText()}' <=> '{panel2.GetPanelText()}'");
            ++_matches;
            panel1.ShowResult(true);
            panel2.ShowResult(true);
        }
        else
        {
            GD.Print($"Incorrect match: '{panel1.GetPanelText()}' -> '{panel2.GetPanelText()}'");
            panel1.ShowResult(false);
            panel2.ShowResult(false);
        }

        // Reset current pair tracking for next selection
        _currentPair[0] = null;
        _currentPair[1] = null;
    }

    public async void OnNoAmmo()
    {
        // Wait for the last bullet to land (or expire) before evaluating
        await ToSignal(GetTree().CreateTimer(5.0f), SceneTreeTimer.SignalName.Timeout);

        // Check for any leftover selection that wasn't compared due to ammo running out
        if (_currentPair[0] != null)
        {
            CompareAndReset();
        }

        // Determine success based on matches vs pairs
        if (_matches >= _pairs / 2) // Arbitrary success threshold: at least half correct
        {
            GD.Print($"Event completed with {_matches} out of {_pairs} pairs correct.");
            CompleteEvent(true);
        }
        else
        {
            GD.Print($"Event completed with only {_matches} out of {_pairs} pairs correct.");
            CompleteEvent(false);
        }
    }
}
