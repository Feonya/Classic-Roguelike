using Godot;
using Godot.Collections;

public partial class EnemySpawner : Node, IManager, ILoadable
{
    [Export]
    private int _maxEnemies = 30;
    [Export]
    private Dictionary<PackedScene, float> _enemyScenes = new();
    [Export]
    private Array<PackedScene> _bossScenes = new();

    private SaveLoadManager _saveLoadManager;

    private Node _enemyContainer;

    public void Initialize()
    {
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");

        if (!InitializeByLoadedData())
        {
            SpawnEnemies();
            SpawnBosses();
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
                var enemies = map["enemies"].AsGodotArray<Dictionary<string, Variant>>();
                for (int j = 0; j < enemies.Count; j++)
                {
                    var enemy = enemies[j];

                    var enemyInstance = GD.Load<PackedScene>(
                        enemy["scene_path"].AsString()
                    ).Instantiate<Enemy>();
                    _enemyContainer.AddChild(enemyInstance);

                    var enemyIndex = enemy["index"].AsInt32();
                    _enemyContainer.MoveChild(enemyInstance, enemyIndex);

                    enemyInstance.Visible = enemy["visible"].AsBool();
                    enemyInstance.GlobalPosition = enemy["position"].AsVector2();

                    enemyInstance.Initialize();
                }

                return true;
            }
        }

        return false;
    }

    private void SpawnEnemies()
    {
        var spawnList = GetSpawnList();
        for (int i = 0; i < spawnList.Count; i++)
        {
            var randomNumber = GD.RandRange(0, spawnList.Count - 1);
            var enemyInstance = spawnList[randomNumber].Instantiate<Enemy>();
            _enemyContainer.AddChild(enemyInstance);

            enemyInstance.Initialize();
        }
    }

    private void SpawnBosses()
    {
        foreach (var bossScene in _bossScenes)
        {
            var bossInstance = bossScene.Instantiate<Enemy>();
            _enemyContainer.AddChild(bossInstance);

            bossInstance.Initialize();
        }
    }

    private System.Collections.Generic.List<PackedScene> GetSpawnList()
    {
        var spawnList = new System.Collections.Generic.List<PackedScene>();
        var weightSum = GetSpawnWeightSum();

        foreach (var element in _enemyScenes)
        {
            var number = element.Value / weightSum * _maxEnemies;
            for (int i = 0; i < number; i++)
            {
                spawnList.Add(element.Key);
            }
        }

        return spawnList;
    }

    private float GetSpawnWeightSum()
    {
        var weightSum = 0f;

        foreach (var element in _enemyScenes)
        {
            weightSum += element.Value;
        }

        return weightSum;
    }
}
