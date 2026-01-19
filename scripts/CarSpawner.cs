using Godot;

/// <summary>
/// Handles spawning cars at random positions with random colors and directions.
/// Manages spawn timing and difficulty scaling.
/// </summary>
public partial class CarSpawner : Node
{
    // Signal: "I spawned a car" (passes the car reference up)
    [Signal]
    public delegate void CarSpawnedEventHandler(Car car);

    [Export]
    public float SpawnInterval = 1.0f; //seconds

    [Export]
    public int CarSpeed = 160;

    private const float SPAWN_THRESHOLD_X = -200f;

    private PackedScene _carScene = GD.Load<PackedScene>("res://scenes/car.tscn");

    private Texture2D[] _carColors =
    [
        GD.Load<Texture2D>("res://graphics/cars/green.png"),
        GD.Load<Texture2D>("res://graphics/cars/red.png"),
        GD.Load<Texture2D>("res://graphics/cars/yellow.png"),
    ];

    private Timer _spawnTimer;
    private Node2D _renderNode;
    private Node2D _spawnsNode;
    private Game _gameManager;

    public override void _Ready()
    {
        // Get references to scene nodes
        _renderNode = GetNode<Node2D>("/root/Game/Render (Ysort)");
        _spawnsNode = GetNode<Node2D>("/root/Game/Spawns");
        _gameManager = GetNode<Game>("/root/Game");

        // Create and configure spawn timer
        // Creates cars based on the spawn interval
        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = SpawnInterval;
        _spawnTimer.Autostart = true;
        _spawnTimer.Timeout += SpawnCar;
        AddChild(_spawnTimer);
    }

    private void SpawnCar()
    {
        // Instantiate car from scene template
        var car = _carScene.Instantiate<Car>();

        // Pick random spawn point
        var spawnMarkers = _spawnsNode.GetChildren();
        Marker2D randomMarker = spawnMarkers[GD.RandRange(0, spawnMarkers.Count - 1)] as Marker2D;
        car.Position = randomMarker.Position;

        // Determine direction based on spawn position
        // Left side of screen (X < -200) = move right, right side = move left
        Vector2 carDirection =
            randomMarker.Position.X < SPAWN_THRESHOLD_X ? Vector2.Right : Vector2.Left;

        // Pick random color from available textures
        Texture2D randomColor = _carColors[GD.RandRange(0, _carColors.Length - 1)];

        // Add to scene FIRST - this triggers _Ready() and initializes _sprite
        _renderNode.AddChild(car);

        // NOW configure - _sprite is available
        car.Configure(CarSpeed, carDirection, randomColor);

        // DATA UP: tell whoever cares that a car was created
        EmitSignal(SignalName.CarSpawned, car);
    }

    /// <summary>
    /// Adjusts the spawn rate for difficulty scaling.
    /// </summary>
    /// <param name="interval">Time in seconds between spawns. Lower values increase difficulty.</param>
    public void SetSpawnRate(float interval)
    {
        SpawnInterval = interval;
        _spawnTimer.WaitTime = interval;
    }
}
