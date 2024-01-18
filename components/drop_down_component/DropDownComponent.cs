using Godot;

public partial class DropDownComponent : Node, IComponent, ILateUpdateComponent
{
    private Node _pickableObjectContainer;

    public void Initialize()
    {
        _pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");
    }

    public void Update()
    {
    }

    public void LateUpdate()
    {
        TryDropPickableObject();
        DropExperience();
    }

    private void DropExperience()
    {
        var owner = GetOwner<Character>();

        if (owner is not Enemy || !owner.IsDead) { return; }

        var droppedExperience = (owner.CharacterData as EnemyData).DeathDropExperience;

        var player = GetTree().CurrentScene.GetNode<Player>("%Player");
        (player.CharacterData as PlayerData).Experience += droppedExperience;
    }

    private void TryDropPickableObject()
    {
        var owner = GetOwner<Character>();

        if (owner is not Enemy || !owner.IsDead) { return; }

        foreach (var element in (owner.CharacterData as EnemyData).DeathDropPickableObjects)
        {
            var dropProbability = element.Value;

            if (GD.RandRange(0f, 1f) > dropProbability) { continue; }

            var pickableObject = element.Key.Instantiate<PickableObject>();

            _pickableObjectContainer.AddChild(pickableObject);
            pickableObject.GlobalPosition = owner.GlobalPosition;
            pickableObject.Initialize();

            break;
        }
    }
}
