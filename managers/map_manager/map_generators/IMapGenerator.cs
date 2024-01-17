using Godot;

public interface IMapGenerator
{
    public void Initialize();
    public void Update();

    public Vector2I GetPlayerStartCell(); //获取玩家出生所在单元格
    public Vector2I GetEnemySpawnCell();//获取敌人出生单元格
}