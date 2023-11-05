using System;
using Godot;

public partial class InputHandler : Node, IManager
{
    public event Action IdleInputHandled;
    public event Action<Vector2I> MovementInputHandled;
    public event Action PickUpInputHandled;
    public event Action PutAwayInventoryObjectInputHandled;
    public event Action ToggleInventoryWindowInputHandled;
    public event Action UseInventoryObjectInputHandled;
    public event Action GoDownStairInputHandled;
    public event Action GoUpStairInputHandled;
    public event Action RestartGameInputHandled;

    private MapData _mapData;

    private CombatManager _combatManager;

    private InventoryWindow _inventoryWindow;
    private VictoryWindow _victoryWindow;
    private DefeatWindow _defeatWindow;

    private Player _player;

    private Timer _interruptMovementTimer;

    private float _maxDurationOfInterruptMovement = 0.5f;
    private float _minDurationOfInterruptMovement = 0.05f;
    private float _currentDurationOfInterruptMovement;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");

        _inventoryWindow = GetTree().CurrentScene.GetNode<InventoryWindow>("%InventoryWindow");
        _victoryWindow = GetTree().CurrentScene.GetNode<VictoryWindow>("%VictoryWindow");
        _defeatWindow = GetTree().CurrentScene.GetNode<DefeatWindow>("%DefeatWindow");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        _interruptMovementTimer = GetNode<Timer>("InterruptMovementTimer");
    }

    public void Update(double delta)
    {
        if (HandleRestarGameInput()) { return; }

        if (_victoryWindow.Visible || _defeatWindow.Visible) { return; }

        if (_player.IsDead) { return; }

        if (HandleToggleInventroyWindowInput()) { return; }

        if (HandleUseInventoryObjectInput()) { return; }

        if (HandlePutAwayInventoryInput()) { return; }

        if (_inventoryWindow.Visible) { return; }

        if (HandleGoDownStairInput()) { return; }

        if (HandleGoUpStairInput()) { return; }

        if (HandlePickUpInput()) { return; }

        if (HandleIdleInput()) { return; }

        if (HandleMovementInput()) { return; }
    }

    private bool HandleRestarGameInput()
    {
        if (!_victoryWindow.Visible && !_defeatWindow.Visible) { return false; }

        if (Input.IsActionJustPressed("restart_game"))
        {
            RestartGameInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleGoDownStairInput()
    {
        if (Input.IsActionJustPressed("go_down_stair"))
        {
            GoDownStairInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleGoUpStairInput()
    {
        if (Input.IsActionJustPressed("go_up_stair"))
        {
            GoUpStairInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleToggleInventroyWindowInput()
    {
        if (Input.IsActionJustPressed("toggle_inventory"))
        {
            ToggleInventoryWindowInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleUseInventoryObjectInput()
    {
        if (Input.IsActionJustPressed("use_inventory_object"))
        {
            UseInventoryObjectInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleIdleInput()
    {
        if (Input.IsActionJustPressed("idle"))
        {
            IdleInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandlePickUpInput()
    {
        if (Input.IsActionJustPressed("pick_up"))
        {
            PickUpInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandlePutAwayInventoryInput()
    {
        if (Input.IsActionJustPressed("put_away"))
        {
            PutAwayInventoryObjectInputHandled?.Invoke();

            return true;
        }

        return false;
    }

    private bool HandleMovementInput()
    {
        var direction = GetMovementDirection();

        // 若未输入任何移动操作。
        if (direction == Vector2I.Zero)
        {
            // 确保Timer停止。
            _interruptMovementTimer.Stop();
            // 确保当前阻止移动持续时间为最大持续时间。
            _currentDurationOfInterruptMovement = _maxDurationOfInterruptMovement;
            // 跳过后续操作。
            return false;
        }

        // 若Timer处于运行状态，跳过后续操作。
        if (!_interruptMovementTimer.IsStopped()) { return false; }

        TryHandleMelee(direction);
        // 发送移动操作事件，参数为移动方向。
        MovementInputHandled?.Invoke(direction);

        // 根据当前阻止移动持续时间启动Timer。
        _interruptMovementTimer.Start(_currentDurationOfInterruptMovement);
        // 将当前阻止移动持续时间设为最小持续时间。
        _currentDurationOfInterruptMovement = _minDurationOfInterruptMovement;

        // 返回true，通知Update不再执行后续其他输入操作。
        return true;
    }

    private Vector2I GetMovementDirection()
    {
        var moveHorizontal =
            Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        var moveVertical =
            Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
        var direction = new Vector2(moveHorizontal, moveVertical);

        if (direction == Vector2.Zero)
        {
            var rightUpStrength = Input.GetActionStrength("move_right_up");
            var moveRightUp = new Vector2(rightUpStrength, -rightUpStrength);

            var upLeftStrength = Input.GetActionStrength("move_up_left");
            var moveUpLeft = new Vector2(-upLeftStrength, -upLeftStrength);

            var leftDownStrength = Input.GetActionStrength("move_left_down");
            var moveLeftDown = new Vector2(-leftDownStrength, leftDownStrength);

            var downRightStrength = Input.GetActionStrength("move_down_right");
            var moveDownRight = new Vector2(downRightStrength, downRightStrength);

            direction = moveRightUp + moveUpLeft + moveLeftDown + moveDownRight;
        }

        return (Vector2I)direction.Sign();
    }

    private void TryHandleMelee(Vector2I direction)
    {
        var targetPosition = _player.GlobalPosition + direction * _mapData.CellSize;

        var space = _player.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            CollideWithAreas = true,
            CollideWithBodies = false,
            CollisionMask = (int)PhysicsLayer.BlockMovement,
            Position = targetPosition
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
