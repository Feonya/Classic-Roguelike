using Godot;

public partial class MovementComponent : Node, IComponent
{
    private MapData _mapData;

    private InputHandler _inputHandler;
    private AStarGridManager _aStarGridManager;

    private Vector2I _currentDirection;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");
        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");

        var parent = GetParent();
        if (parent is Player)
        {
            _inputHandler.IdleInputHandled += On_InputHandler_IdleInputHandled;
            _inputHandler.MovementInputHandled += On_InputHandler_MovementInputHandled;
        }
        else if (parent is Enemy)
        {
            parent
                .GetNode<AiComponent>("AiComponent")
                .GetNode<WalkAroundAi>("WalkAroundAi")
                .Executed += On_WalkAroundAi_Executed;

            parent
                .GetNode<AiComponent>("AiComponent")
                .GetNode<ChaseAi>("ChaseAi")
                .Executed += On_ChaseAi_Executed;
        }
    }

    public void Update(double delta)
    {
        var owner = Owner as Node2D;

        if (_currentDirection == Vector2I.Zero)
        {
            TrySetEnemyOwnerCellSolid(owner);

            return;
        }

        if (IsMovementBlocked())
        {
            TrySetEnemyOwnerCellSolid(owner);

            _currentDirection = Vector2I.Zero;

            return;
        }

        owner.GlobalPosition += _currentDirection * _mapData.CellSize;

        TrySetEnemyOwnerCellSolid(owner);

        _currentDirection = Vector2I.Zero;
    }

    private bool IsMovementBlocked()
    {
        var ownerNode2D = Owner as Node2D;
        var targetPosition = ownerNode2D.GlobalPosition + _currentDirection * _mapData.CellSize;

        var space = ownerNode2D.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = targetPosition,
            CollisionMask = (int)PhysicsLayer.BlockMovement,
            CollideWithAreas = true
        };
        var results = space.IntersectPoint(parameters);

        if (results.Count > 0) { return true; }

        return false;
    }

    private void TrySetEnemyOwnerCellSolid(Node2D owner)
    {
        if (owner is Enemy)
        {
            _aStarGridManager.AStarGrid.SetPointSolid(
                (Vector2I)(owner.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize,
                true
            );
        }
    }

    private void On_InputHandler_IdleInputHandled()
    {
        _currentDirection = Vector2I.Zero;
    }

    private void On_InputHandler_MovementInputHandled(Vector2I direction)
    {
        _currentDirection = direction;
    }

    private void On_WalkAroundAi_Executed(Vector2I direction)
    {
        _currentDirection = direction;
    }

    private void On_ChaseAi_Executed(Vector2I direction)
    {
        _currentDirection = direction;
    }
}
