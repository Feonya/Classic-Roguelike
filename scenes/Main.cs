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
        _fsm.Update();
    }
}

public enum PhysicsLayer
{
    BlockMovement = 1 << 0, // 1
    BlockSight = 1 << 1, // 2
    PickableObject = 1 << 2, // 4
    Fog = 1 << 3 // 8
}

public enum TileMapLayer
{
    Default,
    Fog
}

public enum TerrainSet
{
    Default,
    Fog,
    Stair
}

public enum DungeonTerrain
{
    Floor,
    Wall
}

public enum ForestTerrain
{
    Ground,
    Grass,
    Tree,
    DeadTree
}

public enum FogTerrain
{
    Unexplored,
    OutOfSight,
    InSight
}

public enum StairTerrain
{
    UpStair,
    DownStair
}