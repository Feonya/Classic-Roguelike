using System;
using Godot;
using Godot.Collections;

public partial class Enemy : Character
{
    public event Action Winning;

    private AStarGridManager _aStarGridManager;

    public override void Initialize()
    {
        base.Initialize();

        _aStarGridManager = GetTree().CurrentScene.GetNode<AStarGridManager>("%AStarGridManager");

        _mapManager.Initialized += On_MapManager_Initialized;
        _combatManager.CharacterDied += On_CombatManager_CharacterDied;
    }

    public override Dictionary<string, Variant> GetPersistentData()
    {
        var persistentData = base.GetPersistentData();

        var enemyData = _characterData as EnemyData;

        var deathDropPickableObjectsPersistentData = new Dictionary<string, float>();
        foreach (var deathDropPickableObject in enemyData.DeathDropPickableObjects)
        {
            deathDropPickableObjectsPersistentData.Add(
                deathDropPickableObject.Key.ResourcePath,
                deathDropPickableObject.Value
            );
        }

        persistentData.Add("death_drop_experience", enemyData.DeathDropExperience);
        persistentData.Add("death_drop_pickable_objects", deathDropPickableObjectsPersistentData);
        persistentData.Add("visible", Visible);
        persistentData.Add("scene_path", SceneFilePath);
        persistentData.Add("index", GetIndex());
        persistentData.Add("position", GlobalPosition);

        return persistentData;
    }

    private void On_MapManager_Initialized(
        Vector2I playerStartCell, Callable GetEnemySpawnCell)
    {
        if (TryInitializeEnemyOnMapManagerInititalizedByPersistentData()) { return; }

        var enemySpawnCell = GetEnemySpawnCell.Call().AsVector2I();
        GlobalPosition = enemySpawnCell * _mapData.CellSize + _mapData.CellSize / 2;
    }

    private bool TryInitializeEnemyOnMapManagerInititalizedByPersistentData()
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
                var enemiesPersistentData = mapPersistentData["enemies"]
                    .AsGodotArray<Dictionary<string, Variant>>();
                for (int j = 0; j < enemiesPersistentData.Count; j++)
                {
                    var enemyPersistentData = enemiesPersistentData[j];
                    if (GetIndex() == enemyPersistentData["index"].AsInt32())
                    {
                        (_characterData as EnemyData).DeathDropExperience =
                            enemyPersistentData["death_drop_experience"].AsSingle();

                        foreach (
                            var deathDropPickableObjectsPersistentData in
                            enemyPersistentData["death_drop_pickable_objects"]
                                .AsGodotDictionary<string, float>())
                        {
                            (_characterData as EnemyData).DeathDropPickableObjects[
                                GD.Load<PackedScene>(deathDropPickableObjectsPersistentData.Key)
                            ] = deathDropPickableObjectsPersistentData.Value;
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void On_CombatManager_CharacterDied(Character character)
    {
        if (character != this || _isDead) { return; }

        _aStarGridManager.AStarGrid.SetPointSolid(
            (Vector2I)(GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize,
            false
        );

        var dropDownComponent = GetNode<DropDownComponent>("DropDownComponent");
        dropDownComponent.TryDropPickableObject();
        dropDownComponent.DropExperience();

        QueueFree();

        GD.Print(_characterData.Name + "被击败！");

        if (Name == "SkeletonKing")
        {
            Winning?.Invoke();
        }
    }
}
