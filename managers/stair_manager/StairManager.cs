using Godot;
using Godot.Collections;
using System;

public partial class StairManager : Node, IManager, ILoadable
{
    private MapData _mapData;
    private InputHandler _inputHandler;
    private TileMap _tileMap;
    private Player _player;
    private SaveLoadManager _saveLoadManager;

    [Export]
    private string _nextScenePath;
    [Export]
    private string _previousScenePath;

    private Vector2 _upStairPosition;
    private Vector2 _downStairPosition;

    public void Initialize()
    {
        _mapData = this.GetUnique<MapManager>().MapData;
        _inputHandler = this.GetUnique<InputHandler>();
        _tileMap = this.GetUnique<TileMap>();
        _player = this.GetUnique<Player>();
        _saveLoadManager = this.GetUnique<SaveLoadManager>();

        if (!InitializeByLoadedData())
        {
            GenerateUpStair();
            GenerateDownStair();
        }
    }

    public void Update()
    {
    }

    public void TryGoToPreviourScene()
    {
        if (string.IsNullOrEmpty(_previousScenePath))
            return;

        if (MathfExpand.IsEqualApprox(_player.GlobalPosition, _upStairPosition))
        {
            _saveLoadManager.Save();
            GetTree().ChangeSceneToFile(_previousScenePath);
        }
    }

    public void TryGoToNextScene()
    {
        if (string.IsNullOrEmpty(_nextScenePath))
            return;

        GD.Print("Go Next" + _player.GlobalPosition+ "/ "+_downStairPosition);
        if (MathfExpand.IsEqualApprox(_player.GlobalPosition, _downStairPosition))
        {
            _saveLoadManager.Save();
            GetTree().ChangeSceneToFile(_nextScenePath);
        }
    }


    private void GenerateUpStair()
    {
        if (string.IsNullOrEmpty(_previousScenePath))
            return;

        _upStairPosition = _player.GlobalPosition;
        var upStairCell = (Vector2I)((_upStairPosition - _mapData.CellSize / 2) / _mapData.CellSize);
        _tileMap.SetCellsTerrainConnect(
            (int)TileMapLayer.Default,
            new Godot.Collections.Array<Vector2I> { upStairCell },
            (int)TerrainSet.Stair,
            (int)StairTerrain.UpStair,
            false
            );

    }

    private void GenerateDownStair()
    {
        if (string.IsNullOrEmpty(_nextScenePath))
            return;

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
                Exclude = new Godot.Collections.Array<Rid>
                {
                    _player.GetNode<Area2D>("Area2D").GetRid()
                }
            };

            var results = space.IntersectPoint(parameters);
            if (results.Count > 0)
                continue;

            _downStairPosition = randomCellPosition;

            _tileMap.SetCellsTerrainConnect(
         (int)TileMapLayer.Default,
         new Godot.Collections.Array<Vector2I> { randomCell },
         (int)TerrainSet.Stair,
         (int)StairTerrain.DownStair,
         false
         );
            return;
        }

    }

    public bool InitializeByLoadedData()
    {
        if (!_saveLoadManager.IsInitialized("maps"))
            return false;

        var maps = _saveLoadManager.maps;
        for (int i = 0; i < maps.Count; i++)
        {
            var map = maps[i];
            if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
            {
                var upStairCell = map["up_stair_cell"].AsVector2I();
                var downStairCell = map["down_stair_cell"].AsVector2I();

                _upStairPosition = upStairCell * _mapData.CellSize + _mapData.CellSize / 2;
                _downStairPosition = downStairCell * _mapData.CellSize + _mapData.CellSize / 2;
            
                if(upStairCell != Vector2I.Zero)
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
}
