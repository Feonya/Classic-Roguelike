using Godot;
using Godot.Collections;

public partial class EnemyData : CharacterData
{
    [Export]
    public float DeathDropExperience; //死亡掉落多少经验

    [Export]  //掉落的道具以及起概率
    public Dictionary<PackedScene, float> DeathDropPickableObjects = new Dictionary<PackedScene, float>();

}
