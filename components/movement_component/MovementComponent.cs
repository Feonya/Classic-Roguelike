using Godot;
using System;

public partial class MovementComponent : Node, IComponent
{
    private MapData _mapData;

    private InputHandler _inputHandler;
    private Vector2I _currentDirection;
    private AStarGridManager _aStarGridManager;

    public void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _aStarGridManager = this.GetUnique<AStarGridManager>();

        var owner = GetOwner<Node>();
        if (owner is Enemy && owner.HasNode("AiComponent"))
        {
            if (owner.HasNode("AiComponent/WalkAroundAi"))
            {
                owner.GetNode<WalkAroundAi>("AiComponent/WalkAroundAi").Executed += On_WalkAroundAi_Executed;
            }
            if (owner.HasNode("AiComponent/ChaseAi"))
            {
                owner.GetNode<ChaseAi>("AiComponent/ChaseAi").Executed += On_ChaseAi_Executed;
            }
        }
        else if (owner is Player)
        {
            _inputHandler = this.GetUnique<InputHandler>();
            _inputHandler.MovementInputHandled += On_InputHandler_MovementInputHandled;
        }
    }


    private bool IsMovementBlocked()
    {
        var owner = GetOwner<Node2D>();
        var targetPosition = owner.GlobalPosition + _currentDirection * _mapData.CellSize;

        var space = owner.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = targetPosition,
            CollideWithAreas = true,
            CollisionMask = (int)PhysicsLayer.BlockMovement
        };
        var results = space.IntersectPoint(parameters);
        if(results.Count > 0)
        {
            return true;
        }

        return false;
    }



    public void Update()
    {
        var owner = GetOwner<Node2D>();

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

    private void TrySetEnemyOwnerCellSolid(Node2D owner)
    {
        if(owner is Enemy enemy)
        {
            _aStarGridManager.AStarGrid.SetPointSolid(enemy.GetCell(), true);
        }
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
