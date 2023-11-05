using Godot;

public interface IMapGenerator
{
    public void Initialize();
    public void Update(double delta);

    public Vector2I GetPlayerStartCell();
    public Vector2I GetEnemySpawnCell();
}
