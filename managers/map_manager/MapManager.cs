using System;
using Godot;
using Godot.Collections;

public partial class MapManager : Node, IManager, IPersistence
{
    public event Action<
        Vector2I/*PlayerStartCell*/,
        Callable/*GetEnemySpawnCell()*/
    > Initialized;

    [Export]
    private MapData _mapData;

    private IMapGenerator _mapGenerator;

    private System.Collections.Generic.List<Vector2I> _characterCellsAtSpawn = new();

    public MapData MapData { get => _mapData; }

    public void Initialize()
    {
        _characterCellsAtSpawn.Clear();

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

    public void Update(double delta)
    {
    }

    public bool TryAddCharacterCellAtSpawn(Vector2I cell)
    {
        if (_characterCellsAtSpawn.Contains(cell))
        {
            return false;
        }

        _characterCellsAtSpawn.Add(cell);

        return true;
    }

    public Dictionary<string, Variant> GetPersistentData()
    {
        var groundCells = new Array<Vector2I>();
        var grassCells = new Array<Vector2I>();
        var treeCells = new Array<Vector2I>();
        var deadTreeCells = new Array<Vector2I>();
        var floorCells = new Array<Vector2I>();
        var wallCells = new Array<Vector2I>();
        var downStairCell = Vector2I.Zero;
        var upStairCell = Vector2I.Zero;
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
                if (tileData.TerrainSet == (int)TerrainSet.Default)
                {
                    if (tileMap.TileSet.ResourcePath == "res://resources/tile_sets/forest_tile_set.tres")
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
                        else if (tileData.Terrain == (int)ForestTerrain.DownStair)
                        {
                            downStairCell = cell;
                        }
                        else if (tileData.Terrain == (int)ForestTerrain.UpStair)
                        {
                            upStairCell = cell;
                        }
                    }
                    else if (tileMap.TileSet.ResourcePath == "res://resources/tile_sets/dungeon_tile_set.tres")
                    {
                        if (tileData.Terrain == (int)DungeonTerrain.Floor)
                        {
                            floorCells.Add(cell);
                        }
                        else if (tileData.Terrain == (int)DungeonTerrain.Wall)
                        {
                            wallCells.Add(cell);
                        }
                        else if (tileData.Terrain == (int)DungeonTerrain.DownStair)
                        {
                            downStairCell = cell;
                        }
                        else if (tileData.Terrain == (int)DungeonTerrain.UpStair)
                        {
                            upStairCell = cell;
                        }
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
            enemies.Add(
                enemyContainer.GetChild<Enemy>(i).GetPersistentData()
            );
        }

        var pickableObjectContainer = GetTree().CurrentScene.GetNode<Node>("%PickableObjectContainer");
        var pickableObjects = new Array<Dictionary<string, Variant>>();
        for (int i = 0; i < pickableObjectContainer.GetChildCount(); i++)
        {
            pickableObjects.Add(
                pickableObjectContainer.GetChild<PickableObject>(i).GetPersistentData()
            );
        }

        var player = GetTree().CurrentScene.GetNode<Player>("%Player");

        return new Dictionary<string, Variant>
        {
            { "scene_name", GetTree().CurrentScene.Name },
            { "ground_cells", groundCells },
            { "grass_cells", grassCells },
            { "tree_cells", treeCells },
            { "dead_tree_cells", deadTreeCells },
            { "floor_cells", floorCells },
            { "wall_cells", wallCells },
            { "down_stair_cell", downStairCell },
            { "up_stair_cell", upStairCell },
            { "enemies", enemies},
            { "pickable_objects", pickableObjects },
            { "unexplored_fog_cells", unexploredFogCells },
            { "out_of_sight_fog_cells", outOfSightFogCells },
            { "in_sight_fog_cells", inSightFogCells },
            { "player_last_position", player.GlobalPosition },
        };
    }

    private bool IsMapGenerated()
    {
        var saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        if (saveLoadManager.PersistentData == null ||
            saveLoadManager.PersistentData.Count == 0 ||
            !saveLoadManager.PersistentData.ContainsKey("maps"))
        {
            return false;
        }


        var mapsPersistentData = saveLoadManager
            .PersistentData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < mapsPersistentData.Count; i++)
        {
            var mapPersistentData = mapsPersistentData[i];
            if (mapPersistentData["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                return true;
            }
        }

        return false;
    }
}
