using Godot;
using Godot.Collections;

public partial class PickUpComponent : Node, IComponent
{
    private InputHandler _inputHandler;

    private Node _pickableObjectContainer;

    private bool _isDirty;

    public void Initialize()
    {
        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");

        _pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");

        _inputHandler.PickUpInputHandled += On_InputHandler_PickUpInputHandled;
    }

    public void Update()
    {
        if (!_isDirty) { return; }

        TryPickUpPickableObjects();

        _isDirty = false;
    }

    private void TryPickUpPickableObjects()
    {
        var owner = GetOwner<Character>();

        if (owner is not Player) { return; }

        var space = owner.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = owner.GlobalPosition,
            CollideWithAreas = true,
            CollideWithBodies = false,
            CollisionMask = (int)PhysicsLayer.PickableObject,
            Exclude = new Array<Rid> { owner.GetNode<Area2D>("Area2D").GetRid() }
        };
        var results = space.IntersectPoint(parameters);

        if (results.Count == 0) { return; }

        foreach (var result in results)
        {
            var pickableObject = result["collider"].As<Area2D>().Owner as PickableObject;
            PickUp(owner, pickableObject);
        }
    }

    private void PickUp(Character owner, PickableObject pickableObject)
    {
        if (pickableObject is IImmediateEffectItem)
        {
            (pickableObject as IImmediateEffectItem).DoImmediateEffect();
        }

        var player = owner as Player;
        (player.CharacterData as PlayerData).Inventory.Add(pickableObject);

        pickableObject.Visible = false;
        _pickableObjectContainer.RemoveChild(pickableObject);
    }

    private void On_InputHandler_PickUpInputHandled()
    {
        _isDirty = true;
    }
}
