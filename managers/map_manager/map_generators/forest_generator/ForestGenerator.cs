using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

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
        _tileMap = this.GetUnique<TileMap>();
        _saveLoadManager = this.GetUnique<SaveLoadManager>();

        _tileMap.TileSet = _tileSet;
        if (!InitializeByLoadedData())
        GenerateMap();
    }

    private void GenerateMap()
    {
        GD.Print("----------");
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

                if(x == 0 || y == 0 ||
                    x == _forestData.MapSize.X - 1 ||
                    y == _forestData.MapSize.Y - 1)
                {
                    if (GD.RandRange(0, 5) == 0)
                        deadTreeCells.Add(cell);
                    else
                        treeCells.Add(cell);

                    continue;
                } 

                if(GD.RandRange(0,100)<= 10)
                {
                    // 树木、枯树
                    if(GD.RandRange(0,5)== 0)
                    {
                        deadTreeCells.Add(cell);
                    }
                    else
                    {
                        treeCells.Add(cell);
                    }
                }
                else
                {
                    // 地面、草地
                    if(GD.RandRange(0,5)== 0)
                    {
                        grassCells.Add(cell);
                    }
                    else
                    {
                        groundCells.Add(cell);
                    }
                }
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            treeCells,
            (int)TerrainSet.Default,
            (int)ForestTerrain.Tree,
            false);

        _tileMap.SetCellsTerrainConnect(
          (int)TileMapLayer.Default,
          deadTreeCells,
          (int)TerrainSet.Default,
          (int)ForestTerrain.DeadTree,
          false);

        _tileMap.SetCellsTerrainConnect(
          (int)TileMapLayer.Default,
          grassCells,
          (int)TerrainSet.Default,
          (int)ForestTerrain.Grass,
          false);

        _tileMap.SetCellsTerrainConnect(
          (int)TileMapLayer.Default,
          groundCells,
          (int)TerrainSet.Default,
          (int)ForestTerrain.Ground,
          false);

        GD.Print("=========" + treeCells.Count + " " + deadTreeCells.Count+" "+grassCells.Count+" "+groundCells.Count) ;
    }


    public void Update()
    {

    }


    public Vector2I GetEnemySpawnCell()
    {
        while(true)
        {
            var randomX = GD.RandRange(1, _forestData.MapSize.X - 2);
            var randomY = GD.RandRange(1, _forestData.MapSize.Y - 2);
            var enemyStartCell = new Vector2I(randomX, randomY);
            var mapManager = GetParent<MapManager>();
            if (!mapManager.TryAddCharacterCellAtSpawn(enemyStartCell))
            {
                continue;
            }
            return enemyStartCell;
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
            var result = space.IntersectPoint(parameters);

            if (result.Count > 0)
                continue;

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
            ["ground_cells"] = groundCells,
            ["grass_cells"] = grassCells,
            ["tree_cells"] = treeCells,
            ["dead_tree_cells"] = deadTreeCells
        };
    }
    public bool InitializeByLoadedData()
    {
        if (!_saveLoadManager.IsInitialized("maps"))
            return false;

        var maps = _saveLoadManager.maps;
        foreach (var map in maps)
        {
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                var groundCells = map["ground_cells"].AsGodotArray<Vector2I>();
                var grassCells = map["grass_cells"].AsGodotArray<Vector2I>();
                var treeCells = map["tree_cells"].AsGodotArray<Vector2I>();
                var deadTreeCells = map["dead_tree_cells"].AsGodotArray<Vector2I>();

                _tileMap.SetCellsTerrainConnect
                    (
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

}
