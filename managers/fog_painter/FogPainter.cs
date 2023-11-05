using Godot;
using Godot.Collections;

public partial class FogPainter : Node, IManager, IPersistence
{
    private MapData _mapData;

    private SaveLoadManager _saveLoadManager;

    private TileMap _tileMap;

    private Player _player;

    private Node _enemyContainer;

    private Array<Vector2I> _previousInSightCells = new();

    public async void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        _enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");

        FullFillWithUnexploredFog();
        await ToSignal(GetTree(), "process_frame");
        RefreshFogOnInitialize();
    }

    public void Update(double delta)
    {
        RefreshFog();
    }

    public Dictionary<string, Variant> GetPersistentData()
    {
        var unexploredFogCells = new Array<Vector2I>();
        var outOfSightFogCells = new Array<Vector2I>();
        var inSightFogCells = new Array<Vector2I>();

        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                var cell = new Vector2I(x, y);
                var tileData = _tileMap.GetCellTileData((int)TileMapLayer.Fog, cell);

                if (tileData.TerrainSet == (int)TerrainSet.Fog)
                {
                    if (tileData.Terrain == (int)FogTerrain.Unexplored)
                    {
                        unexploredFogCells.Add(cell);
                    }
                    else if (tileData.Terrain == (int)FogTerrain.OutOfSight)
                    {
                        outOfSightFogCells.Add(cell);
                    }
                    else if (tileData.Terrain == (int)FogTerrain.InSight)
                    {
                        inSightFogCells.Add(cell);
                    }
                }
            }
        }

        return new Dictionary<string, Variant>
        {
            { "unexplored_fog_cells", unexploredFogCells },
            { "out_of_sight_fog_cells", outOfSightFogCells },
            { "in_sight_fog_cells", inSightFogCells }
        };
    }

    private void FullFillWithUnexploredFog()
    {
        var allCells = new Array<Vector2I>();
        for (int y = 0; y < _mapData.MapSize.Y; y++)
        {
            for (int x = 0; x < _mapData.MapSize.X; x++)
            {
                allCells.Add(new Vector2I(x, y));
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Fog,
            allCells,
            (int)TerrainSet.Fog,
            (int)FogTerrain.Unexplored,
            false
        );
    }

    private void RefreshFogOnInitialize()
    {
        if (TryRefreshFogOnInitializeByPersistentData()) { return; }

        RefreshFog();
    }

    private bool TryRefreshFogOnInitializeByPersistentData()
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
                var unexploredFogCells = mapPersistentData["unexplored_fog_cells"].AsGodotArray<Vector2I>();
                var outOfSightFogCells = mapPersistentData["out_of_sight_fog_cells"].AsGodotArray<Vector2I>();
                var inSightFogCells = mapPersistentData["in_sight_fog_cells"].AsGodotArray<Vector2I>();

                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Fog,
                    unexploredFogCells,
                    (int)TerrainSet.Fog,
                    (int)FogTerrain.Unexplored,
                    false
                );

                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Fog,
                    outOfSightFogCells,
                    (int)TerrainSet.Fog,
                    (int)FogTerrain.OutOfSight,
                    false
                );

                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Fog,
                    inSightFogCells,
                    (int)TerrainSet.Fog,
                    (int)FogTerrain.InSight,
                    false
                );

                return true;
            }
        }

        return false;
    }

    private void RefreshFog()
    {
        var currentInSightCells = GetCurrentInSightCells();

        foreach (var currentInSightCell in currentInSightCells)
        {
            if (_previousInSightCells.Contains(currentInSightCell))
            {
                _previousInSightCells.Remove(currentInSightCell);
            }
        }

        var fixableInSightWallCells = GetFixableInSightWallCells(currentInSightCells);

        foreach (var fixableInSightWallCell in fixableInSightWallCells)
        {
            if (!currentInSightCells.Contains(fixableInSightWallCell))
            {
                currentInSightCells.Add(fixableInSightWallCell);
            }

            if (_previousInSightCells.Contains(fixableInSightWallCell))
            {
                _previousInSightCells.Remove(fixableInSightWallCell);
            }
        }

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Fog,
            _previousInSightCells,
            (int)TerrainSet.Fog,
            (int)FogTerrain.OutOfSight,
            false
        );

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Fog,
            currentInSightCells,
            (int)TerrainSet.Fog,
            (int)FogTerrain.InSight,
            false
        );

        _previousInSightCells = currentInSightCells;

        RefreshEnemiesVisibility();
    }

    private void RefreshEnemiesVisibility()
    {
        for (int i = 0; i < _enemyContainer.GetChildCount(); i++)
        {
            var enemy = _enemyContainer.GetChild(i) as Node2D;
            var enemyCell = (Vector2I)
                (enemy.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;

            var enemyTileData = _tileMap.GetCellTileData(
                (int)TileMapLayer.Fog, enemyCell
            );
            if (enemyTileData.TerrainSet == (int)TerrainSet.Fog &&
                enemyTileData.Terrain == (int)FogTerrain.InSight)
            {
                if (enemy.Visible != true)
                {
                    enemy.Visible = true;
                }
            }
            else
            {
                if (enemy.Visible != false)
                {
                    enemy.Visible = false;
                }
            }
        }
    }

    private Array<Vector2I> GetCurrentInSightCells()
    {
        var inSightCells = new Array<Vector2I>();

        var space = _player.GetWorld2D().DirectSpaceState;

        var playerCell = (Vector2I)
            (_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;
        var playerSight = _player.CharacterData.Sight;

        for (int y = playerCell.Y - playerSight; y <= playerCell.Y + playerSight; y++)
        {
            for (int x = playerCell.X - playerSight; x <= playerCell.X + playerSight; x++)
            {
                var cell = new Vector2I(x, y);
                var parameters = new PhysicsRayQueryParameters2D
                {
                    From = cell * _mapData.CellSize + _mapData.CellSize / 2,
                    To = _player.GlobalPosition,
                    CollisionMask = (int)PhysicsLayer.BlockSight,
                    Exclude = new Array<Rid> { _player.GetNode<Area2D>("Area2D").GetRid() }
                };
                var result = space.IntersectRay(parameters);

                if (result.Count == 0)
                {
                    inSightCells.Add(cell);
                }
            }
        }

        return inSightCells;
    }

    private Array<Vector2I> GetFixableInSightWallCells(Array<Vector2I> currentInSightCells)
    {
        var fixableInSightWallCells = new Array<Vector2I>();

        var rightUpInSightCells = new Array<Vector2I>();
        var upLeftInSightCells = new Array<Vector2I>();
        var leftDownInSightCells = new Array<Vector2I>();
        var downRightInSightCells = new Array<Vector2I>();

        var playerCell = (Vector2I)
            (_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;
        var playerSight = _player.CharacterData.Sight;

        foreach (var cell in currentInSightCells)
        {
            if (Mathf.Abs(cell.X - playerCell.X) < playerSight &&
                Mathf.Abs(cell.Y - playerCell.Y) < playerSight)
            {
                if (cell.X >= playerCell.X && cell.Y <= playerCell.Y)
                {
                    rightUpInSightCells.Add(cell);
                }
                if (cell.X <= playerCell.X && cell.Y <= playerCell.Y)
                {
                    upLeftInSightCells.Add(cell);
                }
                if (cell.X <= playerCell.X && cell.Y >= playerCell.Y)
                {
                    leftDownInSightCells.Add(cell);
                }
                if (cell.X >= playerCell.X && cell.Y >= playerCell.Y)
                {
                    downRightInSightCells.Add(cell);
                }
            }
        }

        fixableInSightWallCells.AddRange(GetRightUpFixableInSightWallCells(rightUpInSightCells));
        fixableInSightWallCells.AddRange(GetUpLeftFixableInSightWallCells(upLeftInSightCells));
        fixableInSightWallCells.AddRange(GetLeftDownFixableInSightWallCells(leftDownInSightCells));
        fixableInSightWallCells.AddRange(GetDownRightFixableInSightWallCells(downRightInSightCells));

        return fixableInSightWallCells;
    }

    private Array<Vector2I> GetRightUpFixableInSightWallCells(
        Array<Vector2I> rightUpInSightCells)
    {
        var fixableInSightWallCells = new Array<Vector2I>();

        foreach (var cell in rightUpInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSightWallCells.Add(new Vector2I(cell.X + 1, cell.Y - 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X, cell.Y - 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X + 1, cell.Y));
            }
        }

        return fixableInSightWallCells;
    }

    private Array<Vector2I> GetUpLeftFixableInSightWallCells(
        Array<Vector2I> upLeftInSightCells)
    {
        var fixableInSightWallCells = new Array<Vector2I>();

        foreach (var cell in upLeftInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSightWallCells.Add(new Vector2I(cell.X - 1, cell.Y - 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X, cell.Y - 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X - 1, cell.Y));
            }
        }

        return fixableInSightWallCells;
    }

    private Array<Vector2I> GetLeftDownFixableInSightWallCells(
        Array<Vector2I> leftDownInSightCells)
    {
        var fixableInSightWallCells = new Array<Vector2I>();

        foreach (var cell in leftDownInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSightWallCells.Add(new Vector2I(cell.X - 1, cell.Y + 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X, cell.Y + 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X - 1, cell.Y));
            }
        }

        return fixableInSightWallCells;
    }

    private Array<Vector2I> GetDownRightFixableInSightWallCells(
        Array<Vector2I> downRightInSightCells)
    {
        var fixableInSightWallCells = new Array<Vector2I>();

        foreach (var cell in downRightInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSightWallCells.Add(new Vector2I(cell.X + 1, cell.Y + 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X, cell.Y + 1));
                fixableInSightWallCells.Add(new Vector2I(cell.X + 1, cell.Y));
            }
        }

        return fixableInSightWallCells;
    }

    private bool IsCellBlockSight(Vector2I cell)
    {
        var cellPosition = cell * _mapData.CellSize + _mapData.CellSize / 2;

        var space = _player.GetWorld2D().DirectSpaceState;
        var parameters = new PhysicsPointQueryParameters2D
        {
            Position = cellPosition,
            CollisionMask = (int)PhysicsLayer.BlockSight,
            CollideWithAreas = true
        };
        var results = space.IntersectPoint(parameters);

        if (results.Count > 0) { return true; }

        return false;
    }
}
