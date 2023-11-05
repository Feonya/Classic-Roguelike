using Godot;
using Godot.Collections;

public partial class DungeonGenerator : Node, IMapGenerator
{
    private DungeonData _dungeonData;
    private TileSet _tileSet;
    private TileMap _tileMap;

    private SaveLoadManager _saveLoadManager;

    public MapData MapData { get => _dungeonData; }

    public void Initialize()
    {
        _dungeonData = GetParent<MapManager>().MapData as DungeonData;
        _tileSet = GD.Load<TileSet>("res://resources/tile_sets/dungeon_tile_set.tres");
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
        var randomNumber = GD.RandRange(0, _dungeonData.Rooms.Count - 1);

        var playerStartCell = _dungeonData.Rooms[randomNumber].GetCenter();

        var mapManager = GetParent<MapManager>();
        mapManager.TryAddCharacterCellAtSpawn(playerStartCell);

        return playerStartCell;
    }

    public Vector2I GetEnemySpawnCell()
    {
        while (true)
        {
            var randomNumber = GD.RandRange(0, _dungeonData.Rooms.Count - 1);
            var randomRoom = _dungeonData.Rooms[randomNumber];

            var randomX = GD.RandRange(
                randomRoom.Position.X,
                randomRoom.Position.X + randomRoom.Size.X - 1
            );
            var randomY = GD.RandRange(
                randomRoom.Position.Y,
                randomRoom.Position.Y + randomRoom.Size.Y - 1
            );

            var spawnCell = new Vector2I(randomX, randomY);

            var mapManager = GetParent<MapManager>();
            if (!mapManager.TryAddCharacterCellAtSpawn(spawnCell)) { continue; }

            return spawnCell;
        }
    }

    private void GenerateMap()
    {
        if (TryGenerateMapByPersistentData()) { return; }

        FullFillWithWalls();
        RandomDigRooms();
        RandomDigCorridors();
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
                var floorCells = mapPersistentData["floor_cells"].AsGodotArray<Vector2I>();
                var wallCells = mapPersistentData["wall_cells"].AsGodotArray<Vector2I>();

                _tileMap.SetCellsTerrainConnect(
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

    private void FullFillWithWalls()
    {
        var cells = new Array<Vector2I>();

        for (int y = 0; y < _dungeonData.MapSize.Y; y++)
        {
            for (int x = 0; x < _dungeonData.MapSize.X; x++)
            {
                cells.Add(new Vector2I(x, y));
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            cells,
            (int)TerrainSet.Default,
            (int)DungeonTerrain.Wall
        );
    }

    private void RandomDigRooms()
    {
        var allRoomCells = new Array<Vector2I>();

        for (int i = 0; i < 1000; i++)
        {
            var room = GetRandomRoom();

            if (IsRoomIntersectOthers(room)) { continue; }

            for (int y = room.Position.Y; y < room.Position.Y + room.Size.Y; y++)
            {
                for (int x = room.Position.X; x < room.Position.X + room.Size.X; x++)
                {
                    allRoomCells.Add(new Vector2I(x, y));
                }
            }

            _dungeonData.Rooms.Add(room);
        }

        if (GD.RandRange(0, 1) == 0)
        {
            SortRoomsFromLeftToRight();
        }
        else
        {
            SortRoomsFromTopToBottom();
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            allRoomCells,
            (int)TerrainSet.Default,
            (int)DungeonTerrain.Floor
        );
    }

    private Rect2I GetRandomRoom()
    {
        var roomSizeX = GD.RandRange(_dungeonData.MinRoomSize.X, _dungeonData.MaxRoomSize.X);
        var roomSizeY = GD.RandRange(_dungeonData.MinRoomSize.Y, _dungeonData.MaxRoomSize.Y);

        var roomX = GD.RandRange(1, _dungeonData.MapSize.X - 1 - roomSizeX);
        var roomY = GD.RandRange(1, _dungeonData.MapSize.Y - 1 - roomSizeY);

        return new Rect2I(roomX, roomY, roomSizeX, roomSizeY);
    }

    private bool IsRoomIntersectOthers(Rect2I room)
    {
        for (int i = 0; i < _dungeonData.Rooms.Count; i++)
        {
            var anotherRoom = _dungeonData.Rooms[i];
            if (((Rect2)room).Intersects(anotherRoom, true))
            {
                return true;
            }
        }

        return false;
    }

    private void SortRoomsFromLeftToRight()
    {
        _dungeonData.Rooms.Sort((room1, room2) =>
        {
            return room1.Position.X.CompareTo(room2.Position.X);
        });
    }

    private void SortRoomsFromTopToBottom()
    {
        _dungeonData.Rooms.Sort((room1, room2) =>
        {
            return room1.Position.Y.CompareTo(room2.Position.Y);
        });
    }

    private void RandomDigCorridors()
    {
        for (int i = 0; i < _dungeonData.Rooms.Count - 1; i++)
        {
            var room1 = _dungeonData.Rooms[i];
            var room2 = _dungeonData.Rooms[i + 1];

            var x1 = room1.GetCenter().X;
            var y1 = room1.GetCenter().Y;
            var x2 = room2.GetCenter().X;
            var y2 = room2.GetCenter().Y;

            if (GD.RandRange(0, 1) == 0)
            {
                var horizontalCells = GetHorizontalCorridorCells(x1, x2, y1);
                var verticalCells = GetVerticalCorridorCells(x2, y1, y2);
                _dungeonData.CorridorCells.AddRange(horizontalCells);
                _dungeonData.CorridorCells.AddRange(verticalCells);
            }
            else
            {
                var verticalCells = GetVerticalCorridorCells(x1, y1, y2);
                var horizontalCells = GetHorizontalCorridorCells(x1, x2, y2);
                _dungeonData.CorridorCells.AddRange(verticalCells);
                _dungeonData.CorridorCells.AddRange(horizontalCells);
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            _dungeonData.CorridorCells,
            (int)TerrainSet.Default,
            (int)DungeonTerrain.Floor
        );
    }

    private Array<Vector2I> GetHorizontalCorridorCells(int x1, int x2, int y)
    {
        var corridorCells = new Array<Vector2I>();

        var step = x2 - x1;
        if (step >= 0)
        {
            for (int x = x1; x <= x2; x++)
            {
                corridorCells.Add(new Vector2I(x, y));
            }
        }
        else
        {
            for (int x = x2; x <= x1; x++)
            {
                corridorCells.Add(new Vector2I(x, y));
            }
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
            {
                corridorCells.Add(new Vector2I(x, y));
            }
        }
        else
        {
            for (int y = y2; y <= y1; y++)
            {
                corridorCells.Add(new Vector2I(x, y));
            }
        }

        return corridorCells;
    }
}
