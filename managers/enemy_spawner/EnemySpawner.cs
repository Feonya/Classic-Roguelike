using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node, IManager,ILoadable
{
    [Export] private int _maxEnemies = 30; //生成的敌人数量
    [Export] private Godot.Collections.Dictionary<PackedScene, float> _enemyScenes = new();
    [Export] private Array<PackedScene> _bossScenes = new();

    private SaveLoadManager _saveLoadManager;
    private Node _enemyContainer;

    public void Initialize()
    {
        _enemyContainer = this.GetUnique("%EnemyContainer");
        _saveLoadManager = this.GetUnique< SaveLoadManager>();

        if (!InitializeByLoadedData())
        {
            SpawnEnemies();
            SpawnBosses();
        }
    }

    public void Update()
    {

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
        foreach(var bossScene in _bossScenes)
        {
            var bossInstance = bossScene.Instantiate<Enemy>();
            _enemyContainer.AddChild(bossInstance);
            bossInstance.Initialize();
        }
    }

    private List<PackedScene> GetSpawnList()
    {
        var spawnList = new List<PackedScene>();
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

    /// <summary>
    /// 获取生成权重总和
    /// </summary>
    /// <returns></returns>
    private float GetSpawnWeightSum()
    {
        var weightSum = 0f;
        foreach(var element in _enemyScenes)
        {
            weightSum += element.Value;
        }
        return weightSum;
    }


    public bool InitializeByLoadedData()
    {
        if (!_saveLoadManager.IsInitialized("maps"))
            return false;

        var maps = _saveLoadManager.maps;
        foreach(var map in maps)
        {
            if (map["scene_name"].AsString() == this.GetCurrentSceneName())
            {
                var enemies = map["enemies"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>();
                foreach(var enemy in enemies)
                {
                    var enemyInstance = GD.Load<PackedScene>(enemy["scene_path"].AsString()).Instantiate<Enemy>();
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
}
