using Godot;

/// <summary>
/// Main game coordinator that manages game state, scoring, and win/lose conditions.
/// Orchestrates interactions between player, spawners, and UI elements.
/// </summary>
public partial class Game : Node2D
{
    // internal references to key game systems
    private CarSpawner _carSpawner;
    private Area2D _finishArea;
    private Timer _scoreCountdownTimer;
    private Label _scoreLabel;
    private Label _carCountLabel;
    private int _score = 0;
    private int _carsOnScreen = 0;
    private Global _global;

    // Death sequence UI - get from editor scene
    private CanvasLayer _deathUI;
    private Label _youDiedLabel;
    private Label _pressSpaceLabel;
    private Timer _deathTimer;
    private bool _isPlayerDead = false;

    public override void _Ready()
    {
        // Get references - these paths are fixed in your game structure
        _carSpawner = GetNode<CarSpawner>("Systems/CarSpawner");
        _finishArea = GetNode<Area2D>("Triggers/FinishArea");
        _scoreCountdownTimer = GetNode<Timer>("ScoreCountdownTimer");
        _scoreLabel = GetNode<Label>("CanvasLayer/Score");
        _carCountLabel = GetNode<Label>("CanvasLayer/CarCount");
        _global = GetNode<Global>("/root/Global");

        // Get death UI references from editor scene
        _deathUI = GetNode<CanvasLayer>("DeathScene");
        _youDiedLabel = GetNode<Label>("DeathScene/DeathNotice");
        _pressSpaceLabel = GetNode<Label>("DeathScene/PromptContinue");

        // Create and setup death timer
        _deathTimer = new Timer();
        _deathTimer.WaitTime = 3.0f;
        _deathTimer.OneShot = true;
        _deathTimer.Timeout += OnDeathTimerTimeout;
        AddChild(_deathTimer);

        // Hide death UI initially
        _deathUI.Visible = false;
        _pressSpaceLabel.Visible = false;

        // Connect signals
        _finishArea.BodyEntered += OnFinishAreaEntered;
        _scoreCountdownTimer.Timeout += OnScoreCountdownTimerTimeout;
        _carSpawner.CarSpawned += OnCarSpawned;

        _global.ResetCurrentScore();
        UpdateScoreDisplay();
    }

    public override void _Process(double delta)
    {
        // Check for space input to go to title screen after death
        if (_isPlayerDead && _pressSpaceLabel.Visible && Input.IsActionJustPressed("confirm"))
        {
            GoToTitleScreen();
        }
    }

    /// <summary>
    /// Called when death timer finishes - shows "Press SPACE" message
    /// </summary>
    private void OnDeathTimerTimeout()
    {
        _pressSpaceLabel.Visible = true;
    }

    /// <summary>
    /// Goes to the title screen
    /// </summary>
    private void GoToTitleScreen()
    {
        GetTree().ChangeSceneToFile("res://scenes/title.tscn");
    }

    /// <summary>
    /// Called when player reaches the finish area.
    /// </summary>
    private void OnFinishAreaEntered(Node2D body)
    {
        GD.Print($"Player entered finish area!");

        if (body is Player)
        {
            _global.UpdateHighScore(_score);
            CallDeferred(MethodName.ChangeToTitleScene);
        }
    }

    /// <summary>
    /// Changes to the title scene. Called deferred to avoid physics callback issues.
    /// </summary>
    private void ChangeToTitleScene()
    {
        GetTree().ChangeSceneToFile("res://scenes/title.tscn");
    }

    /// <summary>
    /// Centralized handler for car collisions.
    /// Called by Player when it collides with a car.
    /// </summary>
    public void OnPlayerHitByCar(Node2D body)
    {
        GD.Print("Player hit by car!");
        _isPlayerDead = true;
        _global.UpdateHighScore(_score);

        // Stop score timer but keep cars spawning
        _scoreCountdownTimer.Stop();

        // Show death UI
        _deathUI.Visible = true;
        _youDiedLabel.Visible = true;
        _pressSpaceLabel.Visible = false; // Hide initially

        // Start 3-second timer
        _deathTimer.Start();
    }

    /// <summary>
    /// Called when CarSpawner creates a new car.
    /// Tracks active cars and subscribes to their despawn events.
    /// </summary>
    private void OnCarSpawned(Car car)
    {
        _carsOnScreen++;
        car.Despawned += OnCarDespawned;
        _carCountLabel.Text = $"Car Count: {_carsOnScreen}";
    }

    /// <summary>
    /// Called when a car despawns (entered kill zone).
    /// </summary>
    private void OnCarDespawned(Car car)
    {
        _carsOnScreen--;
        _carCountLabel.Text = $"Car Count: {_carsOnScreen}";
    }

    /// <summary>
    /// Called every time the score timer ticks.
    /// Increments score to reward player for staying alive.
    /// </summary>
    private void OnScoreCountdownTimerTimeout()
    {
        _score++;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Updates the score label text with current score.
    /// </summary>
    private void UpdateScoreDisplay()
    {
        _scoreLabel.Text = $"Score: {_score}";
    }

    /// <summary>
    /// Increases game difficulty by accelerating car spawn rate.
    /// Can be called when player reaches certain score thresholds.
    /// </summary>
    private void IncreaseDifficulty()
    {
        _carSpawner.SetSpawnRate(0.5f);
    }
}
