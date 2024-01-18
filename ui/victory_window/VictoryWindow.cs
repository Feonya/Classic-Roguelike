using Godot;

public partial class VictoryWindow : CanvasLayer, IUi
{
    public void Initialize()
    {
        var enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");
        foreach (var child in enemyContainer.GetChildren())
        {
            var enemy = child as Enemy;
            if (enemy.CharacterData.Name == "骷髅王")
            {
                enemy.SkeletonKingDied += () => Visible = true;
            }
        }
    }

    public void Update()
    {
    }
}
