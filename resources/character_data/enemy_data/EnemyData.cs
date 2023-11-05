using Godot;
using Godot.Collections;

public partial class EnemyData : CharacterData
{
    [Export]
    public float DeathDropExperience;
    [Export]
    public Dictionary<PackedScene/*掉落物品或装备*/, float/*掉落概率*/> DeathDropPickableObjects = new();
}
