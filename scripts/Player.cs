using Godot;

/// <summary>
/// Core player controller that manages state, movement, and physics.
/// Coordinates between MovementPlayer (input) and AnimationPlayer (visuals).
/// </summary>
public partial class Player : CharacterBody2D
{
    /// <summary>
    /// Represents the player's current movement state.
    /// </summary>
    public enum MoveState
    {
        Idle,
        Walk,
        Dead,
    }

    /// <summary>
    /// Direction the player is facing. Used by AnimationPlayer for directional sprites.
    /// </summary>
    public Vector2 Facing { get; private set; } = Vector2.Down;

    /// <summary>
    /// Current desired movement velocity. Set by MovementPlayer, applied in _PhysicsProcess.
    /// </summary>
    public Vector2 MoveVelocity { get; private set; }

    /// <summary>
    /// Current movement state (Idle or Walk). Used by AnimationPlayer for animation selection.
    /// </summary>
    public MoveState CurrentState { get; private set; } = MoveState.Idle;

    private MovementPlayer _movement;
    private Area2D _hitbox;
    private Game _gameManager;

    public override void _Ready()
    {
        _movement = GetNode<MovementPlayer>("MovementPlayer");
        _hitbox = GetNode<Area2D>("HitBox");
        _hitbox.AreaEntered += OnAreaEntered;
        _gameManager = GetNode<Game>("/root/Game");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Don't process movement if dead
        if (CurrentState == MoveState.Dead)
            return;

        // Movement component reads input and determines intent
        _movement.Tick();

        // Apply movement intent to physics body
        Velocity = MoveVelocity;
        MoveAndSlide();
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Car car && CurrentState != MoveState.Dead)
        {
            Die();
            _gameManager.OnPlayerHitByCar(car); //tell manager player died
        }
    }

    /// <summary>
    /// Handles player death - sets state and disables movement
    /// Visual effects are handled by AnimationPlayer
    /// </summary>
    private void Die()
    {
        CurrentState = MoveState.Dead;

        // Disable movement
        _movement.ProcessMode = ProcessModeEnum.Disabled;
    }

    /// <summary>
    /// Single write gate for movement intent. Called by MovementPlayer to update player state.
    /// This pattern ensures all movement-related state changes go through one method.
    /// </summary>
    /// <param name="desiredVelocity">Desired movement velocity in pixels per second.</param>
    /// <param name="facing">Direction the player should face. Zero vector is ignored to preserve current facing.</param>
    /// <param name="state">Current movement state (Idle or Walk).</param>
    public void SetMoveIntent(Vector2 desiredVelocity, Vector2 facing, MoveState state)
    {
        MoveVelocity = desiredVelocity;

        if (facing != Vector2.Zero)
        {
            Facing = facing;
        }

        CurrentState = state;
    }
}
