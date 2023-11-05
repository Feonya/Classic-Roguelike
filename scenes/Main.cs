using Godot;

public partial class Main : Node
{
    private Fsm _fsm;

    public override void _Ready()
    {
        RenderingServer.SetDefaultClearColor(Colors.Black);

        _fsm = GetNode<Fsm>("%Fsm");

        _fsm.Initialize();
    }

    public override void _Process(double delta)
    {
        _fsm.Update(delta);
    }
}

public enum PhysicsLayer
{
    BlockMovement = 1,
    BlockSight = 2,
    PickableObject = 4,
    Fog = 8
}

public enum TileMapLayer
{
    Default,
    Fog
}

public enum TerrainSet
{
    Default,
    Fog
}

public enum ForestTerrain
{
    Ground,
    Grass,
    Tree,
    DeadTree,
    UpStair,
    DownStair
}

public enum DungeonTerrain
{
    Floor,
    Wall,
    UpStair,
    DownStair
}

public enum FogTerrain
{
    Unexplored,
    OutOfSight,
    InSight
}
