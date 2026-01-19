using System;
using Godot;

public partial class Global : Node
{
    public int HighScore { get; set; } = 0;
    public int CurrentScore { get; set; } = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void UpdateHighScore(int score)
    {
        CurrentScore = score;
        if (score > HighScore)
        {
            HighScore = score;
        }
    }

    public void ResetCurrentScore()
    {
        CurrentScore = 0;
    }
}
