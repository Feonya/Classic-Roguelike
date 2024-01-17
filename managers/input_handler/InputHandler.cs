using Godot;
using System;
using System.Diagnostics;

public partial class InputHandler : Node, IManager
{
    //移动方向
    public event Action<Vector2I> MovementInputHandled;
    public event Action PickUpInputHandled;
    public event Action ToggleInventoryWindowInputHandled;
    public event Action UseInventoryObjectInputHandled;
    public event Action PutAwayInventoryObjectInputHandled;
    public event Action GoUpStairInputHandled;
    public event Action GoDownStairInputHandled;

    private MapData _mapData;
    private Player _player;
    private CombatManager _combatManager;
    private InventoryWindow _inventoryWindow;

    private Timer _interruptMovementTimer;

    private float _maxDurationOfInterruptMovement = 0.5f;
    private float _minDurationOfInterruptMovement = 0.05f;
    private float _currentDurationOfInterruptMovement;

    public void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _player = this.GetUnique<Player>();
        _combatManager = this.GetUnique<CombatManager>();
        _interruptMovementTimer = GetNode<Timer>("InterruptMovementTimer");
        _inventoryWindow = this.GetUnique<InventoryWindow>();
         
    }

    public void Update()
    {
        if (_player.IsDead)
            return;

        if (HandleToggleInventoryWindowInput())
            return;

        if (HandleUseInventoryObjectInput())
            return;

        if (HandlePutAwayInventoryObjectInput())
            return;

        if (_inventoryWindow.Visible)
            return;

        if (HandleGoUpStairInput())
            return;
         if (HandleGoDownStairInput())
            return;

        if (HandlePickUpInput())
            return;

        HandleMovementInput();
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



    private bool HandleToggleInventoryWindowInput()
    {
        if (Input.IsActionJustPressed("toggle_inventory"))
        {
            ToggleInventoryWindowInputHandled.Invoke();
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


    public bool HandlePickUpInput()
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

        if (!_interruptMovementTimer.IsStopped())
            return false;
        TryHandleMelee(direction);
        MovementInputHandled?.Invoke(direction);

        _interruptMovementTimer.Start(_currentDurationOfInterruptMovement);
        _currentDurationOfInterruptMovement = _minDurationOfInterruptMovement;

        return true;
    }

    private Vector2I GetMovementDirection()
    {
        var movementHorizontal = Input.GetActionStrength("move_right") -
            Input.GetActionStrength("move_left");
        var moevementVertical = Input.GetActionStrength("move_down") -
            Input.GetActionStrength("move_up");
        var direction = new Vector2(movementHorizontal, moevementVertical);
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
            CollideWithBodies = false,
        };
        var results =  space.IntersectPoint(parameters);
        if(results.Count == 0)
        {
            return;
        }

        foreach(var result in results )
        {
            var collider = result["collider"].As<Area2D>();
            if(collider.Owner is Enemy enemy)
            {
                _combatManager.AddToCombatList(_player,enemy);
                GD.Print("玩家 攻击 " +enemy.CharacterData.Name +" !");
            }
        }
    }
}
