using Godot;

/// <summary>
/// Manages player animation based on movement state and facing direction.
/// Implements an idle delay to prevent animation flickering during quick inputs.
/// Also handles death visual effects including sprite rotation and blood particles.
/// </summary>
/// <remarks>
/// This component should be attached as a child of the Player node, or you can specify
/// a custom path using the <see cref="PlayerPath"/> export. The AnimatedSprite2D must
/// have animations named "up", "down", "left", and "right".
/// </remarks>
public partial class AnimationPlayer : Node2D
{
    /// <summary>
    /// Path to the Player node. If empty, uses the parent node.
    /// </summary>
    [Export]
    public NodePath PlayerPath;

    /// <summary>
    /// Path to the AnimatedSprite2D node. If empty, looks for "AnimatedSprite2D" as a child.
    /// </summary>
    [Export]
    public NodePath SpritePath;

    private Player _player;
    private AnimatedSprite2D _sprite;
    private GpuParticles2D _bloodParticles;

    private Player.MoveState _lastState = Player.MoveState.Idle;
    private Vector2 _lastFacing = Vector2.Down;

    private double _timeSinceLastMove = 0.0;

    /// <summary>
    /// Delay in seconds before showing idle animation. Prevents flickering during quick key taps.
    /// </summary>
    private const double IDLE_DELAY = 0.2;

    /// <summary>
    /// Frame index for the idle pose (legs together, standing still).
    /// </summary>
    private const int IDLE_FRAME = 1;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Initializes references to Player and AnimatedSprite2D nodes.
    /// </summary>
    public override void _Ready()
    {
        // Get reference to Player node (prefer parent if no path specified)
        _player = string.IsNullOrEmpty(PlayerPath)
            ? GetParent<Player>()
            : GetNode<Player>(PlayerPath);

        // Get reference to AnimatedSprite2D child node
        _sprite = string.IsNullOrEmpty(SpritePath)
            ? GetNode<AnimatedSprite2D>("AnimatedSprite2D")
            : GetNode<AnimatedSprite2D>(SpritePath);

        // Get blood particles - should be sibling node under Player
        _bloodParticles = _player.GetNode<GpuParticles2D>("BloodParticles");
        _bloodParticles.Emitting = false;
    }

    /// <summary>
    /// Called every frame. Handles animation updates based on player state.
    /// </summary>
    /// <param name="delta">Time elapsed since the previous frame in seconds.</param>
    public override void _Process(double delta)
    {
        UpdateAnimation(delta);
    }

    /// <summary>
    /// Updates the animation based on the player's current movement state and facing direction.
    /// Implements idle delay logic to prevent animation flickering.
    /// </summary>
    /// <param name="delta">Time elapsed since the previous frame in seconds.</param>
    private void UpdateAnimation(double delta)
    {
        Player.MoveState currentState = _player.CurrentState;
        Vector2 currentFacing = _player.Facing;

        // Check if state or facing changed
        bool stateChanged = currentState != _lastState;
        bool facingChanged = currentFacing != _lastFacing;

        // Handle death state
        if (currentState == Player.MoveState.Dead && stateChanged) //state change check to not continuously run death visuals
        {
            HandleDeathVisuals();
            _lastState = currentState;
            return;
        }

        if (currentState == Player.MoveState.Walk)
        {
            _timeSinceLastMove = 0.0; // Reset idle timer

            // Only update animation if state or facing changed (performance optimization)
            if (stateChanged || facingChanged)
            {
                string direction = GetDirectionString(currentFacing);
                _sprite.Play(direction);

                _lastState = currentState;
                _lastFacing = currentFacing;
            }
        }
        else if (currentState == Player.MoveState.Idle) // Idle state
        {
            _timeSinceLastMove += delta; // Accumulate idle time

            // Only show idle animation after delay has elapsed
            // This prevents flicker when rapidly tapping movement keys
            if (_timeSinceLastMove >= IDLE_DELAY && _lastState != Player.MoveState.Idle)
            {
                _sprite.Stop();
                _sprite.Frame = IDLE_FRAME;

                _lastState = Player.MoveState.Idle;
                // Note: Intentionally don't update _lastFacing so player "remembers" last direction
            }
        }
    }

    /// <summary>
    /// Handles visual effects for player death: rotates sprite and triggers blood particles
    /// </summary>
    private void HandleDeathVisuals()
    {
        // Stop animation and set to a laying down frame
        _sprite.Stop();
        _sprite.Frame = 0; // Adjust frame number if needed

        // Rotate sprite to lay horizontal (90 degrees)
        _sprite.Rotation = Mathf.DegToRad(90);

        // Emit blood particle effect
        _bloodParticles.Emitting = true;
        _bloodParticles.Restart();
    }

    /// <summary>
    /// Converts a facing vector to an animation direction string.
    /// Horizontal directions (left/right) take priority over vertical directions (up/down).
    /// </summary>
    /// <param name="facing">Direction vector. X > 0 is right, X &lt; 0 is left, Y &lt; 0 is up, Y &gt; 0 is down.</param>
    /// <returns>"right", "left", "up", or "down". Defaults to "down" if vector is zero.</returns>
    private string GetDirectionString(Vector2 facing)
    {
        if (facing.X > 0)
            return "right";
        if (facing.X < 0)
            return "left";
        if (facing.Y < 0)
            return "up";

        return "down"; // Default or Y > 0
    }
}
