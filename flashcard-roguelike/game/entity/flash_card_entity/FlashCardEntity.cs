using Godot;
using System;

public partial class FlashCardEntity : Node3D
{
	[Export]
	public Label3D question {get; set;}
	[Export]
	public Label3D answer {get; set;}
	 
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		return;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		return;
	}
	
	public void setLabels(String incoming_question, String incoming_answer)
	{
		question.Text = incoming_question;
		answer.Text = incoming_answer;
	}
}
