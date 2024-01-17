using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
public partial class DungeonGenerator : Node, IMapGenerator, ISavable, ILoadable
{
    private DungeonData _dungeonData;
    private TileSet _tileSet;
    private TileMap _tileMap;
    private SaveLoadManager _saveLoadManager;

    private List<Rect2I> _rooms = new();

    public void Initialize()
    {
        _dungeonData = GetParent<MapManager>().MapData as DungeonData;
        _tileSet = GD.Load<TileSet>("res://resources/tile_sets/dungeon_tile_set.tres");
        _tileMap = this.GetUnique<TileMap>();
        _saveLoadManager = this.GetUnique<SaveLoadManager>();
        _tileMap.TileSet = _tileSet;

        if (!InitializeByLoadedData())
            GenerateMap();
    }

    public void Update()
    {
    }

    public Vector2I GetEnemySpawnCell()
    {
        while (true)
        {
            var randomNumber = GD.RandRange(0, _rooms.Count - 1);
            var randomRoom = _rooms[randomNumber];

            var randomX = GD.RandRange(
                randomRoom.Position.X,
                randomRoom.Position.X + randomRoom.Size.X - 1
                );

            var randomY = GD.RandRange(
                randomRoom.Position.Y,
                randomRoom.Position.Y + randomRoom.Size.Y - 1
                );

            var spawnCell = new Vector2I(randomX, randomY);

            if (!GetParent<MapManager>().TryAddCharacterCellAtSpawn(spawnCell))
            {
                continue;
            }

            return spawnCell;
        }
    }

    public Vector2I GetPlayerStartCell()
    {
        var randomNumber = GD.RandRange(0, _rooms.Count - 1);
        var playerStartCell = _rooms[randomNumber].GetCenter();
        GetParent<MapManager>().TryAddCharacterCellAtSpawn(playerStartCell);
        return playerStartCell;
    }

    private void GenerateMap()
    {
        FullFillWithWalls();
        RandomDigRooms();
        RandomDigCorridors();
    }

    private void SortRooms()
    {
        _rooms.Sort((room1, room2) =>
        {
            if (room1.GetCenter().X < room2.GetCenter().X) return -1;
            else if (room1.GetCenter().X > room2.GetCenter().X) return 1;
            else
                return room1.GetCenter().Y.CompareTo(room2.GetCenter().Y);
        });
    }

    private void SortRoomsFromLeftToRight()
    {
        _rooms.Sort((room1, room2) =>
        {
            return room1.GetCenter().X.CompareTo(room2.GetCenter().Y);
        });
    }

    private void SortRoomsFromTopToBottom()
    {
        _rooms.Sort((room1, room2) =>
        {
            return room1.GetCenter().Y.CompareTo(room2.GetCenter().Y);
        });
    }

    private void RandomDigCorridors()
    {
        var allCorridorCells = new Array<Vector2I>();
        for (int i = 0; i < _rooms.Count - 1; i++)
        {
            var room1 = _rooms[i];
            var room2 = _rooms[i + 1];

            var x1 = room1.GetCenter().X;
            var y1 = room1.GetCenter().Y;
            var x2 = room2.GetCenter().X;
            var y2 = room2.GetCenter().Y;

            if (GD.RandRange(0, 1) == 0)
            {
                var horizontalCells = GetHorizontalCorridorCells(x1, x2, y1);
                var verticalCells = GetVerticalCorridorCells(x2, y1, y2);
                allCorridorCells.AddRange(horizontalCells);
                allCorridorCells.AddRange(verticalCells);
            }
            else
            {
                var verticalCells = GetVerticalCorridorCells(x1, y1, y2);
                var horizontalCells = GetHorizontalCorridorCells(x1, x2, y2);
                allCorridorCells.AddRange(verticalCells);
                allCorridorCells.AddRange(horizontalCells);
            }
        }
        _tileMap.SetCellsTerrainConnect((int)TileMapLayer.Default, allCorridorCells,
            (int)TerrainSet.Default, (int)DungeonTerrain.Floor);
    }

    private Array<Vector2I> GetHorizontalCorridorCells(int x1, int x2, int y)
    {
        var corridorCells = new Array<Vector2I>();
        var step = x2 - x1;
        if (step >= 0)
        {
            for (int x = x1; x <= x2; x++)
                corridorCells.Add(new Vector2I(x, y));
        }
        else
        {
            for (int x = x2; x <= x1; x++)
                corridorCells.Add(new Vector2I(x, y));
        }
        return corridorCells;
    }

