using Godot;
using Godot.Collections;

public partial class ForestGenerator : Node, IMapGenerator
{
    private ForestData _forestData;
    private TileSet _tileSet;
    private TileMap _tileMap;

    private SaveLoadManager _saveLoadManager;

    public MapData MapData { get => _forestData; }

    public void Initialize()
    {
        _forestData = GetParent<MapManager>().MapData as ForestData;
        _tileSet = GD.Load<TileSet>("res://resources/tile_sets/forest_tile_set.tres");
        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");

        _tileMap.TileSet = _tileSet;

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        GenerateMap();
    }

    public void Update(double delta)
    {
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

    private void GenerateMap()
    {
        if (!TryGenerateMapByPersistentData())
        {
            RandomFillTiles();
        }
    }

    private bool TryGenerateMapByPersistentData()
    {
        if (_saveLoadManager.PersistentData == null ||
            _saveLoadManager.PersistentData.Count == 0 ||
            !_saveLoadManager.PersistentData.ContainsKey("maps"))
        {
            return false;
        }

        var mapsPersistentData = _saveLoadManager
            .PersistentData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < mapsPersistentData.Count; i++)
        {
            var mapPersistentData = mapsPersistentData[i];
            if (mapPersistentData["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                var groundCells = mapPersistentData["ground_cells"].AsGodotArray<Vector2I>();
                var grassCells = mapPersistentData["grass_cells"].AsGodotArray<Vector2I>();
                var treeCells = mapPersistentData["tree_cells"].AsGodotArray<Vector2I>();
                var deadTreeCells = mapPersistentData["dead_tree_cells"].AsGodotArray<Vector2I>();

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

                return true;
            }
        }

        return false;
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

                if (x == 0 || y == 0 || x == _forestData.MapSize.X - 1 || y == _forestData.MapSize.Y - 1)
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
