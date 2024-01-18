using Godot;
using Godot.Collections;

public partial class ForestGenerator : Node, IMapGenerator, ISavable, ILoadable
{
    private ForestData _forestData;
    private TileSet _tileSet;
    private TileMap _tileMap;

    private SaveLoadManager _saveLoadManager;

    public void Initialize()
    {
        _forestData = GetParent<MapManager>().MapData as ForestData;
        _tileSet = GD.Load<TileSet>("res://resources/tile_sets/forest_tile_set.tres");
        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _tileMap.TileSet = _tileSet;

        if (!InitializeByLoadedData())
        {
            GenerateMap();
        }
    }

    public void Update()
    {
    }

    public Vector2I GetEnemySpawnCell()
    {
        while (true)
        {
            var randomX = GD.RandRange(1, _forestData.MapSize.X - 2);
            var randomY = GD.RandRange(1, _forestData.MapSize.Y - 2);
            var enemySpawnCell = new Vector2I(randomX, randomY);

            var mapManager = GetParent<MapManager>();
            if (!mapManager.TryAddCharacterCellAtSpawn(enemySpawnCell)) { continue; }

            return enemySpawnCell;
        }
    }

    public Vector2I GetPlayerStartCell()
    {
        var space = _tileMap.GetWorld2D().DirectSpaceState;

        while (true)
        {
            var randomX = GD.RandRange(1, _forestData.MapSize.X - 2);
            var randomY = GD.RandRange(1, _forestData.MapSize.Y - 2);
            var playerStartCell = new Vector2I(randomX, randomY);

            var parameters = new PhysicsPointQueryParameters2D
            {
                CollideWithAreas = true,
                CollisionMask = (int)PhysicsLayer.BlockMovement,
                Position = playerStartCell * _forestData.CellSize + _forestData.CellSize / 2
            };
            var results = space.IntersectPoint(parameters);

            if (results.Count > 0) { continue; }

            var mapManager = GetParent<MapManager>();
            mapManager.TryAddCharacterCellAtSpawn(playerStartCell);

            return playerStartCell;
        }
    }

    public Dictionary<string, Variant> GetDataForSave()
    {
        var groundCells = new Array<Vector2I>();
        var grassCells = new Array<Vector2I>();
        var treeCells = new Array<Vector2I>();
        var deadTreeCells = new Array<Vector2I>();

        var forestData = GetParent<MapManager>().MapData as ForestData;

        for (int y = 0; y < forestData.MapSize.Y; y++)
        {
            for (int x = 0; x < forestData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);

                var tileData = _tileMap.GetCellTileData((int)TileMapLayer.Default, cell);
                if (tileData.TerrainSet == (int)TerrainSet.Default)
                {
                    if (tileData.Terrain == (int)ForestTerrain.Ground)
                    {
                        groundCells.Add(cell);
                    }
                    else if (tileData.Terrain == (int)ForestTerrain.Grass)
                    {
                        grassCells.Add(cell);
                    }
                    else if (tileData.Terrain == (int)ForestTerrain.Tree)
                    {
                        treeCells.Add(cell);
                    }
                    else if (tileData.Terrain == (int)ForestTerrain.DeadTree)
                    {
                        deadTreeCells.Add(cell);
                    }
                }
            }
        }

        return new Dictionary<string, Variant>
        {
            { "ground_cells", groundCells },
            { "grass_cells", grassCells },
            { "tree_cells", treeCells },
            { "dead_tree_cells", deadTreeCells },
        };
    }

    public bool InitializeByLoadedData()
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
                var groundCells = map["ground_cells"].AsGodotArray<Vector2I>();
                var grassCells = map["grass_cells"].AsGodotArray<Vector2I>();
                var treeCells = map["tree_cells"].AsGodotArray<Vector2I>();
                var deadTreeCells = map["dead_tree_cells"].AsGodotArray<Vector2I>();

                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Default,
                    groundCells,
                    (int)TerrainSet.Default,
                    (int)ForestTerrain.Ground,
                    false
                );
                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Default,
                    grassCells,
                    (int)TerrainSet.Default,
                    (int)ForestTerrain.Grass,
                    false
                );
                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Default,
                    treeCells,
                    (int)TerrainSet.Default,
                    (int)ForestTerrain.Tree,
                    false
                );
                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Default,
                    deadTreeCells,
                    (int)TerrainSet.Default,
                    (int)ForestTerrain.DeadTree,
                    false
                );

                return true;
            }
        }

        return false;
    }

    private void GenerateMap()
    {
        RandomFillTiles();
    }

    private void RandomFillTiles()
    {
        var treeCells = new Array<Vector2I>();
        var deadTreeCells = new Array<Vector2I>();
        var groundCells = new Array<Vector2I>();
        var grassCells = new Array<Vector2I>();

        for (int y = 0; y < _forestData.MapSize.Y; y++)
        {
            for (int x = 0; x < _forestData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);

                if (x == 0 ||
                    y == 0 ||
                    x == _forestData.MapSize.X - 1 ||
                    y == _forestData.MapSize.Y - 1)
                {
                    if (GD.RandRange(0, 5) == 0)
                    {
                        deadTreeCells.Add(cell);
                    }
                    else
                    {
                        treeCells.Add(cell);
                    }
                    continue;
                }

                if (GD.RandRange(0, 100) <= 10)
                {
                    // 树木、枯树
                    if (GD.RandRange(0, 5) == 0)
                    {
                        deadTreeCells.Add(cell);
                    }
                    else
                    {
                        treeCells.Add(cell);
                    }
                    continue;
                }
                else
                {
                    // 地面、草地
                    if (GD.RandRange(0, 5) == 0)
                    {
                        grassCells.Add(cell);
                    }
                    else
                    {
                        groundCells.Add(cell);
                    }
                    continue;
                }
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            treeCells,
            (int)TerrainSet.Default,
            (int)ForestTerrain.Tree,
            false
        );

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            deadTreeCells,
            (int)TerrainSet.Default,
            (int)ForestTerrain.DeadTree,
            false
        );

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            groundCells,
            (int)TerrainSet.Default,
            (int)ForestTerrain.Ground,
            false
        );

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            grassCells,
            (int)TerrainSet.Default,
            (int)ForestTerrain.Grass,
            false
        );
    }
}
