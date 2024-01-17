using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : Character, ILateUpdateEntity
{
	private AStarGridManager _aStarGridManager;

	private List<ILateUpdateComponent> _lateUpdateComponents = new();
	public override void Initialize()
	{
		base.Initialize();

		_aStarGridManager = this.GetUnique<AStarGridManager>();

		for (int i = 0; i < GetChildCount(); i++)
		{
			var child = GetChild(i);
            if (child is not ILateUpdateComponent)
            {
				continue;
            }
			var component = child as ILateUpdateComponent;
			_lateUpdateComponents.Add(component); 
        } 

		_mapManager.Initialized += On_MapManager_Initialized;
	}

	private void On_MapManager_Initialized(Vector2I _, Func<Vector2I> getEnemySpawnCell)
	{
		if (InitializeByLoadedData())
			return;

		var enemySpawnCell = getEnemySpawnCell.Invoke();
		GlobalPosition = enemySpawnCell * _mapData.CellSize + _mapData.CellSize /2 ;
	}

	protected override void On_CombatManager_CharacterDied(Character character)
	{
		if (character != this || IsDead)
			return;
		_aStarGridManager.AStarGrid.SetPointSolid(GetCell(),false);
		QueueFree();
		GD.Print(_characterData.Name + " 被击败");
		_isDead = true;
	}

    public void LateUpdate()
    {
        foreach (var component in _lateUpdateComponents)
        {
			component.LateUpdate();
        }
    }

    public override Godot.Collections.Dictionary<string, Variant> GetDataForSave()
    {
        var enemyDataForSave= base.GetDataForSave();
		var enemyData = _characterData as EnemyData;
		var deathDropPickableObjects = new  Godot.Collections.Dictionary< string,float>();
		foreach (var deathDropPickableObject in enemyData.DeathDropPickableObjects)
		{
			deathDropPickableObjects.Add
				(
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
		if (!_saveLoadManager.IsInitialized("maps"))
			return false;

		if (_characterData is EnemyData enemyData)
		{
			var maps = _saveLoadManager.maps;
			foreach (var map in maps)
			{
				if (map["scene_name"].AsString() == GetTree().CurrentScene.Name)
				{
					var enemies = map["enemies"].AsGodotArray<Godot.Collections.Dictionary<string, Variant>>();
					foreach (var enemy in enemies)
					{
						if (enemy["index"].AsInt32() == GetIndex())
						{
                            enemyData.DeathDropExperience = enemy["death_drop_experience"].AsSingle();
							foreach (var deathDropPickableObject in enemy["death_drop_pickable_objects"].AsGodotDictionary<string, float>())
							{
								var packedScene = GD.Load<PackedScene>(deathDropPickableObject.Key);
                                enemyData.DeathDropPickableObjects[packedScene] = deathDropPickableObject.Value;
							}
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
