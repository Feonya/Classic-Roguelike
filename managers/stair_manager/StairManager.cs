using Godot;
using Godot.Collections;

public partial class StairManager : Node, IManager
{
    private MapData _mapData;

    [Export]
    private string _nextScenePath;
    [Export]
    private string _previousScenePath;

    private SaveLoadManager _saveLoadManager;
    private InputHandler _inputHandler;

    private TileMap _tileMap;

    private Player _player;

    private int _upStairTerrainEnumNumber;
    private int _downStairTerrainEnumNumber;

    private Vector2 _upStairPosition;
    private Vector2 _downStairPosition;

    public void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");
        _tileMap = GetTree().CurrentScene.GetNode<TileMap>("%TileMap");

        _inputHandler = GetTree().CurrentScene.GetNode<InputHandler>("%InputHandler");

        _player = GetTree().CurrentScene.GetNode<Player>("%Player");

        var mapGenerator = GetTree().CurrentScene
            .GetNode<MapManager>("%MapManager").GetChild(0);
        if (mapGenerator is ForestGenerator)
        {
            _upStairTerrainEnumNumber = (int)ForestTerrain.UpStair;
            _downStairTerrainEnumNumber = (int)ForestTerrain.DownStair;
        }
        else if (mapGenerator is DungeonGenerator)
        {
            _upStairTerrainEnumNumber = (int)DungeonTerrain.UpStair;
            _downStairTerrainEnumNumber = (int)DungeonTerrain.DownStair;
        }

        _inputHandler.GoDownStairInputHandled += On_Inputhandler_GoDownStairInputHandled;
        _inputHandler.GoUpStairInputHandled += On_Inputhandler_GoUpStairInputHandled;

        GenerateUpStair();
        GenerateDownStair();
    }

    public void Update(double delta)
    {
    }

    private void GenerateUpStair()
    {
        if (_previousScenePath == null || _previousScenePath == "") { return; }

        if (TryGenerateUpStairByPersistentData()) { return; }

        _upStairPosition = _player.GlobalPosition;

        var playerCell = (Vector2I)
            (_player.GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;

        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            new Array<Vector2I> { playerCell },
            (int)TerrainSet.Default,
            _upStairTerrainEnumNumber,
            false
        );
    }

    private void GenerateDownStair()
    {
        if (_nextScenePath == null || _nextScenePath == "") { return; }

        if (TryGenerateDownStairByPersistentData()) { return; }

        var space = _tileMap.GetWorld2D().DirectSpaceState;

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
                (int)TerrainSet.Default,
                _downStairTerrainEnumNumber,
                false
            );

            return;
        }
    }

    private bool TryGenerateUpStairByPersistentData()
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
                var upStairCell = mapPersistentData["up_stair_cell"].AsVector2I();

                _upStairPosition = upStairCell * _mapData.CellSize + _mapData.CellSize / 2;

                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Default,
                    new Array<Vector2I> { upStairCell },
                    (int)TerrainSet.Default,
                    _upStairTerrainEnumNumber,
                    false
                );

                return true;
            }
        }

        return false;
    }

    private bool TryGenerateDownStairByPersistentData()
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
                var downStairCell = mapPersistentData["down_stair_cell"].AsVector2I();

                _downStairPosition = downStairCell * _mapData.CellSize + _mapData.CellSize / 2;

                _tileMap.SetCellsTerrainConnect(
                    (int)TileMapLayer.Default,
                    new Array<Vector2I> { downStairCell },
                    (int)TerrainSet.Default,
                    _downStairTerrainEnumNumber,
                    false
                );

                return true;
            }
        }

        return false;
    }

    private void TryGoToNextScene()
    {
        if (_nextScenePath == null || _nextScenePath == "") { return; }

        if (Mathf.IsEqualApprox(_player.GlobalPosition.X, _downStairPosition.X) &&
            Mathf.IsEqualApprox(_player.GlobalPosition.Y, _downStairPosition.Y))
        {
            _saveLoadManager.Save();

            GetTree().ChangeSceneToFile(_nextScenePath);
        }
    }

    private void TryGoToPreivousScene()
    {
        if (_previousScenePath == null || _previousScenePath == "") { return; }

        if (Mathf.IsEqualApprox(_player.GlobalPosition.X, _upStairPosition.X) &&
            Mathf.IsEqualApprox(_player.GlobalPosition.Y, _upStairPosition.Y))
        {
            _saveLoadManager.Save();

            GetTree().ChangeSceneToFile(_previousScenePath);
        }
    }

    private void On_Inputhandler_GoDownStairInputHandled()
    {
        TryGoToNextScene();
    }

    private void On_Inputhandler_GoUpStairInputHandled()
    {
        TryGoToPreivousScene();
    }
}
