using Godot;

public partial class VictoryWindow : CanvasLayer, IUi
{
    public void Initialize()
    {
        var enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");
        foreach (var child in enemyContainer.GetChildren())
        {
            var enemy = child as Enemy;
            if (enemy.Name == "SkeletonKing")
            {
                enemy.Winning += () => Visible = true;
            }
        }
    }

    public void Update(double delta)
    {
    }
}
