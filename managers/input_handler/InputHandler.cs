using System;
using Godot;

public partial class InputHandler : Node, IManager
{
    public event Action<Vector2I/*移动方向*/> MovementInputHandled;
    public event Action PickUpInputHandled;
    public event Action ToggleInventoryWindowInputHandled;
    public event Action UseInventoryObjectInputHandled;
    public event Action PutAwayInventoryObjectInputHandled;
    public event Action GoUpStairInputHandled;
    public event Action GoDownStairInputHandled;
    public event Action RestartGameInputHandled;

    private MapData _mapData;

    private CombatManager _combatManager;

    private Player _player;

    private InventoryWindow _inventoryWindow;
    private VictoryWindow _victoryWindow;
    private DefeatWindow _defeatWindow;

    private Timer _interruptMovementTimer;

    private float _maxDurationOfInterruptMovement = 0.5f;
    private float _minDurationOfInterruptMovement = 0.05f;
    private float _currentDurationOfInterruptMovement;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        _inventoryWindow = GetTree().CurrentScene.GetNode<InventoryWindow>("%InventoryWindow");
        _victoryWindow = GetTree().CurrentScene.GetNode<VictoryWindow>("%VictoryWindow");
        _defeatWindow = GetTree().CurrentScene.GetNode<DefeatWindow>("%DefeatWindow");

        _interruptMovementTimer = GetNode<Timer>("InterruptMovementTimer");
    }

    public void Update()
    {
        if (HandleRestartGameInput()) { return; }

        if (_victoryWindow.Visible || _defeatWindow.Visible) { return; }

        if (_player.IsDead) { return; }

        if (HandleToggleInventoryWindowInput()) { return; }

        if (HandleUseInventoryObjectInput()) { return; }

        if (HandlePutAwayInventoryObjectInput()) { return; }

        if (_inventoryWindow.Visible) { return; }

        if (HandleGoUpStairInput()) { return; }

        if (HandleGoDownStairInput()) { return; }

        if (HandlePickUpInput()) { return; }

        HandleMovementInput();
    }

    private bool HandleRestartGameInput()
    {
        if (!_victoryWindow.Visible && !_defeatWindow.Visible) { return false; }

        if (Input.IsActionJustPressed("restart_game"))
        {
            RestartGameInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleGoDownStairInput()
    {
        if (Input.IsActionJustPressed("go_down_stair"))
        {
            GoDownStairInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleGoUpStairInput()
    {
        if (Input.IsActionJustPressed("go_up_stair"))
        {
            GoUpStairInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleUseInventoryObjectInput()
    {
        if (Input.IsActionJustPressed("use_inventory_object"))
        {
            UseInventoryObjectInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandlePutAwayInventoryObjectInput()
    {
        if (Input.IsActionJustPressed("put_away_inventory_object"))
        {
            PutAwayInventoryObjectInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleToggleInventoryWindowInput()
    {
        if (Input.IsActionJustPressed("toggle_inventory"))
        {
            ToggleInventoryWindowInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandlePickUpInput()
    {
        if (Input.IsActionJustPressed("pick_up"))
        {
            PickUpInputHandled.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleMovementInput()
    {
        var direction = GetMovementDirection();

        if (direction == Vector2I.Zero)
        {
            _interruptMovementTimer.Stop();

            _currentDurationOfInterruptMovement = _maxDurationOfInterruptMovement;

            return false;
        }

        if (!_interruptMovementTimer.IsStopped()) { return false; }

        TryHandleMelee(direction);
        MovementInputHandled.Invoke(direction);

        _interruptMovementTimer.Start(_currentDurationOfInterruptMovement);

        _currentDurationOfInterruptMovement = _minDurationOfInterruptMovement;

        return true;
    }

    private Vector2I GetMovementDirection()
    {
        var movementHorizontal =
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        var movementVertical =
            Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");

        var direction = new Vector2(movementHorizontal, movementVertical);

        return (Vector2I)direction.Sign();
    }

    private void TryHandleMelee(Vector2I direction)
    {
        var targetPosition = _player.GlobalPosition + direction * _mapData.CellSize;

        var space = _player.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = targetPosition,
            CollideWithAreas = true,
            CollisionMask = (int)PhysicsLayer.BlockMovement,
            CollideWithBodies = false
        };
        var results = space.IntersectPoint(parameters);

        if (results.Count == 0) { return; }

        foreach (var result in results)
        {
            var collider = result["collider"].As<Area2D>();
            if (collider.Owner is Enemy)
            {
                var enemy = collider.Owner as Enemy;

                _combatManager.AddToCombatList(_player, enemy);
                GD.Print("玩家攻击" + enemy.CharacterData.Name + "！");
            }
        }
    }
}
