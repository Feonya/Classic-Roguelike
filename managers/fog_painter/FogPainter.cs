using Godot;
using Godot.Collections;

public partial class FogPainter : Node, IManager, ILoadable
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

        if (!InitializeByLoadedData())
        {
            FullFillWithUnexploredFog();
        }

        await ToSignal(GetTree(), "process_frame");
        RefreshFog();
    }

    public void Update()
    {
        RefreshFog();
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
                var unexploredFogCells = map["unexplored_fog_cells"].AsGodotArray<Vector2I>();
                var outOfSightFogCells = map["out_of_sight_fog_cells"].AsGodotArray<Vector2I>();
                var inSightFogCells = map["in_sight_fog_cells"].AsGodotArray<Vector2I>();

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

    private void RefreshFog()
    {
        // 1. 获取当前状态循环中应被设为InSight图块的所有单元格，存入第一个容器。
        var currentInSightCells = GetCurrentInSightCells();

        // 2. 从第二个容器中，剔除与第一个容器中皆应被设为InSight图块的相同单元格。
        foreach (var currentInSightCell in currentInSightCells)
        {
            if (_previousInSightCells.Contains(currentInSightCell))
            {
                _previousInSightCells.Remove(currentInSightCell);
            }
        }

        var fixableInSightCells = GetFixableInSightCells(currentInSightCells);
        foreach (var fixableInSightCell in fixableInSightCells)
        {
            if (!currentInSightCells.Contains(fixableInSightCell))
            {
                currentInSightCells.Add(fixableInSightCell);
            }

            if (_previousInSightCells.Contains(fixableInSightCell))
            {
                _previousInSightCells.Remove(fixableInSightCell);
            }
        }

        // 3. 将第一个与第二个容器中的单元格，分别设为InSight和OutOfSight图块。
        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Fog,
            currentInSightCells,
            (int)TerrainSet.Fog,
            (int)FogTerrain.InSight,
            false
        );

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Fog,
            _previousInSightCells,
            (int)TerrainSet.Fog,
            (int)FogTerrain.OutOfSight,
            false
        );

        // 4. 将第一个容器的引用赋予第二个容器。
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

            var enemyTileData = _tileMap.GetCellTileData((int)TileMapLayer.Fog, enemyCell);
            if (enemyTileData.TerrainSet == (int)TerrainSet.Fog &&
                enemyTileData.Terrain == (int)FogTerrain.InSight)
            {
                enemy.Visible = true;
            }
            else
            {
                enemy.Visible = false;
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

    private Array<Vector2I> GetFixableInSightCells(Array<Vector2I> currentInSightCells)
    {
        var fixableInSightCells = new Array<Vector2I>();

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

        // 获取真正需要修正的所有单元格。
        fixableInSightCells.AddRange(GetRightUpFixableInSightCells(rightUpInSightCells));
        fixableInSightCells.AddRange(GetUpLeftFixableInSightCells(upLeftInSightCells));
        fixableInSightCells.AddRange(GetLeftDownFixableInSightCells(leftDownInSightCells));
        fixableInSightCells.AddRange(GetDownRightFixableInSightCells(downRightInSightCells));

        return fixableInSightCells;
    }

    private Array<Vector2I> GetRightUpFixableInSightCells(Array<Vector2I> rightUpInSightCells)
    {
        var fixableInSIghtCells = new Array<Vector2I>();

        foreach (var cell in rightUpInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSIghtCells.Add(new Vector2I(cell.X + 1, cell.Y));
                fixableInSIghtCells.Add(new Vector2I(cell.X, cell.Y - 1));
                fixableInSIghtCells.Add(new Vector2I(cell.X + 1, cell.Y - 1));
            }
        }

        return fixableInSIghtCells;
    }

    private Array<Vector2I> GetUpLeftFixableInSightCells(Array<Vector2I> upLeftInSightCells)
    {
        var fixableInSIghtCells = new Array<Vector2I>();

        foreach (var cell in upLeftInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSIghtCells.Add(new Vector2I(cell.X - 1, cell.Y));
                fixableInSIghtCells.Add(new Vector2I(cell.X, cell.Y - 1));
                fixableInSIghtCells.Add(new Vector2I(cell.X - 1, cell.Y - 1));
            }
        }

        return fixableInSIghtCells;
    }

    private Array<Vector2I> GetLeftDownFixableInSightCells(Array<Vector2I> leftDownInSightCells)
    {
        var fixableInSIghtCells = new Array<Vector2I>();

        foreach (var cell in leftDownInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSIghtCells.Add(new Vector2I(cell.X - 1, cell.Y));
                fixableInSIghtCells.Add(new Vector2I(cell.X, cell.Y + 1));
                fixableInSIghtCells.Add(new Vector2I(cell.X - 1, cell.Y + 1));
            }
        }

        return fixableInSIghtCells;
    }

    private Array<Vector2I> GetDownRightFixableInSightCells(Array<Vector2I> downRightInSightCells)
    {
        var fixableInSIghtCells = new Array<Vector2I>();

        foreach (var cell in downRightInSightCells)
        {
            if (!IsCellBlockSight(cell))
            {
                fixableInSIghtCells.Add(new Vector2I(cell.X + 1, cell.Y));
                fixableInSIghtCells.Add(new Vector2I(cell.X, cell.Y + 1));
                fixableInSIghtCells.Add(new Vector2I(cell.X + 1, cell.Y + 1));
            }
        }

        return fixableInSIghtCells;
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
