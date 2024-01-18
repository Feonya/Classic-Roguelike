using System;
using Godot;
using Godot.Collections;

public partial class Enemy : Character, ILateUpdateEntity
{
    public event Action SkeletonKingDied;

    private AStarGridManager _aStarGridManager;

    private System.Collections.Generic.List<ILateUpdateComponent> _lateUpdateComponents = new();

    public override void Initialize()
    {
        base.Initialize();

        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");

        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);

            if (child is not ILateUpdateComponent) { continue; }

            var component = child as ILateUpdateComponent;

            _lateUpdateComponents.Add(component);
        }

        _mapManager.Initialized += On_MapManager_Initialized;
    }

    public void LateUpdate()
    {
        foreach (var component in _lateUpdateComponents)
        {
            component.LateUpdate();
        }
    }

    public override Dictionary<string, Variant> GetDataForSave()
    {
        var enemyDataForSave = base.GetDataForSave();

        var enemyData = _characterData as EnemyData;

        var deathDropPickableObjects = new Dictionary<string, float>();
        foreach (var deathDropPickableObject in enemyData.DeathDropPickableObjects)
        {
            deathDropPickableObjects.Add(
                deathDropPickableObject.Key.ResourcePath,
                deathDropPickableObject.Value
            );
        }

        enemyDataForSave.Add("death_drop_pickable_objects", deathDropPickableObjects);
        enemyDataForSave.Add("death_drop_experience", enemyData.DeathDropExperience);
        enemyDataForSave.Add("visible", Visible);
        enemyDataForSave.Add("scene_path", SceneFilePath);
        enemyDataForSave.Add("index", GetIndex());
        enemyDataForSave.Add("position", GlobalPosition);

        return enemyDataForSave;
    }

    private new bool InitializeByLoadedData()
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
                var enemies = map["enemies"].AsGodotArray<Dictionary<string, Variant>>();
                for (int j = 0; j < enemies.Count; j++)
                {
                    var enemy = enemies[j];
                    if (enemy["index"].AsInt32() == GetIndex())
                    {
                        (_characterData as EnemyData)
                            .DeathDropExperience = enemy["death_drop_experience"].AsSingle();

                        foreach (
                            var deathDropPickableObject in
                            enemy["death_drop_pickable_objects"].AsGodotDictionary<string, float>())
                        {
                            (_characterData as EnemyData).DeathDropPickableObjects[
                                GD.Load<PackedScene>(deathDropPickableObject.Key)
                            ] = deathDropPickableObject.Value;
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void On_MapManager_Initialized(Vector2I _, Callable getEnemySpawnCell)
    {
        if (InitializeByLoadedData()) { return; }

        var enemySpawnCell = getEnemySpawnCell.Call().AsVector2I();

        GlobalPosition = enemySpawnCell * _mapData.CellSize + _mapData.CellSize / 2;
    }

    protected override void On_CombatManager_CharacterDied(Character character)
    {
        if (character != this || _isDead) { return; }

        _aStarGridManager.AStarGrid.SetPointSolid(
            (Vector2I)(GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize,
            false
        );

        QueueFree();
        GD.Print(_characterData.Name + "被击败！");

        if (_characterData.Name == "骷髅王")
        {
            SkeletonKingDied.Invoke();
        }

        _isDead = true;
    }
}