    private Array<Vector2I> GetVerticalCorridorCells(int x, int y1, int y2)
    {
        var corridorCells = new Array<Vector2I>();
        var step = y2 - y1;
        if (step >= 0)
        {
            for (int y = y1; y <= y2; y++)
                corridorCells.Add(new Vector2I(x, y));
        }
        else
        {
            for (int y = y2; y <= y1; y++)
                corridorCells.Add(new Vector2I(x, y));
        }
        return corridorCells;
    }


    private void RandomDigRooms()
    {
        var allRoomCells = new Array<Vector2I>();
        for (int i = 0; i < 1000; i++)
        {
            var room = GetRandomRoom();
            if (IsRoomIntersectOthers(room))
                continue;

            for (int y = room.Position.Y; y < room.Position.Y + room.Size.Y; y++)
            {
                for (int x = room.Position.X; x < room.Position.X + room.Size.X; x++)
                {
                    allRoomCells.Add(new Vector2I(x, y));
                }
            }
            _rooms.Add(room);
        }

        //if(GD.RandRange(0,1)== 0)
        //{
        //    SortRoomsFromLeftToRight();
        //}
        //else
        //{
        //    SortRoomsFromTopToBottom();
        //}
        //SortRooms();
        SortRoomsFromLeftToRight();

        _tileMap.SetCellsTerrainConnect((int)TileMapLayer.Default, allRoomCells,
                (int)TerrainSet.Default, (int)DungeonTerrain.Floor);

    }

    private bool IsRoomIntersectOthers(Rect2I room)
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            var room1 = (Rect2)room;
            var room2 = (Rect2)_rooms[i];
            if (room1.Intersects(room2, true))
            {
                return true;
            }
        }
        return false;
    }


    private Rect2I GetRandomRoom()
    {
        var roomSizeX = GD.RandRange(_dungeonData.MinRoomSize.X, _dungeonData.MaxRoomSize.X);
        var roomSizeY = GD.RandRange(_dungeonData.MinRoomSize.Y, _dungeonData.MaxRoomSize.Y);

        var roomX = GD.RandRange(1, _dungeonData.MapSize.X - 1 - roomSizeX);
        var roomY = GD.RandRange(1, _dungeonData.MapSize.Y - 1 - roomSizeY);

        return new Rect2I(roomX, roomY, roomSizeX, roomSizeY);
    }

    private void FullFillWithWalls()
    {
        var allCells = new Array<Vector2I>();
        for (int y = 0; y < _dungeonData.MapSize.Y; y++)
        {
            for (int x = 0; x < _dungeonData.MapSize.X; x++)
            {
                allCells.Add(new Vector2I(x, y));
            }
        }

        _tileMap.SetCellsTerrainConnect((int)TileMapLayer.Default, allCells, (int)TerrainSet.Default, (int)DungeonTerrain.Wall);
    }

    public Godot.Collections.Dictionary<string, Variant> GetDataForSave()
    {
        var floorCells = new Array<Vector2I>();
        var wallCells = new Array<Vector2I>();

        var dungeonData = GetParent<MapManager>().MapData as DungeonData;
        for (int y = 0; y < dungeonData.MapSize.Y; y++)
        {
            for (int x = 0; x < dungeonData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                var tileData = _tileMap.GetCellTileData((int)TileMapLayer.Default, cell);
                if (tileData.TerrainSet == (int)TerrainSet.Default)
                {
                    if (tileData.Terrain == (int)DungeonTerrain.Wall)
                    {
                        wallCells.Add(cell);
                    }
                    else if (tileData.Terrain == (int)DungeonTerrain.Floor)
                    {
                        floorCells.Add(cell);
                    }
                }
            }
        }

        return new Godot.Collections.Dictionary<string, Variant>
        {
            ["floor_cells"] = floorCells,
            ["wall_cells"] = wallCells,
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
                var floorCells = map["floor_cells"].AsGodotArray<Vector2I>();
                var wallCells = map["wall_cells"].AsGodotArray<Vector2I>();

                _tileMap.SetCellsTerrainConnect
                    (
                        (int)TileMapLayer.Default,
                        floorCells,
                        (int)TerrainSet.Default,
                        (int)DungeonTerrain.Floor,
                        false
                    );
                _tileMap.SetCellsTerrainConnect(
                        (int)TileMapLayer.Default,
                        wallCells,
                        (int)TerrainSet.Default,
                        (int)DungeonTerrain.Wall,
                        false
                    );

                return true;
            }
        }
        return false;
    }
}


