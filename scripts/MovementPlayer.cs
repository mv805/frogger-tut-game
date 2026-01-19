using Godot;

/// <summary>
/// Handles player input and translates it into movement intent.
/// Does not directly modify player position - only sets velocity and state.
/// </summary>
public partial class MovementPlayer : Node
{
    /// <summary>
    /// Movement speed in pixels per second.
    /// </summary>
    [Export]
    public int Speed { get; set; } = 50;

    private Player _player;

    public override void _Ready()
    {
        _player = GetParent<Player>();
    }

    /// <summary>
    /// Called each frame by Player to process input and update movement intent.
    /// Reads input and sends movement commands to Player.
    /// </summary>
    public void Tick()
    {
        // Read input from action map (configured in Project Settings > Input Map)
        Vector2 input = Input.GetVector("left", "right", "up", "down");

        // Calculate desired velocity based on input and speed
        Vector2 desiredVelocity = input * Speed;

        // Determine state: idle if no input, walking if moving
        Player.MoveState state =
            input == Vector2.Zero ? Player.MoveState.Idle : Player.MoveState.Walk;

        // Determine facing: maintain current facing when idle, otherwise face input direction
        Vector2 facing = input == Vector2.Zero ? _player.Facing : input.Normalized();

        // Send movement intent to Player (single write gate pattern)
        _player.SetMoveIntent(desiredVelocity, facing, state);
    }
}
