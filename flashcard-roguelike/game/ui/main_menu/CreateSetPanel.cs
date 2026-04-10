using Godot;
using System.Collections.Generic;

public partial class CreateSetPanel : Control
{
	private LineEdit _setNameEdit;
	private VBoxContainer _cardRowsContainer;
	private Button _addCardButton;
	private Button _saveButton;
	private Button _cancelButton;
	private Label _errorLabel;

	public override void _Ready()
	{
		_setNameEdit = GetNodeOrNull<LineEdit>("Panel/MarginContainer/VBoxContainer/SetNameHBox/SetNameEdit");
		_cardRowsContainer = GetNodeOrNull<VBoxContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer/CardRowsContainer");
		_addCardButton = GetNodeOrNull<Button>("Panel/MarginContainer/VBoxContainer/ButtonRow/AddCardButton");
		_saveButton = GetNodeOrNull<Button>("Panel/MarginContainer/VBoxContainer/ButtonRow/SaveButton");
		_cancelButton = GetNodeOrNull<Button>("Panel/MarginContainer/VBoxContainer/ButtonRow/CancelButton");
		_errorLabel = GetNodeOrNull<Label>("Panel/MarginContainer/VBoxContainer/ErrorLabel");

		if (_addCardButton != null)
		{
			_addCardButton.Pressed += AddCardRow;
		}

		if (_saveButton != null) 
		{
			_saveButton.Pressed += OnSavePressed;
		}

		if (_cancelButton != null) 
		{
			_cancelButton.Pressed += OnCancelPressed;
		}

		AudioManager.Instance?.RegisterButton(_addCardButton);
		AudioManager.Instance?.RegisterButton(_saveButton);
		AudioManager.Instance?.RegisterButton(_cancelButton);

		AddCardRow();
		Visible = false;
	}

	private void AddCardRow()
	{
		var row = new HBoxContainer();
		row.AddThemeConstantOverride("separation", 8);

		var questionLabel = new Label();
		questionLabel.Text = "Q:";
		row.AddChild(questionLabel);

		var questionEdit = new LineEdit();
		questionEdit.PlaceholderText = "Question...";
		questionEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		questionEdit.CustomMinimumSize = new Vector2(0, 36);
		row.AddChild(questionEdit);

		var answerLabel = new Label();
		answerLabel.Text = "A:";
		row.AddChild(answerLabel);

		var answerEdit = new LineEdit();
		answerEdit.PlaceholderText = "Answer...";
		answerEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		answerEdit.CustomMinimumSize = new Vector2(0, 36);
		row.AddChild(answerEdit);

		var removeButton = new Button();
		removeButton.Text = "X";
		removeButton.CustomMinimumSize = new Vector2(36, 36);
		AudioManager.Instance?.RegisterButton(removeButton);
		removeButton.Pressed += () => RemoveCardRow(row);
		row.AddChild(removeButton);

		_cardRowsContainer.AddChild(row);
	}

	private void RemoveCardRow(HBoxContainer row)
	{
		_cardRowsContainer.RemoveChild(row);
		row.QueueFree();
		if (_cardRowsContainer.GetChildCount() == 0)
		{
			AddCardRow();
		}
	}

	private void OnSavePressed()
	{
		string setName = _setNameEdit?.Text?.Trim() ?? "";
		if (string.IsNullOrWhiteSpace(setName))
		{
			_errorLabel.Text = "Set name cannot be empty.";
			_errorLabel.Visible = true;
			return;
		}

		int index = 0;
		var cards = new List<Flashcard>();
		foreach (Node child in _cardRowsContainer.GetChildren())
		{
			if (child is not HBoxContainer row)
			{
				continue;
			}
			string q = row.GetChildOrNull<LineEdit>(1)?.Text?.Trim() ?? "";
			string a = row.GetChildOrNull<LineEdit>(3)?.Text?.Trim() ?? "";

			if (string.IsNullOrWhiteSpace(q) || string.IsNullOrWhiteSpace(a))
			{
				_errorLabel.Text = $"Card {index + 1} is incomplete, both fields need to be filled.";
				_errorLabel.Visible = true;
				return;
			}

			if (!string.IsNullOrWhiteSpace(q) && !string.IsNullOrWhiteSpace(a))
			{
				cards.Add(new Flashcard { Question = q, Answer = a });
			}

			++index;
		}

		if (cards.Count == 0)
		{
			GD.PushError("CreateSetPanel: no cards entered.");
			return;
		}

		FlashcardManager.Instance.CreateAndSaveSet(setName, cards);
		ClearPanel();
		GetParent<MainMenu>().OnCreateSetSaved();
		_errorLabel.Visible = false;
	}

	private void OnCancelPressed()
	{
		ClearPanel();
		GetParent<MainMenu>().OnCreateSetCancelled();
		_errorLabel.Visible = false;
	}

	private void ClearPanel()
	{
		if (_setNameEdit != null) 
		{
			_setNameEdit.Text = "";
		}

		foreach (Node child in _cardRowsContainer.GetChildren())
		{
			_cardRowsContainer.RemoveChild(child);
			child.QueueFree();
		}

		AddCardRow();
	}
}
