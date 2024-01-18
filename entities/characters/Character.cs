using System;
using Godot;
using Godot.Collections;

public partial class Character : Node2D, IEntity, ISavable, ILoadable
{
    [Export]
    protected CharacterData _characterData;

    protected MapData _mapData;

    protected MapManager _mapManager;
    protected CombatManager _combatManager;
    protected SaveLoadManager _saveLoadManager;

    protected System.Collections.Generic.List<IComponent> _components = new();

    protected bool _isDead;

    public CharacterData CharacterData { get => _characterData; }

    public bool IsDead { get => _isDead; }

    public virtual void Initialize()
    {
        _mapManager = GetTree().CurrentScene.GetNode<MapManager>("%MapManager");
        _combatManager = GetTree().CurrentScene.GetNode<CombatManager>("%CombatManager");
        _saveLoadManager = GetTree().CurrentScene.GetNode<SaveLoadManager>("%SaveLoadManager");

        _mapData = _mapManager.MapData;

        for (int i = 0; i < GetChildCount(); i++)
        {
            var child = GetChild(i);

            if (child is not IComponent) { continue; }

            var component = child as IComponent;
            component.Initialize();

            _components.Add(component);
        }

        if (!InitializeByLoadedData())
        {
            InitializeCombatAttributes();
        }

        _combatManager.CharacterDied += On_CombatManager_CharacterDied;
    }

    public virtual void Update()
    {
        foreach (var component in _components)
        {
            component.Update();
        }
    }

    public virtual Dictionary<string, Variant> GetDataForSave()
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
            { "crit", _characterData.Crit },
        };
    }

    public bool InitializeByLoadedData()
    {
        if (_saveLoadManager.LoadedData == null ||
            _saveLoadManager.LoadedData.Count == 0)
        {
            return false;
        }

        if (this is Enemy)
        {
            if (!_saveLoadManager.LoadedData.ContainsKey("maps")) { return false; }

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
                            InitializeByCharacterLoadedData(enemy);

                            return true;
                        }
                    }
                }
            }
        }
        else if (this is Player)
        {
            if (!_saveLoadManager.LoadedData.ContainsKey("player")) { return false; }

            var player = _saveLoadManager
                .LoadedData["player"].AsGodotDictionary<string, Variant>();

            InitializeByCharacterLoadedData(player);

            return true;
        }

        return false;
    }

    public int GetDistanceTo(Vector2I targetCell)
    {
        var startCell = (Vector2I)(GlobalPosition - _mapData.CellSize / 2) / _mapData.CellSize;

        var distanceX = Mathf.Abs(startCell.X - targetCell.X);
        var distanceY = Mathf.Abs(startCell.Y - targetCell.Y);

        return Mathf.Max(distanceX, distanceY);
    }

    protected void InitializeCombatAttributes()
    {
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

    protected void InitializeByCharacterLoadedData(Dictionary<string, Variant> characterLoadedData)
    {
        _characterData.Name = characterLoadedData["name"].AsString();
        _characterData.Sight = characterLoadedData["sight"].AsInt32();

        _characterData.Strength = characterLoadedData["strength"].AsInt32();
        _characterData.Constitution = characterLoadedData["constitution"].AsInt32();
        _characterData.Agility = characterLoadedData["agility"].AsInt32();

        _characterData.StrengthIncrementEffects =
            characterLoadedData["strength_increment_effects"].AsGodotDictionary<string, float>();
        _characterData.ConstitutionIncrementEffects =
            characterLoadedData["constitution_increment_effects"].AsGodotDictionary<string, float>();
        _characterData.AgilityIncrementEffects =
            characterLoadedData["agility_increment_effects"].AsGodotDictionary<string, float>();

        _characterData.Health = characterLoadedData["health"].AsSingle();
        _characterData.MaxHealth = characterLoadedData["max_health"].AsSingle();
        _characterData.Attack = characterLoadedData["attack"].AsSingle();
        _characterData.Defend = characterLoadedData["defend"].AsSingle();
        _characterData.Dodge = characterLoadedData["dodge"].AsSingle();
        _characterData.Crit = characterLoadedData["crit"].AsSingle();
    }

    protected virtual void On_CombatManager_CharacterDied(Character character)
    {
        throw new Exception("不可直接调用本基类方法！");
    }
}
