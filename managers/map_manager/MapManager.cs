using System;
using Godot;
using Godot.Collections;

public partial class MapManager : Node, IManager, ISavable
{
    public event Action<Vector2I, Callable/*GetEnemySpawnCell*/> Initialized;

    [Export]
    private MapData _mapData;

    private SaveLoadManager _saveLoadManager;

    private IMapGenerator _mapGenerator;

    private System.Collections.Generic.List<Vector2I> _characterCellsAtSpawn = new();

    public MapData MapData { get => _mapData; }

    public void Initialize()
    {
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        if (GetChildCount() != 1 || GetChild(0) is not IMapGenerator)
        {
            throw new Exception("MapManager需且仅需1个MapGenerator子节点！");
        }

        _mapGenerator = GetChild(0) as IMapGenerator;

        _mapGenerator.Initialize();

        if (IsMapGenerated())
        {
            Initialized.Invoke(Vector2I.Zero, Callable.From(null));
        }
        else
        {
            Initialized.Invoke(
                _mapGenerator.GetPlayerStartCell(),
                Callable.From(_mapGenerator.GetEnemySpawnCell)
            );
        }
    }

    public void Update()
    {
    }

    public bool TryAddCharacterCellAtSpawn(Vector2I cell)
    {
        if (_characterCellsAtSpawn.Contains(cell)) { return false; }

        _characterCellsAtSpawn.Add(cell);

        return true;
    }

    public Dictionary<string, Variant> GetDataForSave()
    {
        var upStairCell = Vector2I.Zero;
        var downStairCell = Vector2I.Zero;
        var unexploredFogCells = new Array<Vector2I>();
        var outOfSightFogCells = new Array<Vector2I>();
        var inSightFogCells = new Array<Vector2I>();

        var tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");

        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);

                var tileData = tileMap.GetCellTileData((int)TileMapLayer.Default, cell);
                if (tileData.TerrainSet == (int)TerrainSet.Stair)
                {
                    if (tileData.Terrain == (int)StairTerrain.UpStair)
                    {
                        upStairCell = cell;
                    }
                    else if (tileData.Terrain == (int)StairTerrain.DownStair)
                    {
                        downStairCell = cell;
                    }
                }

                var fogTileData = tileMap.GetCellTileData((int)TileMapLayer.Fog, cell);
                if (fogTileData.TerrainSet == (int)TerrainSet.Fog)
                {
                    if (fogTileData.Terrain == (int)FogTerrain.Unexplored)
                    {
                        unexploredFogCells.Add(cell);
                    }
                    else if (fogTileData.Terrain == (int)FogTerrain.OutOfSight)
                    {
                        outOfSightFogCells.Add(cell);
                    }
                    else if (fogTileData.Terrain == (int)FogTerrain.InSight)
                    {
                        inSightFogCells.Add(cell);
                    }
                }
            }
        }

        var enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");
        var enemies = new Array<Dictionary<string, Variant>>();
        for (int i = 0; i < enemyContainer.GetChildCount(); i++)
        {
            enemies.Add(enemyContainer.GetChild<Enemy>(i).GetDataForSave());
        }

        var pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");
        var pickableObjects = new Array<Dictionary<string, Variant>>();
        for (int i = 0; i < pickableObjectContainer.GetChildCount(); i++)
        {
            pickableObjects.Add(pickableObjectContainer.GetChild<PickableObject>(i).GetDataForSave());
        }

        var player = GetTree().CurrentScene.GetNode<Player>("%Player");

        var mapDataForSave = new Dictionary<string, Variant>
        {
            { "scene_name", GetTree().CurrentScene.Name },
            { "up_stair_cell", upStairCell },
            { "down_stair_cell", downStairCell },
            { "enemies", enemies },
            { "pickable_objects", pickableObjects },
            { "unexplored_fog_cells", unexploredFogCells },
            { "out_of_sight_fog_cells", outOfSightFogCells },
            { "in_sight_fog_cells", inSightFogCells },
            { "player_last_position", player.GlobalPosition }
        };

        mapDataForSave.Merge((_mapGenerator as ISavable).GetDataForSave());

        return mapDataForSave;
    }

    private bool IsMapGenerated()
    {
        if (_saveLoadManager.LoadedData == null ||
            _saveLoadManager.LoadedData.Count == 0 ||
            !_saveLoadManager.LoadedData.ContainsKey("maps"))
        {
            return false;
        }

        var maps = _saveLoadManager
            .LoadedData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < maps.Count; i++)
        {
            var map = maps[i];
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                return true;
            }
        }

        return false;
    }
}
