using Godot;
using Godot.Collections;

public partial class PickUpComponent : Node, IComponent
{
    private InputHandler _inputHandler;

    private bool _isDirty;

    public void Initialize()
    {
        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");

        _inputHandler.PickUpInputHandled += On_InputHandler_PickUpInputHandled;
    }

    public void Update(double delta)
    {
        if (!_isDirty) { return; }

        TryPickUpPickableObjects();

        _isDirty = false;
    }

    private void TryPickUpPickableObjects()
    {
        var owner = GetParent<Node2D>();

        var space = owner.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Exclude = new Array<Rid> { owner.GetNode<Area2D>("Area2D").GetRid() },
            CollideWithAreas = true,
            CollideWithBodies = false,
            CollisionMask = (int)PhysicsLayer.PickableObject,
            Position = owner.GlobalPosition
        };
        var results = space.IntersectPoint(parameters);

        if (results.Count == 0) { return; }

        foreach (var result in results)
        {
            var pickableObject = result["collider"].As<Area2D>().Owner as PickableObject;
            pickableObject.BePickedUp(owner as Character);
        }
    }

    private void On_InputHandler_PickUpInputHandled()
    {
        _isDirty = true;
    }
}
