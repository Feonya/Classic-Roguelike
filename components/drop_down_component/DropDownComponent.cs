using Godot;

public partial class DropDownComponent : Node, IComponent
{
    private Node _pickableObjectContainer;

    public void Initialize()
    {
        _pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");
    }

    public void Update(double delta)
    {
    }

    public void DropExperience()
    {
        var owner = GetParent<Character>();
        var droppedExperience = (owner.CharacterData as EnemyData).DeathDropExperience;

        var player = GetTree().CurrentScene.GetNode<Player>("%Player");
        (player.CharacterData as PlayerData).Experience += droppedExperience;
    }

    public void TryDropPickableObject()
    {
        var owner = GetParent<Character>();

        foreach (var element in (owner.CharacterData as EnemyData).DeathDropPickableObjects)
        {
            var dropProbability = element.Value;

            if (GD.RandRange(0f, 1f) > dropProbability) { continue; }

            var pickableObject = element.Key.Instantiate<PickableObject>();

            if (pickableObject is IUniquePickableObject)
            {
                var uniquePickableObject = pickableObject as IUniquePickableObject;
                if (uniquePickableObject.IsAppeared)
                {
                    pickableObject.QueueFree();
                    return;
                }
                else
                {
                    uniquePickableObject.IsAppeared = true;
                }
            }

            _pickableObjectContainer.AddChild(pickableObject);
            pickableObject.GlobalPosition = owner.GlobalPosition;
            pickableObject.Initialize();

            break;
        }
    }
}
