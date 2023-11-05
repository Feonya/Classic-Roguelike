using Godot;
using Godot.Collections;

public partial class EnemySpawner : Node, IManager
{
    [Export]
    private int _maxEnemies = 30;
    [Export]
    private Dictionary<PackedScene, float/*生成权重*/> _enemyScenes = new();
    [Export]
    private Array<PackedScene> _bossScenes = new();

    private SaveLoadManager _saveLoadManager;

    private Node _enemyContainer;

    public void Initialize()
    {
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _enemyContainer = GetTree().CurrentScene.GetNode<Node>("%EnemyContainer");

        SpawnEnemiesAndBosses();
    }

    public void Update(double delta)
    {
    }

    private void SpawnEnemiesAndBosses()
    {
        if (TrySpawnEnemiesAndBossesByPersistentData())
        {
            return;
        }

        SpawnEnemies();
        SpawnBosses();
    }

    private bool TrySpawnEnemiesAndBossesByPersistentData()
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
                var enemiesPersistentData =
                    mapPersistentData["enemies"].AsGodotArray<Dictionary<string, Variant>>();
                for (int j = 0; j < enemiesPersistentData.Count; j++)
                {
                    var enemyPersistentData = enemiesPersistentData[j];

                    var enemyScenePath = enemyPersistentData["scene_path"].AsString();
                    var enemyInstance = GD.Load<PackedScene>(enemyScenePath).Instantiate<Enemy>();
                    _enemyContainer.AddChild(enemyInstance);

                    var enemyIndex = enemyPersistentData["index"].AsInt32();
                    _enemyContainer.MoveChild(enemyInstance, enemyIndex);

                    enemyInstance.Visible = enemyPersistentData["visible"].AsBool();
                    enemyInstance.GlobalPosition = enemyPersistentData["position"].AsVector2();

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
            var enmeyInstance = spawnList[randomNumber].Instantiate<Enemy>();
            _enemyContainer.AddChild(enmeyInstance);

            enmeyInstance.Initialize();
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

        foreach (var item in _enemyScenes)
        {
            var number = item.Value / weightSum * _maxEnemies;
            for (int i = 0; i < number; i++)
            {
                spawnList.Add(item.Key);
            }
        }

        return spawnList;
    }

    private float GetSpawnWeightSum()
    {
        var weightSum = 0f;

        foreach (var item in _enemyScenes)
        {
            weightSum += item.Value;
        }

        return weightSum;
    }
}
