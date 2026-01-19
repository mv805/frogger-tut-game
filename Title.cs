using System;
using Godot;

public partial class Title : Control
{
    private Global _global;
    private Label _highScoreLabel;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _global = GetNode<Global>("/root/Global");
        _highScoreLabel = GetNode<Label>("Score"); // ← Adjust path to match your scene structure

        // Display the high score
        _highScoreLabel.Text = $"High Score: {_global.HighScore}";
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("confirm")) // Your custom confirm action
        {
            GetTree().ChangeSceneToFile("res://scenes/game.tscn");
        }
    }
}
