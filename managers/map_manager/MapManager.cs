using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class MapManager : Node, IManager, ISavable
{
    public event Action<Vector2I,Func<Vector2I>> Initialized;

    [Export] private MapData _mapData;

    private IMapGenerator _mapGenerator;

    private List<Vector2I> _characterCellsAsSpawn = new();
    private SaveLoadManager _saveLoadManager;

    public MapData MapData { get => _mapData; }


    public void Initialize()
    {
        if (GetChildCount() != 1 || GetChild(0) is not IMapGenerator)
        {
            throw new Exception("Mapmanager需且仅需1个MapGenerator子节点！");
        }
        _saveLoadManager = this.GetUnique<SaveLoadManager>();

        _mapGenerator = GetChild(0) as IMapGenerator;
        _mapGenerator.Initialize();

        if (IsMapGenerated())
        {
            Initialized?.Invoke(Vector2I.Zero,null);
        }
        else
        {
            Initialized?.Invoke(
          _mapGenerator.GetPlayerStartCell(),
          _mapGenerator.GetEnemySpawnCell);
        }
    }

    public void Update()
    {
    }

    public bool TryAddCharacterCellAtSpawn(Vector2I cell)
    {
        if (_characterCellsAsSpawn.Contains(cell))
            return false;

        _characterCellsAsSpawn.Add(cell);
        return true;
    }

    public Godot.Collections.Dictionary<string, Variant> GetDataForSave()
    {
        var upStairCell = Vector2I.Zero;
        var downStairCell = Vector2I.Zero;
        var unexploredFogCells = new Godot.Collections.Array<Vector2I>();
        var outOfSightFogCells = new Godot.Collections.Array<Vector2I>();
        var inSightFogCells = new Godot.Collections.Array<Vector2I>();

        var tileMap = this.GetUnique<TileMap>();
        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                var tileData = tileMap.GetCellTileData((int)TileMapLayer.Default,cell);
                if(tileData.TerrainSet == (int)TerrainSet.Stair)
                {
                    if(tileData.Terrain == (int)StairTerrain.UpStair)
                    {
                        upStairCell = cell;
                    }
                    else if(tileData.Terrain == (int)StairTerrain.DownStair)
                    {
                        downStairCell= cell;
                    }
                }

                var fogTileData = tileMap.GetCellTileData((int)TileMapLayer.Fog,cell);
                if (fogTileData.TerrainSet == (int)TerrainSet.Fog)
                {
                    if(fogTileData.Terrain == (int)FogTerrain.Unexplored)
                    {
                         unexploredFogCells.Add(cell);
                    }
                    else if (fogTileData.Terrain == (int)FogTerrain.OutOfSight)
                    {
                        outOfSightFogCells.Add(cell);
                    }
                    else if (fogTileData.Terrain == (int)FogTerrain.InSight)
                    {
                        inSightFogCells.Add(cell) ;
                    }
                }
            }
        }

        var enemyContainer = this.GetUnique("%EnemyContainer");
        var enemies = new Godot.Collections.Array<Godot.Collections.Dictionary<string,Variant>>();
        for (int i = 0; i < enemyContainer.GetChildCount(); i++)
        {
            var enemy = enemyContainer.GetChild<Enemy>(i);
            enemies.Add(enemy.GetDataForSave());
        }

        var pickableObjectContainer = this.GetUnique("%PickableObjectContainer");
        var pickableObjects = new Godot.Collections.Array<Godot.Collections.Dictionary<string,Variant>>();
        for (int i = 0; i < pickableObjectContainer.GetChildCount(); i++)
        {
            var pickableObject = pickableObjectContainer.GetChild<PickableObject>(i);
            pickableObjects.Add(pickableObject.GetDataForSave());
        }

        var player = this.GetUnique<Player>();
        var mapDataForSave = new Godot.Collections.Dictionary<string, Variant>
        {
            ["scene_name"] = GetTree().CurrentScene.Name,
            ["up_stair_cell"] = upStairCell,
            ["down_stair_cell"] = downStairCell,
            ["enemies"] = enemies,
            ["pickable_objects"] = pickableObjects,
            ["unexplored_fog_cells"] = unexploredFogCells,
            ["out_of_sight_fog_cells"] = outOfSightFogCells,
            ["in_sight_fog_cells"] = inSightFogCells,
            ["player_last_position"] = player.GlobalPosition
        };

        mapDataForSave.Merge((_mapGenerator as ISavable).GetDataForSave());
        return mapDataForSave;
    }

    private bool IsMapGenerated()
    {
       if(!_saveLoadManager.IsInitialized("maps"))
            return false;

        var maps = _saveLoadManager.maps;
        foreach ( var map in maps)
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
                return true;

        return false;
    }
}
