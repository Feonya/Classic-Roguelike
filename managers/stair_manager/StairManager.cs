using Godot;
using Godot.Collections;

public partial class StairManager : Node, IManager, ILoadable
{
    private MapData _mapData;

    private InputHandler _inputHandler;
    private SaveLoadManager _saveLoadManager;

    private TileMap _tileMap;
    private Player _player;

    [Export]
    private string _nextScenePath;
    [Export]
    private string _previousScenePath;

    private Vector2 _upStairPosition;
    private Vector2 _downStairPosition;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");
        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        if (!InitializeByLoadedData())
        {
            GenerateUpStair();
            GenerateDownStair();
        }
    }

    public void Update()
    {
    }

    public bool InitializeByLoadedData()
    {
        if (_saveLoadManager.LoadedData == null ||
            _saveLoadManager.LoadedData.Count == 0 ||
            !_saveLoadManager.LoadedData.ContainsKey("maps"))
        {
            return false;
        }

        var maps = _saveLoadManager.LoadedData["maps"].AsGodotArray<Dictionary<string, Variant>>();
        for (int i = 0; i < maps.Count; i++)
        {
            var map = maps[i];
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                var upStairCell = map["up_stair_cell"].AsVector2I();
                var downStairCell = map["down_stair_cell"].AsVector2I();

                _upStairPosition = upStairCell * _mapData.CellSize + _mapData.CellSize / 2;
                _downStairPosition = downStairCell * _mapData.CellSize + _mapData.CellSize / 2;

                if (upStairCell != Vector2I.Zero)
                {
                    _tileMap.SetCellsTerrainConnect(
                        (int)TileMapLayer.Default,
                        new Array<Vector2I> { upStairCell },
                        (int)TerrainSet.Stair,
                        (int)StairTerrain.UpStair,
                        false
                    );
                }
                if (downStairCell != Vector2I.Zero)
                {
                    _tileMap.SetCellsTerrainConnect(
                        (int)TileMapLayer.Default,
                        new Array<Vector2I> { downStairCell },
                        (int)TerrainSet.Stair,
                        (int)StairTerrain.DownStair,
                        false
                    );
                }

                return true;
            }
        }

        return false;
    }

    public void TryGoToPreviousScene()
    {
        if (_previousScenePath == null || _previousScenePath == "") { return; }

        if (Mathf.IsEqualApprox(_player.GlobalPosition.X, _upStairPosition.X) &&
            Mathf.IsEqualApprox(_player.GlobalPosition.Y, _upStairPosition.Y))
        {
            _saveLoadManager.Save();

            GetTree().ChangeSceneToFile(_previousScenePath);
        }
    }

    public void TryGoToNextScene()
    {
        if (_nextScenePath == null || _nextScenePath == "") { return; }

        if (Mathf.IsEqualApprox(_player.GlobalPosition.X, _downStairPosition.X) &&
            Mathf.IsEqualApprox(_player.GlobalPosition.Y, _downStairPosition.Y))
        {
            _saveLoadManager.Save();

            GetTree().ChangeSceneToFile(_nextScenePath);
        }
    }

    private void GenerateUpStair()
    {
        if (_previousScenePath == null || _previousScenePath == "") { return; }

        _upStairPosition = _player.GlobalPosition;

        var upStairCell = (Vector2I)
            (_upStairPosition - _mapData.CellSize / 2) / _mapData.CellSize;

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            new Array<Vector2I> { upStairCell },
            (int)TerrainSet.Stair,
            (int)StairTerrain.UpStair,
            false
        );
    }

    private void GenerateDownStair()
    {
        if (_nextScenePath == null || _nextScenePath == "") { return; }

        var space = _player.GetWorld2D().DirectSpaceState;

        while (true)
        {
            var randomX = GD.RandRange(1, _mapData.MapSize.X - 2);
            var randomY = GD.RandRange(1, _mapData.MapSize.Y - 2);
            var randomCell = new Vector2I(randomX, randomY);
            var randomCellPosition = randomCell * _mapData.CellSize + _mapData.CellSize / 2;

            var parameters = new PhysicsPointQueryParameters2D
            {
                CollisionMask = (int)PhysicsLayer.BlockMovement,
                Position = randomCellPosition,
                Exclude = new Array<Rid> { _player.GetNode<Area2D>("Area2D").GetRid() }
            };
            var results = space.IntersectPoint(parameters);

            if (results.Count > 0) { continue; }

            _downStairPosition = randomCellPosition;

            _tileMap.SetCellsTerrainConnect(
                (int)TileMapLayer.Default,
                new Array<Vector2I> { randomCell },
                (int)TerrainSet.Stair,
                (int)StairTerrain.DownStair,
                false
            );

            return;
        }
    }
}
