using Godot;
using Godot.Collections;

public partial class Character : Node2D, IEntity, IPersistence
{
    [Export]
    protected CharacterData _characterData;

    protected MapData _mapData;

    protected SaveLoadManager _saveLoadManager;
    protected MapManager _mapManager;
    protected CombatManager _combatManager;

    protected System.Collections.Generic.List<IComponent> _components = new();

    protected bool _isDead;

    public CharacterData CharacterData { get => _characterData; }

    public bool IsDead { get => _isDead; }

    public virtual void Initialize()
    {
        _mapData = GetTree().CurrentScene.GetNode<MapManager>("%MapManager").MapData;

        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");
        _mapManager = GetTree().CurrentScene.GetNode<MapManager>("%MapManager");
        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");

        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);

            if (child is not IComponent) { continue; }

            var component = child as IComponent;
            component.Initialize();

            _components.Add(component);
        }

        InitializeCombatAttributes();
    }

    public virtual void Update(double delta)
    {
        foreach (var component in _components)
        {
            component.Update(delta);
        }
    }

    public virtual Dictionary<string, Variant> GetPersistentData()
    {
        return new Dictionary<string, Variant>
        {
            { "name", _characterData.Name },
            { "sight", _characterData.Sight },
            { "strength", _characterData.Strength },
            { "constitution", _characterData.Constitution },
            { "agility", _characterData.Agility },
            { "strength_increment_effects", _characterData.StrengthIncrementEffects },
            { "constitution_increment_effects", _characterData.ConstitutionIncrementEffects },
            { "agility_increment_effects", _characterData.AgilityIncrementEffects },
            { "health", _characterData.Health },
            { "max_health", _characterData.MaxHealth },
            { "attack", _characterData.Attack },
            { "defend", _characterData.Defend },
            { "dodge", _characterData.Dodge },
            { "crit", _characterData.Crit }
        };
    }

    public int GetDistanceTo(Vector2I targetCell)
    {
        var startCell = (Vector2I)
            (GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;

        var distanceX = Mathf.Abs(startCell.X - targetCell.X);
        var distanceY = Mathf.Abs(startCell.Y - targetCell.Y);

        return Mathf.Max(distanceX, distanceY);
    }

    protected void InitializeCombatAttributes()
    {
        if (TryInitializeCombatAttributesByPersistentData()) { return; }

        _characterData.Health =
            _characterData.Constitution *
            _characterData.ConstitutionIncrementEffects["health"];

        _characterData.MaxHealth =
            _characterData.Constitution *
            _characterData.ConstitutionIncrementEffects["max_health"];

        _characterData.Attack =
            _characterData.Strength *
            _characterData.StrengthIncrementEffects["attack"];

        _characterData.Defend =
            _characterData.Strength *
            _characterData.StrengthIncrementEffects["defend"];

        _characterData.Dodge =
            _characterData.Agility *
            _characterData.AgilityIncrementEffects["dodge"];

        _characterData.Crit =
            _characterData.Agility *
            _characterData.AgilityIncrementEffects["crit"];
    }

    protected bool TryInitializeCombatAttributesByPersistentData()
    {
        if (this is Enemy)
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
                            _characterData.Name = enemyPersistentData["name"].AsString();
                            _characterData.Sight = enemyPersistentData["sight"].AsInt32();
                            _characterData.Strength = enemyPersistentData["strength"].AsInt32();
                            _characterData.Constitution = enemyPersistentData["constitution"].AsInt32();
                            _characterData.Agility = enemyPersistentData["agility"].AsInt32();
                            _characterData.StrengthIncrementEffects =
                                enemyPersistentData["strength_increment_effects"].AsGodotDictionary<string, float>();
                            _characterData.ConstitutionIncrementEffects =
                                enemyPersistentData["constitution_increment_effects"].AsGodotDictionary<string, float>();
                            _characterData.AgilityIncrementEffects =
                                enemyPersistentData["agility_increment_effects"].AsGodotDictionary<string, float>();
                            _characterData.Health = enemyPersistentData["health"].AsSingle();
                            _characterData.MaxHealth = enemyPersistentData["max_health"].AsSingle();
                            _characterData.Attack = enemyPersistentData["attack"].AsSingle();
                            _characterData.Defend = enemyPersistentData["defend"].AsSingle();
                            _characterData.Dodge = enemyPersistentData["dodge"].AsSingle();
                            _characterData.Crit = enemyPersistentData["crit"].AsSingle();

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        else if (this is Player)
        {
            if (_saveLoadManager.PersistentData == null ||
                _saveLoadManager.PersistentData.Count == 0 ||
                !_saveLoadManager.PersistentData.ContainsKey("player"))
            {
                return false;
            }

            var playerPersistentData = _saveLoadManager
                .PersistentData["player"].AsGodotDictionary<string, Variant>();

            _characterData.Name = playerPersistentData["name"].AsString();
            _characterData.Sight = playerPersistentData["sight"].AsInt32();
            _characterData.Strength = playerPersistentData["strength"].AsInt32();
            _characterData.Constitution = playerPersistentData["constitution"].AsInt32();
            _characterData.Agility = playerPersistentData["agility"].AsInt32();
            _characterData.StrengthIncrementEffects =
                playerPersistentData["strength_increment_effects"].AsGodotDictionary<string, float>();
            _characterData.ConstitutionIncrementEffects =
                playerPersistentData["constitution_increment_effects"].AsGodotDictionary<string, float>();
            _characterData.AgilityIncrementEffects =
                playerPersistentData["agility_increment_effects"].AsGodotDictionary<string, float>();
            _characterData.Health = playerPersistentData["health"].AsSingle();
            _characterData.MaxHealth = playerPersistentData["max_health"].AsSingle();
            _characterData.Attack = playerPersistentData["attack"].AsSingle();
            _characterData.Defend = playerPersistentData["defend"].AsSingle();
            _characterData.Dodge = playerPersistentData["dodge"].AsSingle();
            _characterData.Crit = playerPersistentData["crit"].AsSingle();

            return true;
        }

        return false;
    }
}
