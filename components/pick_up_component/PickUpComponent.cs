using Godot;
using System;

public partial class PickUpComponent : Node, IComponent
{
    private InputHandler _inputHandler;
    private Node _pickableObjectContainer;
    private bool _isDirty;

    public void Initialize()
    {
        _inputHandler =  this.GetUnique<InputHandler>();
        _pickableObjectContainer = this.GetUnique("%PickableObjectContainer");

        _inputHandler.PickUpInputHandled += On_InputHandler_PickUpInputHandled;
    }

    public void Update()
    {
        if (_isDirty)
        {
            TryPickUpPickableObjects();
            _isDirty = false;
        }
    }


    private void On_InputHandler_PickUpInputHandled()
    {
        _isDirty = true;
    }

    private void TryPickUpPickableObjects()
    {
        var owner = GetOwner<Character>();
        if(owner is not Player)
        {
            return;
        }

        var space = owner.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = owner.GlobalPosition,
            CollideWithAreas = true,
            CollideWithBodies = false,
            CollisionMask = (int)PhysicsLayer.PickableObject,
            Exclude = new Godot.Collections.Array<Rid> 
            {
                owner.GetNode<Area2D>("Area2D").GetRid() 
            }
        };

        var results = space.IntersectPoint(parameters);
        if (results.Count == 0)
            return;

        foreach(var result in results )
        {
            var piickableObject = result["collider"].As<Area2D>().Owner as PickableObject;
            PickUp(owner,piickableObject);
        }
    }

    private void PickUp(Character owner, PickableObject pickableObject)
    {
        if (owner is Player player)
        {
            if(pickableObject is IImmediateEffectItem immediate)
            {
                immediate.DoImmediateEffect();
            }

            var playerData = player.CharacterData as PlayerData;
            playerData.Inventory.Add(pickableObject);

            pickableObject.Visible = false;
            _pickableObjectContainer.RemoveChild(pickableObject);
        }
    }
}
