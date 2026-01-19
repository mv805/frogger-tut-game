using Godot;

/// <summary>
/// Simple car obstacle that moves in a straight line and despawns when entering a kill zone.
/// Color, speed, and direction are configured by CarSpawner at spawn time.
/// </summary>
public partial class Car : Area2D
{
    /// <summary>
    /// Emitted when the car is about to be removed from the scene.
    /// </summary>
    [Signal]
    public delegate void DespawnedEventHandler(Car car);

    private Vector2 _direction;
    private int _speed;
    private Sprite2D _sprite;

    /// <summary>
    /// Called when the car enters the scene tree.
    /// Initializes sprite reference and connects to area detection.
    /// </summary>
    public override void _Ready()
    {
        _sprite = GetNode<Sprite2D>("Sprite2D");

        // Connect to THIS car's AreaEntered signal
        // Fires when THIS car enters any other Area2D
        AreaEntered += OnAreaEntered;
    }

    /// <summary>
    /// Updates car position every physics frame based on speed and direction.
    /// </summary>
    /// <param name="delta">Time elapsed since the previous frame in seconds.</param>
    public override void _PhysicsProcess(double delta)
    {
        Position += _direction * _speed * (float)delta;
    }

    /// <summary>
    /// Configures all car properties in one call. Called by CarSpawner after AddChild().
    /// </summary>
    /// <param name="speed">Movement speed in pixels per second.</param>
    /// <param name="direction">Movement direction vector (will be normalized automatically).</param>
    /// <param name="carColor">Texture2D for the car sprite (green, red, or yellow variant).</param>
    public void Configure(int speed, Vector2 direction, Texture2D carColor)
    {
        _speed = speed;
        _direction = direction.Normalized();
        _sprite.Texture = carColor;
    }

    /// <summary>
    /// Called automatically when this car enters any Area2D.
    /// Checks if the area is a KillZone and despawns if so.
    /// </summary>
    /// <param name="area">The Area2D that this car just entered.</param>
    private void OnAreaEntered(Area2D area)
    {
        // Check if the area we entered is a KillZone
        if (area.IsInGroup("KillZones"))
        {
            Despawn();
        }
    }

    /// <summary>
    /// Removes the car from the scene and emits the Despawned signal.
    /// </summary>
    private void Despawn()
    {
        EmitSignal(SignalName.Despawned, this);
        QueueFree();
    }
}
