using Godot;
using Godot.Collections;

public partial class EnemyData : CharacterData
{
    [Export]
    public float DeathDropExperience;

    [Export]
    public Dictionary<PackedScene, float> DeathDropPickableObjects = new();
}
